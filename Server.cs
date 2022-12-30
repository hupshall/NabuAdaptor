namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// Main implementation of the Nabu server, sits and waits for the nabu to request something and then fulfills that request.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// WebClient to download pak files from cloud
        /// </summary>
        private static WebClient webClient;

        /// <summary>
        /// Nabu connection
        /// </summary>
        private IConnection connection;

        /// <summary>
        /// Server settings
        /// </summary>
        private Settings settings;

        /// <summary>
        /// Cache of loaded PAK files
        /// If you don't cache this, you'll be loading in the file and parsing everything for every individual segment.
        /// </summary>
        private List<NabuPak> cache;

        static Server()
        {
            webClient = new WebClient();
            webClient.Headers.Add("user-agent", "Nabu Network Adapter 1.0");
            webClient.Headers.Add("Content-Type", "application/octet-stream");
            webClient.Headers.Add("Content-Transfer-Encoding", "binary");
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="settings">server settings</param>
        public Server(Settings settings)
        {
            this.settings = settings;
            this.cache = new List<NabuPak>();
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public void StartServer()
        {
            this.StopServer();

            switch (this.settings.OperatingMode)
            {
                case Settings.Mode.Serial:
                    this.connection = new SerialConnection(this.settings.Port);
                    break;
                case Settings.Mode.TCPIP:
                    this.connection = new TcpConnection(Int32.Parse(this.settings.Port));
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
        public void RunServer()
        {
            try
            {
                bool initialized = false;
                bool err = false;

                // Start the server first
                this.StartServer();

                Logger.Log("Listening for Nabu", Logger.Target.file);
                while (this.connection != null && this.connection.Connected && !err)
                {
                    byte b = this.ReadByte();
                    switch (b)
                    {
                        case 0x85: // Channel
                            this.WriteBytes(0x10, 0x6);
                            Logger.Log($"Received Channel {this.ReadByte() + (this.ReadByte() << 8):X8}", Logger.Target.file);
                            this.WriteBytes(0xE4);
                            break;
                        case 0x84: // File Transfer
                            this.HandleFileRequest();
                            break;
                        case 0x83:
                            if (!initialized)
                            {
                                this.InitializeNabu(this.settings.AskForChannel);
                            }
                            else
                            {
                                this.WriteBytes(0x10, 0x6, 0xE4);
                            }
                            break;
                        case 0x82:
                            this.WriteBytes(0x10, 0x6);
                            break;
                        case 0x81:
                            this.WriteBytes(0x10, 0x6);
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
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Exception {ex.Message}", Logger.Target.file);
            }
            finally
            {
                this.StopServer();
            }            
        }

        /// <summary>
        /// Handle the Nabu's file request
        /// </summary>
        private void HandleFileRequest()
        {
            this.WriteBytes(0x10, 0x6);

            // Ok, get the requested packet and segment info
            int segmentNumber = this.GetRequestedSegment();          
            int pakFileName = this.GetRequestedPakFile();

            string pakName = $"{pakFileName:X06}";
            Logger.Log($"Nabu requesting file {pakFileName:X06} and segment {segmentNumber:X06}", Logger.Target.file);
            Spinner.Turn(pakFileName);

            // ok
            this.WriteBytes(0xE4);
            NabuPak nabuPak = null;

            if (pakFileName == 0x7FFFFF)
            {
                nabuPak = SegmentManager.CreateTimePak();
            }
            else
            {
                nabuPak = this.cache.FirstOrDefault((NabuPak x) => x.Name == pakName);

                if (nabuPak == null)
                {
                    if (!string.IsNullOrWhiteSpace(settings.Url))
                    {
                        string downloadUrl = string.Empty;
                        if (!settings.Url.EndsWith("/"))
                        {
                            downloadUrl = $"{settings.Url}/{pakName}.pak";
                        }
                        else
                        {
                            downloadUrl = $"{settings.Url}{pakName}.pak";
                        }

                        byte[] data = webClient.DownloadData(downloadUrl);
                        nabuPak = SegmentManager.LoadSegments(pakName, data);
                        this.cache.Add(nabuPak);
                    }
                    else
                    {
                        string pakFilename = Path.Combine(this.settings.Directory, $"{pakName}.pak");
                        string nabuFilename = Path.Combine(this.settings.Directory, $"{pakName}.nabu");

                        if (File.Exists(pakFilename))
                        {
                            nabuPak = SegmentManager.LoadSegments(pakName, File.ReadAllBytes(pakFilename));
                            this.cache.Add(nabuPak);
                        }
                        else if (File.Exists(nabuFilename))
                        {
                            nabuPak = SegmentManager.CreateSegments(pakName, File.ReadAllBytes(nabuFilename));
                            this.cache.Add(nabuPak);
                        }
                        else
                        {
                            nabuPak = null;
                        }
                    }
                    
                    if (nabuPak == null)
                    {
                        if (pakFileName == 1)
                        {
                            // Nabu can't do anything without an initial pack - throw and be done.
                            throw new FileNotFoundException($"Initial nabu file of {pakName} was not found, fix this");
                        }

                        // File not found, write unauthorized
                        this.WriteBytes(0x90);
                        this.ReadByte(0x10);
                        this.ReadByte(0x6);
                    }
                }                
            }

            // Send the requested segment of the pack
            if (nabuPak != null && segmentNumber <= nabuPak.Segments.Length)
            {
                this.WriteBytes(0x91);
                byte b = this.ReadByte();
                if (b != 0x10)
                {
                    this.WriteBytes(0x10, 0x6, 0xE4);
                    return;
                }

                this.ReadByte(0x6);
                this.SendSegment(nabuPak.Segments[segmentNumber]);
                this.WriteBytes(0x10, 0xE1);
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
        /// Get the requested
        /// </summary>
        /// <returns></returns>
        private byte GetRequestedSegment()
        {
            return this.ReadByte();
        }

        /// <summary>
        /// Send the segment to the nabu
        /// </summary>
        /// <param name="segment">segment to send</param>
        private void SendSegment(NabuSegment segment)
        {
            byte[] array = segment.Data;
            foreach (byte b in array)
            {
                // need to escape 0x10
                if (b == 0x10) 
                {
                    this.connection.NabuStream.Write(new byte[1] { 0x10 }, 0, 1);
                }
                this.connection.NabuStream.Write(new byte[1] { b }, 0, 1);
            }
        }

        /// <summary>
        /// Get the requested pak file name
        /// </summary>
        /// <returns>requested pack file</returns>
        private int GetRequestedPakFile()
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
        /// Read a byte - but throw if the byte we read is not what we expected
        /// </summary>
        /// <param name="validValues">list of possible values</param>
        /// <returns>the read byte</returns>
        private byte ReadByte(byte[] validValues)
        {
            byte num = this.ReadByte();

            if (validValues.Contains(num))
            {
                return num;
            }

            throw new Exception($"Read {num:X02} but expected {BitConverter.ToString(validValues)}");
        }

        /// <summary>
        /// Read a single byte from the stream
        /// </summary>
        /// <returns>read byte</returns>
        private byte ReadByte()
        {
            return (byte)this.connection.NabuStream.ReadByte();
        }

        /// <summary>
        /// Initialize the nabu and prompt for channel if requested
        /// </summary>
        /// <param name="askForChannel">flag for channel prompt</param>
        private void InitializeNabu(bool askForChannel)
        {
            this.WriteBytes(0x10, 0x6, 0xE4);
            if (this.ReadByte() == 0x82) 
            {
                this.WriteBytes(0x10, 0x6);
                this.ReadByte(0x1);
                if (!askForChannel)
                {
                    this.WriteBytes(0x1F, 0x10, 0xE1); 
                }
                else
                {
                    Logger.Log("Asking for channel", Logger.Target.file);
                    this.WriteBytes(0x9F, 0x10, 0xE1);
                }
            }
        }
    }
}
