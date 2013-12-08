using System.Collections.Generic;
using System.Linq;

namespace ImgurPortable.Extensions
{
    internal static class ListExtensions
    {
        internal static string ToCommaSeparated<T>(this IEnumerable<T> list)
        {
            if (list == null || !list.Any())
            {
                return string.Empty;
            }

            return string.Join(",", list);
        }
    }
}
