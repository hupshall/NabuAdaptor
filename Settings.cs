namespace NabuAdaptor
{
    using System;
    using System.Linq;

    /// <summary>
    /// Settings class
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Internal enum for parsing state
        /// </summary>
        private enum ParseState
        {
            start,
            port,
            mode,
            path,
            url
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
        public bool AskForChannel { get; private set; }

        /// <summary>
        /// Gets the port
        /// </summary>
        public string Port { get; private set; }

        /// <summary>
        /// Gets the operating mode
        /// </summary>
        public Mode? OperatingMode { get; private set;  }

        /// <summary>
        /// Gets the current directory, this is where we expect the files to be
        /// </summary>
        public string Directory { get; private set; }

        /// <summary>
        /// Gets the current URL (cloud based file retrieval)
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        ///  Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="args"></param>
        public Settings(string[] args)
        {
            // default the directory to the current location
            this.Directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.AskForChannel = false;
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
                            this.Port = argument;
                            parseState = ParseState.start;
                            break;

                        case ParseState.path:
                            this.Directory = argument;
                            parseState = ParseState.start;
                            break;

                        case ParseState.url:
                            this.Url = argument;
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
                                case "-path":
                                    parseState = ParseState.path;
                                    break;
                                case "-url":
                                    parseState = ParseState.url;
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

        /// <summary>
        /// Display help on error
        /// </summary>
        public void DisplayHelp()
        {
            Console.WriteLine("Nabu console server");
            Console.WriteLine("");
            Console.WriteLine("Parameters:");
            Console.WriteLine("-mode -port -askforchannel -path -url");
            Console.WriteLine();
            Console.WriteLine("mode options: Serial, TCPIP - listen to serial port or TCPIP port");
            Console.WriteLine("port: Which serial port or TCPIP port to listen to, examples would be COM4 or 12345");
            Console.WriteLine("askforchannel - Just sets the flag to prompt the nabu for a channel.");
            Console.WriteLine("path: Local path for files, defaults to current directory");
            Console.WriteLine("url: url to cloud location - overrides path parameter if present, example https://www.mydomain.com/paklocation");
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
