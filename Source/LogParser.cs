using Newtonsoft.Json;
using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using static Parser.StaticLibrary.Config;

//H:\SteamLibrary\steamapps\common\Path of Exile\logs\Client.txt

namespace Parser
{
    public class LogParser
    {
        public static int PARSENUM { get; set; } = 20; // Num of log entries to parse
        public static int INVALIDATETIME { get; set; } = 60; // How many seconds before an entry is too "old"

        public TextBox ClientLogPathTextBox { get; set; }
        public TextBox MinPriceTextBox { get; set; }

        public static string ClientLogPath { get; set; } = GetConfig("Parser", "LogPath", @"C:\Program Files (x86)\Steam\steamapps\common\Path of Exile\logs\Client.txt"); //= Cfg["Parser"]["LogPath"];
        public static string ParsedLogPath { get; } = App.AppPath + "\\ParsedLogs.json";
        public static double MinPriceForNotify { get; set; } = double.Parse(GetConfig("Parser", "MinPriceForNotify", "10")); //= double.Parse(Cfg["Parser"]["MinPriceForNotify"]); // How many chaos worth for notify

        public static List<string> RawLogEntries { get; private set; } = new List<string>();
        public static List<LogEntry> LogEntries { get; private set; } = new List<LogEntry>();
        public static Dictionary<GameCurrency, double> CurrencyValues { get; } = new Dictionary<GameCurrency, double>() { { GameCurrency.ChaosOrb, 1 }, { GameCurrency.UnknownCurrency, 0 } };

        public static event Action<LogEntry> OnNewLogEntry;



        public LogParser()
        {
            ParserSettings.OnAddSettings += OnAddSettings;
            ParserSettings.OnLoadSettings += OnLoadSettings;
            ParserSettings.OnSaveSettings += OnSaveSettings;

            Task.Run(async () =>
            {
                while (true)
                {
                    if (App.bMainWindowInitialized && File.Exists(@ClientLogPath))
                        ReadLog();

                    await Task.Delay(100).ConfigureAwait(false);
                }
            });
        }

        public void ReadLog()
        {
            using StreamReader LogReader = new StreamReader(File.Open(@ClientLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), true);
            LogReader.BaseStream.Seek(0, SeekOrigin.End);

            int i = 0;
            while (i < PARSENUM + 1 && LogReader.BaseStream.Position > 0)
            {
                LogReader.BaseStream.Position--;
                int c = LogReader.BaseStream.ReadByte();

                if (LogReader.BaseStream.Position > 0)
                    LogReader.BaseStream.Position--;

                if (c == Convert.ToInt32('\n'))
                    i++;
            }

            string str = LogReader.ReadToEnd();
            RawLogEntries = new List<string>
            (
                str
                .Replace("\r", "", StringComparison.InvariantCultureIgnoreCase)
                .Split('\n')
                .Where(s => !string.IsNullOrWhiteSpace(s) && s.IndexOf("acf", 20, 20, StringComparison.Ordinal) != -1)
                .Distinct()
            );
            LogReader.Close();

            ParseLog();
        }

        private void ParseLog()
        {
            if (RawLogEntries.Count <= 0)
                return;

            foreach (string RawLogEntry in RawLogEntries)
            {
                string[] EntryData = RawLogEntry.Split(' ', 8); //todo: rename str
                LogEntry LogEntry = new LogEntry
                {
                    LogTime = DateTime.Parse(EntryData[0] + " " + EntryData[1], null, DateTimeStyles.AssumeLocal),
                    Raw = RawLogEntry
                };

                if (EntryData.Length <= 7 || (DateTime.Now - LogEntry.LogTime).TotalSeconds > INVALIDATETIME || LogEntries.Contains(LogEntry))
                    continue;

                string LogMessage = EntryData[7];
                if (LogMessage.StartsWith("@From", StringComparison.InvariantCultureIgnoreCase))
                {
                    LogEntry.LogEntryType = LogType.NormalMessage;

                    string[] PlayerMessage = LogMessage.Split(':', 2);
                    LogEntry.PlayerName = PlayerMessage[0].Remove(0, 6);
                    LogEntry.Message = PlayerMessage[1].TrimStart();

                    if (LogEntry.IsTradeMessage())
                    {
                        LogEntry.LogEntryType = LogType.TradeMessage;

                        string[] Currency = LogEntry.Message.Substring(" listed for ", " in ").Split(' ');
                        LogEntry.Offer = new GameTradeOffer
                        {
                            Item = LogEntry.Message.Substring(" your ", " listed for "),
                            CurrencyAmount = double.Parse(Currency[0], CultureInfo.InvariantCulture.NumberFormat),
                            CurrencyType = CurrencyHelper.ParseCurrencyType(Currency[1]),
                            League = LogEntry.Message.Substring(" in ", " ")
                        };
                    }
                }
                else if (LogMessage.EndsWith(" the area.", StringComparison.InvariantCultureIgnoreCase))
                {
                    LogEntry.LogEntryType = LogMessage.Contains(" has joined", StringComparison.InvariantCultureIgnoreCase) ? LogType.EnterHideoutNotification : LogType.LeaveHideoutNotification;
                    LogEntry.PlayerName = LogMessage.Substring(": ", " has ").Trim();
                }
                else if (LogMessage.StartsWith(": Trade accepted.", StringComparison.InvariantCultureIgnoreCase))
                    LogEntry.LogEntryType = LogType.TradeAcceptedNotification;
                else if (LogMessage.StartsWith(": Trade cancelled.", StringComparison.InvariantCultureIgnoreCase))
                    LogEntry.LogEntryType = LogType.TradeCancelledNotification;
                else if (LogMessage.StartsWith(": AFK mode is now ON.", StringComparison.InvariantCultureIgnoreCase))
                    LogEntry.LogEntryType = LogType.AfkNotification;
                else
                    LogEntry.LogEntryType = LogType.Insignificant;

                LogEntries.Add(LogEntry);
                OnNewLogEntry?.Invoke(LogEntry);
            }
        }

