using System.Text.RegularExpressions;

namespace IsometricShooterWebApp.Utils
{
    public class UriUtils
    {
        public static string ReplaceOrAppendParameters(string url, Dictionary<string, string> prms)
        {
            var uri = new Uri(url);

            UriBuilder builder = new UriBuilder(url);

            var parameters = prms.Select(x => string.Join('=', x.Key, x.Value));

            builder.Query = $"?{string.Join("&", parameters)}";

            if (!string.IsNullOrWhiteSpace(uri.Query))
            {
                var reg = new Regex("[?&]([^&=]+)(=([^&=]+))?");

                var matches = reg.Matches(uri.Query);

                foreach (Match match in matches)
                {
                    var key = match.Groups[1].Value;

                    if (prms.Any(x => key.Equals(x.Key, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    string value = match.Groups[3].Value;

                    if (string.IsNullOrEmpty(value))
                        value = null;

                    string parameter = default;

                    if (value != default)
                        parameter = string.Join('=', key, value);
                    else
                        parameter = key;

                    builder.Query = string.Join('&', builder.Query, parameter);
                }
            }

            return builder.Uri.ToString();
        }
    }
}
