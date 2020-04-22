using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Parser
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            ((App)Application.Current).WindowPlace.Register(this);
        }

        public void Settings_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}
