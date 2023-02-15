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
