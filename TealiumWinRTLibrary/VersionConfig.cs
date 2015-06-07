using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_PHONE_LEGACY
using Newtonsoft.Json.Linq;
#else
using Windows.Data.Json;
#endif

namespace Tealium
{
    public class VersionConfig
    {
        private static VersionConfig _default = new VersionConfig()
        {
            IsEnabled = true,
            BatterySaver = true,
            DispatchExpiration = -1,
            EventBatchSize = 1,
            OfflineDispatchLimit = -1,
            WifiOnlySending = false,
            IVarTracking = true,
            MobileCompanion = true,
            UIAutoTracking = true
        };

        public bool IsEnabled { get; private set; }
        public bool BatterySaver { get; private set; }
        public int DispatchExpiration { get; private set; }
        public int EventBatchSize { get; private set; }
        public int OfflineDispatchLimit { get; private set; }
        public bool WifiOnlySending { get; private set; }

        //not currently implemented...
        public bool IVarTracking { get; private set; }
        public bool MobileCompanion { get; private set; }
        public bool UIAutoTracking { get; private set; }


        public static VersionConfig Parse(string json)
        {
#if WINDOWS_PHONE_LEGACY
            var jobj = JObject.Parse(json);
#else
            var jobj = JsonObject.Parse(json);
#endif
            if (jobj != null)
            {
                var currentVer = jobj[Constants.CURRENT_LIBRARY_VERSION];
#if WINDOWS_PHONE_LEGACY
                if (currentVer != null && currentVer.Type == JTokenType.Object)
#else
                if (currentVer != null && currentVer.ValueType == JsonValueType.Object)
#endif
                {
                    var config = new VersionConfig();
#if WINDOWS_PHONE_LEGACY
                    if (currentVer["_is_enabled"] != null)
                        config.IsEnabled = currentVer.Value<bool>("_is_enabled");
                    if (currentVer["battery_saver"] != null)
                        config.BatterySaver = currentVer.Value<bool>("battery_saver");
                    if (currentVer["dispatch_expiration"] != null)
                        config.DispatchExpiration = currentVer.Value<int>("dispatch_expiration");
                    if (currentVer["event_batch_size"] != null)
                        config.EventBatchSize = currentVer.Value<int>("event_batch_size");
                    if (currentVer["offline_dispatch_limit"] != null)
                        config.OfflineDispatchLimit = currentVer.Value<int>("offline_dispatch_limit");
                    if (currentVer["wifi_only_sending"] != null)
                        config.WifiOnlySending = currentVer.Value<bool>("wifi_only_sending");

                    if (currentVer["ivar_tracking"] != null)
                        config.IVarTracking = currentVer.Value<bool>("ivar_tracking");
                    if (currentVer["mobile_companion"] != null)
                        config.MobileCompanion = currentVer.Value<bool>("mobile_companion");
                    if (currentVer["ui_auto_tracking"] != null)
                        config.UIAutoTracking = currentVer.Value<bool>("ui_auto_tracking");
#else
                    var currObj = currentVer.GetObject();

                    if (currObj.ContainsKey("_is_enabled"))
                        config.IsEnabled = ParseAsBoolean(currObj["_is_enabled"]);
                    if (currObj.ContainsKey("battery_saver"))
                        config.BatterySaver = ParseAsBoolean(currObj["battery_saver"]);
                    if (currObj.ContainsKey("dispatch_expiration"))
                        config.DispatchExpiration = ParseAsInt(currObj["dispatch_expiration"]);
                    if (currObj.ContainsKey("event_batch_size"))
                        config.EventBatchSize = ParseAsInt(currObj["event_batch_size"]);
                    if (currObj.ContainsKey("offline_dispatch_limit"))
                        config.OfflineDispatchLimit = ParseAsInt(currObj["offline_dispatch_limit"]);
                    if (currObj.ContainsKey("wifi_only_sending"))
                        config.WifiOnlySending = ParseAsBoolean(currObj["wifi_only_sending"]);

                    if (currObj.ContainsKey("ivar_tracking"))
                        config.IVarTracking = ParseAsBoolean(currObj["ivar_tracking"]);
                    if (currObj.ContainsKey("mobile_companion"))
                        config.MobileCompanion = ParseAsBoolean(currObj["mobile_companion"]);
                    if (currObj.ContainsKey("ui_auto_tracking"))
                        config.UIAutoTracking = ParseAsBoolean(currObj["ui_auto_tracking"]);
#endif

                    return config;
                }
            }
            return Default;
        }

        public static VersionConfig Default
        {
            get
            {
                return _default;
            }
        }

#if !WINDOWS_PHONE_LEGACY
        private static bool ParseAsBoolean(IJsonValue jsonValue, bool defaultValue = false)
        {
            if (jsonValue.ValueType == JsonValueType.Boolean)
            {
                return jsonValue.GetBoolean();
            }
            else if (jsonValue.ValueType == JsonValueType.String)
            {
                bool rv;
                if (!Boolean.TryParse(jsonValue.GetString(), out rv))
                    return defaultValue;
                return rv;
            }
            else
            {
                return defaultValue;
            }
        }

        private static int ParseAsInt(IJsonValue jsonValue, int defaultValue = -1)
        {
            if (jsonValue.ValueType == JsonValueType.Number)
            {
                return (int)jsonValue.GetNumber();
            }
            else if (jsonValue.ValueType == JsonValueType.String)
            {
                int rv;
                if (!int.TryParse(jsonValue.GetString(), out rv))
                    return defaultValue;
                return rv;
            }
            else
            {
                return defaultValue;
            }
        }
#endif

        protected VersionConfig() { }


    }
}
