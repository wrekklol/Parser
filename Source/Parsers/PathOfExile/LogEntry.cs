using Parser.StaticLibrary;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Parser.PathOfExile
{
    public enum LogType
    {
        Insignificant,

        AfkNotification,
        EnterHideoutNotification,
        LeaveHideoutNotification,
        TradeAcceptedNotification,
        TradeCancelledNotification,

        NormalMessage,
        TradeMessage,
    }

    public enum Currency
    {
        [Display(Name = "Unknown Currency")]
        UnknownCurrency,

        [Display(Name = "Chaos Orb")]
        ChaosOrb,

        [Display(Name = "Mirror of Kalandra")]
        MirrorofKalandra,
        [Display(Name = "Mirror Shard")]
        MirrorShard,
        [Display(Name = "Awakener's Orb")]
        AwakenersOrb,
        [Display(Name = "Warlord's Exalted Orb")]
        WarlordsExaltedOrb,
        [Display(Name = "Crusader's Exalted Orb")]
        CrusadersExaltedOrb,
        [Display(Name = "Hunter's Exalted Orb")]
        HuntersExaltedOrb,
        [Display(Name = "Redeemer's Exalted Orb")]
        RedeemersExaltedOrb,
        [Display(Name = "Exalted Orb")]
        ExaltedOrb,
        [Display(Name = "Blessing of Chayula")]
        BlessingofChayula,
        [Display(Name = "Divine Orb")]
        DivineOrb,
        [Display(Name = "Exalted Shard")]
        ExaltedShard,
        [Display(Name = "Ancient Orb")]
        AncientOrb,
        [Display(Name = "Orb of Annulment")]
        OrbofAnnulment,
        [Display(Name = "Fertile Catalyst")]
        FertileCatalyst,
        [Display(Name = "Prismatic Catalyst")]
        PrismaticCatalyst,
        [Display(Name = "Harbinger's Orb")]
        HarbingersOrb,
        [Display(Name = "Stacked Deck")]
        StackedDeck,
        [Display(Name = "Blessing of Uul-Netol")]
        BlessingofUulNetol,
        [Display(Name = "Awakened Sextant")]
        AwakenedSextant,
        [Display(Name = "Blessing of Xoph")]
        BlessingofXoph,
        [Display(Name = "Blessing of Tul")]
        BlessingofTul,
        [Display(Name = "Blessing of Esh")]
        BlessingofEsh,
        [Display(Name = "Annulment Shard")]
        AnnulmentShard,
        [Display(Name = "Tempering Catalyst")]
        TemperingCatalyst,
        [Display(Name = "Gemcutter's Prism")]
        GemcuttersPrism,
        [Display(Name = "Prime Sextant")]
        PrimeSextant,
        [Display(Name = "Splinter of Chayula")]
        SplinterofChayula,
        [Display(Name = "Orb of Regret")]
        OrbofRegret,
        [Display(Name = "Orb of Scouring")]
        OrbofScouring,
        [Display(Name = "Vaal Orb")]
        VaalOrb,
        [Display(Name = "Splinter of Uul-Netol")]
        SplinterofUulNetol,
        [Display(Name = "Orb of Fusing")]
        OrbofFusing,
        [Display(Name = "Simple Sextant")]
        SimpleSextant,
        [Display(Name = "Orb of Horizons")]
        OrbofHorizons,
        [Display(Name = "Regal Orb")]
        RegalOrb,
        [Display(Name = "Turbulent Catalyst")]
        TurbulentCatalyst,
        [Display(Name = "Intrinsic Catalyst")]
        IntrinsicCatalyst,
        [Display(Name = "Abrasive Catalyst")]
        AbrasiveCatalyst,
        [Display(Name = "Orb of Alchemy")]
        OrbofAlchemy,
        [Display(Name = "Orb of Alteration")]
        OrbofAlteration,
        [Display(Name = "Cartographer's Chisel")]
        CartographersChisel,
        [Display(Name = "Glassblower's Bauble")]
        GlassblowersBauble,
        [Display(Name = "Orb of Binding")]
        OrbofBinding,
        [Display(Name = "Engineer's Orb")]
        EngineersOrb,
        [Display(Name = "Chromatic Orb")]
        ChromaticOrb,
        [Display(Name = "Orb of Augmentation")]
        OrbofAugmentation,
        [Display(Name = "Splinter of Tul")]
        SplinterofTul,
        [Display(Name = "Imbued Catalyst")]
        ImbuedCatalyst,
        [Display(Name = "Splinter of Esh")]
        SplinterofEsh,
        [Display(Name = "Silver Coin")]
        SilverCoin,
        [Display(Name = "Orb of Chance")]
        OrbofChance,
        [Display(Name = "Orb of Transmutation")]
        OrbofTransmutation,
        [Display(Name = "Blacksmith's Whetstone")]
        BlacksmithsWhetstone,
        [Display(Name = "Armourer's Scrap")]
        ArmourersScrap,
        [Display(Name = "Splinter of Xoph")]
        SplinterofXoph,
        [Display(Name = "Jeweller's Orb")]
        JewellersOrb,
        [Display(Name = "Portal Scroll")]
        PortalScroll,
        [Display(Name = "Blessed Orb")]
        BlessedOrb,
        [Display(Name = "Scroll of Wisdom")]
        ScrollofWisdom,
        [Display(Name = "Perandus Coin")]
        PerandusCoin
    }

    public class TradeOffer
    {
        public string Item { get; set; }
        public double CurrencyAmount { get; set; }
        public Currency CurrencyType { get; set; }
        public string League { get; set; }

        //public static double GetCurrencyWorth(double InCurrencyAmount, Currency InCurrencyType)
        //{
        //    return InCurrencyAmount * MainWindow.PoELogParser.CurrencyValues[InCurrencyType];
        //}

        //public static double GetCurrencyWorth(TradeOffer InOffer)
        //{
        //    return InOffer != null ? GetCurrencyWorth(InOffer.CurrencyAmount, InOffer.CurrencyType) : 0;
        //}

        //public static Currency ParseCurrencyType(string InRawType)
        //{
        //    return InRawType.Trim() switch
        //    {
        //        "alch" => Currency.OrbofAlchemy,
        //        "alchemy" => Currency.OrbofAlchemy,
        //        "chaos" => Currency.ChaosOrb,
        //        "exa" => Currency.ExaltedOrb,
        //        "exalted" => Currency.ExaltedOrb,
        //        "mir" => Currency.MirrorofKalandra,
        //        "mirror" => Currency.MirrorofKalandra,
        //        _ => Currency.UnknownCurrency
        //    };
        //}
    }

    public class LogEntry
    {
        public string Message { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public TradeOffer Offer { get; set; }
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
            var TimeDisplay = $"[{LogTime.TimeOfDay.ToString()}] ";
            return LogEntryType switch
            {
                LogType.AfkNotification => $"{TimeDisplay}You are AFK.",
                LogType.NormalMessage => $"{TimeDisplay}{PlayerName}: {Message}",
                LogType.TradeMessage =>
                $"{Offer.CurrencyAmount.ToString(CultureInfo.InvariantCulture)} {EnumHelper<Currency>.GetDisplayValue(Offer.CurrencyType)} - {Offer.Item} ({Offer.League})",
                _ => ""
            };
        }
    }
}
