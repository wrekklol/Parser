﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using static Parser.StaticLibrary.Config;

namespace Parser
{
    public static class ParserSettings
    {
        public static event Action OnAddSettings;
        public static event Action OnLoadSettings;
        public static event Action OnSaveSettings;

        public static volatile Dictionary<string, UIElement> SettingsToAdd = new Dictionary<string, UIElement>();

        public static void AddSettings()
        {
            SettingsWindow sw = App.SettingsWindow;

            OnAddSettings?.Invoke();

            foreach (var s in SettingsToAdd)
                sw.SettingsPanel.Children.Add(s.Value);

            LoadSettings();
            sw.Closing += sw.Settings_Closing;
            sw.Closing += (sender, e) => SaveSettings();
        }

        public static void LoadSettings()
        {
            OnLoadSettings?.Invoke();
        }

        public static void SaveSettings()
        {
            OnSaveSettings?.Invoke();
            SaveConfig();
        }

        public static T AddSetting<T>(UIElement InUI, bool InbAddSeparator = false) where T : UIElement
        {
            return AddSetting<T>(Guid.NewGuid().ToString(), InUI, InbAddSeparator);
        }

        public static T AddSetting<T>(string InName, UIElement InUI, bool InbAddSeparator = false) where T : UIElement
        {
            if (string.IsNullOrEmpty(InName))
                InName = Guid.NewGuid().ToString();

            SettingsToAdd.Add(InName, InUI);

            if (InbAddSeparator)
            {
                for (int i = 0; i < 2; i++)
                {
                    SettingsToAdd.Add(Guid.NewGuid().ToString(), new Label()
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

            return (T)SettingsToAdd[InName];
        }

        public static T GetSetting<T>(string InName) where T : UIElement
        {
            return (T)SettingsToAdd[InName];
        }

        public static T GetSettingContent<T>(string InName) where T : IConvertible
        {
            if (App.SettingsWindow.Dispatcher.CheckAccess())
            {
                if (!SettingsToAdd.ContainsKey(InName))
                    return default(T);

                UIElement Element = SettingsToAdd[InName];
                if (Element == null)
                    return default(T);

                object RetVal = default(T);
                switch (Element)
                {
                    case TextBox tb:
                        RetVal = tb.Text;
                        break;
                    case ComboBox cb:
                        if (cb.SelectedItem != null)
                            RetVal = cb.SelectedItem.ToString();
                        break;
                };

                return (T)RetVal;
            }
            else
                return App.SettingsWindow.Dispatcher.Invoke(new Func<T>(() => GetSettingContent<T>(InName)));
        }

        public static UIElement AddSetting(UIElement InUI)
        {
            return AddSetting<UIElement>(InUI);
        }

        public static UIElement AddSetting(string InName, UIElement InUI)
        {
            return AddSetting<UIElement>(InName, InUI);
        }
    }
}
