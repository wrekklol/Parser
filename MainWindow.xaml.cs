using Newtonsoft.Json.Linq;
using Onova;
using Onova.Services;
using Parser.StaticLibrary;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Parser
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //((App)Application.Current).WindowPlace.Register(this);
            App.WindowPlace.Register(this);

            if (App.PDebug.bShouldGetCurrency)
                MiscLibrary.GetAsync("https://poe.ninja/api/data/currencyoverview?league=Delirium&type=Currency", OnGetCurrencyValues).ConfigureAwait(false);

            Loaded += MainWindow_Loaded;
            LogParser.OnNewLogEntry += AddListLogEntry;

            MouseDown += (sender, e) => { LogEntries.SelectedItem = null; };

            VersionText.Header = $"v{Assembly.GetExecutingAssembly().GetName().Version}";
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ParserSettings.AddSettings();
        }

        private void OnGetCurrencyValues(string InData)
        {
            //todo: get values every hour instead
            JObject Values = JObject.Parse(InData);
            foreach (var x in Values)
            {
                if (x.Key != "lines")
                    continue;

                Logger.WriteLine("Started fetching currency values", true);
                foreach (var y in x.Value.AsJEnumerable())
                {
                    var CurrencyType = y.Value<string>("currencyTypeName");
                    var CurrencyValue = y.Value<JToken>("receive").Value<double>("value");
                    LogParser.CurrencyValues.Add(Enum.Parse<GameCurrency>(CurrencyHelper.GetTrimmedCurrencyName(CurrencyType), true), CurrencyValue);
                    Logger.WriteLine($"{Enum.Parse<GameCurrency>(CurrencyHelper.GetTrimmedCurrencyName(CurrencyType))} => {CurrencyValue}");
                }
                LogParser.CurrencyValues.Add(GameCurrency.UnknownCurrency, 0);
                Logger.WriteLine("Done fetching currency values", true);
            }
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
