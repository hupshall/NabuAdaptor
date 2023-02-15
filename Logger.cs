namespace NabuAdaptor
{
    using System;

    /// <summary>
    /// Logger class
    /// </summary>
    public class Logger
    {
        private EventHandler<string> logEvent;

        /// <summary>
        ///  Name of log file
        /// </summary>
        private const string logFile = "nabu.log";

        /// <summary>
        /// 
        /// </summary>
        public enum Target
        {
            // output to file
            file,
            
            // output to console.
            console
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logEventHandler"></param>
        public Logger(EventHandler<string> logEventHandler)
        {
            this.logEvent = logEventHandler;
        }

        /// <summary>
        /// 
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
