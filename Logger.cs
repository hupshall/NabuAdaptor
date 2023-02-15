namespace NabuAdaptor
{
    using System;

    /// <summary>
    /// Logger class
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Log event handler - used for UI
        /// </summary>
        private EventHandler<string> logEvent;

        /// <summary>
        ///  Name of log file
        /// </summary>
        private const string logFile = "nabu.log";

        /// <summary>
        /// Destination for the log
        /// </summary>
        public enum Target
        {
            // output to file
            file,
            
            // output to console.
            console
        }

        /// <summary>
        /// Create an instance of the logger class with an event handler (used for UI scenarios)
        /// </summary>
        /// <param name="logEventHandler"></param>
        public Logger(EventHandler<string> logEventHandler)
        {
            this.logEvent = logEventHandler;
        }

        /// <summary>
        /// Create an instance of the logger class without an event handler
        /// </summary>
        public Logger()
        {
            this.logEvent = null;
        }

        /// <summary>
        /// Log method, right now, just outputs to the console but we could do other things here like add a trace level, write to a file, etc..
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Log(string message, Target target)
        {
            switch (target)
            {
                case Target.console:
                    if (this.logEvent != null)
                    {
                        logEvent(this, message);
                    }
                    else
                    {
                        Console.WriteLine(message);
                    }

                    break;
                case Target.file:
                    if (this.logEvent != null)
                    {
                        logEvent(this, message);
                    }

                    //System.IO.File.AppendAllText(logFile, message + System.Environment.NewLine);
                    break;
            }
        }
    }
}
