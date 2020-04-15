using Newtonsoft.Json.Linq;
using Onova;
using Onova.Services;
using Parser.StaticLibrary;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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
        public static PathOfExile.LogParser PoELogParser { get; } = new PathOfExile.LogParser();
        public static PathOfExile.Trader PoETrader { get; } = new PathOfExile.Trader();
        public static Settings SettingsWindow { get; set; } = new Settings();
        private readonly IUpdateManager _UpdateManager = new UpdateManager(new GithubPackageResolver("wrekklol", "Parser", "Parser-*.zip"), new ZipPackageExtractor());
        //private readonly IUpdateManager _UpdateManager = new UpdateManager(
        //    new LocalPackageResolver(@"C:\Users\Lars\source\repos\Parser\Builds\", "*.zip"),
        //    new ZipPackageExtractor());

#if DEBUG
        public static ParserDebug PDebug { get; } = new ParserDebug();
#endif



        public MainWindow()
        {
            InitializeComponent();

            //_ = MiscLibrary.GetAsync("https://poe.ninja/api/data/currencyoverview?league=Delirium&type=Currency", OnGetCurrencyValues);

            Loaded += MainWindow_Loaded;
            PathOfExile.LogParser.OnNewLogEntry += AddListLogEntry;

            MouseDown += (sender, e) => { PoELogEntries.SelectedItem = null; };
            PoELogEntries.SelectionChanged += PoELogEntries_SelectionChanged;

            VersionText.Header = $"v{Assembly.GetExecutingAssembly().GetName().Version}";
        }

        private void PoELogEntries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PathOfExile.LogEntry LogEntry = (PoELogEntries.SelectedItem as ParserLogEntry)?.LogData as PathOfExile.LogEntry;
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

                Logger.WriteLine("Started fetching currency values", true);
                foreach (var y in x.Value.AsJEnumerable())
                {
                    var CurrencyType = y.Value<string>("currencyTypeName");
                    var CurrencyValue = y.Value<JToken>("receive").Value<double>("value");
                    PoELogParser.CurrencyValues.Add(Enum.Parse<PathOfExile.Currency>(PathOfExile.LogParser.GetRENAMETHISCurrencyName(CurrencyType), true), CurrencyValue);
                    Logger.WriteLine($"{Enum.Parse<PathOfExile.Currency>(PathOfExile.LogParser.GetRENAMETHISCurrencyName(CurrencyType))} => {CurrencyValue}");
                }
                Logger.WriteLine("Done fetching currency values", true);
            }
        }



        public void AddListLogEntry(PathOfExile.LogEntry InLogEntry)
        {
            ItemCollection LogList = PoELogEntries.Items;
            if (InLogEntry != null && !LogList.Contains(InLogEntry) && InLogEntry.LogType != PathOfExile.LogType.Insignificant)
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

                if (PathOfExile.TradeOffer.GetCurrencyWorth(InLogEntry.Offer) >= PathOfExile.LogParser.MINPRICEFORNOTIFY)
                    Slack.PostMessage(InLogEntry.ToString(), "Notifier", Slack.SlackUsername.Text);
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

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
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
            //Application.Current.Shutdown();  
        }
    }
}
