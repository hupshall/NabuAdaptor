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
    using System.Configuration;
    using System.Linq;

    /// <summary>
    /// Settings class
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Default baud rate
        /// </summary>
        public static string defaultBaudRate = "111865";

        /// <summary>
        /// Internal enum for parsing state
        /// </summary>
        private enum ParseState
        {
            start,
            port,
            mode,
            source
        }

        /// <summary>
        /// server mode
        /// </summary>
        public enum Mode
        {
            Serial,
            TCPIP
        }

        /// <summary>
        /// Gets the ask for channel setting
        /// </summary>
        public bool AskForChannel { get; set; }

        /// <summary>
        /// Baud Rate
        /// </summary>
        public string BaudRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SerialPort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Path { get; set; }
 
        /// <summary>
        /// 
        /// </summary>
        public string TcpipPort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool DisableFlowControl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool TcpNoDelay { get; set; }

        /// <summary>
        /// Gets the port
        /// </summary>
        public string Port
        {
            get
            {
                switch (this.OperatingMode.Value)
                {
                    case Mode.Serial:
                        return this.SerialPort;
                    case Mode.TCPIP:
                        return this.TcpipPort;
                }

                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets the operating mode
        /// </summary>
        public Mode? OperatingMode { get; set;  }

        /// <summary>
        /// 
        /// </summary>
        public Cycle[] Cycles { get; set; }

        /// <summary>
        /// Manual creation of settings
        /// </summary>
        public Settings()
        {
            this.LoadSettings();
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="args"></param>
        public Settings(string[] args)
        {
            this.LoadSettings();

            ParseState parseState = ParseState.start;
            this.OperatingMode = null;

            try
            {
                if (!args.Any())
                {
                    this.DisplayHelp();
                }

                // Parse the arguments into settings
                foreach (string argument in args)
                {
                    switch (parseState)
                    {
                        case ParseState.mode:
                            switch (argument.ToLowerInvariant())
                            {
                                case "serial":
                                    this.OperatingMode = Mode.Serial;
                                    break;
                                case "tcpip":
                                    this.OperatingMode = Mode.TCPIP;
                                    break;                                
                                default:
                                    this.DisplayHelp();
                                    break;
                            }

                            parseState = ParseState.start;
                            break;

                        case ParseState.port:
                            switch (this.OperatingMode)
                            {
                                case Mode.Serial:
                                    this.SerialPort = argument;
                                    break;
                                case Mode.TCPIP:
                                    this.TcpipPort = argument;
                                    break;                                
                            }
                            parseState = ParseState.start;
                            break;

                        case ParseState.source:
                            this.Path = argument;
                            parseState = ParseState.start;
                            break;

                        case ParseState.start:
                            switch (argument.ToLowerInvariant())
                            {
                                case "-mode":
                                    parseState = ParseState.mode;
                                    break;
                                case "-port":
                                    parseState = ParseState.port;
                                    break;
                                case "-askforchannel":
                                    this.AskForChannel = true;
                                    break;
                                case "-source":
                                    parseState = ParseState.source;
                                    break;
                                default:
                                    this.DisplayHelp();
                                    break;
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                this.DisplayHelp();
            }

            if (this.OperatingMode == null || string.IsNullOrWhiteSpace(this.Port))
            {
                this.DisplayHelp();
            }
        }

        public void LoadSettings()
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection settings = configuration.AppSettings.Settings;

            // Default to serial
            this.OperatingMode = Mode.Serial;
            if (settings["Mode"] != null)
            {
                this.OperatingMode = (Mode)Enum.Parse(typeof(Mode), settings["Mode"].Value);
            }

            if (settings["SerialPort"] != null)
            {
                this.SerialPort = settings["SerialPort"].Value;
            }

            if (settings["TcpipPort"] != null)
            {
                this.TcpipPort = settings["TcpipPort"].Value;
            }

            if (settings["AskForChannel"] != null)
            {
                this.AskForChannel = bool.Parse(settings["AskForChannel"].Value);
            }

            if (settings["BaudRate"] != null)
            {
                this.BaudRate = settings["BaudRate"].Value;
            }
            else
            {
                this.BaudRate = defaultBaudRate;
            }

            if (settings["DisableFlowControl"] != null)
            {
                this.DisableFlowControl = bool.Parse(settings["DisableFlowControl"].Value);
            }
            else
            {
                this.DisableFlowControl = false;
            }

            if (settings["TcpNoDelay"] != null)
            {
                this.TcpNoDelay = bool.Parse(settings["TcpNoDelay"].Value);
            }
            else
            {
                this.TcpNoDelay = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SaveSettings()
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection settings = configuration.AppSettings.Settings;

            this.AddOrUpdateSettings(settings, "Mode", this.OperatingMode.ToString());
            this.AddOrUpdateSettings(settings, "SerialPort", this.SerialPort);
            this.AddOrUpdateSettings(settings, "TcpipPort", this.TcpipPort);
            this.AddOrUpdateSettings(settings, "AskForChannel", this.AskForChannel.ToString());
            this.AddOrUpdateSettings(settings, "BaudRate", this.BaudRate.ToString());
            this.AddOrUpdateSettings(settings, "DisableFlowControl", this.DisableFlowControl.ToString());
            this.AddOrUpdateSettings(settings, "TcpNoDelay", this.TcpNoDelay.ToString());

            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configuration.AppSettings.SectionInformation.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        public void AddOrUpdateSettings(KeyValueConfigurationCollection settings, string setting, string value)
        {
            if (settings[setting] == null)
            {
                settings.Add(setting, value);
            }
            else
            {
                settings[setting].Value = value;
            }
        }

        /// <summary>
        /// Display help on error
        /// </summary>
        public void DisplayHelp()
        {
            Console.WriteLine("Nabu console server");
            Console.WriteLine("");
            Console.WriteLine("Parameters:");
            Console.WriteLine("-mode -port -askforchannel -source");
            Console.WriteLine();
            Console.WriteLine("mode options: Serial, TCPIP - listen to serial port or TCPIP port");
            Console.WriteLine("port: Which serial port or TCPIP port to listen to, examples would be COM4 or 12345");
            Console.WriteLine("askforchannel - Just sets the flag to prompt the nabu for a channel.");
            Console.WriteLine("source: url or Local path for files, defaults to current directory");
            Console.WriteLine();
            Console.WriteLine("Serial Mode example:");
            Console.WriteLine("-Mode Serial -Port COM4");
            Console.WriteLine("");
            Console.WriteLine("TCPIP Mode example:");
            Console.WriteLine("-Mode TCPIP -Port 5816");
            Environment.Exit(0);
        }
    }
}
