using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium
{
    internal class Constants
    {
        internal const string TRACKER_EMBED_URL_FORMAT = "{0}://speclet.foo/foo";// "{0}://tags.tiqcdn.com/utag/{1}/{2}/{3}/mobile.html";
        internal const string UTAG_INVOKE_SCRIPT = "utag.track('{0}',{1}, function() {{TealiumTaggerCallback.callback();}});";
        internal const string QUEUE_STORAGE_PATH = "_tealium_queue";

        internal const string DEFAULT_CLICK_EVENT_NAME = "link";
        internal const string DEFAULT_CLICK_ID_PARAM = "link_id";
        internal const string DEFAULT_VIEW_EVENT_NAME = "view";
        internal const string DEFAULT_VIEW_ID_PARAM = "pageName";
    }
}
