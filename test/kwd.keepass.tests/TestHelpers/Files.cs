using System;
using System.IO;
using System.Linq;

namespace kwd_keepass.tests.TestHelpers
{
    public static class Files
    {
        public static string CodePath() => 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../");

        public static string DataPath(params string[] subFolders) =>
            Path.Combine(new [] {CodePath(), "data"}.Concat(subFolders).ToArray());

        public static string TestDb() =>
            Path.Combine(CodePath(), "data", "testDb.kdbx");

        public static string GetExistingKeyPath() =>
            Path.Combine(CodePath(), "data", "ExistingKey.key");
    }
}
