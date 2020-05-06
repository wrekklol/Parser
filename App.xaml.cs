using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using Parser.StaticLibrary;

namespace Parser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string AppPath
        {
            get
            {
                if (_AppPath == null)
                    return _AppPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

                return _AppPath;
            }
            set
            {
                _AppPath = value;
            }
        }
        private static string _AppPath;

        public static SettingsWindow SettingsWindow { get; private set; }
        public static ParserDebug PDebug { get; private set; }
        public static SlackClient Slack { get; private set; }
        public static LogParser PoELogParser { get; private set; }

        public static bool bMainWindowInitialized { get; private set; }
        public static Dictionary<string, List<string>> Args { get; private set; }



        public App()
        {
            SettingsWindow = new SettingsWindow();
            PDebug = new ParserDebug();
            Slack = new SlackClient();
            PoELogParser = new LogParser();

            bMainWindowInitialized = false;
            Args = new Dictionary<string, List<string>>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            File.WriteAllText(AppPath + "\\Output.json", "[]");

            List<string> args = e.Args.ToList();
            //args.Add("-n");
            //args.Add("1000");
            //args.Add("-t");
            //args.Add("10000000");
            //args.Add("-filter");
            //args.Add("7");

            if (args.Count <= 0)
            {
                MainWindow = new MainWindow();
                MainWindow.Show();
                bMainWindowInitialized = true;
            }
            else
            {
                for (int i = 0; i < args.Count; i++)
                {
                    if (args[i][0] != '-')
                        continue;

                    string ArgName = args[i].Replace("-", "");
                    List<string> ArgParams = new List<string>();

                    for (int n = i + 1; n < args.Count; n++)
                    {
                        if (args[n][0] == '-')
                            break;

                        ArgParams.Add(args[n]);
                    }

                    Args.Add(ArgName, ArgParams);
                }

                // Pre-processing
                foreach (var Arg in Args)
                {
                    switch (Arg.Key)
                    {
                        case "n":
                            LogParser.PARSENUM = int.Parse(Arg.Value[0]);
                            break;
                        case "t":
                            LogParser.INVALIDATETIME = int.Parse(Arg.Value[0]);
                            break;
                    }
                }

                // Parse log
                PoELogParser.ReadLog();
                List<LogEntry> LogEntries = LogParser.LogEntries;

                // Post-processing
                foreach (var Arg in Args)
                {
                    switch (Arg.Key)
                    {
                        case "filter":
                            List<LogType> Filter = Arg.Value.Select(x => EnumHelper<LogType>.Parse(x)).ToList();
                            if (Filter.Count > 1)
                                LogEntries = PoELogParser.GetEntriesOfType(Filter);
                            else
                                LogEntries = PoELogParser.GetEntriesOfType(Filter[0]);
                            break;
                    }
                }

                var ArgsJson = JsonConvert.SerializeObject(LogEntries, Formatting.Indented);
                File.WriteAllText(AppPath + "\\Output.json", ArgsJson);

                Application.Current.Shutdown();
            }
        }
    }
}
