using System;
using System.Net;
using System.Windows;

namespace Windows.UI.Xaml
{
    internal static class XamlExtensions
    {
        /// <summary>
        /// Waits for a control to render its first frame on screen, then executes the specified action.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="a"></param>
        public static void OnFirstFrame(this FrameworkElement that, Action a)
        {
            if (a == null || that == null)
                return;
#if NETFX_CORE
            EventHandler<object> handler = null;
#else
            EventHandler handler = null;
#endif
            RoutedEventHandler unload = null;

            handler = (s, e) =>
            {
                that.LayoutUpdated -= handler;
                that.Unloaded -= unload;

                if (a != null)
                    a.Invoke();
            };
            unload = (s, e) =>
            {
                //clean up to preven leaks if this control is never rendered
                that.LayoutUpdated -= handler;
                that.Unloaded -= unload;
                
            };
            that.LayoutUpdated += handler;
            that.Unloaded += unload;
        }

        /// <summary>
        /// Convenience function to HTML encode a string, abstracted for the different framework versions.
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static string HtmlEncode(this string that)
        {
#if NETFX_CORE
            return WebUtility.HtmlEncode(that);
#else
            return HttpUtility.HtmlEncode(that);
#endif
        }
    }
}
