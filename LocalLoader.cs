namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to load nabu files on the local machine
    /// </summary>
    class LocalLoader : ILoader
    {
        /// <summary>
        /// Try to get the contents of the nabu file located at the specified path
        /// </summary>
        /// <param name="path">Path to nabu file</param>
        /// <param name="data">contents of file</param>
        /// <returns>returns true/false if successful or not</returns>
        public bool TryGetData(string path, out byte[] data)
        {
            data = null;

            try
            {
                data = File.ReadAllBytes(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Try to get the parent directory of the specified file
        /// </summary>
        /// <param name="path">Path to get the parent directory</param>
        /// <param name="directoryPath">Parent directory path</param>
        /// <returns>returns true/false if successful or not</returns>
        public bool TryGetDirectory(string path, out string directoryPath)
        {
            directoryPath = string.Empty;

            try
            {
                directoryPath = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
