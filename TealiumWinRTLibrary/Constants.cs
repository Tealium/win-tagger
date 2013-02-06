using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium
{
    /// <summary>
    /// A collection of strings used by the Tealium implementation.
    /// </summary>
    internal class Constants
    {
        /// <summary>
        /// The format of the URL for the mobile tracking page.  Values are substituted in depending on the provided settings.
        /// </summary>
        internal const string TRACKER_EMBED_URL_FORMAT = "{0}://tags.tiqcdn.com/utag/{1}/{2}/{3}/mobile.html";

        /// <summary>
        /// The format of the utag.track script called for every metric.
        /// </summary>
        internal const string UTAG_INVOKE_SCRIPT = "utag.track('{0}',{1}, function() {{TealiumTaggerCallback.callback();}});";

        /// <summary>
        /// Name for the local storage folder that cached/offline requests are temporarily saved to.
        /// </summary>
        internal const string QUEUE_STORAGE_PATH = "_tealium_queue";

        /// <summary>
        /// The name of the "click" event reported by the Tealium tracker.
        /// </summary>
        internal const string DEFAULT_CLICK_EVENT_NAME = "link";

        /// <summary>
        /// The name of the identifier for "click" events.
        /// </summary>
        internal const string DEFAULT_CLICK_ID_PARAM = "link_id";

        /// <summary>
        /// The name of the "view" event reported by the Tealium tracker.
        /// </summary>
        internal const string DEFAULT_VIEW_EVENT_NAME = "view";

        /// <summary>
        /// The name of the identifier for "view" events.
        /// </summary>
        internal const string DEFAULT_VIEW_ID_PARAM = "pageName";

        /// <summary>
        /// The name of the "custom" event reported by the Tealium tracker.
        /// </summary>
        internal const string DEFAULT_CUSTOM_EVENT_NAME = "custom";

        /// <summary>
        /// String representation of "dev" environment.
        /// </summary>
        internal const string ENV_DEV = "dev";

        /// <summary>
        /// String representation of "QA" environment.
        /// </summary>
        internal const string ENV_QA = "qa";

        /// <summary>
        /// String representation of "prod" environment.
        /// </summary>
        internal const string ENV_PROD = "prod";
    }
}
