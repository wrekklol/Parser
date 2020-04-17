using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parser.StaticLibrary
{
    public static class Logger
    {
        public static readonly string LogPath = @Directory.GetCurrentDirectory() + "\\log.txt";
        public static readonly UTF8Encoding Encoding = new UTF8Encoding(false);

        public static void WriteLine(string InString, bool InbAddSeperators = false)
        {
            if (MainWindow.SettingsWindow.Dispatcher.CheckAccess())
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
                MainWindow.SettingsWindow.Dispatcher.Invoke(() => WriteLine(InString, InbAddSeperators));
        }

        public static void WriteLine(object obj, bool bAddSeperators = false)
        {
            WriteLine(obj?.ToString(), bAddSeperators);

        }

        //public static void Write(string str, bool bAddSeperators = false)
        //{
        //    if (string.IsNullOrEmpty(str))
        //        return;

        //    Console.Write(str);
        //    File.AppendAllText(@Directory.GetCurrentDirectory() + "\\Log.txt", $"[{DateTime.Now}] {str}\n", new UTF8Encoding(false));
        //}

        //public static void Write(object obj, bool bAddSeperators = false)
        //{
        //    Write(obj?.ToString(), bAddSeperators);
        //}
    }
}
