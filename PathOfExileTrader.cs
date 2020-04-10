using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;

using static Parser.StaticLibrary.TradeHelper;
using static Parser.StaticLibrary.InputHelper;
using static Parser.StaticLibrary.MiscLibrary;
using static Parser.StaticLibrary.NativeMethods;
using System.Windows.Controls;

using w = System.Windows;

namespace Parser
{
    public class PathOfExileTrader : BaseParser
    {
        bool bShouldDraw = false;
        readonly Random r = new Random();



        public PathOfExileTrader()
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

            PathOfExileItemGrid ItemGrid = ItemGrids["RegularStashTab"];
            for (int x = 0; x < ItemGrid.Size.X; x++)
            {
                for (int y = 0; y < ItemGrid.Size.Y; y++)
                {
                    IntPtr desktop = GetDC(IntPtr.Zero);
                    using (Graphics g = Graphics.FromHdc(desktop))
                    {
                        g.FillRectangle(Brushes.Red, CalcXCoord(ItemGrid, x) - 2, CalcYCoord(ItemGrid, y) - 2, 4, 4);
                    }
                    _ = ReleaseDC(IntPtr.Zero, desktop);
                }
            }
        }

        async void DoTrade(object sender, KeyEventArgs e)
        {
#if DEBUG
            if (e.Key == Key.A)
            {
                GetCursorPos(out Point p);
                Console.WriteLine(p);
            }

            if (e.Key == Key.Q)
            {
                GetCursorPos(out Point p);
                Console.WriteLine(p);
            }

            if (e.Key == Key.W)
            {
                PathOfExileItemGrid ItemGrid = ItemGrids["RegularStashTab"];
                for (int x = 0; x < ItemGrid.Slots.Count; x++)
                {
                    for (int y = 0; y < ItemGrid.Slots[x].Count; y++)
                    {
                        Task asd = Task.Run(() => MouseHelper.MoveMouse(ItemGrid.Slots[x][y].SlotCoord.X, ItemGrid.Slots[x][y].SlotCoord.Y, 12, 12, null));
                        await asd.ConfigureAwait(false);
                        await Task.Delay(100).ConfigureAwait(false);
                    }
                }
            }

            if (e.Key == Key.E)
            {
                bShouldDraw = !bShouldDraw;
            }

            if (e.Key == Key.T)
            {
                FocusProcess("PathOfExile_x64Steam");
                await Task.Delay(150).ConfigureAwait(false);
                FindSlots("Inventory");
            }

            if (e.Key == Key.R)
            {
                PathOfExileItemGrid ItemGrid = ItemGrids["RegularStashTab"];
                if (ItemGrid.Slots.Count <= 0)
                    return;

                FocusProcess("PathOfExile_x64Steam");

                // For debug
                Console.Clear();

                Task MoveToDefaultPoint = Task.Run(() => MouseHelper.MoveMouse(960, 900, 200, 180, null));
                await MoveToDefaultPoint.ConfigureAwait(false);
                await Task.Delay(r.Next(1000)).ConfigureAwait(false);

                List<Point> OccupiedSlots = FindOccupiedSlots(ItemGrid);
                for (int i = 0; i < OccupiedSlots.Count; i++)
                {
                    Point Slot = OccupiedSlots[i];

                    while (GetClipboardText() != null)
                    {
                        Console.WriteLine(ClearClipboard());
                        await Task.Delay(25 + r.Next(175)).ConfigureAwait(false);
                    }

                    if (GetClipboardText() != null)
                        Console.WriteLine("CLIPBOARD NOT EMPTY");

                    PathOfExileItemSlot InventorySlot = ItemGrid.Slots[Slot.X][Slot.Y];

                    Task MoveToSlot = Task.Run(() => MouseHelper.MoveMouse(InventorySlot.SlotCoord.X, InventorySlot.SlotCoord.Y, 12, 12, null));
                    await MoveToSlot.ConfigureAwait(false);

                    InputSimulator k = new InputSimulator();
                    while (GetClipboardText() == null || GetClipboardText().Length <= 0)
                    {
                        k.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                        await Task.Delay(25 + r.Next(175)).ConfigureAwait(false);
                    }

                    InventorySlot.ParseClipboard(GetClipboardText());

                    PathOfExileItem ItemBase = GetItemBase(InventorySlot.BaseItemName, InventorySlot.Rarity == "Magic");
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

                    bool bIsCurrency = Enum.TryParse(PathOfExileLogParser.GetRENAMETHISCurrencyName(InventorySlot.BaseItemName), out PathOfExileCurrency c);
                    Console.WriteLine($"Inventory slot {Slot} has {InventorySlot.StackAmount} {InventorySlot.GeneratedItemName}{InventorySlot.BaseItemName}" + (bIsCurrency ? $" which is worth {MainWindow.PoELogParser.CurrencyValues[c] * InventorySlot.StackAmount} in Chaos Orbs" : ""));
                }
            }
#endif
        }


        public override void OnAddSettings(Settings InSettings)
        {
            InSettings.AddSetting<TextBox>("GridDebug", new TextBox()
            {
                Text = "",
                HorizontalAlignment = w.HorizontalAlignment.Center,
                VerticalAlignment = w.VerticalAlignment.Top,
                TextWrapping = w.TextWrapping.Wrap,
                MinWidth = 500,
                MinHeight = 28.2033333333333
            });

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
                InSettings.AddSetting($"Find{g.Value.Name}SlotsButton", FindSlotsButton);
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
            InSettings.AddSetting($"ReloadGridsButton", ReloadGridsButton);
        }

        public override void OnLoadSettings(Settings InSettings)
        {

        }

        public override void OnSaveSettings(Settings InSettings)
        {

        }
    }
}
