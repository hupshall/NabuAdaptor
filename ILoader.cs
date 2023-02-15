namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    interface ILoader
    {
        bool TryGetData(string path, out byte[] data);
        bool TryGetDirectory(string path, out string directoryPath);
    }
}
