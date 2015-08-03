using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        ObservableCollection<Message> messages;
        BestFriendService.Bot bot;
        SpeechRecognizer speechRecognizer;

        bool listen = true;

        public MainPage()
        {
            this.InitializeComponent();
            
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            messages = new ObservableCollection<Message>();
            ListView.ItemsSource = messages;
            bot = new BestFriendService.Bot();

            if (e.Parameter != null && e.Parameter is bool)
            {
                var response = await bot.SendMessageAndGetResponseFromBot("hello there");
                messages.Add(new Message() { Text = "  > " + response });
                SpeechSynthesizer synt = new SpeechSynthesizer();
                SpeechSynthesisStream syntStream = await synt.SynthesizeTextToStreamAsync(response);
                Media.SetSource(syntStream, syntStream.ContentType);
               // await StartListenMode();
            }
            else if (e.Parameter != null && e.Parameter is string && !string.IsNullOrWhiteSpace(e.Parameter as string))
            {
                await SendMessage(e.Parameter as string, true);
               // await StartListenMode();
            }
        }

        private async void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(box.Text))
            {
                SendMessage(box.Text);
                box.Text = "";

            }
        }

        private async Task StartListenMode()
        {
            while (listen)
            {
                await SendMessage(await ListenForText(), true);
                await Task.Delay(1000);
            }
                
        }

        private async Task<string> ListenForText()
        {
            string result = "";
            InitSpeech();
            try
            {
                Listening.IsActive = true;
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
            }

            return result;

        }

        private async Task InitSpeech()
        {
            if (speechRecognizer == null)
            {
                try
                {
                    speechRecognizer = new SpeechRecognizer();
                    speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

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

        private async Task<string> SendMessage(string message, bool speak = false)
        {
            Debug.WriteLine("sending: " + message);
            messages.Add(new Message() { Text = message });

            var response = await bot.SendMessageAndGetResponseFromBot(message);
            messages.Add(new Message() { Text = "  > " + response });

            if (speak)
            {
                SpeechSynthesizer synt = new SpeechSynthesizer();
                SpeechSynthesisStream syntStream = await synt.SynthesizeTextToStreamAsync(response);
                Media.SetSource(syntStream, syntStream.ContentType);
            }

            return response;
        }

        private void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartListenMode();
        }
    }

    public class Message
    {
        public string Text { get; set; }
    }
}
