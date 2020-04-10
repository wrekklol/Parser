﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

//H:\SteamLibrary\steamapps\common\Path of Exile\logs\Client.txt

namespace Parser
{
    public class PathOfExileLogParser : BaseParser
    {
        public const int PARSENUM = 20; // Num of log entries to parse
        public const int INVALIDATETIME = 60; // How many seconds before an entry is too "old"
        public const double MINPRICEFORNOTIFY = 10; // How many chaos worth for notify

        public string ClientLog { get; set; } = "";
        public TextBox ClientLogPath { get; set; }
        public string ParsedLogPath { get; } = @Directory.GetCurrentDirectory() + "\\ParsedLogs.json";

        public List<string> RawLogEntries { get; private set; } = new List<string>();
        public List<PathOfExileLogEntry> LogEntries { get; private set; } = new List<PathOfExileLogEntry>();
        public Dictionary<PathOfExileCurrency, double> CurrencyValues { get; } = new Dictionary<PathOfExileCurrency, double>() { { PathOfExileCurrency.ChaosOrb, 1 }, { PathOfExileCurrency.UnknownCurrency, 0 } };

        public static event Action<PathOfExileLogEntry> OnNewLogEntry;



        public PathOfExileLogParser() : base()
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
                    MainWindowRef.Dispatcher.Invoke(new Action(() => TryReadLog()));
                }
                catch (TaskCanceledException) { }
            }
        }

        private void ReadLog()
        {
            using (StreamReader LogReader = new StreamReader(File.Open(@ClientLogPath.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), true))
            {
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
                RawLogEntries = new List<string>(str.Replace("\r", "", StringComparison.InvariantCultureIgnoreCase).Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
                LogReader.Close();
            }

            ParseLog();
        }

        private void ParseLog()
        {
            if (RawLogEntries.Count <= 0)
                return;

            for (int i = 0; i < RawLogEntries.Count; i++)
            {
                string RawLogEntry = RawLogEntries[i];
                if (!char.IsDigit(RawLogEntry[0]))
                    continue;

                string[] str = RawLogEntry.Split(' ', 8); //todo: rename str
                PathOfExileLogEntry LogEntry = new PathOfExileLogEntry
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
                        LogEntry.LogType = PathOfExileLogType.TradeMessage;

                        string[] Currency = LogEntry.Message.Substring(" listed for ", " in ").Split(' ');
                        LogEntry.TradeOffer = new PathOfExileTradeOffer
                        {
                            Item = LogEntry.Message.Substring(" your ", " listed for "),
                            CurrencyAmount = double.Parse(Currency[0], CultureInfo.InvariantCulture.NumberFormat),
                            CurrencyType = PathOfExileTradeOffer.ParseCurrencyType(Currency[1]),
                            League = LogEntry.Message.Substring(" in ", " ")
                        };
                    }

                    LogEntry.LogType = LogEntry.IsTradeMessage() ? PathOfExileLogType.TradeMessage : PathOfExileLogType.NormalMessage;
                }
                else if (LogMessage.StartsWith(": AFK mode is now ON.", StringComparison.InvariantCultureIgnoreCase))
                    LogEntry.LogType = PathOfExileLogType.AfkNotification;
                else
                    LogEntry.LogType = PathOfExileLogType.Insignificant;

                LogEntries.Add(LogEntry);
                OnNewLogEntry?.Invoke(LogEntry);
                //MainWindowRef.AddListLogEntry(LogEntry);
            }
        }

        public static string GetRENAMETHISCurrencyName(string InCurrencyName)
        {
            return InCurrencyName.Replace("\"", "").Replace(" ", "").Replace("'", "").Replace("-", "");
        }



        public override void OnAddSettings(Settings InSettings)
        {
            InSettings.AddSetting(new Label()
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
            ClientLogPath = InSettings.AddSetting<TextBox>("PoELogPath", new TextBox()
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
                MinWidth = 500,
                MinHeight = 28.2033333333333
            });
        }

        public override void OnLoadSettings(Settings InSettings)
        {
            string p = Environment.GetEnvironmentVariable("PoELogPath", EnvironmentVariableTarget.User);
            ClientLogPath = InSettings.GetSetting<TextBox>("PoELogPath");
            ClientLogPath.Text = p;

            if (!File.Exists(ParsedLogPath) || new FileInfo(ParsedLogPath).Length == 0)
                return;

            using StreamReader r = new StreamReader(ParsedLogPath);
            List<string> LoadedEntries = JsonConvert.DeserializeObject<List<string>>(r.ReadToEnd());

            foreach (string Entry in LoadedEntries)
                LogEntries.Add(new PathOfExileLogEntry() { Raw = Entry }); //todo: define var which holds the loaded entries, and compare them with RawLogEntries in the ReadLog func

        }

        public override void OnSaveSettings(Settings InSettings)
        {
            Environment.SetEnvironmentVariable("PoELogPath", ClientLogPath.Text, EnvironmentVariableTarget.User);

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