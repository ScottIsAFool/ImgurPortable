using System;

namespace ImgurPortable.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToLower(this object item)

        {
            return item.ToString().ToLower();
        }
    }
}
