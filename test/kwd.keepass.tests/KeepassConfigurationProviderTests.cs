using System;
using System.IO;
using System.Linq;
using kwd.keepass;
using KeePassLib;
using KeePassLib.Security;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Xunit;

namespace kwd_keepass.tests
{
    public class KeepassConfigurationProviderTests
    {
        [Fact]
        public void Load_ExpireSoonEntry_LogsWarning()
        {
            var testDb = TestHelpers.Files.DataPath("expiringEntry");
            File.Delete(testDb);

            var memLog = new TestHelpers.TestLoggerProvider();
            var logger = new LoggerFactory();
            logger.AddProvider(memLog);

            var source = new KeepassConfigurationSource(new KeepassConfigurationOptions
            {
                FileName = testDb,
                CreateIfMissing = true,
                MasterPassword = "a",
                WarnIfExpireWithinDays = 1,
                Logger = logger
            }, new SystemClock());

            const string entryTitle = "SampleExpired";

            using (var db = source.OpenDb())
            {
                var tomorrow = DateTime.UtcNow.AddDays(1).Date;

                var expireSoonEntry = new PwEntry(true, true)
                {
                    Expires = true,
                    ExpiryTime = tomorrow
                };
                expireSoonEntry.Strings.Set("Title", new ProtectedString(false, entryTitle));

                db.Db.RootGroup.Entries.Add(expireSoonEntry);
            }

            var target = source.Build();

            target.Load();

            var warning = memLog.Entries.SingleOrDefault(x => x.logLevel == LogLevel.Warning && x.Msg().Contains(entryTitle));
            Assert.NotNull(warning);
        }

        [Fact]
        public void Load_ExpiredEntry_NotLoaded()
        {
            var memLog = new TestHelpers.TestLoggerProvider();

            bool canGet;
            string expiredKey;
            using (var logger = new LoggerFactory())
            {
                logger.AddProvider(memLog);

                var target = new KeepassConfigurationSource(new KeepassConfigurationOptions
                {
                    CreateIfMissing = false,
                    MasterPassword = "a",
                    FileName = TestHelpers.Files.TestDb(),
                    Logger = logger,
                    
                    ErrorIfExpiredEntry = true
                }).Build();

                target.Load();
                
                canGet = target.TryGet("App3:Expired:Password", out expiredKey);
            }

            var errorEntry = memLog.Entries.SingleOrDefault(x => x.logLevel == LogLevel.Error);

            Assert.False(canGet);
            Assert.Null(expiredKey);
            
            Assert.NotNull(errorEntry);
        }
        
        [Fact]
        public void Load_SubGroupRoot_LimitsLoadedData()
        {
            var source = new KeepassConfigurationSource(new KeepassConfigurationOptions
            {
                CreateIfMissing = false,
                MasterPassword = "a",
                KeyFile = TestHelpers.Files.TestDb(),

                RootSection = "App1"
            });

            var target = source.Build();
           
            target.Load();

            string username;

            Assert.True(target.TryGet("DbStore:Username", out username));

            Assert.False(target.TryGet("App1:DbStore:Username", out username));
        }
    }
}
