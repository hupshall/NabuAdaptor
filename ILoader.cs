namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// interface to define the nabu file loader
    /// </summary>
    interface ILoader
    {
        /// <summary>
        /// Try to get the data
        /// </summary>
        /// <param name="path">path to file</param>
        /// <param name="data">contents of file</param>
        /// <returns>true/false if successful</returns>
        bool TryGetData(string path, out byte[] data);

        /// <summary>
        /// Try to get the containing directory of the specified file
        /// </summary>
        /// <param name="path">path to file</param>
        /// <param name="directoryPath">directory</param>
        /// <returns>true/false if successful</returns>
        bool TryGetDirectory(string path, out string directoryPath);
    }
}
