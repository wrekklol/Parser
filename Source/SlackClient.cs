using Newtonsoft.Json;
using Parser.StaticLibrary;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using static Parser.StaticLibrary.Config;

namespace Parser
{
    public class SlackClient
    {
        private Uri AccessUrl { get; set; } = null;
        private readonly Encoding _encoding = new UTF8Encoding();

        public TextBox SlackUsername { get; set; }
        public TextBox SlackAccessUrl { get; set; }

        public SlackClient()
        {
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
            if (AccessUrl == null)
            {
                Logger.WriteLine("Error: Slack hook url was null or invalid.");
                return;
            }

            using WebClient c = new WebClient();
            NameValueCollection d = new NameValueCollection
            {
                ["payload"] = JsonConvert.SerializeObject(p)
            };

            // The response text is usually "ok"  
            string r = _encoding.GetString(c.UploadValues(AccessUrl, "POST", d));
        }

        public Uri CreateAccessUrl(string InUrl)
        {
            return !string.IsNullOrEmpty(InUrl) ? new Uri(InUrl) : null;
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
                Text = GetConfig("SlackClient", "AccessUrl", "U011432SY95"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
                MinWidth = 500,
                MinHeight = 28.2033333333333
            }, true);

            ParserSettings.AddSetting(new Label()
            {
                Content = "Slack AccessUrl",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.ExtraBold,
                FontStyle = FontStyles.Normal,
                FontSize = 14
            });
            SlackAccessUrl = ParserSettings.AddSetting<TextBox>("SlackAccessUrl", new TextBox()
            {
                Text = GetConfig("SlackClient", "AccessUrl"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
                MinWidth = 500,
                MinHeight = 28.2033333333333
            }, true);
            SlackAccessUrl.TextChanged += SlackAccessUrl_TextChanged;
        }

        private void OnLoadSettings()
        {
            AccessUrl = CreateAccessUrl(SlackAccessUrl.Text);
        }

        private void OnSaveSettings()
        {
            Cfg["SlackClient"]["Username"] = SlackUsername.Text;
            Cfg["SlackClient"]["AccessUrl"] = SlackAccessUrl.Text;//AccessUrl?.OriginalString;
        }

        private void SlackAccessUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            AccessUrl = CreateAccessUrl(SlackAccessUrl.Text);
        }
    }

    // This class serializes into the Json payload required by Slack Incoming WebHooks  
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
