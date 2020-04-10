using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

//public readonly static int[] InventoryGridX = new int[] { 1274, 1326, 1379, 1432, 1484, 1537, 1590, 1642, 1695, 1748, 1800, 1853 };
//public readonly static int[] InventoryGridY = new int[] { 638, 690, 743, 796, 848 };

namespace Parser.StaticLibrary
{
    public class PathOfExileItem
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "inventory_width")]
        public int SizeX { get; set; }

        [JsonProperty(PropertyName = "inventory_height")]
        public int SizeY { get; set; }
    }

    public class PathOfExileItemSlot
    {
        [JsonIgnore]
        public string GeneratedItemName { get; set; } = "";
        [JsonIgnore]
        public string BaseItemName { get; set; } = "";
        [JsonIgnore]
        public string Rarity { get; set; } = "";
        [JsonIgnore]
        public int StackAmount { get; set; } = 1;
        [JsonIgnore]
        public Color SlotColor { get; set; }

        public Color EmptySlotColor { get; set; }
        public Point SlotCoord { get; set; }

        public bool IsSlotEmpty()
        {
            return EmptySlotColor == (SlotColor = MiscLibrary.GetColorAt(SlotCoord));
        }

        public void ParseClipboard(string InClipboard)
        {
            if (string.IsNullOrEmpty(InClipboard))
                return;

            string[] Lines = InClipboard.Split('\n');
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].StartsWith("Rarity:", StringComparison.InvariantCultureIgnoreCase))
                {
                    Rarity = Lines[i].Remove(0, 8).TrimEnd();
                    string UpperName = Lines[i + 1].Trim();
                    string LowerName = Lines[i + 2].Trim();
                    GeneratedItemName = LowerName.StartsWith('-') ? "" : UpperName;
                    BaseItemName = LowerName.StartsWith('-') ? UpperName : LowerName;

                    i += 2;
                }

                if (Lines[i].StartsWith("Stack Size:", StringComparison.InvariantCultureIgnoreCase))
                {
                    StackAmount = int.Parse(Lines[i].Substring(":", "/"), CultureInfo.InvariantCulture.NumberFormat);
                }
            }
        }
    }

    public class PathOfExileItemGrid
    {
        // Slots in grid.
        public List<List<PathOfExileItemSlot>> Slots { get; set; } = new List<List<PathOfExileItemSlot>>();
        // Name of the grid.
        public string Name { get; set; }
        // Start point of the grid.
        public Point StartPoint { get; set; }
        // Amount of slots in the grid.
        public Point Size { get; set; }
        // Size of the slots.
        public Point SlotSize { get; set; }

        public PathOfExileItemGrid(string InName, Point InStartPoint, Point InSize, Point InSlotSize)
        {
            Name = InName;
            StartPoint = InStartPoint;
            Size = InSize;
            SlotSize = InSlotSize;
        }
    }

    public static class TradeHelper
    {
        public static Dictionary<string, PathOfExileItemGrid> ItemGrids { get; set; } = new Dictionary<string, PathOfExileItemGrid>();
        public static string ItemGridsPath { get; } = @Directory.GetCurrentDirectory() + $"\\ItemGrids.json";
        public static Dictionary<string, PathOfExileItem> ItemBases { get; set; } = new Dictionary<string, PathOfExileItem>();
        public static string ItemBasesPath { get; } = @Directory.GetCurrentDirectory() + "\\PathOfExileItemBases.json";



        public static void InitItemBases()
        {
            if (File.Exists(ItemBasesPath))
                ItemBases = MiscLibrary.ReadFromJsonFile<Dictionary<string, PathOfExileItem>>(ItemBasesPath);
        }

        public static PathOfExileItem GetItemBase(string InBaseName, bool InbIsMagicItem)
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



        public static void FindSlots(PathOfExileItemGrid InItemGrid)
        {
            if (InItemGrid == null)
                return;

            InItemGrid.Slots.Clear();
            for (int x = 0; x < InItemGrid.Size.X; x++)
            {
                InItemGrid.Slots.Add(new List<PathOfExileItemSlot>());
                for (int y = 0; y < InItemGrid.Size.Y; y++)
                {
                    Point SlotCoord = CalcPoint(InItemGrid, x, y);
                    Color SlotColor = MiscLibrary.GetColorAt(SlotCoord);
                    InItemGrid.Slots[x].Add(new PathOfExileItemSlot()
                    {
                        SlotColor = SlotColor,
                        EmptySlotColor = SlotColor,
                        SlotCoord = SlotCoord
                    });
                    Console.WriteLine($"{InItemGrid.Name} grid slot ({x},{y}) has the color {SlotColor} and the coord {SlotCoord}");
                }
            }
            Console.WriteLine("Done finding slots");
            SaveGrids();
        }

        public static List<Point> FindOccupiedSlots(PathOfExileItemGrid InItemGrid)
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
                        Console.WriteLine($"{InItemGrid.Name} grid slot ({x},{y}) was empty (checked color was {InItemGrid.Slots[x][y].SlotColor} against {InItemGrid.Slots[x][y].EmptySlotColor})");
                        continue;
                    }
                    Console.WriteLine($"{InItemGrid.Name} grid slot ({x},{y}) was NOT empty (checked color was {InItemGrid.Slots[x][y].SlotColor} against {InItemGrid.Slots[x][y].EmptySlotColor})");

                    OccupiedSlots.Add(new Point(x, y));
                }
            }

            return OccupiedSlots;
        }

        public static void FindSlots(string InItemGridName)
        {
            FindSlots(ItemGrids[InItemGridName]);
        }

        public static List<Point> FindOccupiedSlots(string InItemGridName)
        {
            return FindOccupiedSlots(ItemGrids[InItemGridName]);
        }

        public static void InitGrids()
        {
            ItemGrids.Add("Inventory", new PathOfExileItemGrid("Inventory", new Point(1298, 615), new Point(12, 5), new Point(52, 52)));
            ItemGrids.Add("RegularStashTab", new PathOfExileItemGrid("RegularStashTab", new Point(43, 188), new Point(12, 12), new Point(52, 52)));
            ItemGrids.Add("TheirTradeOffer", new PathOfExileItemGrid("TheirTradeOffer", new Point(340, 215), new Point(12, 5), new Point(52, 52)));
        }

        public static void LoadGrids()
        {
            if (File.Exists(ItemGridsPath))
                ItemGrids = MiscLibrary.ReadFromJsonFile<Dictionary<string, PathOfExileItemGrid>>(ItemGridsPath);

            if (ItemGrids.Count <= 0)
                InitGrids();
        }

        public static void SaveGrids()
        {
            MiscLibrary.WriteToJsonFile(ItemGridsPath, ItemGrids);
        }



        public static Point CalcPoint(PathOfExileItemGrid InItemGrid, int InIndexX, int InIndexY)
        {
            return InItemGrid != null ? new Point(InItemGrid.StartPoint.X + InItemGrid.SlotSize.X * InIndexX, InItemGrid.StartPoint.Y + InItemGrid.SlotSize.Y * InIndexY) : Point.Empty;
        }

        public static int CalcXCoord(PathOfExileItemGrid InItemGrid, int InIndex)
        {
            return InItemGrid != null ? InItemGrid.StartPoint.X + InItemGrid.SlotSize.X * InIndex : -1;
        }

        public static int CalcYCoord(PathOfExileItemGrid InItemGrid, int InIndex)
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
