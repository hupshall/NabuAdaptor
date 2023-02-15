namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to load nabu files on the web
    /// </summary>
    public class WebLoader : ILoader
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
                data = WebClientWrapper.DownloadData(path);
                return true;
            }
            catch (WebException)
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
                if (path.ToLowerInvariant().EndsWith(".pak") || (path.ToLowerInvariant().EndsWith(".nabu")))
                {
                    Uri uri = new Uri(path);

                    directoryPath = string.Format("{0}://{1}", uri.Scheme, uri.Authority);

                    for (int i = 0; i < uri.Segments.Length - 1; i++)
                    {
                        directoryPath += uri.Segments[i];
                    }

                    directoryPath = directoryPath.Trim("/".ToCharArray());
                }
                else
                {
                    directoryPath = path;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
