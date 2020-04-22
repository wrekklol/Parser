using Parser.StaticLibrary;
using Parser.PathOfExile.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentBehaviourTree;
using WindowsInput;
using WindowsInput.Native;

using static Parser.PathOfExile.StaticLibrary.TradeHelper;
using static Parser.StaticLibrary.InputHelper;
using static Parser.StaticLibrary.MiscLibrary;
using static Parser.StaticLibrary.NativeMethods;
using static Parser.Globals.GlobalStatics;

using w = System.Windows;

namespace Parser.PathOfExile
{
    public class Trader : ParserBase
    {
        public List<ItemSlot> SellableItems = new List<ItemSlot>();

        private TestBehaviour asd = null;
        private InitSellablesBehaviour InitSellables = null;
        public ComboBox GridDebug { get; private set; }

        bool bShouldDraw = false;

        public Trader()
        {
            InitItemBases();
            LoadGrids();

            Keyboard.AddKeyDownHandler(MainWindowRef, (sender, e) => Task.Run(() => DebugTrade(sender, e)));

            //asd = new TestBehaviour();
            //asd.Init(this);
            //asd.Start();

            Task.Run(async () =>
            {
                while (true)
                {
                    DrawSlots();
                    await Task.Delay(0).ConfigureAwait(false);
                }
            });
        }

        public void DoTrade(LogEntry InTradeOffer)
        {
            if ((asd == null || !asd.IsRunning()) && InTradeOffer != null)
            {
                asd = new TestBehaviour();
                asd.Init(InTradeOffer);
                asd.Start();
            }
        }

        public void StopTrade()
        {
            if (asd != null)
            {
                asd.Stop();
            }
        }



        private void DrawSlots()
        {
            if (!bShouldDraw)
                return;

            string GridToDebug = ParserSettings.GetSettingContent<string>("GridDebug");
            if (!ItemGrids.ContainsKey(GridToDebug))
                return;

            ItemGrid Grid = ItemGrids[GridToDebug];
            for (int x = 0; x < Grid.Size.X; x++)
            {
                for (int y = 0; y < Grid.Size.Y; y++)
                {
                    IntPtr desktop = GetDC(IntPtr.Zero);
                    using (Graphics g = Graphics.FromHdc(desktop))
                    {
                        g.FillRectangle(Brushes.Red, CalcXCoord(Grid, x) - 2, CalcYCoord(Grid, y) - 2, 4, 4);
                    }
                    _ = ReleaseDC(IntPtr.Zero, desktop);
                }
            }
        }

        void DebugTrade(object sender, KeyEventArgs e)
        {
#if DEBUG
            switch (e.Key)
            {
                case Key.A:
                {
                    GetCursorPos(out Point p);
                    Logger.WriteLine(p);
                    break;
                }
                case Key.E:
                    bShouldDraw = !bShouldDraw;
                    break;
                case Key.T:
                {
                    DoTrade(new LogEntry());
                    break;
                }
                case Key.R:
                {
                    Logger.WriteLine($"Party check: {CheckIsInParty()}");
                    Logger.WriteLine($"Hideout check: {CheckPlayerInHideout()}");
                    break;
                }
            }
#endif
        }



        protected override void OnAddSettings()
        {
#if DEBUG
            foreach (var g in ItemGrids)
            {
                Button FindSlotsButton = new Button()
                {
                    Content = $"Find {g.Value.Name} Slots",
                    HorizontalAlignment = w.HorizontalAlignment.Center,
                    VerticalAlignment = w.VerticalAlignment.Top,
                    HorizontalContentAlignment = w.HorizontalAlignment.Center,
                    VerticalContentAlignment = w.VerticalAlignment.Center,
                    FontWeight = w.FontWeights.ExtraBold,
                    FontStyle = w.FontStyles.Normal,
                    FontSize = 14
                };
                FindSlotsButton.Click += (sender, EventArgs) => { FindSlots(g.Value.Name); };
                ParserSettings.AddSetting<Button>($"Find{g.Value.Name}SlotsButton", FindSlotsButton);
            }

            Button InitSellablesButton = new Button()
            {
                Content = $"Init Sellables",
                HorizontalAlignment = w.HorizontalAlignment.Center,
                VerticalAlignment = w.VerticalAlignment.Top,
                HorizontalContentAlignment = w.HorizontalAlignment.Center,
                VerticalContentAlignment = w.VerticalAlignment.Center,
                FontWeight = w.FontWeights.ExtraBold,
                FontStyle = w.FontStyles.Normal,
                FontSize = 14
            };
            InitSellablesButton.Click += (sender, EventArgs) => 
            {
                InitSellables = new InitSellablesBehaviour();
                InitSellables.Init(this);
                InitSellables.Start();
            };
            ParserSettings.AddSetting<Button>("InitSellablesButton", InitSellablesButton);

            Button ReloadGridsButton = new Button()
            {
                Content = $"Reload Grids",
                HorizontalAlignment = w.HorizontalAlignment.Center,
                VerticalAlignment = w.VerticalAlignment.Top,
                HorizontalContentAlignment = w.HorizontalAlignment.Center,
                VerticalContentAlignment = w.VerticalAlignment.Center,
                FontWeight = w.FontWeights.ExtraBold,
                FontStyle = w.FontStyles.Normal,
                FontSize = 14
            };
            ReloadGridsButton.Click += (sender, EventArgs) => { LoadGrids(); };
            ParserSettings.AddSetting<Button>("ReloadGridsButton", ReloadGridsButton, true);
#endif
        }

        protected override void OnLoadSettings() { }
        protected override void OnSaveSettings() { }
    }
}
