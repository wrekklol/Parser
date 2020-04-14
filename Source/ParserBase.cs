using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Parser
{
    public abstract class ParserBase
    {
        public MainWindow MainWindowRef { get; } = (MainWindow)Application.Current.MainWindow;

        public ParserBase()
        {
            Settings.OnAddSettings += OnAddSettings;
            Settings.OnLoadSettings += OnLoadSettings;
            Settings.OnSaveSettings += OnSaveSettings;
        }

        protected abstract void OnAddSettings(Settings InSettings);
        protected abstract void OnLoadSettings(Settings InSettings);
        protected abstract void OnSaveSettings(Settings InSettings);
    }
}
