using IniParser;
using IniParser.Model;
using System;
using System.IO;

namespace Parser.StaticLibrary
{
    public static class Config
    {
        private static string CfgPath
        {
            get
            {
                if (_CfgPath == null)
                    return _CfgPath = App.AppPath + "\\config.ini";

                return _CfgPath;
            }
            set
            {
                _CfgPath = value;
            }
        }
        private static string _CfgPath;
        private static FileIniDataParser CfgParser
        {
            get
            {
                if (_CfgParser == null)
                    return _CfgParser = new FileIniDataParser();

                return _CfgParser;
            }
            set
            {
                _CfgParser = value;
            }
        }
        private static FileIniDataParser _CfgParser;
        public static IniData Cfg 
        {
            get
            {
                if (_Cfg == null)
                    return _Cfg = InitConfig();

                return _Cfg;
            }
            set
            {
                _Cfg = value;
            }
        }
        private static IniData _Cfg;



        private static IniData InitConfig()
        {
            if (!File.Exists(CfgPath))
                File.Create(CfgPath).Dispose();

            CfgParser.Parser.Configuration.AllowCreateSectionsOnFly = true;
            CfgParser.Parser.Configuration.AllowKeysWithoutSection = false;
            CfgParser.Parser.Configuration.AssigmentSpacer = " ";
            CfgParser.Parser.Configuration.SkipInvalidLines = true;
            CfgParser.Parser.Configuration.OverrideDuplicateKeys = true;

            return CfgParser.ReadFile(CfgPath);
        }

        public static void SaveConfig()
        {
            CfgParser.WriteFile(CfgPath, Cfg);
        }

        public static bool GetConfig<T>(out T OutProperty, string InSection, string InProperty, T InDefaultValue = default)
        {
            string c = Cfg[InSection][InProperty];
            bool b = string.IsNullOrEmpty(c);

            OutProperty = b ? InDefaultValue : c.ConvertTo<T>();
            return !b;
        }

        public static T GetConfig<T>(string InSection, string InProperty, T InDefaultValue = default)
        {
            string c = Cfg[InSection][InProperty];
            return string.IsNullOrEmpty(c) ? InDefaultValue : c.ConvertTo<T>();
        }
    }
}
