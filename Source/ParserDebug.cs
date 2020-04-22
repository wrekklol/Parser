using Parser.PathOfExile.StaticLibrary;
using Parser.StaticLibrary;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using static Parser.Globals.GlobalStatics;

namespace Parser
{
    public class ParserDebug
    {
        public bool bShouldGetCurrency { get; private set; } = true;
        public bool bShouldPrintBTConstantly { get; private set; } = false;

        public ParserDebug()
        {
#if DEBUG
            ParserSettings.OnAddSettings += OnAddSettings;
            ParserSettings.OnLoadSettings += OnLoadSettings;
            ParserSettings.OnSaveSettings += OnSaveSettings;

            NativeMethods.AllocConsole();

            //bShouldGetCurrency = false;
            if (GetConfig(out string ShouldFetchCurrency, "Debug", "ShouldFetchCurrency"))
                bShouldGetCurrency = bool.Parse(ShouldFetchCurrency);

            bShouldPrintBTConstantly = true;
#endif



            if (!bShouldGetCurrency)
                Logger.WriteLine("Warning: bShouldGetCurrency is false, which means no currency values will be set!");
            if (bShouldPrintBTConstantly)
                Logger.WriteLine("Warning: bShouldPrintBTConstantly is true, which causes lag!");
        }

#if DEBUG
        private void OnAddSettings()
        {
            ParserSettings.AddSetting(new Label()
            {
                Content = "Grid Debug",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.ExtraBold,
                FontStyle = FontStyles.Normal,
                FontSize = 14
            });
            GridDebug = ParserSettings.AddSetting<ComboBox>("GridDebug", new ComboBox()
            {
                ItemsSource = TradeHelper.ItemGrids.Select(x => x.Key)
            }, true);

            FetchCurrencyDebug = ParserSettings.AddSetting<CheckBox>(new CheckBox()
            {
                Content = "Should Fetch Currency on Startup",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.ExtraBold,
                FontStyle = FontStyles.Normal,
                FontSize = 14
            });
            FetchCurrencyDebug.Click += (sender, e) => { bShouldGetCurrency = FetchCurrencyDebug.IsChecked ?? false; };
        }

        private void OnLoadSettings() 
        {
            //GridDebug = ParserSettings.GetSetting<ComboBox>("GridDebug");
            //string s = Config["Debug"]["Grid"];
            //if (!string.IsNullOrEmpty(s))
            //    GridDebug.SelectedItem = s;

            //string ShouldFetchCurrency = Config["Debug"]["ShouldFetchCurrency"];
            //if (!string.IsNullOrEmpty(ShouldFetchCurrency))
            //    FetchCurrencyDebug.IsChecked = bool.Parse(ShouldFetchCurrency);
            if (GetConfig(out string GridToDebug, "Debug", "Grid"))
                GridDebug.SelectedItem = GridToDebug;
            if (GetConfig(out string ShouldFetchCurrency, "Debug", "ShouldFetchCurrency"))
                FetchCurrencyDebug.IsChecked = bool.Parse(ShouldFetchCurrency);
        }
        private void OnSaveSettings() 
        {
            if (GridDebug.SelectedItem != null)
                Config["Debug"]["Grid"] = GridDebug.SelectedItem.ToString();

            Config["Debug"]["ShouldFetchCurrency"] = FetchCurrencyDebug.IsChecked.ToString();
        }

        private ComboBox GridDebug { get; set; }
        private CheckBox FetchCurrencyDebug { get; set; }
#endif
    }
}
