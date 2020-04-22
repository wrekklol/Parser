using Newtonsoft.Json;
using Parser.PathOfExile.StaticLibrary;
using Parser.StaticLibrary;
using System;
using System.Drawing;
using System.Globalization;

namespace Parser.PathOfExile
{
    public class ItemSlot
    {
        public bool bIsSellableItem { get; set; } = false;
        public ItemPrice SellPrice { get; set; } = new ItemPrice();
        public string GeneratedItemName { get; set; } = "";
        public string BaseItemName { get; set; } = "";
        public string Rarity { get; set; } = "";
        public int StackAmount { get; set; } = 1;
        public ParserColor SlotColor { get; set; } = null;

        public ParserColor EmptySlotColor { get; set; }
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
                else if (Lines[i].StartsWith("Stack Size:", StringComparison.InvariantCultureIgnoreCase))
                {
                    StackAmount = int.Parse(Lines[i].Substring(":", "/"), CultureInfo.InvariantCulture.NumberFormat);
                }
                else if (Lines[i].StartsWith("Note: ~price", StringComparison.InvariantCultureIgnoreCase) || Lines[i].StartsWith("Note: ~b/o", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] s = Lines[i].TrimEnd().Split(" ");
                    SellPrice = new ItemPrice(TradeHelper.ParseCurrencyType(s[3]), double.Parse(s[2]));
                    bIsSellableItem = true;
                }
            }
        }

        public string GetFullName()
        {
            return string.IsNullOrEmpty(GeneratedItemName) ? BaseItemName : $"{GeneratedItemName} {BaseItemName}";
        }

        public void OnItemRemoved()
        {
            bIsSellableItem = false;
            SellPrice = new ItemPrice();
            GeneratedItemName = "";
            BaseItemName = "";
            Rarity = "";
            StackAmount = 1;
            SlotColor = null;
    }



        public bool ShouldSerializebIsSellableItem() { return bIsSellableItem; }
        public bool ShouldSerializeSellPrice() { return bIsSellableItem; }
        public bool ShouldSerializeGeneratedItemName() { return bIsSellableItem; }
        public bool ShouldSerializeBaseItemName() { return bIsSellableItem; }
        public bool ShouldSerializeRarity() { return bIsSellableItem; }
        public bool ShouldSerializeStackAmount() { return bIsSellableItem; }
        public bool ShouldSerializeSlotColor() { return bIsSellableItem; }
    }
}
