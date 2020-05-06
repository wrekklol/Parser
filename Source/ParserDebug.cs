using Parser.StaticLibrary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Converters;
using System.Windows.Markup;
using System.Windows.Media;
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

            Rect Pos = Rect.Parse(GetConfig("Debug", "ConsolePos", "0,0,1000,500"));

            NativeMethods.AllocConsole();
            NativeMethods.MoveWindow(NativeMethods.GetConsoleWindow(), (int)Pos.X, (int)Pos.Y, (int)Pos.Width, (int)Pos.Height, true);

            bShouldGetCurrency = bool.Parse(GetConfig("Debug", "ShouldFetchCurrency", bShouldGetCurrency.ToString()));
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
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Normal,
                FontFamily = new FontFamily("Roboto"),
                FontSize = 14,
                IsChecked = bool.Parse(GetConfig("Debug", "ShouldFetchCurrency", bShouldGetCurrency.ToString()))
            });
            FetchCurrencyDebug.Click += (sender, e) => { bShouldGetCurrency = FetchCurrencyDebug.IsChecked ?? false; };
        }

        private void OnSaveSettings() 
        {
            Cfg["Debug"]["ShouldFetchCurrency"] = FetchCurrencyDebug.IsChecked.ToString();
            Cfg["Debug"]["ConsolePos"] = NativeMethods.GetWindowRect(NativeMethods.GetConsoleWindow()).ToString();
        }

        private CheckBox FetchCurrencyDebug { get; set; }
#endif
    }
}
