namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    public static class WebClientWrapper
    {
        /// <summary>
        /// WebClient to download segment files from cloud
        /// </summary>
        private static WebClient webClient;

        static WebClientWrapper()
        {
            webClient = new WebClient();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            webClient.Headers.Add("user-agent", "Nabu Network Adapter 1.0");
            webClient.Headers.Add("Content-Type", "application/octet-stream");
            webClient.Headers.Add("Content-Transfer-Encoding", "binary");
        }

        public static byte[] DownloadData(string url)
        {
            return webClient.DownloadData(url);
        }

        public static string DownloadString(string url)
        {
            return webClient.DownloadString(url);
        }
    }
}
