using Parser.StaticLibrary;
using System.Windows;
using System.Windows.Controls;

using static Parser.StaticLibrary.Config;

namespace Parser
{
    public class ParserDebug
    {
        public bool bShouldGetCurrency { get; private set; } = true;

        public ParserDebug()
        {
#if DEBUG
            ParserSettings.OnAddSettings += OnAddSettings;
            ParserSettings.OnSaveSettings += OnSaveSettings;

            NativeMethods.AllocConsole();

            //bShouldGetCurrency = false;
            if (GetConfig(out string ShouldFetchCurrency, "Debug", "ShouldFetchCurrency"))
                bShouldGetCurrency = bool.Parse(ShouldFetchCurrency);
#endif



            if (!bShouldGetCurrency)
                Logger.WriteLine("Warning: bShouldGetCurrency is false, which means no currency values will be set!");
        }

#if DEBUG
        private void OnAddSettings()
        {
            FetchCurrencyDebug = ParserSettings.AddSetting<CheckBox>(new CheckBox()
            {
                Content = "Should Fetch Currency on Startup",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.ExtraBold,
                FontStyle = FontStyles.Normal,
                FontSize = 14,
                IsChecked = bool.Parse(GetConfig("Debug", "ShouldFetchCurrency", bShouldGetCurrency.ToString()))
            });
            FetchCurrencyDebug.Click += (sender, e) => { bShouldGetCurrency = FetchCurrencyDebug.IsChecked ?? false; };
        }

        private void OnSaveSettings() 
        {
            Cfg["Debug"]["ShouldFetchCurrency"] = FetchCurrencyDebug.IsChecked.ToString();
        }

        private CheckBox FetchCurrencyDebug { get; set; }
#endif
    }
}
