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

using static Parser.StaticLibrary.Config;

namespace Parser
{
    public partial class MainWindow : Window
    {
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
            ParserSettings.AddSettings();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cfg["Parser"]["MainWindowPos"] = new Point(Left, Top).ConvertToString();
        }



        public void AddListLogEntry(LogEntry InLogEntry)
        {
            ItemCollection LogList = LogEntries.Items;
            if (InLogEntry != null && !LogList.Contains(InLogEntry) && InLogEntry.IsPrintableLogType())
            {
                ParserTextBlock l = new ParserTextBlock
                {
                    Text = InLogEntry.ToString(),
                    FontWeight = FontWeights.Heavy,
                    FontStyle = FontStyles.Normal,
                    FontFamily = new FontFamily("Roboto"),
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

                if (InLogEntry.IsTradeMessage() && CurrencyHelper.GetCurrencyWorth(InLogEntry.Offer) >= LogParser.MinPriceForNotify)
                    App.Slack.PostMessage(InLogEntry.ToString(), "Notifier", App.Slack.SlackUsername.Text);
            }
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
