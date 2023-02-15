namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to wrap the web client
    /// </summary>
    public static class WebClientWrapper
    {
        /// <summary>
        /// WebClient to download segment files from cloud
        /// </summary>
        private static WebClient webClient;

        /// <summary>
        /// Static constructor - set the global headers and SSL/TLS settings
        /// </summary>
        static WebClientWrapper()
        {
            webClient = new WebClient();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            webClient.Headers.Add("user-agent", "Nabu Network Adapter 1.0");
            webClient.Headers.Add("Content-Type", "application/octet-stream");
            webClient.Headers.Add("Content-Transfer-Encoding", "binary");
        }

        /// <summary>
        /// Download the specified URL as a byte array
        /// </summary>
        /// <param name="url">URL to download</param>
        /// <returns>contents as bytes</returns>
        public static byte[] DownloadData(string url)
        {
            return webClient.DownloadData(url);
        }

        /// <summary>
        /// Download the specified URL as a string
        /// </summary>
        /// <param name="url">URL to download</param>
        /// <returns>contents as string</returns>
        public static string DownloadString(string url)
        {
            return webClient.DownloadString(url);
        }
    }
}
