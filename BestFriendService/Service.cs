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

                        var city = voiceCommand.Properties["city"][0];

                        var imageFile = await GenerateWideIconWithCity(city);
                        var localFolder = ApplicationData.Current.LocalFolder;
                        StorageFile cityIcon = await localFolder.GetFileAsync(imageFile);

                        var contentTiles = new List<VoiceCommandContentTile>();
                        var tile1 = new VoiceCommandContentTile();
                        tile1.ContentTileType = VoiceCommandContentTileType.TitleWith280x140IconAndText;
                        tile1.AppLaunchArgument = city;
                        tile1.Image = cityIcon;
                        contentTiles.Add(tile1);

                        userMessage = new VoiceCommandUserMessage()
                        {
                            DisplayMessage = "Here you go Best Friend, it's " + city,
                            SpokenMessage = "Here you go Best Friend, it's " + city
                        };

                        response = VoiceCommandResponse.CreateResponse(userMessage, contentTiles);
                        await voiceServiceConnection.ReportSuccessAsync(response);

                        break;

                    
                    case "sendMessageInCanvas":
                        var message = voiceCommand.Properties["message"][0];
                        var bot = new Bot();
                        string firstResponse = await bot.SendMessageAndGetResponseFromBot(message);

                        var responseMessage = new VoiceCommandUserMessage();
                        responseMessage.DisplayMessage = responseMessage.SpokenMessage = "Your Best Friend says \"" + firstResponse + "\"";
                        
                        response = VoiceCommandResponse.CreateResponse(responseMessage);
                        await voiceServiceConnection.ReportSuccessAsync(response);

                        break;

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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

        private async Task<String> GenerateWideIconWithCity(string city)
        {
            string filename = city + ".png";

            try
            {
                var pkgFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                var assetsFolder = await pkgFolder.GetFolderAsync("Assets");
                var xamlFile = await assetsFolder.GetFileAsync("cityTile.xml");
                var xamlContent = await FileIO.ReadTextAsync(xamlFile);
                Windows.UI.Xaml.FrameworkElement drawRoot = (FrameworkElement)XamlReader.Load(xamlContent);

                Image image = (Image)drawRoot.FindName("Image");
                TextBlock name = (TextBlock)drawRoot.FindName("Name");

                image.Source = new BitmapImage(new Uri("ms-appx:///Assets/city/" + city.ToLower() + ".jpg"));
                name.Text = city;


                RenderTargetBitmap rtb = new RenderTargetBitmap();
                await rtb.RenderAsync(drawRoot, 280, 140);
                var buffer = await rtb.GetPixelsAsync();
                byte[] outputArray = buffer.ToArray();

                var localFolder = ApplicationData.Current.LocalFolder;
                var rtbData = await localFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                using (var rtbStream = await rtbData.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, rtbStream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, 96, 96, outputArray);
                    await encoder.FlushAsync();
                }


            }
            catch (Exception ex)
            {
                var s = ex.Message;
                filename = null;
            }

            return filename;
        }
    }

}
