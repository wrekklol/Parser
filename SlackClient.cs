using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace Parser
{
    public class SlackClient
    {
        private readonly Uri AccessUrl;
        private readonly Encoding _encoding = new UTF8Encoding();

        public SlackClient(string urlWithAccessToken)
        {
            AccessUrl = new Uri(urlWithAccessToken);
        }

        public void PostMessage(string text, string username = null, string channel = null)
        {
            Payload payload = new Payload()
            {
                Channel = channel,
                Username = username,
                Text = text
            };

            PostMessage(payload);
        }

        public void PostMessage(Payload payload)
        {
            string payloadJson = JsonConvert.SerializeObject(payload);

            using WebClient client = new WebClient();
            NameValueCollection data = new NameValueCollection
            {
                ["payload"] = payloadJson
            };

            var response = client.UploadValues(AccessUrl, "POST", data);

            //The response text is usually "ok"  
            string responseText = _encoding.GetString(response);
        }
    }

    //This class serializes into the Json payload required by Slack Incoming WebHooks  
    public class Payload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
