using System;
using System.Collections.Generic;
namespace NabuAdaptor
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class LocalLoader : ILoader
    {
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
