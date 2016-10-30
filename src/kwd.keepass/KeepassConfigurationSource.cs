using System;
using System.IO;
using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace kwd.keepass
{
    public class KeepassConfigurationSource : FileConfigurationSource
    {
        private readonly KeepassConfigurationOptions _options;
        private readonly ILoggerFactory _logFactory;
        private readonly ILogger _log;

        public KeepassConfigurationSource(KeepassConfigurationOptions options, ILoggerFactory logFactory = null)
        {
            if (options == null) { throw new ArgumentNullException(nameof(options)); }

            _log = logFactory?.CreateLogger(GetType().FullName) ?? new NullLogger();

            _options = options;
            _logFactory = logFactory;
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            //todo: Check options are good, and db access is ok.

            return new KeepassConfigurationProvider(this);
        }

        public PwGroup GetRoot(PwDatabase db)
        {
            if (String.IsNullOrEmpty(_options.RootSection) || _options.RootSection == ConfigurationPath.KeyDelimiter)
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
            
            db.Open(IOConnectionInfo.FromPath(_options.FileName), key, new NullStatusLogger());

            return new KeePassDbSession(db);
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

            File.Delete(_options.FileName);

            return true;
        }

        private void CreateNew()
        {
            var newDb = new PwDatabase();

            var key = _options.CreateAccessKey();
            var src = IOConnectionInfo.FromPath(_options.FileName);
            newDb.New(src, key);

            newDb.Save(new NullStatusLogger());
            newDb.Close();
        }
    }
}
