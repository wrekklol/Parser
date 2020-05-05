using IniParser;
using IniParser.Model;
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
                    return _CfgPath = @Directory.GetCurrentDirectory() + "\\config.ini";

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

        public static bool GetConfig(out string OutString, string InCategory, string InProperty)
        {
            string a = Cfg[InCategory][InProperty];
            OutString = string.IsNullOrEmpty(a) ? "" : a;

            return !string.IsNullOrEmpty(OutString);
        }
    }
}
