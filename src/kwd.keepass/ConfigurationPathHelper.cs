using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace kwd.keepass
{
    public class ConfigurationPathHelper
    {
        /// <summary>
		/// Create config path, without adding blank entries.
		/// </summary>
		public static string CombineNonBlank(params string[] pathSegments)
        {
            return ConfigurationPath.Combine(
                pathSegments
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .Select(x => x)
                    .ToArray());
        }
    }
}
