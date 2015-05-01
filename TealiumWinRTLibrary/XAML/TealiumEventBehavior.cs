using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Tealium
{
    /// <summary>
    /// Attached property/behavior that listens for a WinRT event and fires a corresponding Tealium event in response.
    /// </summary>
    public class TealiumEventBehavior
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

        /// <summary>
        /// Removes the registered handler for reporting the event to Tealium.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="evt"></param>
        private static void UnregisterForEvent(DependencyObject d, EventInfo evt)
        {
            Type handlerType = evt.EventHandlerType;

            var dm = typeof(TealiumEventBehavior).GetTypeInfo().GetDeclaredMethod("EventActionHandler");
            var executemethodinfo = dm.CreateDelegate(evt.EventHandlerType, null);

            WindowsRuntimeMarshal.RemoveEventHandler(
                token => evt.RemoveMethod.Invoke(d, new object[] { token }),
                executemethodinfo);
        }

        /// <summary>
        /// Registers a handler for reporting the event to Tealium.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="evt"></param>
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
            object context = args;
            if (sender != null && typeof(DependencyObject).GetTypeInfo().IsAssignableFrom(sender.GetType().GetTypeInfo()))
            {
                TealiumEvent evt = GetEvent((DependencyObject)sender);
                if (evt != null)
                {
                    //evt.DataContext = context;
                    string varName = evt.VariableName;
                    if (string.IsNullOrWhiteSpace(varName))
                        varName = Constants.DEFAULT_CUSTOM_EVENT_NAME;
                    Dictionary<string, object> vars = new Dictionary<string, object>();
                    foreach (var item in evt.Parameters)
                    {
                        item.DataContext = context;
                        string paramName = item.PropertyName;
                        object paramVal = item.GetValue(ParameterValue.PropertyValueProperty);
                        
                        vars.Add(paramName, paramVal);
                    }
                    TealiumTagger.Instance.TrackCustomEvent(varName, vars);
                }
            }
        }

    }

}
