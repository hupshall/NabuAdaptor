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

        /// <summary>
        /// Serial port to use
        /// </summary>
        private string ComPort { get; set; }

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
                return this.serialPort.IsOpen;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialConnection"/> class.
        /// </summary>
        /// <param name="comPort">Com port to use</param>
        public SerialConnection(string comPort)
        {
            this.ComPort = comPort;
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public void StartServer()
        {
            Logger.Log("Server running in Serial mode");
            this.serialPort?.Dispose();
            this.serialPort = new SerialPort();
            this.serialPort.PortName = this.ComPort;
            this.serialPort.BaudRate = 111865;
            this.serialPort.ReadTimeout = -1;
            this.serialPort.DtrEnable = true;
            this.serialPort.RtsEnable = true;
            this.serialPort.StopBits = StopBits.Two;
            this.serialPort.Handshake = Handshake.RequestToSend;
            this.serialPort.Open();
            this.NabuStream = this.serialPort.BaseStream;
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
