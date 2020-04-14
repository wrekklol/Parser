using Parser.StaticLibrary;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Parser
{
    public enum PathOfExileLogType
    {
        Insignificant,
        AfkNotification,

        NormalMessage,
        TradeMessage,
    }

    public enum PathOfExileCurrency
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

    public class PathOfExileTradeOffer
    {
        public string Item { get; set; }
        public double CurrencyAmount { get; set; }
        public PathOfExileCurrency CurrencyType { get; set; }
        public string League { get; set; }

        public static double GetCurrencyWorth(double InCurrencyAmount, PathOfExileCurrency InCurrencyType)
        {
            return InCurrencyAmount * MainWindow.PoELogParser.CurrencyValues[InCurrencyType];
        }

        public static double GetCurrencyWorth(PathOfExileTradeOffer InTradeOffer)
        {
            return InTradeOffer != null ? GetCurrencyWorth(InTradeOffer.CurrencyAmount, InTradeOffer.CurrencyType) : 0;
        }

        public static PathOfExileCurrency ParseCurrencyType(string InRawType)
        {
            return InRawType switch
            {
                "alch" => PathOfExileCurrency.OrbofAlchemy,
                "alchemy" => PathOfExileCurrency.OrbofAlchemy,
                "chaos" => PathOfExileCurrency.ChaosOrb,
                "exa" => PathOfExileCurrency.ExaltedOrb,
                "exalted" => PathOfExileCurrency.ExaltedOrb,
                "mir" => PathOfExileCurrency.MirrorofKalandra,
                "mirror" => PathOfExileCurrency.MirrorofKalandra,
                _ => PathOfExileCurrency.UnknownCurrency
            };
        }
    }

    public class PathOfExileLogEntry
    {
        public string Message { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public PathOfExileTradeOffer TradeOffer { get; set; }
        public PathOfExileLogType LogType { get; set; } = PathOfExileLogType.Insignificant;
        public DateTime LogTime { get; set; }
        public string Raw { get; set; } = "";

        public bool IsTradeMessage()
        {
            return Message.StartsWith("Hi, I would like to buy your", StringComparison.InvariantCultureIgnoreCase);
        }



        public static bool operator ==(PathOfExileLogEntry Entry1, PathOfExileLogEntry Entry2)
        {
            if (ReferenceEquals(Entry1, Entry2))
                return true;
            if (ReferenceEquals(Entry1, null))
                return false;
            if (ReferenceEquals(Entry2, null))
                return false;

            return (Entry1.Raw == Entry2.Raw);
        }

        public static bool operator !=(PathOfExileLogEntry Entry1, PathOfExileLogEntry Entry2)
        {
            return !(Entry1 == Entry2);
        }

        public bool Equals(PathOfExileLogEntry Other)
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

            return InEntry.GetType() == GetType() && Equals((PathOfExileLogEntry)InEntry);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Raw);
        }

        public override string ToString()
        {
            var TimeDisplay = $"[{LogTime.TimeOfDay.ToString()}] ";
            return LogType switch
            {
                PathOfExileLogType.AfkNotification => $"{TimeDisplay}You are AFK.",
                PathOfExileLogType.NormalMessage => $"{TimeDisplay}{PlayerName}: {Message}",
                PathOfExileLogType.TradeMessage =>
                $"{TradeOffer.CurrencyAmount.ToString(CultureInfo.InvariantCulture)} {EnumHelper<PathOfExileCurrency>.GetDisplayValue(TradeOffer.CurrencyType)} - {TradeOffer.Item} ({TradeOffer.League})",
                _ => ""
            };
        }
    }
}
