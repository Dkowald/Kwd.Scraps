using System;
using System.Collections.Generic;
using System.Linq;
using KeePassLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Internal;

namespace kwd.keepass
{
    public class KeepassConfigurationProvider : ConfigurationProvider
    {
        private readonly KeepassConfigurationSource _source;
        private readonly KeepassConfigurationOptions _options;
        private readonly ISystemClock _clock;
        private readonly ILogger _log;

        public KeepassConfigurationProvider(KeepassConfigurationSource source, KeepassConfigurationOptions options, ISystemClock clock)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source));}
            if (options == null) { throw new ArgumentNullException(nameof(options));}
            if (clock == null) { throw new ArgumentNullException(nameof(clock));}

            _source = source;
            _options = options;
            _clock = clock;
            
            _log = options.Logger?.CreateLogger(GetType().FullName);
        }

        public override void Load()
        {
            using (var wrappedDb = _source.OpenDb())
            {
                var root = _source.GetRoot((PwDatabase)wrappedDb);
                
                _log?.LogInformation("Loading secrets starting at root: '{rootName}'", root.Name);

                Data = LoadGroup(root).ToDictionary(k => k.Key, v => v.Value);
            }
        }

        #region Overrides of ConfigurationProvider

        public override bool TryGet(string key, out string value)
        {
            var dataKey = Data.Keys.SingleOrDefault(x => string.Compare(x, key, StringComparison.CurrentCultureIgnoreCase) == 0);

            if (dataKey == null) {
                value = null;
                return false;
            }

            return base.TryGet(dataKey, out value);
        }

        #endregion

        public List<KeyValuePair<string,string>> LoadGroup(PwGroup group, string prefix = null)
        {
            var result = new List<KeyValuePair<string, string>>();

            //add group entries
            foreach (var item in group.Entries)
            {
                result.AddRange(LoadEntryKeys(item, prefix));
            }
            
            //add child groups.
            foreach (var item in group.Groups)
            {
                var groupPrefix = ConfigurationPathHelper.CombineNonBlank(new[] { prefix, item.Name });
                result.AddRange(LoadGroup(item, $"{groupPrefix}"));
            }

            return result;
        }

        public List<KeyValuePair<string,string>> LoadEntryKeys(PwEntry entry, string prefix)
        {
            if(entry == null) { throw new ArgumentNullException(nameof(entry));}
            
            var entryTitle = entry.Strings.Get("Title")?.ReadString();

            if (entryTitle == null)
            {
                _log?.LogWarning("Entry at path '{path}' has no title, skipping entry load", prefix);
                return new List<KeyValuePair<string, string>>();
            }

            if (entry.Expires)
            {
                if (_options.ErrorIfExpiredEntry && entry.ExpiryTime < _clock.UtcNow.DateTime)
                {
                    _log?.LogError("The entry {path}:{title} has expired, cannot load it.", prefix, entryTitle);
                    return new List<KeyValuePair<string, string>>();
                }

                if (_options.WarnIfExpireWithinDays > 0 && (entry.ExpiryTime - _clock.UtcNow.Date).Days <= _options.WarnIfExpireWithinDays)
                {
                    _log?.LogWarning("The entry {path}:{title} will expire in less than {expireDays} days", 
                        prefix, 
                        entryTitle, 
                        _options.WarnIfExpireWithinDays);
                }
            }
            
            var keyPrefix = ConfigurationPathHelper.CombineNonBlank(prefix, entryTitle);

            var data = entry.Strings.Where(x => x.Key != "Title")
                .Where(x => !x.Value.IsEmpty)
                .Select(x => new KeyValuePair<string, string>(
                    ConfigurationPathHelper.CombineNonBlank(keyPrefix, x.Key),
                    x.Value.ReadString()));

            return data.ToList();
        }
    }
}
