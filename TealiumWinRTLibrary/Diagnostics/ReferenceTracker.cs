using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Tealium
{
    public class ReferenceTracker
    {
        protected static List<WeakReference> openRefs = new List<WeakReference>();
        protected static int openRefCount = 0;
        protected static DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1d) };


        static ReferenceTracker()
        {
#if DEBUG
            Application.Current.Suspending += Current_Suspending;
            StartRefCounter();
#endif
        }

        static void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            StopRefCounter();
        }

        public static int OpenReferenceCount
        {
            get;
            private set;
        }


        [Conditional("DEBUG")]
        public static void TrackReference(object o)
        {
            if (o != null)
            {
                var exists = openRefs.Any(w => w.Target == o && w.IsAlive);
                if (!exists)
                {
                    openRefs.Add(new WeakReference(o));
                    //Debug.WriteLine(o);
                    if (!timer.IsEnabled && CalcOpenReferences() > 0)
                        StartRefCounter();
                }

            }
        }

        private static void StopRefCounter()
        {
            timer.Stop();
            timer.Tick -= timer_Tick;

        }

        private static void StartRefCounter()
        {
            if (Debugger.IsAttached)
            {
                timer.Tick += timer_Tick;
                timer.Start();
            }
        }

        static void timer_Tick(object sender, object e)
        {
            var newCount = CalcOpenReferences();
            Debug.WriteLineIf(OpenReferenceCount != newCount, "#Open refs: " + newCount + " @" + DateTime.Now);
            OpenReferenceCount = newCount;
            if (OpenReferenceCount == 0)
                StopRefCounter();
        }

        protected static int CalcOpenReferences()
        {
            openRefCount = 0;
            List<WeakReference> alive = new List<WeakReference>();
            for (int i = 0; i < openRefs.Count; i++)
            {
                if (openRefs[i].IsAlive)
                {
                    openRefCount++;
                    alive.Add(openRefs[i]);
                }
            }

            if (openRefCount % 4 == 0)
                GC.Collect();

            return openRefCount;
        }



    }
}
