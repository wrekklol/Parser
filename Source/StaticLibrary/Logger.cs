using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parser.StaticLibrary
{
    public static class Logger
    {
        public static void WriteLine(string str, bool bAddSeperators = false)
        {
            if (string.IsNullOrEmpty(str))
                return;

            if (bAddSeperators)
            {
                string CurTime = $"[{DateTime.Now}] ";
                string[] lines =
                {
                    "--------------------------------------------------------------------------------------------------",
                    str,
                    "--------------------------------------------------------------------------------------------------"
                };

                foreach (string l in lines)
                {
                    Console.WriteLine(l);
                    File.AppendAllText(@Directory.GetCurrentDirectory() + "\\log.txt", $"{CurTime}{l}\n", new UTF8Encoding(false));
                }
            }
            else
            {
                Console.WriteLine(str);
                File.AppendAllText(@Directory.GetCurrentDirectory() + "\\log.txt", $"[{DateTime.Now}] {str}\n", new UTF8Encoding(false));
            }
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
