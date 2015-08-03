using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace BestFriendService
{
    public sealed class Service : XamlRenderingBackgroundTask
    {
        private BackgroundTaskDeferral serviceDeferral;
        VoiceCommandServiceConnection voiceServiceConnection;

        protected override async void OnRun(IBackgroundTaskInstance taskInstance)
        {
            this.serviceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            VoiceCommandUserMessage userMessage;
            VoiceCommandResponse response;
            try
            {
                voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                voiceServiceConnection.VoiceCommandCompleted += VoiceCommandCompleted;
                VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                switch (voiceCommand.CommandName)
                {
                    case "where":
                    
                        userMessage = new VoiceCommandUserMessage()
                        {
                            DisplayMessage = "Why don't you go outside, take a look, and find out for yourself!",
                            SpokenMessage = "Why don't you go outside, take a look, and find out for yourself!"
                        };
                        
                        response = VoiceCommandResponse.CreateResponse(userMessage);
                        await voiceServiceConnection.ReportSuccessAsync(response);

                        break;

                    
                    case "sendMessage":
                        var message = voiceCommand.Properties["message"][0];
                        var bot = new Bot();
                        string firstResponse = await bot.SendMessageAndGetResponseFromBot(message);

                        var responseMessage = new VoiceCommandUserMessage();
                        var responseMessage2 = new VoiceCommandUserMessage();
                        responseMessage.DisplayMessage = responseMessage.SpokenMessage = firstResponse;
                        responseMessage2.DisplayMessage = responseMessage2.SpokenMessage = "did you not hear me?";
                        
                        response = VoiceCommandResponse.CreateResponse(responseMessage);
                        await voiceServiceConnection.ReportSuccessAsync(response);

                        break;

                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (this.serviceDeferral != null)
                {
                    //Complete the service deferral
                    this.serviceDeferral.Complete();
                }
            }
        }

        
        private void VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }

        private bool CurrentCityMatches(string city)
        {
            return false;
        }
        
    }

}
