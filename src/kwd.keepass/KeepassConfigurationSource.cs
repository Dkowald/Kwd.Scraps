using System;
using System.IO;
using KeePassLib;
using KeePassLib.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace kwd.keepass
{
    public class KeepassConfigurationSource : FileConfigurationSource
    {
        private readonly KeepassConfigurationOptions _options;
        private readonly ILogger _log;

        public KeepassConfigurationSource(KeepassConfigurationOptions options)
        {
            if (options == null) { throw new ArgumentNullException(nameof(options)); }

            _log = options.Logger?.CreateLogger(GetType().FullName);

            _options = options;
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new KeepassConfigurationProvider(this, _options);
        }

        public KeepassConfigurationProvider Build() => new KeepassConfigurationProvider(this, _options);

        public PwGroup GetRoot(PwDatabase db)
        {
            if (string.IsNullOrEmpty(_options.RootSection) || _options.RootSection == ConfigurationPath.KeyDelimiter)
            { return db.RootGroup; }

            var result = db.RootGroup.FindCreateSubTree(_options.RootSection, new[] { ConfigurationPath.KeyDelimiter }, _options.CreateIfMissing);
            if (result == null) { throw new Exception($"Cannot locate root group: {_options.RootSection}"); }

            return result;
        }

        public KeePassDbSession OpenDb()
        {
            var db = new PwDatabase();

            var key = _options.CreateAccessKey();

            if (!File.Exists(_options.FileName) && _options.CreateIfMissing)
            {
                CreateNew();
            }
               
            db.Open(IOConnectionInfo.FromPath(_options.FileName), key, KeepassStatusLogger.SelectLoggerStrategy(_log));

            return new KeePassDbSession(KeepassStatusLogger.SelectLoggerStrategy<KeePassDbSession>(_options), db);
        }
        
        /// <summary>
        /// Delete secrets Db, if exists.
        /// </summary>
        /// <returns>True if secrets db was removed.</returns>
        /// <remarks>
        /// Does NOT delete associated key file.
        /// </remarks>
        public bool Delete()
        {
            if (!File.Exists(_options.FileName)) { return false; }

            _log?.LogInformation($"Deleting database: {_options.FileName}");

            File.Delete(_options.FileName);

            return true;
        }

        private void CreateNew()
        {
            var newDb = new PwDatabase();

            var key = _options.CreateAccessKey();
            var src = IOConnectionInfo.FromPath(_options.FileName);
            newDb.New(src, key);

            newDb.Save(KeepassStatusLogger.SelectLoggerStrategy(_log));
            newDb.Close();

            _log?.LogWarning("Creating database : '{FileName}', be sure to store it safely", _options.FileName);
        }
    }
}
