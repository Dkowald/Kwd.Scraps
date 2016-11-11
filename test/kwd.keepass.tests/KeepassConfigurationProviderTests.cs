using kwd.keepass;
using Xunit;

namespace kwd_keepass.tests
{
    public class KeepassConfigurationProviderTests
    {
        [Fact]
        public void Load_FindsItemsInChildGroups()
        {
            var options = new KeepassConfigurationOptions
            {
                CreateIfMissing = false,
                MasterPassword = "a",
                FileName = TestHelpers.Files.TestDb(),
            };

            var source = new KeepassConfigurationSource(options); 
            
            var target = new KeepassConfigurationProvider(source,  options);

            target.Load();

            string username;
            Assert.True(target.TryGet("App1:DbStore:UserName", out username));

            Assert.Equal("Admin", username);
        }

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
