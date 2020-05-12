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

        public SettingsTextBox ClientLogPathTextBox { get; set; }
        public SettingsTextBox MinPriceTextBox { get; set; }

        public static string ClientLogPath { get; set; } = GetConfig("Parser", "LogPath", @"C:\Program Files (x86)\Steam\steamapps\common\Path of Exile\logs\Client.txt");
        public static double MinPriceForNotify { get; set; } = double.Parse(GetConfig("Parser", "MinPriceForNotify", "10")); // How many chaos worth for notify

        public static string ParsedLogPath { get; } = App.AppPath + "\\ParsedLogs.json";
        public static List<string> ParsedLogEntries { get; private set; } = MiscLibrary.ReadFromJsonFile<List<string>>(ParsedLogPath);
        public static List<string> RawLogEntries { get; private set; } = new List<string>();
        public static List<LogEntry> LogEntries { get; private set; } = new List<LogEntry>();
        public static Dictionary<GameCurrency, double> CurrencyValues { get; } = new Dictionary<GameCurrency, double>() { { GameCurrency.ChaosOrb, 1 }, { GameCurrency.UnknownCurrency, 0 } };

        public static event Action<LogEntry> OnNewLogEntry;



        public LogParser()
        {
            ParserSettings.OnAddSettings += OnAddSettings;
            ParserSettings.OnSaveSettings += OnSaveSettings;
            MainWindow.OnMainWindowInit += OnMainWindowInit;
        }

        private void OnMainWindowInit()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (File.Exists(@ClientLogPath))
                        ReadLog();

                    await Task.Delay(500).ConfigureAwait(false);
                }
            });
        }

        public void ReadLog()
        {
            LogEntries = new List<LogEntry>();

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

            string Log = LogReader.ReadToEnd();
            RawLogEntries = new List<string>
            (
                Log
                .Replace("\r", "", StringComparison.InvariantCultureIgnoreCase)
                .Split('\n')
                .Where(s => !string.IsNullOrWhiteSpace(s) && s.IndexOf("acf", 20, 20, StringComparison.Ordinal) != -1)
                .Except(ParsedLogEntries)
                .Distinct()
            );

            ParsedLogEntries.AddRange(RawLogEntries);
            MiscLibrary.WriteToJsonFile(ParsedLogPath, ParsedLogEntries);
            LogReader.Close();

            ParseLog();
        }

        private void ParseLog()
        {
            if (RawLogEntries.Count <= 0)
                return;

            foreach (string RawLogEntry in RawLogEntries)
            {
                string[] EntryData = RawLogEntry.Split(' ', 8);
                DateTime EntryTime = DateTime.Parse(EntryData[0] + " " + EntryData[1], null, DateTimeStyles.AssumeLocal);

                if (EntryData.Length <= 7 || (DateTime.Now - EntryTime).TotalSeconds > INVALIDATETIME)
                    continue;

                LogEntry LogEntry = new LogEntry
                {
                    LogTime = EntryTime,
                    Raw = RawLogEntry
                };

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
            ClientLogPathTextBox = ParserSettings.AddSetting<SettingsTextBox>("LogPath", new SettingsTextBox("Client Log Path: ", ClientLogPath));
            ClientLogPathTextBox._TextBox.TextChanged += ClientLogPathTextBox_TextChanged;

            MinPriceTextBox = ParserSettings.AddSetting<SettingsTextBox>("MinPrice", new SettingsTextBox("Minimum Price in Chaos for Notify: ", MinPriceForNotify.ToString()));
            MinPriceTextBox.PreviewTextInput += MinPriceTextBox_PreviewTextInput;
            DataObject.AddPastingHandler(MinPriceTextBox, MinPriceTextBox_OnPaste);
            MinPriceTextBox._TextBox.TextChanged += MinPriceTextBox_TextChanged;
        }

        protected void OnSaveSettings()
        {
            Cfg["Parser"]["LogPath"] = ClientLogPathTextBox.Text;
            Cfg["Parser"]["MinPriceForNotify"] = MinPriceTextBox.Text;
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
