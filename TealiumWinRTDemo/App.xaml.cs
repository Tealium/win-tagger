﻿using TealiumWinRTDemo.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Tealium;
using Windows.Graphics.Display;

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace TealiumWinRTDemo
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                //TEALIUM: example for initializing the Tealium Tagger
                TealiumTagger.Initialize(new TealiumSettings(
                    "tealiummobile", //Tealium account name
                    "win", //Tealium profile name
#if DEBUG
                    TealiumEnvironment.TealiumTargetDev //Run against the Dev environment when in "DEBUG" configuration
#else
                    TealiumEnvironment.TealiumTargetProd //Run against the Prod environment when in "RELEASE" configuration
#endif
                    , new Dictionary<string, object>() { { "SampleGlobalVar1", "test-global" }, { "ScreenOrientation", Windows.Graphics.Display.DisplayProperties.CurrentOrientation.ToString() } }
                    ));

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }

                //TEALIUM: track the current screen orientation as a global variable...
                Windows.Graphics.Display.DisplayProperties.OrientationChanged += DisplayProperties_OrientationChanged;
                
                
                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(GroupedItemsPage), "AllGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Tealium Demo: keep track of the screen orientation as a tracking variable.
        /// </summary>
        /// <param name="sender"></param>
        void DisplayProperties_OrientationChanged(object sender)
        {
            TealiumTagger.Instance.SetGlobalVariable("ScreenOrientation", Windows.Graphics.Display.DisplayProperties.CurrentOrientation.ToString());
            //You may also want to track the orientation change as a custom event.
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}
