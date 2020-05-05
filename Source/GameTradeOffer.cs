using Parser.StaticLibrary;
using System;
using System.Globalization;

namespace Parser
{
    public class GameTradeOffer
    {
        public string Item { get; set; }
        public double CurrencyAmount { get; set; }
        public GameCurrency CurrencyType { get; set; }
        public string League { get; set; }
    }
}
