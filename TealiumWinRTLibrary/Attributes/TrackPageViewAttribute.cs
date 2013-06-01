using System;

namespace Tealium
{

    /// <summary>
    /// Wires up a 'view' metric for the page when it is navigated to.  If "AutoTrackPageViews" is enabled in
    /// the settings, this will override the page name reported to Tealium.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TrackPageViewAttribute : TrackBaseAttribute
    {
        /// <summary>
        /// Wires up a "view" metric for the page when it is navigated to.
        /// </summary>
        /// <param name="pageName">Overrides the page name reported to Tealium when logging views via "AutoTrackPageViews".</param>
        public TrackPageViewAttribute(string pageName)
        {
            this.Value = pageName;
        }

        /// <summary>
        /// The name of the page to report a 'view' metric for in Tealium.
        /// </summary>
        public string Value { get; set; }
    }

}
