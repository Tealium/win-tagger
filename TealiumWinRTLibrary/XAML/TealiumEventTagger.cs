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
    public class TealiumEventTagger : DependencyObject
    {

        public static TealiumEvent GetEvent(DependencyObject obj)
        {
            return (TealiumEvent)obj.GetValue(EventProperty);
        }

        public static void SetEvent(DependencyObject obj, TealiumEvent value)
        {
            obj.SetValue(EventProperty, value);
        }

        // Using a DependencyProperty as the backing store for Event.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EventProperty =
            DependencyProperty.RegisterAttached("Event", typeof(TealiumEvent), typeof(TealiumEventTagger), new PropertyMetadata(null, OnEventPropertyChanged));

        private static void OnEventPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d != null && e.NewValue != null && !string.IsNullOrEmpty(((TealiumEvent)e.NewValue).EventName))
            {
                var evt = d.GetType().GetRuntimeEvent(((TealiumEvent)e.NewValue).EventName);
                if (evt != null)
                {
                    Type handlerType = evt.EventHandlerType;

                    var dm = typeof(TealiumEventTagger).GetTypeInfo().GetDeclaredMethod("EventActionHandler");
                    var executemethodinfo = dm.CreateDelegate(evt.EventHandlerType, null);
                    
                    WindowsRuntimeMarshal.AddEventHandler(
                        del => (EventRegistrationToken)evt.AddMethod.Invoke(d, new object[] { del }),
                        token => evt.RemoveMethod.Invoke(d, new object[] { token }), executemethodinfo); 

                }
            }
        }


        public static void EventActionHandler(object sender, object args)
        {
            Debug.WriteLine("*********************Event fired!!!!");

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
