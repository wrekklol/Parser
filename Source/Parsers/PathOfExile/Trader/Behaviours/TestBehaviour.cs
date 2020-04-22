using FluentBehaviourTree;
using Parser.PathOfExile.StaticLibrary;
using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static Parser.StaticLibrary.MiscLibrary;
using static Parser.StaticLibrary.InputHelper;
using WindowsInput;
using WindowsInput.Native;
using System.IO;
using System.Linq;

namespace Parser.PathOfExile
{
    public class TestBehaviour : ParserBehaviourBase
    {
		private LogEntry TradeMessage;
		private ItemGrid InventoryGrid;
		private ItemGrid StashGrid;
		private ItemGrid TradeGrid;
		private ItemGrid MyOfferGrid;

		private bool bHasEnteredHideout = false;
		private bool bHasAcceptedTrade = false;
		private ItemSlot ItemToSell = null;
		//private ItemPrice ItemSellPrice = null;

		protected override void BuildTree()
        {
			//Builder
			//	.Sequence("Test")
			//		.Do("CheckTheirOffer", CheckTheirOffer)
			//	.End();
			Builder
				.Sequence("Test")
					.Do("FocusAndCenterMouse", TradeHelper.FocusAndCenterMouse)
					.Do("GetSellable", GetSellable)
					.Do("CheckStage", CheckStage)
					.Do("InvitePlayer", InvitePlayer)
					.Do("TakeItemFromStash", TakeItemFromStash)
					.Do("TradePlayer", TradePlayer)
					.Do("PlaceItemInTrade", PlaceItemInTrade)
					.Do("CheckTheirOffer", CheckTheirOffer)
				//.Do("Action2", Action2)
				.End();
		}

		protected override void OnStart()
		{
			TradeMessage = (LogEntry)Data;

			InventoryGrid = TradeHelper.ItemGrids["Inventory"];
			StashGrid = TradeHelper.ItemGrids["RegularStashTab"];
			TradeGrid = TradeHelper.ItemGrids["TheirTradeOffer"];
			MyOfferGrid = TradeHelper.ItemGrids["MyTradeOffer"];

			LogParser.OnNewLogEntry += LogParser_OnNewLogEntry;
		}

		protected override void OnStop()
		{
			LogParser.OnNewLogEntry -= LogParser_OnNewLogEntry;
		}

		private void LogParser_OnNewLogEntry(LogEntry InEntry)
		{
			if (InEntry.LogEntryType == LogType.EnterHideoutNotification && InEntry.PlayerName == TradeMessage?.PlayerName)
				bHasEnteredHideout = true;
			else if (InEntry.LogEntryType == LogType.LeaveHideoutNotification && InEntry.PlayerName == TradeMessage?.PlayerName)
				bHasEnteredHideout = false;
			else if (InEntry.LogEntryType == LogType.TradeAcceptedNotification)
				bHasAcceptedTrade = true;
			else if (InEntry.LogEntryType == LogType.TradeCancelledNotification)
				bHasAcceptedTrade = false;
		}



		private BehaviourTreeStatus GetSellable()
		{
			return (ItemToSell = TradeHelper.GetSellable(StashGrid, TradeMessage.Offer.Item)) != null ? BehaviourTreeStatus.Success : BehaviourTreeStatus.Failure;
		}

		private BehaviourTreeStatus CheckStage()
		{
			IParentBehaviourTreeNode t = (IParentBehaviourTreeNode)Tree;
			if (TradeHelper.CheckIsInParty())
			{
				if (TradeHelper.CheckPlayerInHideout())
				{
					List<ItemSlot> ItemsInInventory = TradeHelper.GetAllItemsInGrid(InventoryGrid);
					if (ItemsInInventory.First(x => x.GetFullName() == ItemToSell.GetFullName()) == null)
					{
						t.SkipTo("TakeItemFromStash");
						return BehaviourTreeStatus.Success;
					}
					else
					{
						if (!TradeHelper.CheckGridVisibility(TradeGrid))
						{
							t.SkipTo("TradePlayer");
							return BehaviourTreeStatus.Success;
						}
						else
						{
							t.SkipTo("PlaceItemInTrade");
							return BehaviourTreeStatus.Success;
						}
					}
				}
			}

			return BehaviourTreeStatus.Success;
		}

		private BehaviourTreeStatus InvitePlayer()
		{
			if (TradeHelper.CheckIsInParty())
				return BehaviourTreeStatus.Success;

			TradeHelper.InvitePlayer(TradeMessage.PlayerName);
			while (ShouldContinueLoop() && !bHasEnteredHideout)
			{
				Logger.WriteLine("BehaviourTree: Waiting for player to enter hideout.");

				//check for INVITETIMEOUT
				Thread.Sleep(1);
			}
			Thread.Sleep(500 + r.Next(1500));

			return BehaviourTreeStatus.Success;
		}

