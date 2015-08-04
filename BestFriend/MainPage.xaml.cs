using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BestFriend
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Message> messages;
        private BestFriendService.Bot bot;
        private SpeechRecognizer speechRecognizer;
        private SpeechRecognizer speechRecognizerContinuous;
        private ManualResetEvent manualResetEvent;

        bool listening = false;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            messages = new ObservableCollection<Message>();
            ListView.ItemsSource = messages;
            bot = new BestFriendService.Bot();

            messages.CollectionChanged += Messages_CollectionChanged;

            manualResetEvent = new ManualResetEvent(false);

            Media.MediaEnded += Media_MediaEnded;

            InitContiniousRecognition();

            if (e.Parameter != null && e.Parameter is bool)
            {
                var response = await bot.SendMessageAndGetResponseFromBot("hello there");
                messages.Add(new Message() { Text = "  > " + response });
                await SpeakAsync(response);
                await SetListening(true);
            }
            else if (e.Parameter != null && e.Parameter is string && !string.IsNullOrWhiteSpace(e.Parameter as string))
            {
                await SendMessage(e.Parameter as string, true);
                await SetListening(true);
            }
        }

        private async void StartListenMode()
        {
            
            while (listening)
            {
                string spokenText = await ListenForText();
                while (string.IsNullOrWhiteSpace(spokenText) && listening)
                    spokenText = await ListenForText();

                if (spokenText.ToLower().Contains("stop listening"))
                {
                    speechRecognizer.UIOptions.AudiblePrompt = "Are you sure you want me to stop listening?";
                    speechRecognizer.UIOptions.ExampleText = "Yes/No";
                    speechRecognizer.UIOptions.ShowConfirmation = false;
                    SpeakAsync(speechRecognizer.UIOptions.AudiblePrompt);
                    var result = await speechRecognizer.RecognizeWithUIAsync();

                    if (!string.IsNullOrWhiteSpace(result.Text) && result.Text.ToLower() == "yes")
                    {
                        await SetListening(false);
                    }
                }

                if (listening)
                {
                    await SendMessage(spokenText, true);
                }
            }
                
        }

        private async Task<string> ListenForText()
        {
            string result = "";
            await InitSpeech();
            try
            {
                Listening.IsActive = true;
                text.Text = "Listening...";
                SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
                if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    result = speechRecognitionResult.Text;
                }
            } 
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Listening.IsActive = false;
                text.Text = "";
            }

            return result;

        }

        private async Task SpeakAsync(string toSpeak)
        {
            text.Text = "Speaking...";
            SpeechSynthesizer speechSyntesizer = new SpeechSynthesizer();
            SpeechSynthesisStream syntStream = await speechSyntesizer.SynthesizeTextToStreamAsync(toSpeak);
            Media.SetSource(syntStream, syntStream.ContentType);

            Task t = Task.Run(() =>
            {
                manualResetEvent.Reset();
                manualResetEvent.WaitOne();
            });

            await t;
            text.Text = "";
        }

        private async Task InitSpeech()
        {
            if (speechRecognizer == null)
            {
                try
                {
                    speechRecognizer = new SpeechRecognizer();

                    SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();

                    if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                        throw new Exception();

                    Debug.WriteLine("SpeechInit AOK");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SpeechInit Failed");
                    speechRecognizer = null;
                }
            }
        }

        private async Task InitContiniousRecognition()
        {
            try
            {

                if (speechRecognizerContinuous == null)
                {
                    speechRecognizerContinuous = new SpeechRecognizer();
                    speechRecognizerContinuous.Constraints.Add(new SpeechRecognitionListConstraint(new List<String>() { "Start Listenning" }, "start"));
                    speechRecognizerContinuous.Constraints.Add(new SpeechRecognitionListConstraint(new List<String>() { "Stop Listenning" }, "stop"));
                    SpeechRecognitionCompilationResult contCompilationResult = await speechRecognizerContinuous.CompileConstraintsAsync();
                    if (contCompilationResult.Status != SpeechRecognitionResultStatus.Success)
                    {
                        throw new Exception();
                    }
                    speechRecognizerContinuous.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                }

                await speechRecognizerContinuous.ContinuousRecognitionSession.StartAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                if (args.Result.Text == "Start Listenning")
                {
                    Media.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        SetListening(true);
                    });
                }
            }
        }

        private async Task<string> SendMessage(string message, bool speak = false)
        {
            Debug.WriteLine("sending: " + message);
            messages.Add(new Message() { Text = message });

            var response = await bot.SendMessageAndGetResponseFromBot(message);
            messages.Add(new Message() { Text = "  > " + response });

            if (speak)
            {
                Debug.WriteLine("starting to speak");
                await SpeakAsync(response);
                Debug.WriteLine("done speaking");

            }

            return response;
        }

        private async Task SetListening(bool toListen)
        {
            if (toListen)
            {
                listening = true;
                text.IsEnabled = false;
                symbol.Symbol = Symbol.FontColor;

                if (speechRecognizerContinuous != null)
                    await speechRecognizerContinuous.ContinuousRecognitionSession.CancelAsync();

                StartListenMode();
            }
            else
            {
                listening = false;
                text.IsEnabled = true;
                symbol.Symbol = Symbol.Microphone;
                Listening.IsActive = false;
                text.Text = "";

                if (speechRecognizerContinuous != null)
                    await speechRecognizerContinuous.ContinuousRecognitionSession.StartAsync();
            }
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ListView.ScrollIntoView(messages.Last(), ScrollIntoViewAlignment.Leading);
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            manualResetEvent.Set();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetListening(!listening);
        }
        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(box.Text))
            {
                SendMessage(box.Text);
                box.Text = "";

            }
        }
    }

    public class Message
    {
        public string Text { get; set; }
    }
}
