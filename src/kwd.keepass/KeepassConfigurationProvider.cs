using System;
using System.Collections.Generic;
using System.Linq;
using KeePassLib;
using Microsoft.Extensions.Configuration;

namespace kwd.keepass
{
    public class KeepassConfigurationProvider : ConfigurationProvider
    {
        private readonly KeepassConfigurationSource _source;

        public KeepassConfigurationProvider(KeepassConfigurationSource source)
        {
            _source = source;
        }

        public override void Load()
        {
            using (var wrappedDb = _source.OpenDb())
            {
                var root = _source.GetRoot((PwDatabase)wrappedDb);
                
                Data = LoadGroup(root).ToDictionary(k => k.Key, v => v.Value);
            }
        }

        #region Overrides of ConfigurationProvider

        public override bool TryGet(string key, out string value)
        {
            var dataKey = Data.Keys.SingleOrDefault(x => string.Compare(x, key, StringComparison.CurrentCultureIgnoreCase) == 0);

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
            var entryTitle = entry.Strings.Get("Title").ReadString();

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
