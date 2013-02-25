using System;
#if NETFX_CORE
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium.Utility
{
    public class RequestQueue
    {
#if NETFX_CORE
        ConcurrentQueue<string> q;
#else
        Queue<string> q;
#endif
        public RequestQueue()
        {
#if NETFX_CORE
            q = new ConcurrentQueue<string>();
#else
            q = new Queue<string>();
#endif

        }

        public bool IsEmpty
        {
            get
            {
#if NETFX_CORE
                return q == null || q.IsEmpty;
#else
                return q == null || q.Count == 0;
#endif

            }
        }

        public bool TryDequeue(out string value)
        {
#if NETFX_CORE
            return q.TryDequeue(out value);
#else
            if (q.Count == 0)
            {
                value = null;
                return false;
            }
            value = q.Dequeue();
            return true;
#endif

        }

        public void Enqueue(string value)
        {
            q.Enqueue(value);
        }

        public List<string> ToList()
        {
            return q.ToList();
        }
    }
}
