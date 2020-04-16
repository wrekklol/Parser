using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using static Parser.Globals.Globals;

//H:\SteamLibrary\steamapps\common\Path of Exile\logs\Client.txt

namespace Parser.PathOfExile
{
    public class LogParser : ParserBase
    {
        public const int PARSENUM = 20; // Num of log entries to parse
        public const int INVALIDATETIME = 60; // How many seconds before an entry is too "old"
        public const double MINPRICEFORNOTIFY = 10; // How many chaos worth for notify

        public string ClientLog { get; set; } = "";
        public TextBox ClientLogPath { get; set; }
        public string ParsedLogPath { get; } = @Directory.GetCurrentDirectory() + "\\ParsedLogs.json";

        public List<string> RawLogEntries { get; private set; } = new List<string>();
        public List<LogEntry> LogEntries { get; private set; } = new List<LogEntry>();
        public Dictionary<Currency, double> CurrencyValues { get; } = new Dictionary<Currency, double>() { { Currency.ChaosOrb, 1 }, { Currency.UnknownCurrency, 0 } };

        public static event Action<LogEntry> OnNewLogEntry;



        public LogParser() : base()
        {
            MainWindowRef.Loaded += (sender, e) => 
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        TryReadLog();
                        await Task.Delay(100).ConfigureAwait(false);
                    }
                });
            };
        }

        private void TryReadLog()
        {
            if (MainWindowRef.Dispatcher.CheckAccess())
            {
                if (ClientLogPath != null && File.Exists(@ClientLogPath.Text))
                    ReadLog();
            }
            else
            {
                try
                {
                    MainWindowRef.Dispatcher?.Invoke(TryReadLog);
                }
                catch (TaskCanceledException) { }
            }
        }

        private void ReadLog()
        {
            using StreamReader LogReader = new StreamReader(File.Open(@ClientLogPath.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), true);
            LogReader.BaseStream.Seek(0, SeekOrigin.End);

            int i = 0;
            while ((i < PARSENUM + 1) && (LogReader.BaseStream.Position > 0))
            {
                LogReader.BaseStream.Position--;
                int c = LogReader.BaseStream.ReadByte();

                if (LogReader.BaseStream.Position > 0)
                    LogReader.BaseStream.Position--;

                if (c == Convert.ToInt32('\n'))
                    ++i;
            }

            string str = LogReader.ReadToEnd();
            RawLogEntries = new List<string>(collection: str.Replace("\r", "", StringComparison.InvariantCultureIgnoreCase).Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
            LogReader.Close();

            ParseLog();
        }

        private void ParseLog()
        {
            if (RawLogEntries.Count <= 0)
                return;

            foreach (string RawLogEntry in RawLogEntries)
            {
                if (!char.IsDigit(RawLogEntry[0]))
                    continue;

                string[] str = RawLogEntry.Split(' ', 8); //todo: rename str
                LogEntry LogEntry = new LogEntry
                {
                    LogTime = DateTime.Parse(str[0] + " " + str[1], null, DateTimeStyles.AssumeLocal),
                    Raw = RawLogEntry
                };

                TimeSpan asdd = DateTime.Now - LogEntry.LogTime;
                if (LogEntries.Contains(LogEntry) || str.Length <= 7 || asdd.TotalSeconds > INVALIDATETIME)
                    continue;

                string LogMessage = str[7];
                if (LogMessage.StartsWith("@From", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] PlayerMessage = LogMessage.Split(':', 2);
                    LogEntry.PlayerName = PlayerMessage[0].Remove(0, 6);
                    LogEntry.Message = PlayerMessage[1].TrimStart();

                    if(LogEntry.IsTradeMessage())
                    {
                        LogEntry.LogType = LogType.TradeMessage;

                        string[] Currency = LogEntry.Message.Substring(" listed for ", " in ").Split(' ');
                        LogEntry.Offer = new TradeOffer
                        {
                            Item = LogEntry.Message.Substring(" your ", " listed for "),
                            CurrencyAmount = double.Parse(Currency[0], CultureInfo.InvariantCulture.NumberFormat),
                            CurrencyType = TradeOffer.ParseCurrencyType(Currency[1]),
                            League = LogEntry.Message.Substring(" in ", " ")
                        };
                    }

                    LogEntry.LogType = LogEntry.IsTradeMessage() ? LogType.TradeMessage : LogType.NormalMessage;
                }
                else if (LogMessage.StartsWith(": AFK mode is now ON.", StringComparison.InvariantCultureIgnoreCase))
                    LogEntry.LogType = LogType.AfkNotification;
                else
                    LogEntry.LogType = LogType.Insignificant;

                LogEntries.Add(LogEntry);
                OnNewLogEntry?.Invoke(LogEntry);
            }
        }

        public static string GetRENAMETHISCurrencyName(string InCurrencyName)
        {
            return InCurrencyName != null
                ? InCurrencyName.Replace("\"", "").Replace(" ", "").Replace("'", "").Replace("-", "")
                : "";
        }


        protected override void OnAddSettings()
        {
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
            ClientLogPath = ParserSettings.AddSetting<TextBox>("PoELogPath", new TextBox()
            {
                Text = @"H:\SteamLibrary\steamapps\common\Path of Exile\logs\Client.txt",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
                MinWidth = 500,
                MinHeight = 28.2033333333333
            }, true);
        }

        protected override void OnLoadSettings()
        {
            ClientLogPath = ParserSettings.GetSetting<TextBox>("PoELogPath");
            ClientLogPath.Text = Config["PathofExileParser"]["LogPath"];

            if (!File.Exists(ParsedLogPath) || new FileInfo(ParsedLogPath).Length == 0)
                return;

            using StreamReader r = new StreamReader(ParsedLogPath);
            List<string> LoadedEntries = JsonConvert.DeserializeObject<List<string>>(r.ReadToEnd());

            foreach (string Entry in LoadedEntries)
                LogEntries.Add(new LogEntry() { Raw = Entry }); //todo: define var which holds the loaded entries, and compare them with RawLogEntries in the ReadLog func

        }

        protected override void OnSaveSettings()
        {
            Config["PathofExileParser"]["LogPath"] = ClientLogPath.Text;

            if (RawLogEntries.Count > PARSENUM)
                File.WriteAllText(ParsedLogPath, string.Empty);

            JsonSerializer s = new JsonSerializer();
            s.Formatting = Formatting.Indented;

            using StreamWriter sw = new StreamWriter(ParsedLogPath);
            using JsonWriter jw = new JsonTextWriter(sw);
            s.Serialize(jw, RawLogEntries);
        }
    }
}
