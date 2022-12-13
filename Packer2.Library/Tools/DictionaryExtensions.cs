using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer2.Library.Tools
{
    public static class DictionaryExtensions
    {
        public static V GetValueOrInitialize<T, V>(this IDictionary<T, V> dict, T key, Func<T, V> createFunc)
        {
            if (dict.TryGetValue(key, out var res))
                return res;
            else
                return dict[key] = createFunc(key);
        }
    }
}
