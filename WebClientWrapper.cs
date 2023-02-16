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
