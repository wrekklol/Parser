using FluentBehaviourTree;
using Parser.PathOfExile.StaticLibrary;
using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static Parser.PathOfExile.StaticLibrary.TradeHelper;
using static Parser.StaticLibrary.MiscLibrary;
using static Parser.StaticLibrary.InputHelper;
using WindowsInput;
using WindowsInput.Native;

namespace Parser.PathOfExile
{
    public class TestBehaviour : ParserBehaviourBase
    {
		private Trader t;
		private ItemGrid InventoryGrid;

		protected override void BuildTree()
        {
			Builder
				.Sequence("my-sequence")
					.Condition("action1_condition", Action1_Condition)
					.Do("action1", Action1)
					.Condition("action2_condition", Action2_Condition)
					.Do("action2", Action2)
				.End();
		}

		protected override void OnStart()
		{
			t = (Trader)Data;
			InventoryGrid = ItemGrids["Inventory"];
		}



		// Action 1
		private BehaviourTreeStatus Action1()
		{
			FocusProcess("PathOfExile_x64Steam");

			MouseHelper.MoveMouse(960, 900, 200, 180, null);
			Thread.Sleep(r.Next(1000));

			return BehaviourTreeStatus.Success;
		}

		private bool Action1_Condition()
		{
			return InventoryGrid.Slots.Count > 0;
		}



		// Action 2
		private BehaviourTreeStatus Action2()
		{
			List<Point> OccupiedSlots = FindOccupiedSlots(InventoryGrid);

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

				ItemSlot InvSlot = InventoryGrid.Slots[Slot.X][Slot.Y];
				MouseHelper.MoveMouse(InvSlot.SlotCoord.X, InvSlot.SlotCoord.Y, 12, 12, null);

				InputSimulator k = new InputSimulator();
				while (GetClipboardText() == null || GetClipboardText().Length <= 0)
				{
					k.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
					Thread.Sleep(25 + r.Next(175));
				}

				InvSlot.ParseClipboard(GetClipboardText());

				GameItem ItemBase = GetItemBase(InvSlot.BaseItemName, InvSlot.Rarity == "Magic");
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

				bool bIsCurrency = Enum.TryParse(LogParser.GetRENAMETHISCurrencyName(InvSlot.BaseItemName), out Currency c);
				Logger.WriteLine($"Inventory slot {Slot} has {InvSlot.StackAmount} {InvSlot.GeneratedItemName}{InvSlot.BaseItemName}" + (bIsCurrency ? $" which is worth {MainWindow.PoELogParser.CurrencyValues[c] * InvSlot.StackAmount} in Chaos Orbs." : "."));
			}

			Stop();

			return BehaviourTreeStatus.Success;
		}

		private bool Action2_Condition()
		{
			return true;
		}
	}
}
