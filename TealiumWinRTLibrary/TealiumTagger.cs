using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using Tealium.Utility;
using Windows.UI.Xaml;
#if NETFX_CORE
using Windows.ApplicationModel;
using Windows.UI.Core;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
#endif
#if WINDOWS_PHONE
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.Windows.Threading;
using System.Windows;

#endif

namespace Tealium
{
    /// <summary>
    /// The core component for integration with Tealium.  Must be initialized via the TealiumTagger.Initialize static method prior to calling any member methods.
    /// The majority of use cases will leverage the TealiumTagger.TrackScreenViewed, TrackItemClicked, and TrackCustomEvent methods.  Custom attributes and
    /// XAML attached properties also exist for convenience.
    /// </summary>
    public sealed class TealiumTagger : Tealium.IAnalyticsTagger
    {
        #region Private Members

        Frame rootFrame;
        TealiumSettings settings;
        Dictionary<string, object> providedVariables;
        TrackingFrame trackingFrame;

        #endregion Private Members

        #region Singleton Implementation

        /// <summary>
        /// Initializes the singleton instance of the TealiumTagger with the specified settings.
        /// Assumes that the root visual of the application is an instance of Frame.
        /// </summary>
        /// <param name="settings"></param>
        public static void Initialize(TealiumSettings settings)
        {
            instance = new TealiumTagger(settings);
        }

        /// <summary>
        /// Initializes the singleton instance of the TealiumTagger with the specified settings and specified Frame instance.
        /// This version is useful when the root visual of the application is not a Frame.
        /// </summary>
        /// <param name="rootFrame"></param>
        /// <param name="settings"></param>
        public static void Initialize(Frame rootFrame, TealiumSettings settings)
        {
            instance = new TealiumTagger(rootFrame, settings);
        }


        /// <summary>
        /// Singleton instance of the TealiumTagger accessible to the calling application.
        /// </summary>
        public static TealiumTagger Instance
        {
            get
            {
                if (instance == null)
                    TealiumStatusLog.Error("TealiumTagger instance has not been initialized. Please initialize in your App.xaml.cs OnLaunched handler.");

                return instance;
            }
        }
        private static TealiumTagger instance;

        #endregion Singleton Implementation


        #region Constructors

        private TealiumTagger(TealiumSettings settings)
        {
            this.settings = settings;
            RegisterWithRootFrame();
        }

        private TealiumTagger(Frame rootFrame, TealiumSettings settings)
        {
            this.settings = settings;
            this.rootFrame = rootFrame;
            RegisterWithRootFrame();
        }

        #endregion Constructors

        #region Public API Surface

        /// <summary>
        /// Adds the supplied collection of name/value pairs to the collection of variables.  These values will be persisted between calls until ClearVariables is called or the application navigates to a new page.
        /// </summary>
        /// <param name="variables"></param>
        public void SetVariables(IDictionary variables)
        {
            if (variables == null)
            {
                providedVariables = null;
                return;
            }
            providedVariables = new Dictionary<string, object>();
            var e = variables.GetEnumerator();
            while (e.MoveNext())
            {
                providedVariables[e.Key.ToString()] = e.Value;
            }
        }

        /// <summary>
        /// Adds an individual name/value pair to the persisted collection of variables.  The value will be persisted between calls until ClearVariables is called or the application navigates to a new page.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetVariable(string name, string value)
        {
            if (providedVariables == null)
                providedVariables = new Dictionary<string, object>();
            providedVariables[name] = value;
        }

        /// <summary>
        /// Adds the supplied collection of name/value pairs to the collection of global/base variables.  These values will be persisted for the lifetime of the application, or until manually cleared.
        /// </summary>
        /// <param name="variables"></param>
        public void SetGlobalVariables(IDictionary variables)
        {
            if (variables == null)
            {
                return;
            }
            settings.BaseVariables = new Dictionary<string, object>();
            var e = variables.GetEnumerator();
            while (e.MoveNext())
            {
                settings.BaseVariables[e.Key.ToString()] = e.Value;
            }
        }

        /// <summary>
        /// Adds an individual name/value pair to the collection of global/base variables.  The value will be persisted for the lifetime of the application, or until manually cleared.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetGlobalVariable(string name, string value)
        {
            if (settings.BaseVariables == null)
                settings.BaseVariables = new Dictionary<string, object>();
            settings.BaseVariables[name] = value;
        }

        /// <summary>
        /// Reports a click/link event with the specified details.
        /// Variables that have the same name as persisted variables set by SetVariables will take precedence.  All variables passed to this call will not be persisted.
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="variables"></param>
        public void TrackItemClicked(string itemName, IDictionary variables = null)
        {
            if (variables == null)
                variables = new Dictionary<string, string>();

            variables[settings.ClickMetricIdParam] = itemName;
            TrackCustomEvent(settings.ClickMetricEventName, variables);
        }

        /// <summary>
        /// Reports a page view event with the specified details.
        /// Variables that have the same name as persisted variables set by SetVariables will take precedence.  All variables passed to this call will not be persisted, with the exception of the screen/view/page name.
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="variables"></param>
        public void TrackScreenViewed(string viewName, IDictionary variables = null)
        {
            if (variables == null)
                variables = new Dictionary<string, string>();

            variables[settings.ViewMetricIdParam] = viewName;
            SetVariable(this.settings.ViewMetricIdParam, viewName);

            TrackCustomEvent(settings.ViewMetricEventName, variables);
        }

