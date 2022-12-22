namespace NabuAdaptor
{
    using System;
    using System.IO;

    /// <summary>
    /// Interface to define a nabu to server connection type
    /// </summary>
    interface IConnection
    {
        /// <summary>
        /// Gets whether the connection is connected
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets the stream to read/write from
        /// </summary>
        Stream NabuStream { get; }

        /// <summary>
        /// Stop the server
        /// </summary>
        void StopServer();

        /// <summary>
        /// Start the server
        /// </summary>
        void StartServer();

    }
}
