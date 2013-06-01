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
        /// <summary>
        /// Associates the navigation parameter for the page with a Tealium tracking parameter.
        /// </summary>
        /// <param name="variableName">The name of the variable being reported to Tealium.  The entire navigation parameter will be sent as the value.</param>
        public TrackNavigationParameterAttribute(string variableName)
        {
            this.VariableName = variableName;
        }

        /// <summary>
        /// Associates a navigation parameter for the page with a Tealium tracking parameter.
        /// </summary>
        /// <param name="variableName">The name of the variable being reported to Tealium.</param>
        /// <param name="parameterName">The navigation parameter to use when reporting to Tealium.  For WinRT projects,
        /// this will be a property on the context parameter.  For Windows Phone projects, it will be the name of the
        /// query string parameter.</param>
        public TrackNavigationParameterAttribute(string variableName, string parameterName)
        {
            this.ParameterName = parameterName;
            this.VariableName = variableName;
        }

        /// <summary>
        /// Optional parameter to use if your navigation parameter contains multiple properties.  For example, if you
        /// are interested in property "Bar" on the class "Foo", ParameterName should be "Bar".
        /// In Windows Phone projects, this corresponds to the name of the query string parameter (i.e. "MyPage.xaml?Bar=123").
        /// </summary>
        public string ParameterName { get; set; }
        
        /// <summary>
        /// The name of the Tealium tracking variable to report this property as.
        /// </summary>
        public string VariableName { get; set; }
    }
}
