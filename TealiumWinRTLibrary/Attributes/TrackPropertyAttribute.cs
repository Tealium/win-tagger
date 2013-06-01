using System;

namespace Tealium
{
    /// <summary>
    /// Associates a name/value pair for any reported events on this page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TrackPropertyAttribute : TrackBaseAttribute
    {
        /// <summary>
        /// Associates a name/value pair for any reported events on this page.
        /// </summary>
        /// <param name="name">The name of the Tealium tracking variable to report.</param>
        /// <param name="value">The value of the property to report to Tealium.</param>
        public TrackPropertyAttribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// The name of the Tealium tracking variable to report.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the property to report to Tealium.
        /// </summary>
        public string Value { get; set; }
    }
        
}
