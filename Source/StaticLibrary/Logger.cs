using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parser.StaticLibrary
{
    public static class Logger
    {
        public static readonly string LogPath = App.AppPath + "\\log.txt";
        public static readonly UTF8Encoding Encoding = new UTF8Encoding(false);

        public static void WriteLine(string InString, bool InbAddSeperators = false)
        {
            if (App.SettingsWindow.Dispatcher.CheckAccess())
            {
                if (string.IsNullOrEmpty(InString))
                    return;

                if (InbAddSeperators)
                {
                    string CurTime = $"[{DateTime.Now}] ";
                    string[] lines =
                    {
                    "--------------------------------------------------------------------------------------------------",
                    InString,
                    "--------------------------------------------------------------------------------------------------"
                };

                    foreach (string l in lines)
                    {
                        Console.WriteLine(l);
                        File.AppendAllText(LogPath, $"{CurTime}{l}\n", Encoding);
                    }
                }
                else
                {
                    Console.WriteLine(InString);
                    File.AppendAllText(LogPath, $"[{DateTime.Now}] {InString}\n", Encoding);
                }
            }
            else
                App.SettingsWindow.Dispatcher.Invoke(() => WriteLine(InString, InbAddSeperators));
        }

        public static void WriteLine(object obj, bool bAddSeperators = false)
        {
            WriteLine(obj?.ToString(), bAddSeperators);

        }
    }
}
