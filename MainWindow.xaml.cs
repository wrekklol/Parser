using Newtonsoft.Json.Linq;
using Onova;
using Onova.Services;
using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using static Parser.StaticLibrary.Config;

namespace Parser
{
    public partial class MainWindow : Window
    {
        public static event Action OnMainWindowInit;



        public MainWindow()
        {
            InitializeComponent();

            CurrencyHelper.FetchCurrency();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            LogParser.OnNewLogEntry += AddListLogEntry;

            MouseDown += (sender, e) => { LogEntries.SelectedItem = null; };

            VersionText.Header = $"v{Assembly.GetExecutingAssembly().GetName().Version}";

            Point Pos = Point.Parse(GetConfig("Parser", "MainWindowPos", "0,0"));
            Left = Pos.X;
            Top = Pos.Y;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            OnMainWindowInit?.Invoke();
            ParserSettings.AddSettings();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cfg["Parser"]["MainWindowPos"] = new Point(Left, Top).ConvertToString();
        }



        public void AddListLogEntry(LogEntry InLogEntry)
        {
            if (App.SettingsWindow.Dispatcher.CheckAccess())
            {
                ItemCollection LogList = LogEntries.Items;
                if (InLogEntry != null && !LogList.Contains(InLogEntry) && InLogEntry.IsPrintableLogType())
                {
                    double ExaltedWorth = CurrencyHelper.GetCurrencyWorth(GameCurrency.ExaltedOrb, 1);
                    double CurrencyWorth = InLogEntry.IsTradeMessage() ? CurrencyHelper.GetCurrencyWorth(InLogEntry.Offer) : -1;

                    FontWeight fw = FontWeights.Normal;
                    double fs = 14;

                    switch (InLogEntry.LogEntryType)
                    {
                        case LogType.Insignificant:
                        case LogType.AfkNotification:
                        case LogType.EnterHideoutNotification:
                        case LogType.LeaveHideoutNotification:
                        case LogType.TradeAcceptedNotification:
                        case LogType.TradeCancelledNotification:
                        case LogType.NormalMessage:
                            fw = FontWeights.ExtraLight;
                            fs = 10;
                            break;
                        case LogType.TradeMessage:
                            fw = FontWeights.Bold;
                            fs = Math.Clamp(6 * Math.Log10(CurrencyWorth / ExaltedWorth) + 18, 12, 24);
                            break;
                    }

                    ParserTextBlock l = new ParserTextBlock
                    {
                        Text = InLogEntry.ToString(),
                        FontWeight = fw,
                        FontStyle = FontStyles.Normal,
                        FontFamily = new FontFamily("Roboto"),
                        FontSize = fs,
                        LogData = InLogEntry,
                        AllowDrop = false,
                        Focusable = false,
                        IsHitTestVisible = false,
                        MinWidth = 780
                    };

                    LogList.Add(l);

                    if (CurrencyWorth >= LogParser.MinPriceForNotify)
                        App.Slack.PostMessage(InLogEntry.ToString(), "Notifier", App.Slack.SlackUsername.Text);
                }
            }
            else
                App.SettingsWindow.Dispatcher.Invoke(() => AddListLogEntry(InLogEntry));
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            App.SettingsWindow.Show();
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            LogEntries.Items.Clear();
        }

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            IUpdateManager _UpdateManager = new UpdateManager(new GithubPackageResolver("wrekklol", "Parser", "Parser-*.zip"), new ZipPackageExtractor());

            var check = await _UpdateManager.CheckForUpdatesAsync().ConfigureAwait(false);

            // If there are none, notify user and return
            if (!check.CanUpdate)
            {
                MessageBox.Show("There are no updates available.");
                return;
            }

            await _UpdateManager.PrepareUpdateAsync(check.LastVersion).ConfigureAwait(false);

            // Launch updater and exit
            _UpdateManager.LaunchUpdater(check.LastVersion);
            Close();
        }
    }
}
