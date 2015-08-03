using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Media.SpeechRecognition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BestFriend
{
    sealed partial class App : Application
    {
        
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            #region OnLaunched default code
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            #endregion

            Window.Current.Activate();

            // Install VCD
            try
            {
                var storageFile =
                await Windows.Storage.StorageFile
                .GetFileFromApplicationUriAsync(new Uri("ms-appx:///vcd.xml"));

                await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager
                    .InstallCommandDefinitionsFromStorageFileAsync(storageFile);

                Debug.WriteLine("VCD installed");
            }
            catch
            {
                Debug.WriteLine("VCD installation failed");
            }
            
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            #region Activation Code
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                Window.Current.Content = rootFrame;
            }
            #endregion

            if (e.Kind == Windows.ApplicationModel.Activation.ActivationKind.VoiceCommand)
            {
                var commandArgs = e as Windows.ApplicationModel.Activation.VoiceCommandActivatedEventArgs;
                SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;
                string voiceCommandName = speechRecognitionResult.RulePath[0];

                switch (voiceCommandName)
                {
                    case "startChat":
                        {
                            rootFrame.Navigate(typeof(MainPage), true);
                            break;
                        }
                    case "sendMessage":
                        if (speechRecognitionResult.SemanticInterpretation.Properties.ContainsKey("message"))
                        {
                            string message = speechRecognitionResult.SemanticInterpretation.Properties["message"][0];
                            rootFrame.Navigate(typeof(MainPage), message);
                        }
                        else
                        {
                            rootFrame.Navigate(typeof(MainPage), true);
                        }
                        break;
                    default:
                        {
                            rootFrame.Navigate(typeof(MainPage));

                            break;
                        }
                }
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), null);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }


    }
}
