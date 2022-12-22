namespace NabuAdaptor
{
    using System;

    /// <summary>
    /// Logger class
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Log method, right now, just outputs to the console but we could do other things here like add a trace level, write to a file, etc..
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
