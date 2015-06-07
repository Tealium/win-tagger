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
        static List<EventHandler<bool>> connectivityHandlers = new List<EventHandler<bool>>();

        static ConnectionUtility()
        {
#if WINDOWS_PHONE_LEGACY
            System.Net.NetworkInformation.NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

#else
            Windows.Networking.Connectivity.NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
#endif

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
#if WINDOWS_PHONE_LEGACY
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
#else
            var connectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile == null)
                return false;

            var connectivityLevel = connectionProfile.GetNetworkConnectivityLevel();
            isOnline = (connectivityLevel == Windows.Networking.Connectivity.NetworkConnectivityLevel.InternetAccess);
            return isOnline.Value;
#endif
        }

        private static bool DetermineIsOnWifi()
        {
#if WINDOWS_PHONE_LEGACY
            //source: https://social.msdn.microsoft.com/Forums/windowsapps/en-US/d8e76732-19d3-47b3-840f-70d87c75ce9f/network-checking-in-winrt?forum=winappswithcsharp
            var profile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            if (profile != null)
            {
                var interfaceType = profile.NetworkAdapter.IanaInterfaceType;
                return interfaceType == 71 || interfaceType == 6;
            }
            return false;
#else
            var connectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            var connectivityLevel = connectionProfile.GetNetworkConnectivityLevel();
            return (connectionProfile.GetConnectionCost().NetworkCostType == Windows.Networking.Connectivity.NetworkCostType.Unrestricted);
#endif
        }

        public static event EventHandler<bool> ConnectionStatusChanged
        {
            add { SubscribeConnectionEvent(value); }
            remove { UnsubscribeConnectionEvent(value); }
        }

        private static void UnsubscribeConnectionEvent(EventHandler<bool> value)
        {
            if (connectivityHandlers.Contains(value))
                connectivityHandlers.Remove(value);
        }

        private static void SubscribeConnectionEvent(EventHandler<bool> value)
        {
            if (!connectivityHandlers.Contains(value))
                connectivityHandlers.Add(value);
        }

#if WINDOWS_PHONE_LEGACY
        static void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
#else
        static void NetworkInformation_NetworkStatusChanged(object sender)
#endif
        {
            var previousState = isOnline;
#if WINDOWS_PHONE_LEGACY
            isOnline = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

#else
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
#endif

            lock (connectivityHandlers)
            {
                if (previousState != isOnline && connectivityHandlers.Any())
                {
                    foreach (var item in connectivityHandlers)
                    {
                        item.Invoke(sender, isOnline.Value);
                    }
                }
            }
        }

    }
}
