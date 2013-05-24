using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.UI.Core;
#else
using System.Windows;
#endif

namespace Tealium.Utility
{
    internal class ThreadHelper
    {
        public static void OnUiThread(Action a)
        {
#if NETFX_CORE
            CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (a != null)
                    a.Invoke();
            });
#else
            Application.Current.RootVisual.Dispatcher.BeginInvoke(a);
#endif

        }

        public static void OnBackgroundThread(Action a)
        {

        }
    }
}
