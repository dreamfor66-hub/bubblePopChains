using System;
using System.Collections.Generic;

namespace SnowLib.Extensions
{
    public static class EnumerableExtension
    {
        public static T MinBy<T>(this IEnumerable<T> enumerable, Func<T,float> valueFunc)
        {
            var minValue = float.MaxValue;
            T selected = default;
            foreach (var e in enumerable)
            {
                var value = valueFunc(e);
                if (value < minValue)
                {
                    selected = e;
                    minValue = value;
                }
            }

            return selected;
        }
    }
}