		private BehaviourTreeStatus TakeItemFromStash()
		{
			TradeHelper.OpenStash(StashGrid);
			if (!TradeHelper.CheckGridVisibility(StashGrid) || ItemToSell == null || ItemToSell.IsSlotEmpty())
				return BehaviourTreeStatus.Failure;

			while (ShouldContinueLoop() && !ItemToSell.IsSlotEmpty())
			{
				TradeHelper.MoveMouseAndClick(ItemToSell.SlotCoord.X, ItemToSell.SlotCoord.Y, true);

				Thread.Sleep(500 + r.Next(1500));
			}

			//ItemSellPrice = ItemToSell.SellPrice;
			//ItemToSell.OnItemRemoved();
			//TradeHelper.SaveGrids(); //todo: clear on trade succesful instead

			return BehaviourTreeStatus.Success;
		}

		private BehaviourTreeStatus TradePlayer()
		{
			TradeHelper.TradePlayer(TradeMessage.PlayerName);
			while (ShouldContinueLoop() && !TradeHelper.CheckGridVisibility(TradeGrid))
			{
				Logger.WriteLine("BehaviourTree: Waiting for player to enter trade.");
				//check for TRADETIMEOUT
				Thread.Sleep(1);
			}

			return BehaviourTreeStatus.Success;
		}

		private BehaviourTreeStatus PlaceItemInTrade()
		{
			ItemSlot InvSlot = InventoryGrid.Slots[0][0];
			TradeHelper.MoveMouseAndClick(InvSlot.SlotCoord.X, InvSlot.SlotCoord.Y, true);
			Thread.Sleep(500 + r.Next(1500));

			return BehaviourTreeStatus.Success;
		}

		private BehaviourTreeStatus CheckTheirOffer()
		{
			bool bHasFoundFullValue = false;
			while (ShouldContinueLoop() && !bHasFoundFullValue)
			{
				double CurrentValue = 0;

				List<ItemSlot> TheirOffers = TradeHelper.GetAllItemsInGrid(TradeGrid);
				foreach (var TheirOffer in TheirOffers)
				{
					Enum.TryParse(TradeHelper.GetTrimmedCurrencyName(TheirOffer.GetFullName()), true, out Currency Cur);
					CurrentValue += TradeHelper.GetCurrencyWorth(Cur, TheirOffer.StackAmount);

					if (CurrentValue.LargerThanPercent(TradeHelper.GetCurrencyWorth(ItemToSell.SellPrice.CurrencyType, ItemToSell.SellPrice.Amount), 0.01))
					{
						bHasFoundFullValue = true;
						Logger.WriteLine("BehaviourTree: Has found full currency value.");
					}
					else
					{
						Logger.WriteLine("BehaviourTree: Couldn't find full currency value.");
					}
				}

				Thread.Sleep(500 + r.Next(1500));
			}

			return BehaviourTreeStatus.SuccessWithStop;
		}



		private bool ShouldContinueLoop()
		{
			return TradeMessage != null && !bWantsToStop;
		}



















		private BehaviourTreeStatus Action2()
		{
			List<Point> OccupiedSlots = TradeHelper.FindOccupiedSlots(TradeGrid);
			for (int i = 0; i < OccupiedSlots.Count; i++)
			{
				Point Slot = OccupiedSlots[i];

				while (GetClipboardText() != null)
				{
					ClearClipboard();
					Thread.Sleep(25 + r.Next(175));
				}

				if (GetClipboardText() != null)
					Logger.WriteLine("CLIPBOARD NOT EMPTY");

				ItemSlot InvSlot = TradeGrid.Slots[Slot.X][Slot.Y];
				MouseHelper.MoveMouse(InvSlot.SlotCoord.X, InvSlot.SlotCoord.Y, 12, 12, null);

				InputSimulator k = new InputSimulator();
				while (GetClipboardText() == null || GetClipboardText().Length <= 0)
				{
					k.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
					Thread.Sleep(25 + r.Next(175));
				}

				InvSlot.ParseClipboard(GetClipboardText());

				GameItem ItemBase = TradeHelper.GetItemBase(InvSlot.BaseItemName, InvSlot.Rarity == "Magic");
				if (ItemBase != null && (ItemBase.SizeX > 1 || ItemBase.SizeY > 1))
				{
					for (int x = Slot.X; x < Slot.X + ItemBase.SizeX; x++)
					{
						for (int y = Slot.Y; y < Slot.Y + ItemBase.SizeY; y++)
						{
							if (x != Slot.X || y != Slot.Y)
							{
								OccupiedSlots.Remove(new Point(x, y));
							}
						}
					}
				}

				bool bIsCurrency = Enum.TryParse(TradeHelper.GetTrimmedCurrencyName(InvSlot.BaseItemName), out Currency c);
				Logger.WriteLine($"Inventory slot {Slot} has {InvSlot.StackAmount} {InvSlot.GeneratedItemName}{InvSlot.BaseItemName}" + (bIsCurrency ? $" which is worth {TradeHelper.GetCurrencyWorth(c, InvSlot.StackAmount)/*MainWindow.PoELogParser.CurrencyValues[c] * InvSlot.StackAmount*/} in Chaos Orbs." : "."));
			}

			Stop();

			return BehaviourTreeStatus.Success;
		}
	}
}
