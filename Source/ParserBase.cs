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
            ParserSettings.OnAddSettings += OnAddSettings;
            ParserSettings.OnLoadSettings += OnLoadSettings;
            ParserSettings.OnSaveSettings += OnSaveSettings;
        }

        protected abstract void OnAddSettings();
        protected abstract void OnLoadSettings();
        protected abstract void OnSaveSettings();
    }
}
