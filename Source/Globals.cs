﻿using IniParser;
using IniParser.Model;
using System.IO;

namespace Parser.Globals
{
    public static class Globals
    {
        public static ParserDebug PDebug { get; } = new ParserDebug();
        public static SlackClient Slack { get; } = new SlackClient("https://hooks.slack.com/services/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");



        // Config
        private static string CfgPath { get; } = @Directory.GetCurrentDirectory() + "\\config.ini";
        private static FileIniDataParser CfgParser { get; } = new FileIniDataParser();
        public static IniData Config { get; } = InitConfig();

        private static IniData InitConfig()
        {
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
    }
}
