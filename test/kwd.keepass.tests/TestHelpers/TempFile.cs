using System;
using System.IO;

namespace kwd_keepass.tests.TestHelpers
{
    public class TempFile : IDisposable
    {
        public TempFile(string path, bool deleteOnCreate = true)
        {
            Path = path;

            File.Delete(path);
        }

        public string Path { get; set; }

        public void Dispose()
        {
            File.Delete(Path);
        }
    }
}