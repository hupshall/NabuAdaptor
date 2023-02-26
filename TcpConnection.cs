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
    using System.IO;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Class to talk to nabu emulator over TCP/IP
    /// </summary>
    public class TcpConnection : IConnection
    {
        /// <summary>
        /// TCP/IP Client
        /// </summary>
        private TcpClient tcpClient;

        /// <summary>
        /// TCP/IP Listener
        /// </summary>
        private static TcpListener tcpListener = null;

        /// <summary>
        /// settings
        /// </summary>
        private Settings Settings { get; set; }

        /// <summary>
        /// logger class
        /// </summary>
        private Logger logger;

        /// <summary>
        /// Stream used to read/write from/to the nabu
        /// </summary>
        public Stream NabuStream { get; private set; }

        /// <summary>
        /// Sets up the TCPIP listener singleton
        /// </summary>
        /// <param name="port">specified port</param>
        /// <returns>a TCPIP Listener</returns>
        private static TcpListener GetListener(int port)
        {
            if (tcpListener == null)
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
            }

            return tcpListener;
        }

        /// <summary>
        /// Flag to determine if the port is connected - TODO - doesn't work.
        /// </summary>
        public bool Connected
        {
            get
            {
                if (this.tcpClient != null)
                {
                    return this.tcpClient.Connected;
                }
                if (tcpListener != null)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpConnection"/> class.
        /// </summary>
        /// <param name="settings">Settings object</param>
        /// <param name="logger">Logger</param>
        public TcpConnection(Settings settings, Logger logger)
        {
            this.Settings = settings;
            this.logger = logger;
        }

        /// <summary>
        /// Start the Server - open the socket and wait for a connection
        /// </summary>
        public void StartServer()
        {
            this.logger.Log("Server running in TCP/IP mode", Logger.Target.console);
            this.logger.Log("Waiting for connection", Logger.Target.console);
            this.tcpClient = GetListener(Int32.Parse(this.Settings.Port)).AcceptTcpClient();

            if (this.Settings.TcpNoDelay)
            {
                this.tcpClient.NoDelay = true;
            }

            this.logger.Log("Connected", Logger.Target.console);
            this.NabuStream = tcpClient.GetStream();
        }

        /// <summary>
        /// Stop and clean up
        /// </summary>
        public void StopServer()
        {
            GetListener(Int32.Parse(this.Settings.Port)).Stop();
            tcpListener = null;

            if (this.tcpClient != null)
            {
                this.tcpClient.GetStream().Close();
                this.tcpClient.Close();
            }
        }
    }
}
