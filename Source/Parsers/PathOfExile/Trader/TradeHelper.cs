using FluentBehaviourTree;
using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;

//public readonly static int[] InventoryGridX = new int[] { 1274, 1326, 1379, 1432, 1484, 1537, 1590, 1642, 1695, 1748, 1800, 1853 };
//public readonly static int[] InventoryGridY = new int[] { 638, 690, 743, 796, 848 };

namespace Parser.PathOfExile.StaticLibrary
{
    public static class TradeHelper
    {
        public const double TRADETIMEOUT = 30;

        public static Dictionary<string, ItemGrid> ItemGrids { get; set; } = new Dictionary<string, ItemGrid>();
        public static string ItemGridsPath { get; } = @Directory.GetCurrentDirectory() + "\\Source\\Parsers\\PathOfExile\\ItemGrids.json";
        public static Dictionary<string, GameItem> ItemBases { get; set; } = new Dictionary<string, GameItem>();
        public static string ItemBasesPath { get; } = @Directory.GetCurrentDirectory() + "\\Source\\Parsers\\PathOfExile\\ItemBases.json";

        public static InputSimulator InputSim { get; } = new InputSimulator();



        public static void InitItemBases()
        {
            if (File.Exists(ItemBasesPath))
                ItemBases = MiscLibrary.ReadFromJsonFile<Dictionary<string, GameItem>>(ItemBasesPath);
        }

        public static GameItem GetItemBase(string InBaseName, bool InbIsMagicItem)
        {
            if (InbIsMagicItem)
            {
                foreach (var ItemBase in ItemBases)
                    if (InBaseName.Contains(ItemBase.Value.Name))
                        return ItemBase.Value;
            }
            else
            {
                foreach (var ItemBase in ItemBases)
                    if (ItemBase.Value.Name == InBaseName)
                        return ItemBase.Value;
            }

            return null;
        }



        public static double GetCurrencyWorth(Currency InCurrencyType, double InAmountOfCurrency)
        {
            return MainWindow.PoELogParser.CurrencyValues[InCurrencyType] * InAmountOfCurrency;
        }

        public static double GetCurrencyWorth(TradeOffer InTradeOffer)
        {
            return MainWindow.PoELogParser.CurrencyValues[InTradeOffer.CurrencyType] * InTradeOffer.CurrencyAmount;
        }

        public static string GetTrimmedCurrencyName(string InCurrencyName)
        {
            return InCurrencyName != null ? InCurrencyName.Replace("\"", "").Replace(" ", "").Replace("'", "").Replace("-", "") : "";
        }

        public static Currency ParseCurrencyType(string InRawType)
        {
            return InRawType.Trim() switch
            {
                "alch" => Currency.OrbofAlchemy,
                "alchemy" => Currency.OrbofAlchemy,
                "chaos" => Currency.ChaosOrb,
                "exa" => Currency.ExaltedOrb,
                "exalted" => Currency.ExaltedOrb,
                "mir" => Currency.MirrorofKalandra,
                "mirror" => Currency.MirrorofKalandra,
                _ => Currency.UnknownCurrency
            };
        }



        public static void FindSlots(ItemGrid InItemGrid)
        {
            if (InItemGrid == null)
                return;

            foreach (var p in InItemGrid.VisibilityPoints.Keys.ToList())
                InItemGrid.VisibilityPoints[p] = MiscLibrary.GetColorAt(p);

            InItemGrid.Slots.Clear();
            for (int x = 0; x < InItemGrid.Size.X; x++)
            {
                InItemGrid.Slots.Add(new List<ItemSlot>());
                for (int y = 0; y < InItemGrid.Size.Y; y++)
                {
                    Point SlotCoord = CalcPoint(InItemGrid, x, y);
                    ParserColor SlotColor = MiscLibrary.GetColorAt(SlotCoord);
                    InItemGrid.Slots[x].Add(new ItemSlot()
                    {
                        SlotColor = SlotColor,
                        EmptySlotColor = SlotColor,
                        SlotCoord = SlotCoord
                    });
                    Logger.WriteLine($"{InItemGrid.Name} grid slot ({x},{y}) has the color {SlotColor} and the coord {SlotCoord}");
                }
            }
            Logger.WriteLine("Done finding slots");
            SaveGrids();
        }

        public static void FindSlots(string InItemGridName)
        {
            FindSlots(ItemGrids[InItemGridName]);
        }

        public static List<Point> FindOccupiedSlots(ItemGrid InItemGrid)
        {
            if (InItemGrid == null)
                return new List<Point>();

            List<Point> OccupiedSlots = new List<Point>();
            for (int x = 0; x < InItemGrid.Size.X; x++)
            {
                for (int y = 0; y < InItemGrid.Size.Y; y++)
                {
                    if (InItemGrid.Slots[x][y].IsSlotEmpty())
                    {
                        Logger.WriteLine($"{InItemGrid.Name} grid slot ({x},{y}) was empty (checked color was {InItemGrid.Slots[x][y].SlotColor} against {InItemGrid.Slots[x][y].EmptySlotColor})");
                        continue;
                    }
                    Logger.WriteLine($"{InItemGrid.Name} grid slot ({x},{y}) was NOT empty (checked color was {InItemGrid.Slots[x][y].SlotColor} against {InItemGrid.Slots[x][y].EmptySlotColor})");

                    OccupiedSlots.Add(new Point(x, y));
                }
            }

            return OccupiedSlots;
        }

