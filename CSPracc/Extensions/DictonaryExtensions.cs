using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Extensions
{
    internal static class DictonaryExtensions
    {
        internal static void SetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, TValue value)
        {
            if (@this == null) { return; }
            if(key == null) { return; }
            if (value == null) { return; }
            if(@this.ContainsKey(key))
            {
                @this[key] = value;
            }
            else
            {
                @this.Add(key, value);
            }
        }
    }
}
