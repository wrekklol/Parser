using Parser.StaticLibrary;
using System;
using System.Globalization;

namespace Parser
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
}
