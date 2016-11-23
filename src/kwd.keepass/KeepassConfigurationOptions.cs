using System;
using System.IO;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace kwd.keepass
{
    /// <summary>
    /// Provides access to kee pass db for configuration data.
    /// </summary>
    /// <remarks>
    /// todo: redesign; behavior needs logger.
    /// </remarks>
    public class KeepassConfigurationOptions
    {
        public const string FileExtension = "kdbx";
        
        public KeepassConfigurationOptions()
        {
            IgnoreCaseForKeys = true;
            CreateIfMissing = false;
            RootSection = ConfigurationPath.KeyDelimiter;
        }

        public KeepassConfigurationOptions(string fileName, string masterPassword, string keyFile, bool createIfMissing = false, string rootGroup = ":")
            :this()
        {
            if (string.IsNullOrWhiteSpace(fileName)) { throw new ArgumentNullException(nameof(fileName));}

            if (string.IsNullOrWhiteSpace(masterPassword) &&
                string.IsNullOrWhiteSpace(keyFile))
            {
                throw new NoCredentialsProvided();
            }

            FileName = fileName;

            MasterPassword = masterPassword;
            KeyFile = keyFile;

            RootSection = rootGroup;
            CreateIfMissing = createIfMissing;
        }

        public KeepassConfigurationOptions(KeepassConfigurationOptions rhs)
            :this()
        {
            if (rhs == null) { throw new ArgumentNullException(nameof(rhs)); }

            FileName = rhs.FileName;
            MasterPassword = rhs.MasterPassword;
            KeyFile = rhs.KeyFile;
            RootSection = rhs.RootSection;

            CreateIfMissing = rhs.CreateIfMissing;
        }

        /// <summary>
        /// FileName path for kee pass db.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// (Optional) Master password for kee pass db.
        /// </summary>
        public string MasterPassword { get; set; }

        /// <summary>
        /// Filename path for Db key file.
        /// </summary>
        public string KeyFile { get; set; }

        /// <summary>
        /// Root password group in db, defaults to All (':')
        /// </summary>
        /// <remarks>
        /// Describe the root group in the same manner as config sections, 
        /// use group name(s) seperated via ':'
        /// </remarks>
        public string RootSection { get; set; }

        /// <summary>
        /// When true, creates db if missing.
        /// </summary>
        /// <remarks>
        /// Only use in Development, production should NOT create db.
        /// </remarks>
        public bool CreateIfMissing { get; set; }

        public bool IgnoreCaseForKeys { get; set; }

        public ILoggerFactory Logger { get; set; }

        /// <summary>
        /// When true, attempts to laod expired entries fail,
        /// recording a log entry to report the problem.
        /// </summary>
        public bool ErrorIfExpiredEntry { get; set; }

        /// <summary>
        /// Log a warning on the first attempt (since inital load) to read an entry
        /// that is scheduled to expire within X days.
        /// </summary>
        public int WarnIfExpireWithinDays { get; set; }
        
        /// <summary>
        /// Create an access key for the Db
        /// </summary>
        public CompositeKey CreateAccessKey()
        {
            var key = new CompositeKey();

            if (MasterPassword != null)
            {
                key.AddUserKey(new KcpPassword(MasterPassword));
            }

            if (CreateIfMissing)
            {
                CreateKeyFileIfMissing();
            }

            if (KeyFile != null)
            {
                key.AddUserKey(new KcpKeyFile(IOConnectionInfo.FromPath(KeyFile)));
            }

            return key;
        }

        /// <summary>
        /// Delete the key file if it exists.
        /// </summary>
        /// <returns>True if key file was removed</returns>
        public bool DeleteAccessKey()
        {
            if (!File.Exists(KeyFile)) { return false; }
            
            Logger?.CreateLogger(GetType().FullName)
                ?.LogInformation($"Deleting key file : '{KeyFile}'");

            File.Delete(KeyFile);

            return true;
        }
        
        public class NoCredentialsProvided : Exception
        {
            public NoCredentialsProvided()
                : base("Credentials are required to open or create a keepass db") { }
        }

        private void CreateKeyFileIfMissing()
        {
            if (KeyFile == null || File.Exists(KeyFile)) return;

            var entropy = new byte[1024];
            new Random().NextBytes(entropy);

            var log = Logger?.CreateLogger(GetType().FullName);
            log?.LogWarning("Generating keyfile: '{KeyFile}'", KeyFile);
            log?.LogInformation("The generated key file should be kept safe");

            KcpKeyFile.Create(KeyFile, entropy);
        }
    }
}
