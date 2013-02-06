using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium
{
    public class TealiumSettings
    {
        /// <summary>
        /// The Tealium account name for your company.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// The reporting profile for your application.
        /// </summary>
        public string Profile { get; set; }

        /// <summary>
        /// The reporting environment for your application (i.e. "Dev", "QA", "Prod").
        /// </summary>
        public TealiumEnvironment Environment { get; set; }

        /// <summary>
        /// Whether requests should be queued for later delivery whenever a network connection is unavailable.
        /// Note that queued requests will retain their order when sent, but timestamps may not be accurate.
        /// </summary>
        public bool EnableOfflineMode { get; set; }

        /// <summary>
        /// Whether traffic should run over https (true) or http (false).
        /// </summary>
        public bool UseSSL { get; set; }


        //public string StartupEventName { get; set; }
        //public string ShutdownEventName { get; set; }


        /// <summary>
        /// The name of the Tealium tracking event for page views (default 'view').
        /// </summary>
        public string ViewMetricEventName { get; set; }

        /// <summary>
        /// The name of the page identifier for view metrics (default 'pageName').
        /// </summary>
        public string ViewMetricIdParam { get; set; }


        /// <summary>
        /// The name of the Tealium tracking event for link clicks (default 'link').
        /// </summary>
        public string ClickMetricEventName { get; set; }

        /// <summary>
        /// The name of the page identifier for click metrics (default ('link-id').
        /// </summary>
        public string ClickMetricIdParam { get; set; }

        /// <summary>
        /// Creates a new instance of the TealiumSettings object.  The account, profile, and environment
        /// properties are required and must be specified.
        /// </summary>
        /// <param name="account">The Tealium account name for your company.</param>
        /// <param name="profile">The reporting profile for your application.</param>
        /// <param name="environment">The reporting environment for your application (i.e. "Dev", "QA", "Prod").</param>
        public TealiumSettings(string account, string profile, TealiumEnvironment environment)
        {
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(profile))
                throw new TealiumConfigurationException("The Account, Profile, and Environment settings are required when initializing a new Settings instance.");

            this.Account = account;
            this.Profile = profile;
            this.Environment = environment;

            //set defaults for the rest of the settings.
            this.EnableOfflineMode = true;
            this.UseSSL = false;
            ViewMetricEventName = Constants.DEFAULT_VIEW_EVENT_NAME;
            ClickMetricEventName = Constants.DEFAULT_CLICK_EVENT_NAME;
            ViewMetricIdParam = Constants.DEFAULT_VIEW_ID_PARAM;
            ClickMetricIdParam = Constants.DEFAULT_CLICK_ID_PARAM;
        }
    }
}
