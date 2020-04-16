using Parser.PathOfExile.StaticLibrary;
using Parser.StaticLibrary;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Parser
{
    public class ParserDebug
    {
        public bool bShouldGetCurrency { get; private set; } = true;

        public ParserDebug()
        {
#if DEBUG
            ParserSettings.OnAddSettings += OnAddSettings;
            ParserSettings.OnLoadSettings += OnLoadSettings;
            ParserSettings.OnSaveSettings += OnSaveSettings;

            NativeMethods.AllocConsole();

            bShouldGetCurrency = false;
#endif



            if (!bShouldGetCurrency)
                Logger.WriteLine("Warning: bShouldGetCurrency is false, which means no currency values will be set!");
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
            ComboBox cb = ParserSettings.AddSetting<ComboBox>("GridDebug", new ComboBox()
            {
                ItemsSource = TradeHelper.ItemGrids.Select(x => x.Key)
            }, true);
        }

        private void OnLoadSettings() { }
        private void OnSaveSettings() { }
#endif
    }
}
