namespace NabuAdaptor
{
    using System;

    /// <summary>
    /// Logger class
    /// </summary>
    public static class Logger
    {
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
        /// Log method, right now, just outputs to the console but we could do other things here like add a trace level, write to a file, etc..
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Log(string message, Target target)
        {
            switch (target)
            {
                case Target.console:
                    Console.WriteLine(message);
                    break;
                case Target.file:
                    System.IO.File.AppendAllText(logFile, message + System.Environment.NewLine);
                    break;
            }
        }
    }
}
