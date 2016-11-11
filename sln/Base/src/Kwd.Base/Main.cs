using System.IO;
using kwd.keepass;
using Microsoft.Extensions.Configuration;

namespace Kwd.Base
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var kpConfig = new KeepassConfigurationOptions
            {
                CreateIfMissing = true,
                MasterPassword = "a",
                FileName = Path.Combine(Directory.GetCurrentDirectory(), "secrets." + KeepassConfigurationOptions.FileExtension)
            };

            var config = new ConfigurationBuilder()
                .Add(new KeepassConfigurationSource(kpConfig))
                .Build();
        }
    }
}
