using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium
{
    public class TealiumSettings
    {

        public string Account { get; set; }
        public string Profile { get; set; }
        public string Environment { get; set; }

        public bool EnableOfflineMode { get; set; }
        public bool UseSSL { get; set; }

        public string StartupEventName { get; set; }
        public string ShutdownEventName { get; set; }

        public string ViewMetricEventName { get; set; }
        public string ViewMetricIdParam { get; set; }
        public string ClickMetricEventName { get; set; }
        public string ClickMetricIdParam { get; set; }

        public TealiumSettings()
        {
            this.EnableOfflineMode = true;
            this.UseSSL = false;
            ViewMetricEventName = Constants.DEFAULT_VIEW_EVENT_NAME;
            ClickMetricEventName = Constants.DEFAULT_CLICK_EVENT_NAME;
            ViewMetricIdParam = Constants.DEFAULT_VIEW_ID_PARAM;
            ClickMetricIdParam = Constants.DEFAULT_CLICK_ID_PARAM;
        }
    }
}