        /// <summary>
        /// Reports a custom event with the specified details.
        /// Variables that have the same name as persisted variables set by SetVariables will take precedence.  All variables passed to this call will not be persisted.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="variables"></param>
        public void TrackCustomEvent(string eventName, IDictionary variables = null)
        {
            Dictionary<string, string> variablesToSend = CopyVarsFromBase();
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


        #region Initialization

        private void RegisterWithRootFrame()
        {

            InitializeTrackingFrame();

            if (rootFrame == null)
            {
#if NETFX_CORE
                rootFrame = Window.Current.Content as Frame;
#else
                rootFrame = Application.Current.RootVisual as Frame;
#endif
            }
            if (rootFrame != null)
            {
                SubscribeEvents();

            }
            else
            {
#if NETFX_CORE
                if (Window.Current.Content != null)
                    ErrorRootIsNotFrame();
                Window.Current.VisibilityChanged += Current_VisibilityChanged;
#else
                if (Application.Current.RootVisual != null)
                    ErrorRootIsNotFrame();

                
#endif
            }
        }

        private void InitializeTrackingFrame()
        {
            string trackingPage = GetWebViewUrl();
            trackingFrame = new TrackingFrame(trackingPage);
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
            }
        }

        #endregion Initialization


        #region Event Handlers

#if NETFX_CORE
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
#endif


        void rootFrame_Unloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeEvents();
        }

        void rootFrame_Navigated(object sender, NavigationEventArgs e)
        {

#if NETFX_CORE
            object page = ((Frame)sender).Content;
#else
            object page = rootFrame.Content; //((NavigationService)sender).CurrentSource;
#endif

            if (page != null)
            {
                ((FrameworkElement)page).OnFirstFrame(() =>
                {
                    //We delay this call until we know the page has rendered. This helps to ensure this call fires only after navigation has completed.
                    SetVariables(null); //clear previous vars so they don't interfere w/ the next page

#if NETFX_CORE
                    LoadAutomaticNavigationProperties(page, e.Parameter);
#else
                    object param = null;
                    if (page is PhoneApplicationPage)
                        param = ((PhoneApplicationPage)page).NavigationContext;
                    LoadAutomaticNavigationProperties(page,  param);
#endif

                    ReportPageNavigation(page, e.NavigationMode);
                });
            }

        }

        void rootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
        }

        #endregion Event Handlers


        #region Configuration

        private string GetWebViewUrl()
        {
            return string.Format(Constants.TRACKER_EMBED_URL_FORMAT,
                    settings.UseSSL ? "https" : "http",
                    settings.Account,
                    settings.Profile,
                    GetEnvironmentString(settings.Environment),
                    BustCacheParam());
        }

        private object BustCacheParam()
        {
            return "?bust=" + DateTimeOffset.Now.Ticks;
        }

        private bool SettingsValid()
        {
            string embedUrl = GetWebViewUrl();
            return !string.IsNullOrEmpty(embedUrl) && Uri.IsWellFormedUriString(embedUrl, UriKind.Absolute);
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

        private Dictionary<string, string> CopyVarsFromBase()
        {
            Dictionary<string,string> baseVars = new Dictionary<string,string>();
            if (settings.BaseVariables != null)
            {
                foreach (var item in settings.BaseVariables)
                {
                    if (item.Value != null)
                        baseVars[item.Key] = item.Value.ToString();
                    else
                        baseVars[item.Key] = string.Empty;

                }
            }

            return baseVars;
        }

        #endregion Configuration

        #region Initialization

        #endregion Initialization

        #region Reporting

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
                    v += string.Format("\"{0}\": \"{1}\"", item.Key.HtmlEncode(), item.Value.HtmlEncode());
                }
            }
            return "{ " + v + " }";
        }

        private void SendEvent(string eventName, string jsonParams)
        {
            string invokeScript = string.Format(Constants.UTAG_INVOKE_SCRIPT,
                    eventName, jsonParams);

            trackingFrame.TrackEvent(invokeScript);
        }

        private void ReportPageNavigation(object page, NavigationMode navigationMode = NavigationMode.New)
        {
            string pageName = null;

            var name = TypeHelper.GetAttribute<TrackPageViewAttribute>(page);
            if (name != null)
                pageName = name.Value;

            if (string.IsNullOrEmpty(pageName) && settings.AutoTrackPageViews)
            {
                //auto-track enabled for navigation, report based on the type of page we are navigating to
                pageName = page.GetType().Name;
            }

            //FUTURE: track "NavigationMode" to distinguish between New, Back, & Forward navigation events

            if (!string.IsNullOrEmpty(pageName))
            {
                //we have the page param; track this view
                TrackScreenViewed(pageName, null);
            }

        }

        private void LoadAutomaticNavigationProperties(object page, object parameter)
        {
            Dictionary<string, object> vars = new Dictionary<string, object>();

            var props = TypeHelper.GetAttributes<TrackPropertyAttribute>(page);
            if (props != null && props.Any())
            {
                foreach (var item in props)
                {
                    if (item.Name != null && item.Value != null)
                        vars[item.Name] = item.Value;
                }

            }

            var pars = TypeHelper.GetAttributes<TrackNavigationParameterAttribute>(page);
            if (pars != null && pars.Any())
            {
                foreach (var item in pars)
                {
                    if (!string.IsNullOrEmpty(item.VariableName))
                    {
                        if (!string.IsNullOrEmpty(item.ParameterName) && parameter != null)
                        {
#if NETFX_CORE
                            vars[item.VariableName] = TypeHelper.LookupProperty(item.ParameterName, parameter);
#else
                            var context = parameter as NavigationContext;
                            if (context != null && context.QueryString.ContainsKey(item.ParameterName))
                            {
                                vars[item.VariableName] = context.QueryString[item.ParameterName];
                            }
#endif
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


        #endregion Reporting

    }
}
