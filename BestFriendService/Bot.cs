using System;
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
    public sealed class Bot
    {
        private string key = "Y5YF7tlYibRW0OsK";
        private string externalId;
        
        public IAsyncOperation<string> SendMessageAndGetResponseFromBot(string message)
        {
            return Task.Run<string>(async () =>
            {
                string msg = "";

                try
                {
                    HttpClient client = new HttpClient();
                    string uri = "http://www.personalityforge.com/api/chat/?apiKey=" + key +
                                 "&chatBotID=6" +
                                 "&message=" + message +
                                 "&externalID=nikolachatmode";
                    string response = await client.GetStringAsync(new Uri(uri));
                    //string response = "Checking origin: '*' (regex: '[a-z0-9]+')<br>Matched!<br>{\"success\":1,\"errorMessage\":\"\",\"message\":{\"chatBotName\":\"Desti\",\"chatBotID\":\"6\",\"message\":\"You're repeating yourself.\",\"emotion\":\"normal\"}}";

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