        public static List<Point> FindOccupiedSlots(string InItemGridName)
        {
            return FindOccupiedSlots(ItemGrids[InItemGridName]);
        }

        public static List<ItemSlot> GetAllItemsInGrid(ItemGrid InItemGrid)
        {
            FocusPathOfExile();

            List<ItemSlot> ItemsInGrid = new List<ItemSlot>();

            Random r = new Random();
            List<Point> OccupiedSlots = FindOccupiedSlots(InItemGrid);
            for (int i = 0; i < OccupiedSlots.Count; i++)
            {
                Point Slot = OccupiedSlots[i];

                while (InputHelper.GetClipboardText() != null)
                {
                    Logger.WriteLine("Clearing clipboard.");
                    InputHelper.ClearClipboard();
                    Thread.Sleep(25 + r.Next(175));
                }

                if (InputHelper.GetClipboardText() != null)
                    Logger.WriteLine("CLIPBOARD NOT EMPTY");

                ItemSlot GridItem = InItemGrid.Slots[Slot.X][Slot.Y];
                MouseHelper.MoveMouse(GridItem.SlotCoord.X, GridItem.SlotCoord.Y, 12, 12, null);

                while (InputHelper.GetClipboardText() == null || InputHelper.GetClipboardText().Length <= 0)
                {
                    Logger.WriteLine("Setting clipboard to item.");
                    InputSim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                    Thread.Sleep(25 + r.Next(175));
                }

                GridItem.ParseClipboard(InputHelper.GetClipboardText());

                GameItem ItemBase = GetItemBase(GridItem.BaseItemName, GridItem.Rarity == "Magic");
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

                ItemsInGrid.Add(GridItem);

                bool bIsCurrency = Enum.TryParse(GetTrimmedCurrencyName(GridItem.BaseItemName), out Currency c);
                Logger.WriteLine($"Inventory slot {Slot} has {GridItem.StackAmount} {GridItem.GeneratedItemName}{GridItem.BaseItemName}" + (bIsCurrency ? $" which is worth {GetCurrencyWorth(c, GridItem.StackAmount)/*MainWindow.PoELogParser.CurrencyValues[c] * InvSlot.StackAmount*/} in Chaos Orbs." : "."));
            }

            return ItemsInGrid;
        }

        public static List<ItemSlot> GetAllItemsInGrid(string InItemGridName)
        {
            return GetAllItemsInGrid(ItemGrids[InItemGridName]);
        }

        public static bool CheckGridVisibility(ItemGrid InItemGrid)
        {
            if (InItemGrid == null)
                return false;

            foreach (var p in InItemGrid.VisibilityPoints)
            {
                ParserColor c = MiscLibrary.GetColorAt(new Point(p.Key.X, p.Key.Y));
                if (p.Value != c)
                    return false;
            }

            return true;
        }

        public static bool CheckGridVisibility(string InItemGridName)
        {
            return CheckGridVisibility(ItemGrids[InItemGridName]);
        }

        public static bool CheckIsInParty()
        {
            Point[] Points =
            {
                new Point(21, 251),
                new Point(11, 262),
                new Point(10, 252)
            };

            ParserColor[] Colors =
            {
                new ParserColor(112, 129, 160),
                new ParserColor(39, 76, 126),
                new ParserColor(28, 61, 109)
            };

            for (int i = 0; i < Points.Length; i++)
            {
                ParserColor c = MiscLibrary.GetColorAt(new Point(Points[i].X, Points[i].Y));
                Logger.WriteLine(c.ToString());
                if (Colors[i] != c)
                    return false;
            }

            return true;
        }

        public static bool CheckPlayerInHideout()
        {
            Point[] Points =
            {
                new Point(104, 237),
                new Point(234, 270),
                new Point(102, 269)
            };

            ParserColor[] Colors =
            {
                new ParserColor(82, 56, 47),
                new ParserColor(43, 38, 25),
                new ParserColor(116, 79, 65)
            };

            for (int i = 0; i < Points.Length; i++)
            {
                ParserColor c = MiscLibrary.GetColorAt(new Point(Points[i].X, Points[i].Y));
                Logger.WriteLine(c.ToString());
                if (Colors[i] != c)
                    return false;
            }

            return true;
        }

        public static ItemSlot GetSellable(ItemGrid InItemGrid, string InSellableName)
        {
            foreach (var l in InItemGrid.Slots)
                foreach (var s in l)
                    if (s.bIsSellableItem && s.GetFullName() == InSellableName)
                        return s;

            return null;
        }



