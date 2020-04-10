﻿using Newtonsoft.Json.Linq;
using Parser.StaticLibrary;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

//if (Dispatcher.CheckAccess())
//{
//}
//else
//    Dispatcher.Invoke(new Action(() => AddListLogEntry(InLogEntry)));

namespace Parser
{
    public partial class MainWindow : Window
    {
        public static SlackClient Slack { get; } = new SlackClient("https://hooks.slack.com/services/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");
        public static PathOfExileLogParser PoELogParser { get; } = new PathOfExileLogParser();
        public static PathOfExileTrader PoETrader { get; } = new PathOfExileTrader();
        public static Settings SettingsWindow { get; set; } = new Settings();

#if DEBUG
        public static ParserDebug PDebug { get; } = new ParserDebug();
#endif



        public MainWindow()
        {
            InitializeComponent();

            _ = MiscLibrary.GetAsync("https://poe.ninja/api/data/currencyoverview?league=Delirium&type=Currency", OnGetCurrencyValues);

            Loaded += MainWindow_Loaded;
            PathOfExileLogParser.OnNewLogEntry += AddListLogEntry;

            MouseDown += (sender, e) => { PoELogEntries.SelectedItem = null; };
            PoELogEntries.SelectionChanged += PoELogEntries_SelectionChanged;
        }

        private void PoELogEntries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PathOfExileLogEntry LogEntry = (PoELogEntries.SelectedItem as ParserLogEntry)?.LogData as PathOfExileLogEntry;
            if (LogEntry == null || !LogEntry.IsTradeMessage())
            {
                BeginTradeButton.IsEnabled = false;
                return;
            }

            BeginTradeButton.IsEnabled = true;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsWindow.AddSettings();
        }

        private void OnGetCurrencyValues(string InData)
        {
            //todo: get values every hour instead
            JObject Values = JObject.Parse(InData);
            foreach (var x in Values)
            {
                if (x.Key != "lines")
                    continue;

                foreach (var y in x.Value.AsJEnumerable())
                {
                    var CurrencyType = y.Value<string>("currencyTypeName");
                    var CurrencyValue = y.Value<JToken>("receive").Value<double>("value");
                    PoELogParser.CurrencyValues.Add(Enum.Parse<PathOfExileCurrency>(PathOfExileLogParser.GetRENAMETHISCurrencyName(CurrencyType), true), CurrencyValue);
                    Console.WriteLine(Enum.Parse<PathOfExileCurrency>(PathOfExileLogParser.GetRENAMETHISCurrencyName(CurrencyType)));
                }
                Console.WriteLine(
                    "--------------------------------------------------------------------------------------------------\n" + 
                    "Done fetching currency values\n" +
                    "--------------------------------------------------------------------------------------------------\n\n");
            }
        }



        public void AddListLogEntry(PathOfExileLogEntry InLogEntry)
        {
            ItemCollection LogList = PoELogEntries.Items;
            if (InLogEntry != null && !LogList.Contains(InLogEntry) && InLogEntry.LogType != PathOfExileLogType.Insignificant)
            {
                ParserLogEntry l = new ParserLogEntry
                {
                    Text = InLogEntry.ToString(),
                    FontWeight = FontWeights.Heavy,
                    FontStyle = FontStyles.Normal,
                    FontSize = 14,
                    //Content = InLogEntry.ToString(),
                    LogData = InLogEntry,
                    AllowDrop = false,
                    Focusable = false,
                    IsHitTestVisible = false,
                    //IsTabStop = false,
                    //AcceptsReturn = false,
                    //AcceptsTab = false,
                    MinWidth = 780
                };

                LogList.Add(l);

                if (PathOfExileTradeOffer.GetCurrencyWorth(InLogEntry.TradeOffer) >= PathOfExileLogParser.MINPRICEFORNOTIFY)
                    Slack.PostMessage(username: "Notifier", text: InLogEntry.ToString(), channel: "#Path of Exile");
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow.Show();
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            PoELogEntries.Items.Clear();
        }
    }
}