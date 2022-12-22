﻿namespace NabuAdaptor
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// Main entry point
    /// </summary>
    class Program
    {
        /// <summary>
        /// Nabu server
        /// </summary>
        /// <param name="args">command line arguments</param>
        static void Main(string[] args)
        {
            // Get the settings from the command line.
            Settings settings = new Settings(args);

            // Create the server
            Server server = new Server(settings);

            // Run the server
            do
            {
                Task task = Task.Run(() => server.RunServer());

                do
                {
                    task.Wait(1000);
                    if (Console.KeyAvailable)
                    {
                        switch (Console.ReadKey(true).Key)
                        {
                            case ConsoleKey.Enter:
                                System.Environment.Exit(0);
                                break;
                        }
                    }
                    if (task.IsCompleted || task.IsCanceled)
                    {
                        break;
                    }
                } while (true);

            } while (true);
        }
    }
}