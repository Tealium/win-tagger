using System;
#if NETFX_CORE
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tealium.Utility
{
    public class RequestQueue
    {
        private class RequestQueueEntry
        {
            public DateTimeOffset QueuedDate { get; set; }
            public string Content { get; set; }
        }

        private int maxSize;
        private int maxAgeInDays;

#if NETFX_CORE
        ConcurrentQueue<RequestQueueEntry> q;
#else
        Queue<RequestQueueEntry> q;
#endif
        public RequestQueue(int maxSize = 0, int maxAgeInDays = -1)
        {
            this.maxSize = maxSize;
            this.maxAgeInDays = maxAgeInDays;

#if NETFX_CORE
            q = new ConcurrentQueue<RequestQueueEntry>();
#else
            q = new Queue<RequestQueueEntry>();
#endif

        }

        public bool IsEmpty
        {
            get
            {
                PurgeExpired();
#if NETFX_CORE
                return q == null || q.IsEmpty;
#else
                return q == null || q.Count == 0;
#endif

            }
        }

        public int Size
        {
            get
            {
                PurgeExpired();
                return q == null ? 0 : q.Count;
            }
        }

        public int MaxSize
        {
            get
            {
                return this.maxSize;
            }
        }

        public int MaxAgeInDays
        {
            get
            {
                return this.maxAgeInDays;
            }
        }

        public void ResetQueueSize(int newMaxSize = 0, int newMaxAgeInDays = -1)
        {
            this.maxSize = newMaxSize;
            this.maxAgeInDays = newMaxAgeInDays;

            PurgeExpired();
            PurgeExcess();
        }

        public bool TryDequeue(out string value)
        {
            PurgeExpired();

#if NETFX_CORE
            RequestQueueEntry v;
            if (q.TryDequeue(out v))
            {
                value = v.Content;
                return true;
            }
            value = null;
            return false;
#else
            if (q.Count == 0)
            {
                value = null;
                return false;
            }
            value = q.Dequeue().Content;
            return true;
#endif

        }

        public void Enqueue(string value)
        {
            q.Enqueue(new RequestQueueEntry() { QueuedDate = DateTimeOffset.Now, Content = value });
            //if we limit the size of the queue, pop off the oldest items to keep under the max size
            if (maxSize > 1 && q.Count > maxSize)
            {
                PurgeExpired();
                PurgeExcess();
            }
        }

        public List<string> ToList()
        {
            PurgeExpired();
            return q.Select(v => v.Content).ToList();
        }

        private void PurgeExpired()
        {
            //if max age was set, we need to clear old/expired records to get an accurate count
            if (this.maxAgeInDays < 0)
                return; //negative values ignored, nothing to purge

            RequestQueueEntry t;
            bool purge = true;
            while (purge)
            {
#if NETFX_CORE
                q.TryPeek(out t);
#else
                t = q.Peek();
#endif
                if (t.QueuedDate.AddDays(this.maxAgeInDays) < DateTimeOffset.Now)
#if NETFX_CORE
                    q.TryDequeue(out t);
#else
                    t = q.Dequeue();
#endif
                else
                    purge = false;
            }
        }

        private void PurgeExcess()
        {
            if (q.Count < 0)
                return;

            for (int i = 0; i < q.Count - maxSize; i++)
            {
#if NETFX_CORE
                RequestQueueEntry throwaway;
                q.TryDequeue(out throwaway);
#else
                    q.Dequeue();
#endif
            }
        }



    }
}
