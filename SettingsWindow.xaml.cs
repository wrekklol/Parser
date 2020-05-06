using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

using static Parser.StaticLibrary.Config;

namespace Parser
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Point InitialPos { get; set; } = new Point(0, 0);

        public SettingsWindow()
        {
            InitializeComponent();

            ContentRendered += SettingsWindow_ContentRendered;
        }

        private void SettingsWindow_ContentRendered(object sender, EventArgs e)
        {
            Window w = Application.Current.MainWindow;

            InitialPos = Point.Parse(GetConfig("Parser", "SettingsPos", $"{(int)(w.Left + w.ActualWidth / 2 - ActualWidth / 2)},{(int)(w.Top + w.ActualHeight / 2 - ActualHeight / 2)}"));
            Left = InitialPos.X;
            Top = InitialPos.Y;
        }

        public void Settings_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;

            Point CurPos = new Point(Left, Top);
            if (CurPos != InitialPos && !double.IsNaN(Left) && !double.IsNaN(Top))
                Cfg["Parser"]["SettingsPos"] = CurPos.ConvertToString();
        }
    }
}
