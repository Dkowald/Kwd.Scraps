using System;
using System.IO;
using kwd.keepass;
using KeePassLib;
using Xunit;

namespace kwd_keepass.tests
{
    public class KeepassConfigurationSourceTests
    {
        [Fact]
        public void OpenDb_NoDb()
        {
            var newDb = TestHelpers.Files.DataPath("NewDb.kbdx");

            File.Delete(newDb);

            var target = new KeepassConfigurationSource(new KeepassConfigurationOptions(
                newDb, "a", null, true));


            using (target.OpenDb())
            
            Assert.True(File.Exists(newDb));

            File.Delete(newDb);
        }
        
        [Fact]
        public void OpenDb_New_CreateBaseRoot()
        {
            var dbFile = TestHelpers.Files.DataPath("newDbWithRoot.kbdx");

            var target = new KeepassConfigurationSource(
                new KeepassConfigurationOptions(dbFile, "a", null, true, "AppX"));
        }

        [Fact]
        public void Delete_RemovesFile()
        {
            var dbFile = TestHelpers.Files.DataPath("removeDb");

            File.WriteAllText(dbFile, "a");

            var target = new KeepassConfigurationSource(
                new KeepassConfigurationOptions(dbFile, "a", null, true));

            Assert.True(target.Delete());
            Assert.False(File.Exists(dbFile));

            Assert.False(target.Delete());
        }
    }
}
