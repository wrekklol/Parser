using Parser.PathOfExile.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.PathOfExile
{
    /// <summary>
    /// not used
    /// </summary>
    public class TradeManager
    {
        public List<LogEntry> TradeOffers { get; private set; } = new List<LogEntry>();

        public TradeManager()
        {
            LogParser.OnNewLogEntry += LogParser_OnNewLogEntry;
        }

        private void LogParser_OnNewLogEntry(LogEntry InEntry)
        {
            if (InEntry.IsTradeMessage() && !CheckOfferAlreadyExists(InEntry))
            {
                TradeOffers.Insert(DetermineTradeImportance(InEntry), InEntry);
            }
        }

        private bool CheckOfferAlreadyExists(LogEntry InEntry)
        {
            foreach (LogEntry o in TradeOffers)
                if (o.PlayerName == InEntry.PlayerName && o.Offer.Item == InEntry.Offer.Item)
                    return true;

            return false;
        }

        private int DetermineTradeImportance(LogEntry InEntry)
        {
            int InsertIndex = 0;
            for (int i = 1; i < TradeOffers.Count; i++)
            {
                if (TradeHelper.GetCurrencyWorth(InEntry.Offer) > TradeHelper.GetCurrencyWorth(TradeOffers[i].Offer))
                    InsertIndex = i;
            }

            return InsertIndex;
        }
    }
}
