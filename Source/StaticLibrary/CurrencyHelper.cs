using Newtonsoft.Json.Linq;
using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

using static Parser.StaticLibrary.Config;

namespace Parser.StaticLibrary
{
    public static class CurrencyHelper
    {
        public static string FetchData { get; private set; } = "";
        public static double FetchCurrencyInterval { get; } = GetConfig("Parser", "FetchCurrencyInterval", 3600);

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
            return LogParser.CurrencyValues.TryGetValue(InCurrencyType, out double value) ? value * InAmountOfCurrency : 0;
        }

        public static double GetCurrencyWorth(GameTradeOffer InTradeOffer)
        {
            return InTradeOffer != null ? GetCurrencyWorth(InTradeOffer.CurrencyType, InTradeOffer.CurrencyAmount) : 0;
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



        public static async void FetchCurrency(bool InbForce = false)
        {
            if (App.PDebug.bShouldGetCurrency)
            {
                bool bShouldFetch = false;
                Dictionary<string, dynamic> PreviousFetch = MiscLibrary.ReadFromJsonFile<Dictionary<string, dynamic>>(App.AppPath + "\\FetchedCurrency.json");

                if (PreviousFetch.Count > 0)
                {
                    if ((DateTime.Now - ((DateTime)PreviousFetch["FetchTime"])).TotalSeconds > FetchCurrencyInterval)
                        bShouldFetch = true;
                    else
                        OnGetCurrencyValues((string)PreviousFetch["CurrencyData"]);
                }
                else
                    bShouldFetch = true;

                if (bShouldFetch || InbForce)
                {
                    await MiscLibrary.GetAsync("https://poe.ninja/api/data/currencyoverview?league=Delirium&type=Currency", OnGetCurrencyValues).ConfigureAwait(false);
                    Dictionary<string, dynamic> CurrentFetch = new Dictionary<string, dynamic>()
                    {
                        { "FetchTime", DateTime.Now },
                        { "CurrencyData", FetchData }
                    };

                    MiscLibrary.WriteToJsonFile(App.AppPath + "\\FetchedCurrency.json", CurrentFetch);
                }
            }
        }

        private static void OnGetCurrencyValues(string InData)
        {
            if (string.IsNullOrEmpty(FetchData = InData))
                return;

            //todo: get values every hour instead
            JObject Values = JObject.Parse(InData);
            foreach (var x in Values)
            {
                if (x.Key != "lines")
                    continue;

                Logger.WriteLine("Started fetching currency values", true);
                foreach (var y in x.Value.AsJEnumerable())
                {
                    var CurrencyType = y.Value<string>("currencyTypeName");
                    var CurrencyValue = y.Value<JToken>("receive").Value<double>("value");
                    LogParser.CurrencyValues.Add(Enum.Parse<GameCurrency>(CurrencyHelper.GetTrimmedCurrencyName(CurrencyType), true), CurrencyValue);
                    Logger.WriteLine($"{Enum.Parse<GameCurrency>(CurrencyHelper.GetTrimmedCurrencyName(CurrencyType))} => {CurrencyValue}");
                }
                Logger.WriteLine("Done fetching currency values", true);
            }
        }
    }
}
