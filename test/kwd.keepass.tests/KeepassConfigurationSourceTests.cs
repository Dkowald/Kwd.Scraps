using System.IO;
using System.Linq;
using kwd.keepass;
using Microsoft.Extensions.Logging;
using Xunit;

namespace kwd_keepass.tests
{
    public class KeepassConfigurationSourceTests
    {
        [Fact]
        public void OpenDb_Logs()
        {
            var options = new KeepassConfigurationOptions
            {
                CreateIfMissing = true,
                KeyFile = TestHelpers.Files.DataPath("LogingTest.key"),
                FileName = TestHelpers.Files.DataPath($"LogingTest.{KeepassConfigurationOptions.FileExtension}")
            };

            if(File.Exists(options.FileName)) { File.Delete(options.FileName);}
            if(File.Exists(options.FileName)) { File.Delete(options.KeyFile);}

            var memLog = new TestHelpers.TestLoggerProvider();

            using (var logger = new LoggerFactory())
            {
                logger.AddProvider(memLog);

                options.Logger = logger;

                var target = new KeepassConfigurationSource(options);

                using (target.OpenDb()) { }

                var logs = memLog
                    .Entries.Where(x => x.Name == typeof(KeepassConfigurationSource).FullName)
                    .ToList();

                Assert.NotNull(logs.SingleOrDefault(x => x.logLevel == LogLevel.Information));
            }
        }

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

            new KeepassConfigurationSource(
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