        public List<LogEntry> GetEntriesOfType(LogType InLogType)
        {
            return LogEntries.Where(l => l.LogEntryType == InLogType).ToList();
        }

        public List<LogEntry> GetEntriesOfType(List<LogType> InLogTypes)
        {
            return LogEntries.Where(l => InLogTypes.Contains(l.LogEntryType)).ToList();
        }



        protected void OnAddSettings()
        {
            var test1 = ParserSettings.AddSetting(new SettingsTextBox("Test1: ", "Lort"));
            var test2 = ParserSettings.AddSetting(new SettingsTextBox("Test2: ", "Fisse", true));





            ParserSettings.AddSetting(new Label()
            {
                Content = "Client Log Location",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.ExtraBold,
                FontStyle = FontStyles.Normal,
                FontSize = 14
            });
            ClientLogPathTextBox = ParserSettings.AddSetting<TextBox>("LogPath", new TextBox()
            {
                Text = ClientLogPath,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
                MinWidth = 500,
                MinHeight = 28.2033333333333
            }, true);
            ClientLogPathTextBox.TextChanged += ClientLogPathTextBox_TextChanged;

            ParserSettings.AddSetting(new Label()
            {
                Content = "Minimum Price in Chaos for Notify",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.ExtraBold,
                FontStyle = FontStyles.Normal,
                FontSize = 14
            });
            MinPriceTextBox = ParserSettings.AddSetting<TextBox>("MinPrice", new TextBox()
            {
                Text = MinPriceForNotify.ToString(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
                MinWidth = 500,
                MinHeight = 28.2033333333333
            }, true);
            MinPriceTextBox.PreviewTextInput += MinPriceTextBox_PreviewTextInput;
            DataObject.AddPastingHandler(MinPriceTextBox, MinPriceTextBox_OnPaste);
            MinPriceTextBox.TextChanged += MinPriceTextBox_TextChanged;
        }

        protected void OnLoadSettings()
        {
            if (!File.Exists(ParsedLogPath) || new FileInfo(ParsedLogPath).Length == 0)
                return;

            using StreamReader r = new StreamReader(ParsedLogPath);
            List<string> LoadedEntries = JsonConvert.DeserializeObject<List<string>>(r.ReadToEnd());

            foreach (string Entry in LoadedEntries)
                LogEntries.Add(new LogEntry() { Raw = Entry }); //todo: define var which holds the loaded entries, and compare them with RawLogEntries in the ReadLog func
        }

        protected void OnSaveSettings()
        {
            Cfg["Parser"]["LogPath"] = ClientLogPathTextBox.Text;
            Cfg["Parser"]["MinPriceForNotify"] = MinPriceTextBox.Text;

            if (RawLogEntries.Count > PARSENUM)
                File.WriteAllText(ParsedLogPath, string.Empty);

            JsonSerializer s = new JsonSerializer();
            s.Formatting = Formatting.Indented;

            using StreamWriter sw = new StreamWriter(ParsedLogPath);
            using JsonWriter jw = new JsonTextWriter(sw);
            s.Serialize(jw, RawLogEntries);
        }

        private void ClientLogPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClientLogPath = ClientLogPathTextBox.Text;
        }

        private void MinPriceTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void MinPriceTextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var bIsText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!bIsText)
                return;

            Regex regex = new Regex("[^0-9]+");
            if (!regex.IsMatch(e.SourceDataObject.GetData(DataFormats.UnicodeText) as string))
                e.CancelCommand();
        }

        private void MinPriceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MinPriceForNotify = double.Parse(MinPriceTextBox.Text);
        }
    }
}
