using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium
{
    /// <summary>
    /// Associates a name/value pair for any reported events on this page.
    /// </summary>
    public class TrackPropertyAttribute : TrackBaseAttribute
    {
        public TrackPropertyAttribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Automatically wires up a 'view' metric for the page when it is navigated to.
    /// </summary>
    public class TrackPageViewAttribute : TrackBaseAttribute
    {
        public TrackPageViewAttribute(string pageName)
        {
            this.Value = pageName;
        }
        public string Value { get; set; }
    }

    /// <summary>
    /// Associates the navigation parameter for the page with a tracking parameter.  
    /// Optionally can specify a property or field on the navigation parameter to use as the tracking value.
    /// </summary>
    public class TrackParameterAttribute : TrackBaseAttribute
    {
        public TrackParameterAttribute(string variableName)
        {
            this.VariableName = variableName;
        }
        public TrackParameterAttribute(string variableName, string parameterName)
        {
            this.ParameterName = parameterName;
            this.VariableName = variableName;
        }

        public string ParameterName { get; set; }
        public string VariableName { get; set; }
    }

    public abstract class TrackBaseAttribute : Attribute
    {

    }

}