        public static void InitGrids()
        {
            ItemGrids.Add("Inventory", new ItemGrid("Inventory", new Point(1298, 615), new Point(12, 5), new Point(52, 52), new List<Point>() 
            {
                new Point(1515, 67),
                new Point(1499, 237),
                new Point(1875, 465)
            }, null));
            ItemGrids.Add("RegularStashTab", new ItemGrid("RegularStashTab", new Point(43, 188), new Point(12, 12), new Point(52, 52), new List<Point>() 
            {
                new Point(240, 16),
                new Point(640, 139),
                new Point(621, 66)
            }, new ItemPrice(Currency.ChaosOrb, 10)));
            ItemGrids.Add("TheirTradeOffer", new ItemGrid("TheirTradeOffer", new Point(340, 215), new Point(12, 5), new Point(52, 52), new List<Point>()
            {
                new Point(575, 121),
                new Point(923, 124),
                new Point(806, 115)
            }, null));
            ItemGrids.Add("MyTradeOffer", new ItemGrid("MyTradeOffer", new Point(340, 542), new Point(12, 5), new Point(52, 52), new List<Point>()
            {
                new Point(575, 121),
                new Point(923, 124),
                new Point(806, 115)
            }, null));
        }

        public static void LoadGrids()
        {
            if (File.Exists(ItemGridsPath))
                ItemGrids = MiscLibrary.ReadFromJsonFile<Dictionary<string, ItemGrid>>(ItemGridsPath);

            if (ItemGrids.Count <= 0)
                InitGrids();
        }

        public static void SaveGrids()
        {
            MiscLibrary.WriteToJsonFile(ItemGridsPath, ItemGrids);
        }



        public static void InvitePlayer(string InPlayerName)
        {
            //InputSim.Keyboard.TextEntry($"/invite {InPlayerName}");
            WriteInChat($"/invite {InPlayerName}");
        }

        public static void TradePlayer(string InPlayerName)
        {
            //InputSim.Keyboard.TextEntry($"/tradewith {InPlayerName}");
            WriteInChat($"/tradewith {InPlayerName}");
        }

        public static void WriteInChat(string InMessage)
        {
            MiscLibrary.FocusProcess("PathOfExile_x64Steam");

            InputSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            InputSim.Keyboard.TextEntry(InMessage);
            InputSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        }

        public static void MoveMouseAndClick(int InX, int InY, bool InbCtrlClick = false)
        {
            MiscLibrary.FocusProcess("PathOfExile_x64Steam");

            MouseHelper.MoveMouse(InX, InY, 12, 12);
            if (InbCtrlClick)
            {
                InputSim.Keyboard.KeyDown(VirtualKeyCode.LCONTROL);
                InputSim.Mouse.LeftButtonClick();
                InputSim.Keyboard.KeyUp(VirtualKeyCode.LCONTROL);
            }
            else
                InputSim.Mouse.LeftButtonClick();
        }



        public static BehaviourTreeStatus FocusAndCenterMouse()
        {
            MiscLibrary.FocusProcess("PathOfExile_x64Steam");
            MouseHelper.MoveMouse(960, 900, 200, 180);

            return BehaviourTreeStatus.Success;
        }

        public static BehaviourTreeStatus OpenStash(ItemGrid InStashGrid)
        {
            if (!CheckGridVisibility(InStashGrid))
            {
                MoveMouseAndClick(964, 520);
                Thread.Sleep(400);

                //todo: check for "stash" text on screen
            }

            return BehaviourTreeStatus.Success;
        }



        public static void FocusPathOfExile()
        {
            if (MiscLibrary.GetActiveWindowTitle() != "PathOfExile_x64Steam")
                MiscLibrary.FocusProcess("PathOfExile_x64Steam");
        }



        public static Point CalcPoint(ItemGrid InItemGrid, int InIndexX, int InIndexY)
        {
            return InItemGrid != null ? new Point(InItemGrid.StartPoint.X + InItemGrid.SlotSize.X * InIndexX, InItemGrid.StartPoint.Y + InItemGrid.SlotSize.Y * InIndexY) : Point.Empty;
        }

        public static int CalcXCoord(ItemGrid InItemGrid, int InIndex)
        {
            return InItemGrid != null ? InItemGrid.StartPoint.X + InItemGrid.SlotSize.X * InIndex : -1;
        }

        public static int CalcYCoord(ItemGrid InItemGrid, int InIndex)
        {
            return InItemGrid != null ? InItemGrid.StartPoint.Y + InItemGrid.SlotSize.Y * InIndex : -1;
        }
        public static Point CalcPoint(string InItemGridName, int InIndexX, int InIndexY)
        {
            return CalcPoint(ItemGrids[InItemGridName], InIndexX, InIndexY);
        }

        public static int CalcXCoord(string InItemGridName, int InIndex)
        {
            return CalcXCoord(ItemGrids[InItemGridName], InIndex);
        }

        public static int CalcYCoord(string InItemGridName, int InIndex)
        {
            return CalcYCoord(ItemGrids[InItemGridName], InIndex);
        }
    }
}
