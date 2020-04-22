using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.PathOfExile
{
    public class ItemPrice
    {
        public Currency CurrencyType { get; set; }
        public double Amount { get; set; }

        public ItemPrice(Currency InCurrencyType, double InAmount)
        {
            CurrencyType = InCurrencyType;
            Amount = InAmount;
        }

        public ItemPrice(double InAmount) : this(Currency.ChaosOrb, InAmount) { }
        public ItemPrice() : this(Currency.ChaosOrb, 0) { }
    }
}
