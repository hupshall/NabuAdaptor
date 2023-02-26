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
        private Settings Settings { get; set; }

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
            this.Settings = settings;
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
            this.serialPort.PortName = this.Settings.SerialPort;
            this.serialPort.BaudRate = int.Parse(this.Settings.BaudRate);
            this.serialPort.StopBits = StopBits.Two;
            this.serialPort.Parity = Parity.None;
            this.serialPort.DataBits = 8;
            this.serialPort.ReceivedBytesThreshold = 1;
            this.serialPort.ReadBufferSize = 2;
            this.serialPort.WriteBufferSize = 2;
            this.serialPort.ReadTimeout = -1;

            if (!this.Settings.DisableFlowControl)
            {
                this.serialPort.DtrEnable = true;
                this.serialPort.RtsEnable = true;
            }
            else
            {
                this.serialPort.DtrEnable = false;
                this.serialPort.RtsEnable = false;
            }

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
