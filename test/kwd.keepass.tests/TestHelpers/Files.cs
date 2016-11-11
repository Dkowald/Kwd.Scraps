using System;
using System.IO;
using System.Linq;

namespace kwd_keepass.tests.TestHelpers
{
    public static class Files
    {
        public static string CodePath() => 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../");

        public static string DataPath(params string[] subFoldersAndFileName)
        {
            var path = Path.Combine(new[] {CodePath(), "data"}.Concat(subFoldersAndFileName).ToArray());
            
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            return path;
        }
            

        public static string TestDb() =>
            Path.Combine(CodePath(), "TestHelpers", "testDb.kdbx");

        public static string GetSampleMasterKeyPath() =>
            Path.Combine(CodePath(), "TestHelpers", "TestMasterKey.key");
    }
}
