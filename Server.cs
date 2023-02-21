// BSD 2-Clause License
//
// Copyright(c) 2022, Huw Upshall
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;

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
        public Logger Logger
        {
            get;
            set;
        }

        /// <summary>
        /// Progress event handler (for UI scenarios)
        /// </summary>
        private EventHandler<ProgressEventArgs> progress;

        /// <summary>
        /// Nabu connection
        /// </summary>
        public IConnection Connection
        {
            get; set;
        }

        /// <summary>
        /// Modules to handle non-standard NABU op-codes.
        /// </summary>
        public List<IServerExtension> Extensions
        {
            get; set;
        }

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

            if (this.Logger != null)
            {
                this.Logger = new Logger(logger);
            }
            else
            {
                this.Logger = new Logger();
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
                    this.Connection = new SerialConnection(this.settings, this.Logger);
                    break;
                case Settings.Mode.TCPIP:
                    this.Connection = new TcpConnection(Int32.Parse(this.settings.Port), this.Logger);
                    break;
            }

            this.Connection.StartServer();
            this.Extensions = new List<IServerExtension>();
            this.Extensions.Add(new FileStoreExtensions.FileStoreExtensions(this));
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void StopServer()
        {
            if (this.Connection != null && this.Connection.Connected)
            {
                this.Connection.StopServer();
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
                    bool err = false;

                    // Start the server first
                    try
                    {
                        this.StartServer();
                    }
                    catch (Exception e)
                    {
                        err = true;

                        if (e is System.IO.IOException || e is System.UnauthorizedAccessException)
                        {
                            this.Logger.Log("Invalid PORT settings, Stop the server and select the correct port", Logger.Target.console);
                            return;
                        }
                    }

                    this.Logger.Log("Listening for NABU", Logger.Target.file);
                    while (this.Connection != null && this.Connection.Connected && !err && !token.IsCancellationRequested)
                    {
                        byte b = this.ReadByte();
                        switch (b)
                        {
                            case 0x8F:
                                this.ReadByte();
                                this.WriteBytes(0xE4);
                                break;
                            case 0x85: // Channel
                                this.WriteBytes(0x10, 0x6);
                                int channel = this.ReadByte() + (this.ReadByte() << 8);
                                this.Logger.Log($"Received Channel {channel:X8}", Logger.Target.file);
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
                                bool completed = false;

                                foreach (IServerExtension extension in this.Extensions)
                                {
                                    if (extension.TryProcessCommand(b))
                                    {
                                        completed = true;
                                        break;
                                    }
                                }

                                if (!completed)
                                {
                                    this.Logger.Log($"Unknown command {b:X8}", Logger.Target.console);
                                    this.WriteBytes(0x10, 0x6);
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Log($"Exception {ex.Message}", Logger.Target.file);
                }
                finally
                {
                    this.StopServer();
                }
            } while (!token.IsCancellationRequested);

        }

        /// <summary>
        /// The the current working directory
        /// </summary>
        /// <returns>the working directory</returns>
        public string GetWorkingDirectory()
        {
            ILoader loader;

            if (this.settings.Path.ToLowerInvariant().StartsWith("http"))
            {
                loader = new WebLoader();
            }
            else
            {
                loader = new LocalLoader();
            }

            string directory = string.Empty;

            loader.TryGetDirectory(this.settings.Path, out directory);

            return directory;
        }

        /// <summary>
        /// Write the byte array to the stream
        /// </summary>
        /// <param name="bytes">bytes to send</param>
        public void WriteBytes(params byte[] bytes)
        {
            this.Connection.NabuStream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Read a single byte from the stream
        /// </summary>
        /// <returns>read byte</returns>
        public byte ReadByte()
        {
            return (byte)this.Connection.NabuStream.ReadByte();
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
            this.Logger.Log($"NABU requesting segment {segmentNumber:X06} and packet {packetNumber:X06}", Logger.Target.file);

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
                            this.Logger.Log($"Creating NABU segment {segmentNumber:X06} from {this.settings.Path}", Logger.Target.console);
                            segment = SegmentManager.CreatePackets(segmentName, data);
                        }
                    }
                    else if (this.settings.Path.ToLowerInvariant().EndsWith(".pak") && segmentNumber == 1)
                    {
                        if (loader.TryGetData($"{this.settings.Path}", out data))
                        {
                            this.Logger.Log($"Loading NABU segment {segmentNumber:X06} from {this.settings.Path}", Logger.Target.console);
                            segment = SegmentManager.LoadPackets(segmentName, data, this.Logger);
                        }
                    }
                    else
                    {
                        string directory;

                        if (loader.TryGetDirectory(this.settings.Path, out directory))
                        {
                            if (loader.TryGetData($"{directory}/{segmentName}.nabu", out data))
                            {
                                this.Logger.Log($"Creating NABU segment {segmentNumber:X06} from {$"{directory}/{segmentName}.nabu"}", Logger.Target.console);
                                segment = SegmentManager.CreatePackets(segmentName, data);
                            }
                            else if (loader.TryGetData($"{directory}/{segmentName}.pak", out data))
                            {
                                this.Logger.Log($"loading NABU segment {segmentNumber:X06} from {directory}/{segmentName}.pak", Logger.Target.console);
                                segment = SegmentManager.LoadPackets(segmentName, data, this.Logger);
                            }
                        }
                    }
                    
                    if (segment == null)
                    {
                        if (segmentNumber == 1)
                        {
                            // NABU can't do anything without an initial segment - throw and be done.
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
            this.WriteBytes(packet.EscapedData);
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
        /// Read a sequence of bytes from the stream
        /// </summary>
        /// <returns>read bytes</returns>
        private byte[] ReadBytes()
        {
            byte[] buffer = new byte[1024];

            this.Connection.NabuStream.Read(buffer, 0, 1024);
            return buffer;
        }

        /// <summary>
        /// tell the NABU to present the channel prompt
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
                this.Logger.Log("Asking for channel", Logger.Target.file);
                this.WriteBytes(0xFF, 0x10, 0xE1);
            }
        }
    }
}
