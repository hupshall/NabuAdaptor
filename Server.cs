namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Xml.Serialization;

    /// <summary>
    /// Main implementation of the Nabu server, sits and waits for the nabu to request something and then fulfills that request.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Cache of loaded segments
        /// If you don't cache this, you'll be loading in the file and parsing everything for every individual packet.
        /// </summary>       
        public static ConcurrentDictionary<string, NabuSegment> cache = new ConcurrentDictionary<string, NabuSegment>();

        /// <summary>
        /// Logger
        /// </summary>
        private Logger logger;

        /// <summary>
        /// 
        /// </summary>
        private EventHandler<ProgressEventArgs> progress;

        /// <summary>
        /// Nabu connection
        /// </summary>
        private IConnection connection;

        /// <summary>
        /// Server settings
        /// </summary>
        private Settings settings;

        /// <summary>
        ///  Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="settings">server settings</param>
        public Server(Settings settings, EventHandler<string> logger = null, EventHandler<ProgressEventArgs> progress = null)
        {
            this.settings = settings;

            if (logger != null)
            {
                this.logger = new Logger(logger);
            }
            else
            {
                this.logger = new Logger();
            }

            this.progress = progress;
        }

        /// <summary>
        /// Start the server
        /// </summary>
        private void StartServer()
        {
            this.StopServer();
            
            switch (this.settings.OperatingMode)
            {
                case Settings.Mode.Serial:
                    this.connection = new SerialConnection(this.settings, this.logger);
                    break;
                case Settings.Mode.TCPIP:
                    this.connection = new TcpConnection(Int32.Parse(this.settings.Port), this.logger);
                    break;
            }

            this.connection.StartServer();
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void StopServer()
        {
            if (this.connection != null && this.connection.Connected)
            {
                this.connection.StopServer();
            }
        }

        /// <summary>
        /// Simple server to handle Nabu requests
        /// </summary>
        public void RunServer(CancellationToken token)
        {
            do
            {
                try
                {
                    //bool initialized = false;
                    bool err = false;

                    // Start the server first
                    try
                    {
                        this.StartServer();
                    }
                    catch (Exception e)
                    {
                        if (e is System.IO.IOException || e is System.UnauthorizedAccessException)
                        {
                            this.logger.Log("Invalid PORT settings, Stop the server and select the correct port", Logger.Target.console);
                            return;
                        }

                        err = true;
                    }

                    this.logger.Log("Listening for NABU", Logger.Target.file);
                    while (this.connection != null && this.connection.Connected && !err && !token.IsCancellationRequested)
                    {
                        byte b = this.ReadByte();
                        switch (b)
                        {
                            case 0x8F:
                                this.logger.Log($"{(this.ReadByte() << 8):X8}", Logger.Target.console);
                                this.WriteBytes(0xE4);
                                break;
                            case 0x85: // Channel
                                this.WriteBytes(0x10, 0x6);
                                int channel = this.ReadByte() + (this.ReadByte() << 8);
                                this.logger.Log($"Received Channel {channel:X8}", Logger.Target.file);
                                this.WriteBytes(0xE4);
                                break;
                            case 0x84: // File Transfer
                                this.HandleFileRequest();
                                break;
                            case 0x83:
                                 this.WriteBytes(0x10, 0x6, 0xE4);
                                break;
                            case 0x82:
                                this.ConfigureChannel(this.settings.AskForChannel);
                                break;
                            case 0x81:
                                this.WriteBytes(0x10, 0x6);
                                break;
                            case 0x20:
                                // Send the main menu
                                this.SendMainMenu();
                                break;
                            case 0x21:
                                // send specified menu
                                this.SendSubMenu();
                                break;
                            case 0x1E:
                                this.WriteBytes(0x10, 0xE1);
                                break;
                            case 0x5:
                                this.WriteBytes(0xE4);
                                break;
                            case 0xF:
                                break;
                            case 0xFF:
                                // Well, we are reading garbage, socket has probably closed, quit this loop
                                err = true;
                                break;
                            default:
                                this.logger.Log($"Unknown command {b:X8}", Logger.Target.console);
                                this.WriteBytes(0x10, 0x6);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Log($"Exception {ex.Message}", Logger.Target.file);
                }
                finally
                {
                    this.StopServer();
                }
            } while (!token.IsCancellationRequested);

        }

        private void SendSubMenu()
        {
            byte menu = this.ReadByte();

            List<string> names = new List<string>();

            // send down the specified menu
            switch (menu)
            {
                case 0:
                    IEnumerable<Cycle> cycles = from item in this.settings.Cycles where item.TargetType == Cycle.Target.Cycle select item;
                    names = cycles.Select(cycle => cycle.Name).ToList();
                    break;
                case 1:
                    IEnumerable<Cycle> programs = (from item in this.settings.Cycles where item.TargetType == Cycle.Target.Program select item).OrderBy(x => x.Name).ToList();
                    names = programs.Select(cycle => cycle.Name).ToList();
                    break;
                case 2:
                    IEnumerable<Cycle> arcade = (from item in this.settings.Cycles where item.TargetType == Cycle.Target.Arcade select item).OrderBy(x => x.Name).ToList();
                    names = arcade.Select(cycle => cycle.Name).ToList();
                    break;
            }

            if (names.Any())
            {
                foreach (string name in names)
                {
                    this.WriteBytes(this.LatinToAscii($"{name}\r"));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SendMainMenu()
        {
        }

        public byte[] LatinToAscii(string str)
        {
            List<byte> data = new List<byte>();

            foreach (byte b in System.Text.Encoding.UTF8.GetBytes(str.ToCharArray()))
            {

                data.Add(b);
            }

            return data.ToArray();
        }

        /// <summary>
        /// Handle the Nabu's file request
        /// </summary>
        private void HandleFileRequest()
        {
            this.WriteBytes(0x10, 0x6);

            // Ok, get the requested packet and segment info
            int packetNumber = this.GetRequestedPacket();          
            int segmentNumber = this.GetRequestedSegment();

            string segmentName = $"{segmentNumber:X06}";
            this.logger.Log($"NABU requesting segment {segmentNumber:X06} and packet {packetNumber:X06}", Logger.Target.file);

            // ok
            this.WriteBytes(0xE4);
            NabuSegment segment = null;

            if (segmentNumber == 0x7FFFFF)
            {
                segment = SegmentManager.CreateTimeSegment();
            }
            else
            {
                if (cache.ContainsKey(segmentName))
                {
                    segment = cache[segmentName];
                }

                if (segment == null)
                {
                    // If the path starts with http, go cloud - otherwise local
                    ILoader loader;
                    if (this.settings.Path.ToLowerInvariant().StartsWith("http"))
                    {
                        loader = new WebLoader();
                    }
                    else
                    {
                        loader = new LocalLoader();
                    }

                    // if the path ends with .nabu:
                    byte[] data = null;

                    if (this.settings.Path.ToLowerInvariant().EndsWith(".nabu") && segmentNumber == 1)
                    {
                        if (loader.TryGetData($"{this.settings.Path}", out data))
                        {
                            this.logger.Log($"Loading NABU segment {segmentNumber:X06} from {this.settings.Path}", Logger.Target.console);

                            segment = SegmentManager.CreatePackets(segmentName, data);
                        }
                    }
                    else if (this.settings.Path.ToLowerInvariant().EndsWith(".pak") && segmentNumber == 1)
                    {
                        if (loader.TryGetData($"{this.settings.Path}", out data))
                        {
                            this.logger.Log($"Creating NABU segment {segmentNumber:X06} from {this.settings.Path}", Logger.Target.console);

                            segment = SegmentManager.LoadPackets(segmentName, data, this.logger);
                        }
                    }
                    else
                    {
                        string directory;

                        if (loader.TryGetDirectory(this.settings.Path, out directory))
                        {
                            string fileName = $"{directory}/{segmentName}.nabu";

                            if (loader.TryGetData(fileName, out data))
                            {
                                this.logger.Log($"Creating NABU segment {segmentNumber:X06} from {fileName}", Logger.Target.console);

                                segment = SegmentManager.CreatePackets(segmentName, data);
                            }
                            else if (loader.TryGetData($"{directory}/{segmentName}.pak", out data))
                            {
                                this.logger.Log($"loading NABU segment {segmentNumber:X06} from {directory}/{segmentName}.pak", Logger.Target.console);
                                segment = SegmentManager.LoadPackets(segmentName, data, this.logger);
                            }
                        }
                    }
                    
                    if (segment == null)
                    {
                        if (segmentNumber == 1)
                        {
                            // Nabu can't do anything without an initial pack - throw and be done.
                            throw new FileNotFoundException($"Initial NABU file of {segmentName} was not found, fix this");
                        }

                        // File not found, write unauthorized
                        this.WriteBytes(0x90);
                        this.ReadByte(0x10);
                        this.ReadByte(0x6);
                    }
                    else
                    {
                        cache.TryAdd(segmentName, segment);
                    }
                }                
            }

            // Send the requested segment of the pack
            if (segment != null && packetNumber <= segment.Packets.Length)
            {
                this.WriteBytes(0x91);
                byte b = this.ReadByte();
                if (b != 0x10)
                {
                    this.WriteBytes(0x10, 0x6, 0xE4);
                    return;
                }

                this.ReadByte(0x6);
                this.SendPacket(segment.Packets[packetNumber]);
                this.WriteBytes(0x10, 0xE1);

                if (this.progress != null)
                {
                    ProgressEventArgs args = new ProgressEventArgs(segmentNumber, packetNumber, segment.Packets.Length);
                    progress(this, args);
                }
                else
                {
                    Spinner.Turn(segmentNumber);
                }
            }
        }

        /// <summary>
        /// Write the byte array to the stream
        /// </summary>
        /// <param name="bytes">bytes to send</param>
        private void WriteBytes(params byte[] bytes)
        {
            this.connection.NabuStream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Get the requested packet
        /// </summary>
        /// <returns>requested packet</returns>
        private byte GetRequestedPacket()
        {
            return this.ReadByte();
        }

        /// <summary>
        /// Send the packet to the nabu
        /// </summary>
        /// <param name="packet">packet to send</param>
        private void SendPacket(NabuPacket packet)
        {
            byte[] array = packet.EscapedData;

            this.connection.NabuStream.Write(array, 0, array.Length);
        }

        /// <summary>
        /// Get the requested segment name
        /// </summary>
        /// <returns>requested segment file</returns>
        private int GetRequestedSegment()
        {
            byte b1 = this.ReadByte();
            byte b2 = this.ReadByte();
            byte b3 = this.ReadByte();
            int packFile = b1 + (b2 << 8) + (b3 << 16);
            return packFile;
        }

        /// <summary>
        /// Read Byte - but throw if the byte we read is not what we expect (passed in)
        /// </summary>
        /// <param name="expectedByte">This is the value we expect to read</param>
        /// <returns>The read byte, or throw</returns>
        private byte ReadByte(byte expectedByte)
        {
            byte num = this.ReadByte();
           
            if (num != expectedByte)
            {
                throw new Exception($"Read {num:X02} but expected {expectedByte:X02}");
            }

            return num;
        }

        /// <summary>
        /// Read a single byte from the stream
        /// </summary>
        /// <returns>read byte</returns>
        private byte ReadByte()
        {
            return (byte)this.connection.NabuStream.ReadByte();
        }

        private byte[] ReadBytes()
        {
            byte[] buffer = new byte[1024];

            this.connection.NabuStream.Read(buffer, 0, 1024);
            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="askForChannel"></param>
        private void ConfigureChannel(bool askForChannel)
        {
            this.WriteBytes(0x10, 0x6);
            byte[] temp = this.ReadBytes();

            if (!askForChannel)
            {
                this.WriteBytes(0x1F, 0x10, 0xE1);
            }
            else
            {
                this.logger.Log("Asking for channel", Logger.Target.file);
                this.WriteBytes(0xFF, 0x10, 0xE1);
            }
        }
    }
}
