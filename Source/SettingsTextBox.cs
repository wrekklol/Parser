using Parser.StaticLibrary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Parser
{
    public class SettingsTextBox : StackPanel
    {
        public Label _Label { get; }
        public TextBox _TextBox { get; }

        public string Text 
        { 
            get
            {
                return _TextBox.Text;
            } 
        }



        public SettingsTextBox(string InLabelText, string InTextBoxText, bool InbAddSeparator = false) : base()
        {
            Orientation = Orientation.Horizontal;

            _Label = new Label()
            {
                Content = InLabelText,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Normal,
                FontFamily = new FontFamily("Roboto"),
                FontSize = 14
            };
            Children.Add(_Label);

            _TextBox = new TextBox()
            {
                Text = InTextBoxText,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
                Width = 500 - UIHelper.MeasureString(_Label).Width,
                MinHeight = 28.2033333333333,
                FontWeight = FontWeights.Regular,
                FontStyle = FontStyles.Normal,
                FontFamily = new FontFamily("Roboto"),
                FontSize = 14
            };
            Children.Add(_TextBox);

            if (InbAddSeparator)
            {
                Children.Add(new Label()
                {
                    Content = "",//"_________________________________________________________________________________________________________________________________________________________________",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.ExtraBold,
                    FontStyle = FontStyles.Normal,
                    FontFamily = new FontFamily("Roboto"),
                    FontSize = 14
                });
            }
        }
    }
}
