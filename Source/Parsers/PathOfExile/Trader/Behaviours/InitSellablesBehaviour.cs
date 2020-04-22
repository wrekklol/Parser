using FluentBehaviourTree;
using Parser.PathOfExile.StaticLibrary;
using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static Parser.StaticLibrary.MiscLibrary;
using static Parser.StaticLibrary.InputHelper;
using WindowsInput;
using WindowsInput.Native;

namespace Parser.PathOfExile
{
    public class InitSellablesBehaviour : ParserBehaviourBase
    {
        private Trader t;
        private ItemGrid StashGrid;

        protected override void BuildTree()
        {
            Builder
                .Sequence("InitSellables")
                    .Condition("CheckStashOpen", CheckStashOpen)
                    .Do("GetSellables", GetSellables)
                .End();
        }

        protected override void OnStart()
        {
            TradeHelper.FocusAndCenterMouse();

            t = (Trader)Data;

            StashGrid = TradeHelper.ItemGrids["RegularStashTab"];
        }



        private bool CheckStashOpen()
        {
            return TradeHelper.CheckGridVisibility(StashGrid);
        }

        private BehaviourTreeStatus GetSellables()
        {
            if (StashGrid.Slots.Count <= 0)
            {
                Logger.WriteLine("BehaviourTree: StashGrid slots not initialized.");
                return BehaviourTreeStatus.Running;
            }

            t.SellableItems = TradeHelper.GetAllItemsInGrid(StashGrid);
            foreach (var s in t.SellableItems)
            {
                if (s.bIsSellableItem != false)
                    continue;

                s.bIsSellableItem = true;
                s.SellPrice = new ItemPrice((Currency)StashGrid.Data.CurrencyType, (double)StashGrid.Data.Amount);
            }

            TradeHelper.SaveGrids();
            Stop();

            return BehaviourTreeStatus.Success;
        }
    }
}
