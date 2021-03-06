﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Data.Json;
using Windows.Foundation;

namespace BestFriendService
{
    // very simple implementation for the Personality Forge API
    // www.personalityforge.com
    public sealed class Bot
    {
        // You will need to create your own API key and enable simple API
        private string key = "<INSERT YOUR API KEY HERE>";

        // You will need to either use a bot that  has been enabled for the API
        // or create your own bot. I've been using the example bot from the API
        // but be warned, it's very much rated R. The id is 6
        private string botId = "<INSERT YOUR BOT NUMBER HERE>";
        
        public IAsyncOperation<string> SendMessageAndGetResponseFromBot(string message)
        {
            return Task.Run<string>(async () =>
            {
                if (key == "<INSERT YOUR API KEY HERE>" || botId == "<INSERT YOUR BOT NUMBER HERE>")
                {
                    return "Please update the API key and/or botId in Bot.cs in order to talk to me!";
                }

                string msg = "";

                try
                {
                    HttpClient client = new HttpClient();
                    string uri = "http://www.personalityforge.com/api/chat/?apiKey=" + key +
                                 "&chatBotID=" + botId +
                                 "&message=" + message +
                                 "&externalID=demo";
                    string response = await client.GetStringAsync(new Uri(uri));
                    
                    Match match = Regex.Match(response, "{\"success\".*}");
                    string json = match.ToString();

                    var jObject = JsonObject.Parse(json);
                    msg = jObject["message"].GetObject()["message"].GetString();

                }
                catch (Exception e)
                {
                    // no op
                    Debug.WriteLine(e.Message);
                }

                return msg;
            }).AsAsyncOperation();
        }
    }
}
