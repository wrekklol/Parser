using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

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
            switch (InRawType)
            {
                case "alch":
                case "alchemy":
                    return PathOfExileCurrency.OrbofAlchemy;
                case "chaos":
                    return PathOfExileCurrency.ChaosOrb;
                case "exa":
                case "exalted":
                    return PathOfExileCurrency.ExaltedOrb;
                case "mir":
                case "mirror":
                    return PathOfExileCurrency.MirrorofKalandra;
            }

            return PathOfExileCurrency.UnknownCurrency;
        }
    }

    public class PathOfExileLogEntry
    {
        public string Message { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public PathOfExileTradeOffer TradeOffer { get; set; } = null;
        public PathOfExileLogType LogType { get; set; } = PathOfExileLogType.Insignificant;
        public DateTime LogTime { get; set; }
        public string Raw { get; set; } = "";

        public bool IsTradeMessage()
        {
            return Message.StartsWith("Hi, I would like to buy your", StringComparison.InvariantCultureIgnoreCase);
        }



        public static bool operator ==(PathOfExileLogEntry e1, PathOfExileLogEntry e2)
        {
            if (ReferenceEquals(e1, e2))
                return true;
            if (ReferenceEquals(e1, null))
                return false;
            if (ReferenceEquals(e2, null))
                return false;

            return (e1.Raw == e2.Raw);
        }

        public static bool operator !=(PathOfExileLogEntry e1, PathOfExileLogEntry e2)
        {
            return !(e1 == e2);
        }

        public bool Equals(PathOfExileLogEntry other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Raw.Equals(other.Raw, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object e)
        {
            if (ReferenceEquals(null, e))
                return false;
            if (ReferenceEquals(this, e))
                return true;

            return e.GetType() == GetType() && Equals((PathOfExileLogEntry)e);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Raw);
            //unchecked
            //{
            //    //int hashCode = height.GetHashCode();
            //    //hashCode = (hashCode * 397) ^ length.GetHashCode();
            //    //hashCode = (hashCode * 397) ^ breadth.GetHashCode();
            //    //return hashCode;
            //    return LogID.GetHashCode();
            //}
        }

        public override string ToString()
        {
            string TimeDisplay = "[" + LogTime.TimeOfDay.ToString() + "] ";
            switch (LogType)
            {
                case PathOfExileLogType.AfkNotification:
                    return TimeDisplay + "You are AFK.";
                case PathOfExileLogType.NormalMessage:
                    return TimeDisplay + PlayerName + ": " + Message;
                case PathOfExileLogType.TradeMessage:
                    return TradeOffer.CurrencyAmount + " " + EnumHelper<PathOfExileCurrency>.GetDisplayValue(TradeOffer.CurrencyType) + " - " + TradeOffer.Item + " (" + TradeOffer.League + ")";
                    //return TimeDisplay + " - Trade Offer - " + TradeOffer.Item + " for " + TradeOffer.CurrencyAmount + " " + TradeOffer.CurrencyType + "(" + TradeOffer.League + ")";
            }

            return "";
        }
    }
}
