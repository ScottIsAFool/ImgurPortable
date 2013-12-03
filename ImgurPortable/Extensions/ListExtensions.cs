using System.Collections.Generic;

namespace ImgurPortable.Extensions
{
    internal static class ListExtensions
    {
        internal static string ToCommaSeparated<T>(this List<T> list)
        {
            return string.Join(",", list);
        }
    }
}
