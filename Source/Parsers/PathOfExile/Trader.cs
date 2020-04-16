using Parser.StaticLibrary;
using Parser.PathOfExile.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;

using static Parser.PathOfExile.StaticLibrary.TradeHelper;
using static Parser.StaticLibrary.InputHelper;
using static Parser.StaticLibrary.MiscLibrary;
using static Parser.StaticLibrary.NativeMethods;
using static Parser.Globals.Globals;

using w = System.Windows;

namespace Parser.PathOfExile
{
    public class Trader : ParserBase
    {
        bool bShouldDraw = false;
        readonly Random r = new Random();

        public ComboBox GridDebug { get; private set; }



        public Trader()
        {
            InitItemBases();
            LoadGrids();

            Keyboard.AddKeyDownHandler(MainWindowRef, (sender, e) => Task.Run(() => DoTrade(sender, e)));

            Task.Run(async () =>
            {
                while (true)
                {
                    DrawSlots();
                    await Task.Delay(0).ConfigureAwait(false);
                }
            });
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

        async void DoTrade(object sender, KeyEventArgs e)
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
                case Key.Q:
                {
                    GetCursorPos(out Point p);
                    Logger.WriteLine(p);
                    break;
                }
                case Key.W:
                {
                    ItemGrid Grid = ItemGrids[ParserSettings.GetSettingContent<string>("GridDebug")];
                    foreach (Task t in from T in Grid.Slots
                        from T1 in T
                        select Task.Run(() => MouseHelper.MoveMouse(T1.SlotCoord.X, T1.SlotCoord.Y, 12, 12, null)))
                    {
                        await t.ConfigureAwait(false);
                        await Task.Delay(100).ConfigureAwait(false);
                    }

                    break;
                }
                case Key.E:
                    bShouldDraw = !bShouldDraw;
                    break;
                case Key.T:
                    FocusProcess("PathOfExile_x64Steam");
                    await Task.Delay(150).ConfigureAwait(false);
                    FindSlots("Inventory");
                    break;
                case Key.R:
                {
                    ItemGrid Grid = ItemGrids[ParserSettings.GetSettingContent<string>("GridDebug")];
                    if (Grid.Slots.Count <= 0)
                        return;

                    FocusProcess("PathOfExile_x64Steam");

                    // For debug
                    Console.Clear();

                    Task MoveToDefaultPoint = Task.Run(() => MouseHelper.MoveMouse(960, 900, 200, 180, null));
                    await MoveToDefaultPoint.ConfigureAwait(false);
                    await Task.Delay(r.Next(1000)).ConfigureAwait(false);

                    List<Point> OccupiedSlots = FindOccupiedSlots(Grid);
                    for (int i = 0; i < OccupiedSlots.Count; i++)
                    {
                        Point Slot = OccupiedSlots[i];

                        while (GetClipboardText() != null)
                        {
                            Logger.WriteLine(ClearClipboard());
                            await Task.Delay(25 + r.Next(175)).ConfigureAwait(false);
                        }

                        if (GetClipboardText() != null)
                            Logger.WriteLine(@"CLIPBOARD NOT EMPTY");

                        ItemSlot InvSlot = Grid.Slots[Slot.X][Slot.Y];

                        Task MoveToSlot = Task.Run(() => MouseHelper.MoveMouse(InvSlot.SlotCoord.X, InvSlot.SlotCoord.Y, 12, 12, null));
                        await MoveToSlot.ConfigureAwait(false);

                        InputSimulator k = new InputSimulator();
                        while (GetClipboardText() == null || GetClipboardText().Length <= 0)
                        {
                            k.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                            await Task.Delay(25 + r.Next(175)).ConfigureAwait(false);
                        }

                        InvSlot.ParseClipboard(GetClipboardText());

                        GameItem ItemBase = GetItemBase(InvSlot.BaseItemName, InvSlot.Rarity == "Magic");
                        if (ItemBase != null && (ItemBase.SizeX > 1 || ItemBase.SizeY > 1))
                        {
                            for (int x = Slot.X; x < Slot.X + ItemBase.SizeX; x++)
                            {
                                for (int y = Slot.Y; y < Slot.Y + ItemBase.SizeY; y++)
                                {
                                    if (x != Slot.X || y != Slot.Y)
                                    {
                                        OccupiedSlots.Remove(new Point(x, y));
                                    }
                                }
                            }
                        }

                        bool bIsCurrency = Enum.TryParse(LogParser.GetRENAMETHISCurrencyName(InvSlot.BaseItemName), out Currency c);
                        Logger.WriteLine($"Inventory slot {Slot} has {InvSlot.StackAmount} {InvSlot.GeneratedItemName}{InvSlot.BaseItemName}" + (bIsCurrency ? $" which is worth {MainWindow.PoELogParser.CurrencyValues[c] * InvSlot.StackAmount} in Chaos Orbs" : ""));
                    }

                    break;
                }
            }
#endif
        }



        protected override void OnAddSettings()
        {
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
        }

        protected override void OnLoadSettings()
        {
            GridDebug = ParserSettings.GetSetting<ComboBox>("GridDebug");
            GridDebug.SelectedItem = Config["Debug"]["Grid"];
        }

        protected override void OnSaveSettings()
        {
            Config["Debug"]["Grid"] = GridDebug.SelectedItem.ToString();
        }
    }
}
