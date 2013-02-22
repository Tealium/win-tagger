using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using System.Collections.Concurrent;

#endif

namespace Tealium.Utility
{
    public class VariableCollection
    {
#if NETFX_CORE
        ConcurrentDictionary<string, object> variables;

#endif

        public VariableCollection()
        {
#if NETFX_CORE
            variables = new ConcurrentDictionary<string, object>();
#endif
        }

        public void Add(string key, object value)
        {
#if NETFX_CORE
            variables.TryAdd(key, value);
#endif

        }

        public object this[string key]
        {
            get
            {
#if NETFX_CORE
                object val = null;
                variables.TryGetValue(key, out val);

                return val;
#endif

            }
            set
            {
                variables[key] = value;
            }
        }
    }
}
