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
