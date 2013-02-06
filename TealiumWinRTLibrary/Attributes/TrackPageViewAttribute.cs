
namespace Tealium
{

    /// <summary>
    /// Automatically wires up a 'view' metric for the page when it is navigated to.
    /// </summary>
    public class TrackPageViewAttribute : TrackBaseAttribute
    {
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
