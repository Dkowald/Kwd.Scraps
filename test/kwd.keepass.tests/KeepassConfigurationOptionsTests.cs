using System;
using System.IO;
using kwd.keepass;
using KeePassLib.Keys;
using Xunit;

namespace kwd_keepass.tests
{
    public class KeepassConfigurationOptionsTests
    {
        [Fact]
        public void Ctor_NoCredentials()
        {
            Action act = () => new KeepassConfigurationOptions("aFile", null, null, true);

            Assert.Throws<KeepassConfigurationOptions.NoCredentialsProvided>(act);
        }

        [Fact]
        public void CreateAccessKey_WhenKeyFileExists()
        {
            var keyFile = TestHelpers.Files.DataPath("existingKey.key");
            const string keyText = "Sample keyfile text";

            File.WriteAllText(keyFile, keyText);

            var target = new KeepassConfigurationOptions
            {
                KeyFile = TestHelpers.Files.GetExistingKeyPath(),
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
