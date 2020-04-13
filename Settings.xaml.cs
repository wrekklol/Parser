using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Parser
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public static event Action<Settings> OnAddSettings;
        public static event Action<Settings> OnLoadSettings;
        public static event Action<Settings> OnSaveSettings;

        private Dictionary<string, UIElement> SettingsToAdd { get; } = new Dictionary<string, UIElement>();

        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
        public void AddSettings()
        {
            OnAddSettings?.Invoke(this);

            foreach (var s in SettingsToAdd)
                SettingsPanel.Children.Add(s.Value);

            LoadSettings();
            Closing += Settings_Closing;
            Closing += (sender, e) => SaveSettings();
        }

        public void LoadSettings()
        {
            OnLoadSettings?.Invoke(this);
        }

        public void SaveSettings()
        {
            OnSaveSettings?.Invoke(this);
        }

        public T AddSetting<T>(UIElement InUI) where T : UIElement
        {
            SettingsToAdd.Add(Guid.NewGuid().ToString(), InUI);
            return (T)InUI;
        }

        public T AddSetting<T>(string InName, UIElement InUI) where T : UIElement
        {
            if (string.IsNullOrEmpty(InName))
                InName = Guid.NewGuid().ToString();

            SettingsToAdd.Add(InName, InUI);
            return (T)SettingsToAdd[InName];
        }

        public T GetSetting<T>(string InName) where T : UIElement
        {
            return (T)SettingsToAdd[InName];
        }

        public UIElement AddSetting(UIElement InUI)
        {
            return AddSetting<UIElement>(InUI);
        }

        public UIElement AddSetting(string InName, UIElement InUI)
        {
            return AddSetting<UIElement>(InName, InUI);
        }
    }
}
