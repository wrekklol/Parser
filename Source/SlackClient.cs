using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using static Parser.Globals.Globals;

namespace Parser
{
    public class SlackClient
    {
        private readonly Uri AccessUrl;
        private readonly Encoding _encoding = new UTF8Encoding();

        public TextBox SlackUsername { get; set; }

        public SlackClient(string urlWithAccessToken)
        {
            AccessUrl = new Uri(urlWithAccessToken);

            ParserSettings.OnAddSettings += OnAddSettings;
            ParserSettings.OnLoadSettings += OnLoadSettings;
            ParserSettings.OnSaveSettings += OnSaveSettings;
        }

        // Usage: Slack.PostMessage("Hello world!", "Name of the Message Poster", "#TestChannel", "YourSlackUsername");
        public void PostMessage(string InText, string InBotUsername = null, string InChannel = null, string InAtUsername = null)
        {
            Payload p = new Payload()
            {
                Channel = InChannel,
                Username = InBotUsername,
                Text = !string.IsNullOrEmpty(InAtUsername) ? $"<@{InAtUsername}> {InText}" : InText
            };

            PostMessage(p);
        }

        public void PostMessage(Payload p)
        {
            using WebClient c = new WebClient();
            NameValueCollection d = new NameValueCollection
            {
                ["payload"] = JsonConvert.SerializeObject(p)
            };

            //The response text is usually "ok"  
            string r = _encoding.GetString(c.UploadValues(AccessUrl, "POST", d));
        }



        private void OnAddSettings()
        {
            ParserSettings.AddSetting(new Label()
            {
                Content = "Slack Username",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.ExtraBold,
                FontStyle = FontStyles.Normal,
                FontSize = 14
            });
            SlackUsername = ParserSettings.AddSetting<TextBox>("SlackUsername", new TextBox()
            {
                Text = "U011432SY95",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
                MinWidth = 500,
                MinHeight = 28.2033333333333
            }, true);
        }

        private void OnLoadSettings()
        {
            SlackUsername = ParserSettings.GetSetting<TextBox>("SlackUsername");
            SlackUsername.Text = Config["SlackClient"]["Username"];
        }

        private void OnSaveSettings()
        {
            Config["SlackClient"]["Username"] = SlackUsername.Text;
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
