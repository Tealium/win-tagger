using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Tealium
{
    /// <summary>
    /// Defines a name/value pair for a Tealium parameter that is defined in XAML.  Either property can be data bound.
    /// </summary>
    public class ParameterValue : DependencyObject
    {

        /// <summary>
        /// The name of the parameter to include in the Tealium tracking call.
        /// </summary>
        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(ParameterValue), new PropertyMetadata(null));



        /// <summary>
        /// The value of the parameter to include in the Tealium tracking call.
        /// </summary>
        public object PropertyValue
        {
            get { return (object)GetValue(PropertyValueProperty); }
            set { SetValue(PropertyValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyValueProperty =
            DependencyProperty.Register("PropertyValue", typeof(object), typeof(ParameterValue), new PropertyMetadata(null));


    }

}
