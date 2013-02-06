using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Tealium
{
    /// <summary>
    /// Attaches to the current control and listens to the specified event to report back to Tealium.
    /// </summary>
    [ContentProperty(Name = "Parameters")]
    public class TealiumEvent : DependencyObject
    {

        public TealiumEvent()
        {
            this.Parameters = new Collection<ParameterValue>();
            ReferenceTracker.TrackReference(this);
        }

        /// <summary>
        /// The name of the event to listen for, belonging to the control this is attached to.
        /// </summary>
        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EventName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName", typeof(string), typeof(TealiumEvent), new PropertyMetadata(null, OnEventNameChanged));

        private static void OnEventNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }


        /// <summary>
        /// The name of the event/variable to report to Tealium when the event is triggered (i.e. "link").
        /// </summary>
        public string VariableName
        {
            get { return (string)GetValue(VariableNameProperty); }
            set { SetValue(VariableNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VariableName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VariableNameProperty =
            DependencyProperty.Register("VariableName", typeof(string), typeof(TealiumEvent), new PropertyMetadata(string.Empty));


        /// <summary>
        /// An optional collection of parameters to include on the tracking call.  These values can be bound with values in the control's DataContext.
        /// </summary>
        public ICollection<ParameterValue> Parameters
        {
            get { return (ICollection<ParameterValue>)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Parameters.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParametersProperty =
            DependencyProperty.Register("Parameters", typeof(ICollection<ParameterValue>), typeof(TealiumEvent), new PropertyMetadata(null));


    }

}
