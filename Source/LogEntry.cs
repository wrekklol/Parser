using Parser.StaticLibrary;
using System;
using System.Globalization;

namespace Parser
{
    public class LogEntry
    {
        public string Message { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public GameTradeOffer Offer { get; set; }
        public LogType LogEntryType { get; set; } = LogType.Insignificant;
        public DateTime LogTime { get; set; }
        public string Raw { get; set; } = "";

        public bool IsTradeMessage()
        {
            return Message.StartsWith("Hi, I would like to buy your", StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IsPrintableLogType()
        {
            return LogEntryType == LogType.TradeMessage || LogEntryType == LogType.AfkNotification;
        }



        public static bool operator ==(LogEntry Entry1, LogEntry Entry2)
        {
            if (ReferenceEquals(Entry1, Entry2))
                return true;
            if (ReferenceEquals(Entry1, null))
                return false;
            if (ReferenceEquals(Entry2, null))
                return false;

            return (Entry1.Raw == Entry2.Raw);
        }

        public static bool operator !=(LogEntry Entry1, LogEntry Entry2)
        {
            return !(Entry1 == Entry2);
        }

        public bool Equals(LogEntry Other)
        {
            if (ReferenceEquals(null, Other))
                return false;
            return ReferenceEquals(this, Other) || Raw.Equals(Other.Raw, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object InEntry)
        {
            if (ReferenceEquals(null, InEntry))
                return false;
            if (ReferenceEquals(this, InEntry))
                return true;

            return InEntry.GetType() == GetType() && Equals((LogEntry)InEntry);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Raw);
        }

        public override string ToString()
        {
            var TimeDisplay = $"[{LogTime.TimeOfDay}] ";
            return LogEntryType switch
            {
                LogType.AfkNotification => $"{TimeDisplay}You are AFK.",
                LogType.NormalMessage => $"{TimeDisplay}{PlayerName}: {Message}",
                LogType.TradeMessage =>
                $"{Offer.CurrencyAmount.ToString(CultureInfo.InvariantCulture)} {EnumHelper<GameCurrency>.GetDisplayValue(Offer.CurrencyType)} - {Offer.Item} ({Offer.League})",
                _ => ""
            };
        }
    }
}
