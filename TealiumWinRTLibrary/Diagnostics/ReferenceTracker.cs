using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
#if WINDOWS_PHONE
using System.Windows.Threading;
using System.Windows;

#endif

namespace Tealium
{
    /// <summary>
    /// This is an internal class for performance testing.
    /// </summary>
    internal class ReferenceTracker
    {
        protected static List<WeakReference> openRefs = new List<WeakReference>();
        protected static int openRefCount = 0;
        protected static DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1d) };


        static ReferenceTracker()
        {
#if DEBUG
#if NETFX_CORE
            Application.Current.Suspending += Current_Suspending;
#else
            Application.Current.Exit += Current_Suspending;
#endif
            StartRefCounter();
#endif
        }

#if NETFX_CORE
        static void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
#else
        static void Current_Suspending(object sender, EventArgs e)
#endif
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
