using System;
using System.IO;
using System.Linq;
using kwd.keepass;
using kwd_keepass.tests.TestHelpers;
using KeePassLib.Keys;
using Microsoft.Extensions.Logging;
using Xunit;

namespace kwd_keepass.tests
{
    public class KeepassConfigurationOptionsTests
    {
        [Fact]
        public void CreateAccessKey_LogCreation()
        {
            var keyPath = Files.DataPath("newKey.key");

            File.Delete(keyPath);

            var memLog = new TestLoggerProvider();

            using (var logFactory = new LoggerFactory())
            {
                logFactory.AddProvider(memLog);
                
                var target = new KeepassConfigurationOptions
                {
                    CreateIfMissing = true,
                    KeyFile = keyPath,
                    Logger = logFactory
                };

                target.CreateAccessKey();

                var data = memLog.Entries
                    .FirstOrDefault(x => x.Name == typeof(KeepassConfigurationOptions).FullName);

                Assert.NotNull(data);
            }
        }

        [Fact]
        public void Ctor_NoCredentials()
        {
            Action act = () => new KeepassConfigurationOptions("aFile", null, null, true);

            Assert.Throws<KeepassConfigurationOptions.NoCredentialsProvided>(act);
        }

        [Fact]
        public void CreateAccessKey_WhenKeyFileExists()
        {
            var keyFile = Files.DataPath("existingKey.key");
            const string keyText = "Sample keyfile text";

            File.WriteAllText(keyFile, keyText);

            var target = new KeepassConfigurationOptions
            {
                KeyFile = Files.GetSampleMasterKeyPath(),
                CreateIfMissing = true
            };

            var result = target.CreateAccessKey();

            var keyPath = (result.GetUserKey(typeof(KcpKeyFile))as KcpKeyFile)?.Path;
            Assert.NotNull(keyPath);

            var loadedKeyTxt = File.ReadAllText(keyPath);
            
            Assert.Equal(keyText, loadedKeyTxt);
        }

        [Fact]
        public void CreateAccessKey_WhenNoFileExists()
        {
            var keyFile = TestHelpers.Files.DataPath("autoCreate.key");
            
            File.Delete(keyFile);

            //and not create missing.
             var target =  new KeepassConfigurationOptions
             {
                 CreateIfMissing = false,
                 KeyFile = keyFile
             };

            Action actNoKeyFile = () => target.CreateAccessKey();
            Assert.Throws<FileNotFoundException>(actNoKeyFile);

            Assert.False(File.Exists(keyFile));

            //and create missing is true
            target.CreateIfMissing = true;

            Assert.NotNull(target.CreateAccessKey());
            Assert.True(File.Exists(keyFile));
        }

        [Fact]
        public void CreateAccessKey_NoAutoCreate()
        {
            var target = new KeepassConfigurationOptions
            {
                CreateIfMissing = false,
                KeyFile = TestHelpers.Files.DataPath("NoCreate.key")
            };

            Action act = () => target.CreateAccessKey();

            Assert.ThrowsAny<Exception>(act);
        }

        [Fact]
        public void DeleteAccessKey_RemovesFile()
        {
            var file = TestHelpers.Files.DataPath("testkeyX.key");

            var target = new KeepassConfigurationOptions
            {
                KeyFile = file
            };

            Assert.False(target.DeleteAccessKey());

            File.WriteAllText(file, "key_file_content");

            Assert.True(target.DeleteAccessKey());

            Assert.False(File.Exists(file));
        }
    }
}
