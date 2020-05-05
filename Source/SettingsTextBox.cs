using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Parser
{
    public class SettingsTextBox : StackPanel
    {
        public Label _Label { get; }
        public TextBox _TextBox { get; }

        public SettingsTextBox(string InLabelText, string InTextBoxText, bool InbAddSeparator = false) : base()
        {
            Orientation = Orientation.Horizontal;

            Width = double.NaN;
            Height = double.NaN;

            _Label = new Label()
            {
                Content = InLabelText,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.ExtraBold,
                FontStyle = FontStyles.Normal,
                FontSize = 14
            };
            Children.Add(_Label);

            _TextBox = new TextBox()
            {
                Text = InTextBoxText,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
                MinWidth = 500,
                MinHeight = 28.2033333333333
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
                    FontSize = 14
                });
            }
        }
    }
}
