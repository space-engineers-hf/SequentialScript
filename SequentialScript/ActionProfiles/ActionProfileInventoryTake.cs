using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    sealed class ActionProfileInventoryTake : ActionProfile<IMyTerminalBlock>
    {

        public static IMyGridTerminalSystem GridTerminalSystem { get; set; }


        public override IEnumerable<string> ActionNames => new[] { "Take" };
        public override string GroupName => "Take";

        public override Action<IMyTerminalBlock, IDictionary<string, string>> OnActionCallback =>
            (block, args) =>
            {
                MyItemType itemType;
                MyFixedPoint amount;
                IMyInventory inventory;

                if (!IsCompleteCallback(block, args))
                {
                    Parse(block, args, out itemType, out amount);
                    inventory = block.TryGetInventory(itemType);
                    if (inventory == null)
                    {
                        throw new ArgumentException($"Item type '{itemType}' cannot store in '{block.DisplayNameText}'.");
                    }
                    else
                    {
                        var blockItems = GetOtherBlocks(block);
                        var inventories = blockItems.Where(x => x.IsSameConstructAs(block)).FindItem(itemType, amount, inventory);
                        var etor = inventories.GetEnumerator();
                        var amountPending = amount;

                        while (etor.MoveNext() && amountPending > 0)
                        {
                            var amountTransfer = MyFixedPoint.Min(amountPending, etor.Current.Item.Amount);

                            inventory.TransferItemFrom(etor.Current.Inventory, etor.Current.Item, amountTransfer);
                            amountPending -= amountTransfer;
                        }
                    }
                }
            };

        public override Func<IMyTerminalBlock, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) =>
            {
                bool result;
                if (!block.HasInventory)
                {
                    result = false;
                }
                else
                {
                    MyItemType itemType;
                    MyFixedPoint amount;
                    IMyInventory inventory;

                    Parse(block, args, out itemType, out amount);
                    inventory = block.TryGetInventory(itemType);
                    if (inventory == null)
                    {
                        throw new ArgumentException($"Item type '{itemType}' cannot store in '{block.DisplayNameText}'.");
                    }
                    else
                    {
                        result = (inventory.GetItemAmount(itemType) >= amount);
                    }
                }
                return result;
            };

        protected override string GetCompletionDetails(IMyTerminalBlock block, IDictionary<string, string> arguments)
        {
            string result;
            if (!block.HasInventory)
            {
                result = null;
            }
            else
            {
                MyItemType itemType;
                MyFixedPoint amount;
                IMyInventory inventory;

                Parse(block, arguments, out itemType, out amount);
                inventory = block.TryGetInventory(itemType);
                if (inventory == null)
                {
                    throw new ArgumentException($"Item type '{itemType}' cannot store in '{block.DisplayNameText}'.");
                }
                else
                {
                    result = $"Amount: {inventory.GetItemAmount(itemType)}";
                }
            }
            return result;
        }

        static void Parse(IMyTerminalBlock block, IDictionary<string, string> args, out MyItemType itemType, out MyFixedPoint amount)
        {
            if (!block.HasInventory)
            {
                throw new NotSupportedException($"'{block.DisplayNameText}' does not support inventory.");
            }
            else
            {
                string itemName;
                string amountString;
                int amountInt;
                MyDefinitionId itemDefintion;

                if (args.TryGetValue("ITEM", out itemName))
                {
                    if (MyDefinitionId.TryParse(itemName, out itemDefintion))
                    {
                        itemType = MyItemType.Parse(itemName);
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown item with name '{itemName}'.", "ITEM");
                    }
                }
                else
                {
                    throw new ArgumentNullException("ITEM", $"'item' argument is mandatory.");
                }
                if (args.TryGetValue("QUANTITY", out amountString))
                {
                    if (!int.TryParse(amountString, out amountInt))
                    {
                        throw new ArgumentException($"Invalid value '{amountString}', quantity must be a numeric value.", "ITEM");
                    }
                    else
                    {
                        amount = amountInt;
                    }
                }
                else
                {
                    amount = 1;
                }
            }
        }

        static IEnumerable<IMyTerminalBlock> GetOtherBlocks(IMyTerminalBlock block)
        {
            return GridTerminalSystem.GetBlocks().Where(x => x != block);
        }

    }
}
