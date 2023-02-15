namespace NabuAdaptor
{
    using System;
    using System.IO;
    using System.IO.Ports;

    /// <summary>
    /// Class to talk to nabu over serial
    /// </summary>
    public class SerialConnection : IConnection
    {
        /// <summary>
        /// Serial port 
        /// </summary>
        private SerialPort serialPort;

        private Logger logger;

        /// <summary>
        /// settings
        /// </summary>
        private Settings settings { get; set; }

        /// <summary>
        /// Stream used to read/write to/from nabu
        /// </summary>
        public Stream NabuStream { get; private set; }

        /// <summary>
        /// Flag to determine if the nabu is connected
        /// </summary>
        public bool Connected
        {
            get
            {
                if (this.serialPort != null)
                {
                    return this.serialPort.IsOpen;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialConnection"/> class.
        /// </summary>
        /// <param name="comPort">Com port to use</param>
        public SerialConnection(Settings settings, Logger logger)
        {
            this.settings = settings;
            this.logger = logger;
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public void StartServer()
        {
            this.logger.Log("Server running in Serial mode", Logger.Target.console);
            this.serialPort?.Dispose();
            this.serialPort = new SerialPort();
            this.serialPort.PortName = this.settings.SerialPort;
            this.serialPort.BaudRate = int.Parse(this.settings.BaudRate);
            this.serialPort.StopBits = StopBits.Two;
            this.serialPort.Parity = Parity.None;
            this.serialPort.DataBits = 8;
            this.serialPort.ReceivedBytesThreshold = 1;
            this.serialPort.ReadBufferSize = 2;
            this.serialPort.WriteBufferSize = 2;
            this.serialPort.ReadTimeout = -1;
            this.serialPort.DtrEnable = true;
            this.serialPort.RtsEnable = true;
            this.serialPort.Handshake = Handshake.None;

            this.serialPort.Open();
            this.NabuStream = this.serialPort.BaseStream;
            this.logger.Log("Connected", Logger.Target.console);
        }

        /// <summary>
        /// Stop the server and clean up
        /// </summary>
        public void StopServer()
        {
            this.serialPort.Close();
            this.serialPort.Dispose();
        }
    }
}
