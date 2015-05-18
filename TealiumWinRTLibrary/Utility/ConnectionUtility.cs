using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium.Utility
{
    public static class ConnectionUtility
    {
        static bool? isOnline = null;
        static bool? isWifi = null;
        static List<EventHandler> connectivityHandlers = new List<EventHandler>();

        static ConnectionUtility()
        {
            Windows.Networking.Connectivity.NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;


        }

        public static bool IsOnline
        {
            get 
            {
                if (isOnline.HasValue)
                    return isOnline.Value;

                return DetermineIsOnline(); 
            }
        }

        public static bool IsOnWiFi 
        {
            get 
            { 
                return DetermineIsOnWifi(); 
            }
        }

        public static bool IsBatterySaver
        {
            get 
            {
#if WINDOWS_PHONE
                return Windows.Phone.System.Power.PowerManager.PowerSavingMode == Windows.Phone.System.Power.PowerSavingMode.On;
#elif  WINDOWS_PHONE_APP
                return Windows.Phone.System.Power.PowerManager.PowerSavingMode == Windows.Phone.System.Power.PowerSavingMode.On;
#else
                return false; 
#endif
            } 
        }

        private static bool DetermineIsOnline()
        {
            var connectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile == null)
                return false;

            var connectivityLevel = connectionProfile.GetNetworkConnectivityLevel();
            isOnline = (connectivityLevel == Windows.Networking.Connectivity.NetworkConnectivityLevel.InternetAccess);
            return isOnline.Value;
        }

        private static bool DetermineIsOnWifi()
        {
            var connectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            var connectivityLevel = connectionProfile.GetNetworkConnectivityLevel();
            return (connectionProfile.GetConnectionCost().NetworkCostType == Windows.Networking.Connectivity.NetworkCostType.Unrestricted);
        }

        public static event EventHandler ConnectionStatusChanged
        {
            add { SubscribeConnectionEvent(value); }
            remove { UnsubscribeConnectionEvent(value); }
        }

        private static void UnsubscribeConnectionEvent(EventHandler value)
        {
            if (connectivityHandlers.Contains(value))
                connectivityHandlers.Remove(value);
        }

        private static void SubscribeConnectionEvent(EventHandler value)
        {
            if (!connectivityHandlers.Contains(value))
                connectivityHandlers.Add(value);
        }


        static void NetworkInformation_NetworkStatusChanged(object sender)
        {
            var previousState = isOnline;
            var connectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile == null)
            {
                isOnline = false;
            }
            else
            {
                var connectivityLevel = connectionProfile.GetNetworkConnectivityLevel();
                isOnline = (connectivityLevel == Windows.Networking.Connectivity.NetworkConnectivityLevel.InternetAccess);
            }

            lock (connectivityHandlers)
            {
                if (previousState != isOnline && connectivityHandlers.Any())
                {
                    foreach (var item in connectivityHandlers)
                    {
                        item.Invoke(sender, EventArgs.Empty);
                    }
                }
            }
        }

    }
}
