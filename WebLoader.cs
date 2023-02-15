namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    public class WebLoader : ILoader
    {
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
