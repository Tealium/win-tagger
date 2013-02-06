using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Markup;

namespace Tealium
{
    public class TealiumEventBehavior : DependencyObject
    {
        /// <summary>
        /// Gets the attached property for the registered Tealium event.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TealiumEvent GetEvent(DependencyObject obj)
        {
            return (TealiumEvent)obj.GetValue(EventProperty);
        }

        /// <summary>
        /// Sets the attached property for the registered Tealium event.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetEvent(DependencyObject obj, TealiumEvent value)
        {
            obj.SetValue(EventProperty, value);
        }

        // Using a DependencyProperty as the backing store for Event.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EventProperty =
            DependencyProperty.RegisterAttached("Event", typeof(TealiumEvent), typeof(TealiumEventBehavior), new PropertyMetadata(null, OnEventPropertyChanged));

        /// <summary>
        /// Handler for the registered event property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnEventPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d != null && e.NewValue != null && !string.IsNullOrEmpty(((TealiumEvent)e.NewValue).EventName))
            {
                var evt = d.GetType().GetRuntimeEvent(((TealiumEvent)e.NewValue).EventName);
                if (evt != null)
                {
                    RegisterForEvent(d, evt);

                }
            }
            if (d != null && e.OldValue != null && !string.IsNullOrEmpty(((TealiumEvent)e.OldValue).EventName))
            {
                var oldEvt = d.GetType().GetRuntimeEvent(((TealiumEvent)e.OldValue).EventName);
                if (oldEvt != null)
                {
                    UnregisterForEvent(d, oldEvt);
                }
            }
        }

        private static void UnregisterForEvent(DependencyObject d, EventInfo evt)
        {
            Type handlerType = evt.EventHandlerType;

            var dm = typeof(TealiumEventBehavior).GetTypeInfo().GetDeclaredMethod("EventActionHandler");
            var executemethodinfo = dm.CreateDelegate(evt.EventHandlerType, null);

            WindowsRuntimeMarshal.RemoveEventHandler(
                token => evt.RemoveMethod.Invoke(d, new object[] { token }),
                executemethodinfo);
        }

        private static void RegisterForEvent(DependencyObject d, EventInfo evt)
        {
            ReferenceTracker.TrackReference(d);
            Type handlerType = evt.EventHandlerType;

            var dm = typeof(TealiumEventBehavior).GetTypeInfo().GetDeclaredMethod("EventActionHandler");
            var executemethodinfo = dm.CreateDelegate(evt.EventHandlerType, null);

            WindowsRuntimeMarshal.AddEventHandler(
                del => (EventRegistrationToken)evt.AddMethod.Invoke(d, new object[] { del }),
                token => evt.RemoveMethod.Invoke(d, new object[] { token }), executemethodinfo);
        }


        /// <summary>
        /// Handles the registered event and fires the corresponding Tealium event variable.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal static void EventActionHandler(object sender, object args)
        {
            
            if (sender != null && typeof(DependencyObject).GetTypeInfo().IsAssignableFrom(sender.GetType().GetTypeInfo()))
            {
                TealiumEvent evt = GetEvent((DependencyObject)sender);
                if (evt != null)
                {
                    string varName = evt.VariableName;
                    if (string.IsNullOrWhiteSpace(varName))
                        varName = Constants.DEFAULT_CUSTOM_EVENT_NAME;
                    TealiumTagger.Instance.TrackCustomEvent(varName, null);
                }
            }
        }

    }

}
