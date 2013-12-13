using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImgurPortable.Extensions
{
    public static class DictionaryExtensions
    {
        public static string ToEncodedString(this Dictionary<string, string> postData)
        {
            var pairs = postData.Select(x => string.Format("{0}={1}", x.Key, x.Value.EncodeString()));

            return string.Join("&", pairs);
        }

        private static string EncodeString(this string value)
        {
            const int maxUriLength = 32766;

            var sb = new StringBuilder();
            for (var i = 0; i < value.Length; i += maxUriLength)
            {
                sb.Append(Uri.EscapeDataString(value.Substring(i, Math.Min(maxUriLength, value.Length - i))));
            }

            return sb.ToString();
        }
    }
}
