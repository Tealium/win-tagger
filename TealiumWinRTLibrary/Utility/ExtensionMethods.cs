using System;

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

            EventHandler<object> handler = null;
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


    }
}
