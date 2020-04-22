using IniParser;
using IniParser.Model;
using System.IO;

namespace Parser.Globals
{
    public static class GlobalStatics
    {
        public static ParserDebug PDebug { get; } = new ParserDebug();
        public static SlackClient Slack { get; } = new SlackClient();



        // Config
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
        public static IniData Config 
        {
            get
            {
                if (_Config == null)
                    return _Config = InitConfig();

                return _Config;
            }
            set
            {
                _Config = value;
            }
        }
        private static IniData _Config;



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
            CfgParser.WriteFile(CfgPath, Config);
        }

        public static bool GetConfig(out string OutString, string InCategory, string InProperty)
        {
            string a = Config[InCategory][InProperty];
            OutString = string.IsNullOrEmpty(a) ? "" : a;

            return !string.IsNullOrEmpty(OutString);
        }
    }
}
