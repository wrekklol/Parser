using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace Parser.StaticLibrary
{
    public static class CurrencyHelper
    {
        public static Dictionary<string, GameItem> ItemBases { get; set; } = new Dictionary<string, GameItem>();
        public static string ItemBasesPath { get; } = App.AppPath + "\\ItemBases.json";



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



        public static double GetCurrencyWorth(GameCurrency InCurrencyType, double InAmountOfCurrency)
        {
            return LogParser.CurrencyValues[InCurrencyType] * InAmountOfCurrency;
        }

        public static double GetCurrencyWorth(GameTradeOffer InTradeOffer)
        {
            return LogParser.CurrencyValues[InTradeOffer.CurrencyType] * InTradeOffer.CurrencyAmount;
        }

        public static string GetTrimmedCurrencyName(string InCurrencyName)
        {
            return InCurrencyName != null ? InCurrencyName.Replace("\"", "").Replace(" ", "").Replace("'", "").Replace("-", "") : "";
        }

        public static GameCurrency ParseCurrencyType(string InRawType)
        {
            return InRawType.Trim() switch
            {
                "alch" => GameCurrency.OrbofAlchemy,
                "alchemy" => GameCurrency.OrbofAlchemy,
                "chaos" => GameCurrency.ChaosOrb,
                "exa" => GameCurrency.ExaltedOrb,
                "exalted" => GameCurrency.ExaltedOrb,
                "mir" => GameCurrency.MirrorofKalandra,
                "mirror" => GameCurrency.MirrorofKalandra,
                _ => GameCurrency.UnknownCurrency
            };
        }
    }
}
