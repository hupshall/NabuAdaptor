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
        /// TCP/IP Port
        /// </summary>
        private int port;

        /// <summary>
        /// 
        /// </summary>
        private Logger logger;

        /// <summary>
        /// Stream used to read/write from/to the nabu
        /// </summary>
        public Stream NabuStream { get; private set; }

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
        /// <param name="port">TCPIP port to listen to</param>
        public TcpConnection(int port, Logger logger)
        {
            this.port = port;
            this.logger = logger;
        }

        /// <summary>
        /// Start the Server - open the socket and wait for a connection
        /// </summary>
        public void StartServer()
        {
            this.logger.Log("Server running in TCP/IP mode", Logger.Target.console);
            this.logger.Log("Waiting for connection", Logger.Target.console);
            this.tcpClient = GetListener(this.port).AcceptTcpClient();
            this.logger.Log("Connected", Logger.Target.console);
            this.NabuStream = tcpClient.GetStream();
        }

        /// <summary>
        /// Stop and clean up
        /// </summary>
        public void StopServer()
        {
            GetListener(this.port).Stop();
            tcpListener = null;

            if (this.tcpClient != null)
            {
                this.tcpClient.GetStream().Close();
                this.tcpClient.Close();
                //this.tcpClient.Dispose();
            }
        }
    }
}
