using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Mvx.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> CastEachAs<T>(this IEnumerable items)
            where T: class
        {
            foreach (var item in items)
            {
                var other = item as T;
                if (other != null)
                    yield return other;
            }
        }
    }
}
