using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Tealium.Enumerations;
#if NETFX_CORE
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
#endif
#if WINDOWS_PHONE
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using System.Windows;
#endif

namespace Tealium.Utility
{
    internal class TrackingFrame
    {
        private string url;
        private VersionConfig config;
        private RequestQueue requestQueue = new RequestQueue();
#if NETFX_CORE
        private WebView taggerWebView;
#elif WINDOWS_PHONE
        private WebBrowser taggerWebView;
#endif
        private DispatcherTimer queueTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };

        public bool IsReady { get; private set; }

        public bool IsEnabled
        {
            get { return this.config == null || config.IsEnabled; }
        }

        public WebViewStatus Status { get; private set; }

        public TrackingFrame(string url)
        {
            this.url = url;

            ConnectionUtility.ConnectionStatusChanged += ConnectionUtility_ConnectionStatusChanged;

#if NETFX_CORE
            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
#endif


            InitializeWebView();
            LoadPersistedQueue();

            if (ConnectionUtility.IsOnline)
            {
                OpenTrackingPage();
            }
        }

        public void TrackEvent(string script)
        {
            if (!this.IsEnabled)
                return;

            QueueAndProcess(script);
        }



        private void InitializeWebView()
        {
#if NETFX_CORE
            taggerWebView = new WebView();
#elif WINDOWS_PHONE
            taggerWebView = new WebBrowser();
            taggerWebView.IsScriptEnabled = true;
            taggerWebView.ScriptNotify += taggerWebView_ScriptNotify;
#endif
            Status = WebViewStatus.Unknown;
            queueTimer.Tick += queueTimer_Tick;
        }

        void taggerWebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            
        }


        private async Task LoadPersistedQueue()
        {
            var resumedQueue = await StorageHelper.Load<List<string>>(Constants.QUEUE_STORAGE_PATH);
            if (resumedQueue != null && resumedQueue.Count > 0)
            {
                foreach (var item in resumedQueue)
                {
                    requestQueue.Enqueue(item);

                }
            }
        }

        private void OpenTrackingPage()
        {
#if NETFX_CORE
            taggerWebView.NavigationCompleted += taggerWebView_NavigationCompleted;
#elif WINDOWS_PHONE
            taggerWebView.NavigationFailed += taggerWebView_NavigationFailed;
            taggerWebView.LoadCompleted += taggerWebView_LoadCompleted;
#endif
            taggerWebView.Unloaded += taggerWebView_Unloaded;


            Status = WebViewStatus.Loading;
            taggerWebView.Navigate(new Uri(url));
        }

        private async Task LoadVersionConfig()
        {
#if NETFX_CORE
            var configData = await taggerWebView.InvokeScriptAsync("eval", new[] { "if (typeof mps !== 'undefined') { JSON.stringify(mps); }" });
#elif WINDOWS_PHONE
            var configData = taggerWebView.InvokeScript("eval", new[] { "if (typeof mps !== 'undefined') { JSON.stringify(mps); }" });
#endif
            if (configData != null && !string.IsNullOrWhiteSpace(configData.ToString()))
                config = VersionConfig.Parse(configData.ToString());
            else
                config = VersionConfig.Default;

            requestQueue.ResetQueueSize(config.OfflineDispatchLimit, config.DispatchExpiration);
        }



        private void QueueAndProcess(string script)
        {
            requestQueue.Enqueue(script);
            ProcessRequestQueue();

        }

        private void ProcessRequestQueue()
        {
            var tm = GetTrackingMode();
            if (Status != WebViewStatus.Loaded
                || tm != TrackingMode.Enabled
                || queueTimer.IsEnabled
                || requestQueue.IsEmpty)
            {
                if (Status == WebViewStatus.Failure && ConnectionUtility.IsOnline)
                    OpenTrackingPage(); //if the app was offline when launched, the tracking page wouldn't have loaded, so try loading it now.
                return;
            }

            //kick off timer to process the queue
            queueTimer.Start();

        }

        private TrackingMode GetTrackingMode()
        {
            bool enabled = config == null || config.IsEnabled;
            if (!enabled)
                return TrackingMode.Disabled;

            bool online = ConnectionUtility.IsOnline &&
                (config == null || !config.WifiOnlySending || ConnectionUtility.IsOnWiFi);
            bool conserve = (config == null || config.BatterySaver) && ConnectionUtility.IsBatterySaver;

            if (online && !conserve)
                return TrackingMode.Enabled;
            
            return TrackingMode.Deferred;
        }


        private bool IsFrameReady()
        {
            return taggerWebView.Source != null;
        }

        private async Task FrameNavigationSuccess()
        {
            await LoadVersionConfig();
            if (config.IsEnabled)
            {
                Status = WebViewStatus.Loaded;
                ProcessRequestQueue();
            }
            else
            {
                Status = WebViewStatus.Disabled;
            }
        }

