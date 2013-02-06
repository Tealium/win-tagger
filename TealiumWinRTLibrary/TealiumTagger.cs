using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Reflection;
using System.Net.NetworkInformation;
using Windows.UI.Core;
using Windows.ApplicationModel;
using Tealium.Utility;
using System.Collections;

namespace Tealium
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TealiumTagger
    {
        #region Private Members

        WebView taggerWebView;
        Frame rootFrame;
        TealiumSettings settings;
        Dictionary<string, string> baseVariables;
        ConcurrentDictionary<string, object> providedVariables;
        bool connectivityStatus = true;
        WebViewStatus webViewStatus = WebViewStatus.Unknown;
        ConcurrentQueue<string> requestQueue = new ConcurrentQueue<string>();
        DispatcherTimer queueTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };

        #endregion Private Members

        #region Singleton Implementation

        public static void Initialize(TealiumSettings settings)
        {
            instance = new TealiumTagger(settings);
        }

        public static void Initialize(Frame rootFrame, TealiumSettings settings)
        {
            instance = new TealiumTagger(rootFrame, settings);
        }

        public static void Shutdown()
        {

        }

        #endregion Singleton Implementation

        public static TealiumTagger Instance
        {
            get
            {
                return instance;
            }
        }
        private static TealiumTagger instance;

        protected TealiumTagger(TealiumSettings settings)
        {
            this.settings = settings;
            RegisterWithRootFrame();
        }

        protected TealiumTagger(Frame rootFrame, TealiumSettings settings)
        {
            this.settings = settings;
            this.rootFrame = rootFrame;
            RegisterWithRootFrame();
        }

        private void RegisterWithRootFrame()
        {
            baseVariables = new Dictionary<string, string>();

            LoadPersistedQueue();
            InitializeWebView();

            if (rootFrame == null)
                rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null)
            {
                SubscribeEvents();

            }
            else
            {
                if (Window.Current.Content != null)
                    ErrorRootIsNotFrame();
                Window.Current.VisibilityChanged += Current_VisibilityChanged;
            }
        }

        private void InitializeWebView()
        {
            queueTimer.Tick += queueTimer_Tick;
            taggerWebView = new WebView();

            OpenTrackingPage();
        }

        private void OpenTrackingPage()
        {
            taggerWebView.NavigationFailed += taggerWebView_NavigationFailed;
            taggerWebView.LoadCompleted += taggerWebView_LoadCompleted;

            string trackingPage = GetWebViewUrl();
            webViewStatus = WebViewStatus.Loading;
            taggerWebView.Navigate(new Uri(trackingPage));
        }

        void taggerWebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            taggerWebView.NavigationFailed -= taggerWebView_NavigationFailed;
            taggerWebView.LoadCompleted -= taggerWebView_LoadCompleted;
            webViewStatus = WebViewStatus.Loaded;
            ProcessRequestQueue();
        }

        void taggerWebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            taggerWebView.NavigationFailed -= taggerWebView_NavigationFailed;
            taggerWebView.LoadCompleted -= taggerWebView_LoadCompleted;
            webViewStatus = WebViewStatus.Failure;
        }

        private void ErrorRootIsNotFrame()
        {
            throw new Exception("The root visual is not an instance of the Frame class.  If your root visual is not a Frame, please pass a Frame reference in the first parameter to Initialize().");
        }


        private void SubscribeEvents()
        {
            if (rootFrame != null)
            {
                rootFrame.Navigating += rootFrame_Navigating;
                rootFrame.Navigated += rootFrame_Navigated;
                rootFrame.Unloaded += rootFrame_Unloaded;

                Window.Current.Activated += Current_Activated;
                Window.Current.Closed += Current_Closed;
                Application.Current.Suspending += Current_Suspending;
                Application.Current.Resuming += Current_Resuming;
                Application.Current.UnhandledException += Current_UnhandledException;
                NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

                if (rootFrame.Content != null)
                {
                    LoadAutomaticNavigationProperties(rootFrame.Content, null);
                    ReportPageNavigation(rootFrame.Content);
                }
            }
        }

        private void UnsubscribeEvents()
        {
            if (rootFrame != null)
            {
                rootFrame.Navigating -= rootFrame_Navigating;
                rootFrame.Navigated -= rootFrame_Navigated;
                rootFrame.Unloaded -= rootFrame_Unloaded;

                Window.Current.Activated -= Current_Activated;
                Window.Current.Closed -= Current_Closed;
                Application.Current.Suspending -= Current_Suspending;
                Application.Current.Resuming -= Current_Resuming;
                Application.Current.UnhandledException -= Current_UnhandledException;
                NetworkChange.NetworkAddressChanged -= NetworkChange_NetworkAddressChanged;
            }
        }

        #region Event Handlers

        void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            Window.Current.VisibilityChanged -= Current_VisibilityChanged;
            rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null)
            {
                SubscribeEvents();
            }
            else if (Window.Current.Content != null)
                ErrorRootIsNotFrame();

        }

        void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("Application.Current.UnhandledException");
        }

        async void Current_Resuming(object sender, object e)
        {
            Debug.WriteLine("Application.Current.Resuming");
            await LoadPersistedQueue();
            Debug.WriteLine("Queue loaded from disk");
            //SubscribeEvents();
        }

        async void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            Debug.WriteLine("Application.Current.Suspending");
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            var throwaway = await StorageHelper.Save(requestQueue, "_tealium_queue");
            Debug.WriteLine("Queue saved to disk");
            deferral.Complete();
            //UnsubscribeEvents();
        }

        void Current_Closed(object sender, Windows.UI.Core.CoreWindowEventArgs e)
        {
            //this may never get called?
            Debug.WriteLine("Window.Current.Closed");
        }

        void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            Debug.WriteLine("Window.Current.Activated: {0}", e.WindowActivationState);
        }

        void rootFrame_Unloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeEvents();
        }

        void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            bool newConnectivityStatus = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            if (newConnectivityStatus != connectivityStatus && newConnectivityStatus)
            {
                //run following command on UI thread
                CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ProcessRequestQueue();
                });
            }
            connectivityStatus = newConnectivityStatus;
        }

        void rootFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (sender != null && e.NavigationMode == NavigationMode.New)
            {
                var page = ((Frame)sender).Content;
                if (page != null)
                {
                    LoadAutomaticNavigationProperties(page, e.Parameter);

                    ((FrameworkElement)page).OnFirstFrame(() =>
                        {
                            //We delay this call until we know the page has rendered. This helps to ensure this call fires only after navigation has completed.
                            ReportPageNavigation(page, e.Parameter);
                        });
                }
            }

        }

        private void ReportPageNavigation(object page, object parameter = null)
        {
            string pageName = null;
            
            var name = page.GetType().GetTypeInfo().GetCustomAttribute<TrackPageViewAttribute>();
            if (name != null)
                pageName = name.Value;

            if (!string.IsNullOrEmpty(pageName))
            {
                //we have the page param; track this view
                TrackScreenViewed(pageName, null);
            }
        }

        private void LoadAutomaticNavigationProperties(object page, object parameter)
        {
            Dictionary<string, object> vars = new Dictionary<string, object>();

            var props = page.GetType().GetTypeInfo().GetCustomAttributes<TrackPropertyAttribute>();
            if (props != null && props.Any())
            {
                foreach (var item in props)
                {
                    if (item.Name != null && item.Value != null)
                        vars[item.Name] = item.Value;
                }

            }

            var pars = page.GetType().GetTypeInfo().GetCustomAttributes<TrackNavigationParameterAttribute>();
            if (pars != null && pars.Any())
            {
                foreach (var item in pars)
                {
                    if (!string.IsNullOrEmpty(item.VariableName))
                    {
                        if (!string.IsNullOrEmpty(item.ParameterName) && parameter != null)
                        {
                            vars[item.VariableName] = LookupProperty(item.ParameterName, parameter);
                        }
                        else
                        {
                            vars[item.VariableName] = parameter;
                        }
                    }
                }

            }

            this.SetVariables(vars);

        }

        private object LookupProperty(string p, object parameter)
        {
            //check for properties w/ that name first
            var props = parameter.GetType().GetRuntimeProperties();
            foreach (var item in props)
            {
                if (string.Equals(item.Name, p, StringComparison.OrdinalIgnoreCase))
                {
                    var s = item.GetValue(null);
                    if (s != null)
                        return s.ToString();
                }

            }
            //if not found, check the fields
            var fields = parameter.GetType().GetRuntimeFields();
            foreach (var item in props)
            {
                if (string.Equals(item.Name, p, StringComparison.OrdinalIgnoreCase))
                {
                    var s = item.GetValue(null);
                    if (s != null)
                        return s.ToString();
                }

            } 
            return null;
        }

        void rootFrame_Navigating(object sender, Windows.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            SetVariables(null); //clear previous vars so they don't interfere w/ the next page
        }

        void queueTimer_Tick(object sender, object e)
        {
            if (requestQueue.IsEmpty)
                queueTimer.Stop();

            string invokeScript = string.Empty;
            if (requestQueue.TryDequeue(out invokeScript) && !string.IsNullOrWhiteSpace(invokeScript))
            {
                try
                {
                    taggerWebView.InvokeScript("eval", new[] { invokeScript });
                    Debug.WriteLine("*********{0}", invokeScript);
                }
                catch (Exception ex)
                {
                    //TODO: logging if request failed
                    Debugger.Break();
                }
            }

        }


        #endregion Event Handlers


        #region Public API Surface

        public void SetVariables(IDictionary variables)
        {
            if (variables == null)
            {
                providedVariables = null;
                return;
            }
            providedVariables = new ConcurrentDictionary<string, object>();
            var e = variables.GetEnumerator();
            while (e.MoveNext())
            {
                providedVariables.TryAdd(e.Key.ToString(), e.Value);
            }
        }

        public void SetVariable(string name, string value)
        {
            if (providedVariables == null)
                providedVariables = new ConcurrentDictionary<string, object>();
            providedVariables[name] = value;
        }

        public void TrackItemClicked(string itemName, IDictionary variables = null)
        {
            if (variables == null)
                variables = new Dictionary<string, string>();

            variables[settings.ClickMetricIdParam] = itemName;
            TrackCustomEvent(settings.ClickMetricEventName, variables);
        }


        public void TrackScreenViewed(string viewName, IDictionary variables = null)
        {
            //TODO: implementation question - should the page name be persisted in providedVariables for future analytics calls on the same screen?
            if (variables == null)
                variables = new Dictionary<string, string>();

            variables[settings.ViewMetricIdParam] = viewName;
            TrackCustomEvent(settings.ViewMetricEventName, variables);
        }

        public void TrackCustomEvent(string eventName, IDictionary variables)
        {
            Dictionary<string, string> variablesToSend = new Dictionary<string, string>(baseVariables);
            if (providedVariables != null)
            {
                foreach (var item in providedVariables)
                {
                    if (item.Value != null)
                        variablesToSend[item.Key] = item.Value.ToString();
                    else
                        variablesToSend[item.Key] = string.Empty;

                }
            }

            if (variables != null)
            {
                var e = variables.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Value != null)
                        variablesToSend[e.Key.ToString()] = e.Value.ToString();
                    else
                        variablesToSend[e.Key.ToString()] = string.Empty;

                }
                
            }

            string jsonParams = GetJson(variablesToSend);
            SendEvent(eventName, jsonParams);

        }


        #endregion Public API Surface


        #region Utility Methods



        private async Task LoadPersistedQueue()
        {
            var resumedQueue = await StorageHelper.Load<ConcurrentQueue<string>>(Constants.QUEUE_STORAGE_PATH);
            if (resumedQueue != null && resumedQueue.Count > 0)
            {
                string val;
                while (resumedQueue.TryDequeue(out val))
                {
                    requestQueue.Enqueue(val);
                }
            }
        }

        private bool SettingsValid()
        {
            string embedUrl = GetWebViewUrl();
            return (taggerWebView != null && !string.IsNullOrEmpty(embedUrl) && Uri.IsWellFormedUriString(embedUrl, UriKind.Absolute));
        }


        private bool IsFrameReady()
        {
            return taggerWebView.Source != null;
        }


        private string GetWebViewUrl()
        {
            return string.Format(Constants.TRACKER_EMBED_URL_FORMAT, 
                    settings.UseSSL ?  "https" : "http",
                    settings.Account, 
                    settings.Profile, 
                    GetEnvironmentString(settings.Environment));
        }

        private object GetEnvironmentString(TealiumEnvironment tealiumEnvironment)
        {
            string env = string.Empty;
            switch (tealiumEnvironment)
            {
                case TealiumEnvironment.TealiumTargetDev:
                    env = Constants.ENV_DEV;
                    break;
                case TealiumEnvironment.TealiumTargetQA:
                    env = Constants.ENV_QA;
                    break;
                case TealiumEnvironment.TealiumTargetProd:
                    env = Constants.ENV_PROD;
                    break;
                default:
                    env = Constants.ENV_DEV;
                    break;
            }
            return env;
        }

        private string GetJson(Dictionary<string, string> variablesToSend)
        {
            if (variablesToSend == null || variablesToSend.Count == 0)
                return "{ }"; //equivalent to string.Empty for our purposes

            string v = string.Empty;
            foreach (var item in variablesToSend)
            {
                if (item.Key != null && item.Value != null)
                {
                    if (v != string.Empty)
                        v += ",";
                    v += string.Format("\"{0}\": \"{1}\"", WebUtility.HtmlEncode(item.Key), WebUtility.HtmlEncode(item.Value));
                }
            }
            return "{ " + v + " }";
        }


        private void SendEvent(string eventName, string jsonParams)
        {
            if (!SettingsValid())
                return;

            string invokeScript = string.Format(Constants.UTAG_INVOKE_SCRIPT,
                    eventName, jsonParams);

            requestQueue.Enqueue(invokeScript);
            ProcessRequestQueue();

        }

        private void ProcessRequestQueue()
        {
            if (webViewStatus != WebViewStatus.Loaded
                    || !IsOnline()
                    || queueTimer.IsEnabled
                    || requestQueue.IsEmpty)
            {
                if (webViewStatus == WebViewStatus.Failure && IsOnline())
                    OpenTrackingPage(); //if the app was offline when launched, the tracking page wouldn't have loaded, so try loading it now.
                return;
            }

            //kick off timer to process the queue
            queueTimer.Start();

        }

        private bool IsOnline()
        {
            connectivityStatus = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            return connectivityStatus;
        }

        #endregion Utility Methods
    }
}
