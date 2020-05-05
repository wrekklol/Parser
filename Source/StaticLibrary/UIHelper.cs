using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Parser.StaticLibrary
{
    public static class UIHelper
    {
        public static Size MeasureString(string InStr, Typeface InTypeface, double InFontSize)
        {
            if (InTypeface == null)
                InTypeface = new Typeface(new FontFamily("Roboto"), FontStyles.Normal, FontWeights.Regular, FontStretches.Normal);

            FormattedText t = new FormattedText(InStr, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, InTypeface, InFontSize, Brushes.Black, new NumberSubstitution(), 1);
            return new Size(t.Width, t.Height);
        }

        public static Size MeasureString(UIElement InElement)
        {
            string s = "";
            Typeface tf = null;
            double fs = 0;

            switch (InElement)
            {
                case Label l:
                    s = l.Content.ToString();
                    tf = new Typeface(l.FontFamily, l.FontStyle, l.FontWeight, l.FontStretch);
                    fs = l.FontSize;
                    break;
            }

            FormattedText t = new FormattedText(s, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, tf, fs, Brushes.Black, new NumberSubstitution(), 1);
            return new Size(t.Width, t.Height);
        }
    }
}