#if NETFX_CORE
        async void taggerWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            taggerWebView.NavigationCompleted -= taggerWebView_NavigationCompleted;
            if (args.IsSuccess)
            {
                await FrameNavigationSuccess();
            }
            else
            {
                Status = WebViewStatus.Failure;
            }
        }

#elif WINDOWS_PHONE
        async void taggerWebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            taggerWebView.NavigationFailed -= taggerWebView_NavigationFailed;
            taggerWebView.NavigationFailed += taggerWebView_NavigationFailed;
            taggerWebView.LoadCompleted -= taggerWebView_LoadCompleted;
            await FrameNavigationSuccess();
        }

        void taggerWebView_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            taggerWebView.NavigationFailed -= taggerWebView_NavigationFailed;
            taggerWebView.LoadCompleted -= taggerWebView_LoadCompleted;
            Status = WebViewStatus.Failure;
        }
#endif


#if NETFX_CORE
        async void Current_Resuming(object sender, object e)
        {
            TealiumStatusLog.Information("Application.Current.Resuming");
            await LoadPersistedQueue();
            TealiumStatusLog.Information("Queue loaded from disk");
        }

        async void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            TealiumStatusLog.Information("Application.Current.Suspending");
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            var throwaway = await StorageHelper.Save(requestQueue.ToList(), "_tealium_queue");

            TealiumStatusLog.Information("Queue saved to disk");
            deferral.Complete(); //needed to ensure the suspend process waits for this to finish
        }
#endif

        void queueTimer_Tick(object sender, object e)
        {
            if (requestQueue.IsEmpty || requestQueue.Size < config.EventBatchSize || !ConnectionUtility.IsOnline)
                queueTimer.Stop();

            string invokeScriptBatch = string.Empty;
            for (int i = 0; i < config.EventBatchSize; i++)
            {
                string invokeScript = string.Empty;
                if (requestQueue.TryDequeue(out invokeScript))
                    invokeScriptBatch += invokeScript;
            }

            if (!string.IsNullOrWhiteSpace(invokeScriptBatch))
            {
                try
                {
                    TealiumStatusLog.Information(invokeScriptBatch);
#if NETFX_CORE
                    taggerWebView.InvokeScriptAsync("eval", new[] { invokeScriptBatch });
#elif WINDOWS_PHONE
                    taggerWebView.InvokeScript("eval", new[] { invokeScriptBatch });
#endif
                }
                catch (Exception ex)
                {
                    TealiumStatusLog.Error(ex.Message);
                }
            }

        }

        void taggerWebView_Unloaded(object sender, RoutedEventArgs e)
        {
#if NETFX_CORE
            taggerWebView.NavigationCompleted -= taggerWebView_NavigationCompleted;
#elif WINDOWS_PHONE
            taggerWebView.NavigationFailed -= taggerWebView_NavigationFailed;
            taggerWebView.LoadCompleted -= taggerWebView_LoadCompleted;
#endif
            taggerWebView.Unloaded -= taggerWebView_Unloaded;
            ConnectionUtility.ConnectionStatusChanged -= ConnectionUtility_ConnectionStatusChanged;
            queueTimer.Tick -= queueTimer_Tick;
#if NETFX_CORE
            Application.Current.Suspending -= Current_Suspending;
            Application.Current.Resuming -= Current_Resuming;
#endif

        }

        void ConnectionUtility_ConnectionStatusChanged(object sender, EventArgs e)
        {
            if (ConnectionUtility.IsOnline)
            {
                if (Status == WebViewStatus.Unknown || Status == WebViewStatus.Failure)
                    ThreadHelper.OnUiThread(OpenTrackingPage); //retry loading tracking page due to offline failure
                else if (Status == WebViewStatus.Loaded)
                    ThreadHelper.OnUiThread(ProcessRequestQueue);
            }
        }


    }
}
