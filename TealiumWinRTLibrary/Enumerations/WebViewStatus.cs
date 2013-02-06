using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium
{

    /// <summary>
    /// Internal status for the hidden WebView frame responsible for firing off Tealium events.
    /// </summary>
    internal enum WebViewStatus
    {
        /// <summary>
        /// WebView has not loaded yet.
        /// </summary>
        Unknown,

        /// <summary>
        /// WebView has been initialized and assigned a URL, but has not finished loading yet.
        /// </summary>
        Loading,

        /// <summary>
        /// WebView has loaded and is ready to fire events.
        /// </summary>
        Loaded,

        /// <summary>
        /// WebView has encountered an error, likely due to the app being offline.
        /// </summary>
        Failure
    }

}
