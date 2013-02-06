using System;

namespace Tealium
{

    /// <summary>
    /// Associates the navigation parameter for the page (the second parameter in the Frame.Navigate call) with a tracking parameter.
    /// Optionally can specify a property or field on the navigation parameter to use as the tracking value.
    /// If you are interested in just providing a name/value parameter on the page, use TrackPropertyAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TrackNavigationParameterAttribute : TrackBaseAttribute
    {
        public TrackNavigationParameterAttribute(string variableName)
        {
            this.VariableName = variableName;
        }
        public TrackNavigationParameterAttribute(string variableName, string parameterName)
        {
            this.ParameterName = parameterName;
            this.VariableName = variableName;
        }

        /// <summary>
        /// Optional parameter to use if your navigation parameter contains multiple properties.  For example, if you
        /// are interested in property "Bar" on the class "Foo", ParameterName should be "Bar".
        /// </summary>
        public string ParameterName { get; set; }
        
        /// <summary>
        /// The name of the Tealium tracking variable to report this property as.
        /// </summary>
        public string VariableName { get; set; }
    }
}
