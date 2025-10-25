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
    sealed class ActionProfileConveyorSorterSet : ActionProfile<IMyConveyorSorter>
    {

        public override IEnumerable<string> ActionNames => new[] { "Set" };
        public override string GroupName => "Set";

        public override Action<IMyConveyorSorter, IDictionary<string, string>> OnActionCallback =>
            (block, args) =>
            {
                MyConveyorSorterMode? mode;
                List<MyInventoryItemFilter> itemTypes;

                Parse(args, out mode, out itemTypes);
                if (mode == null)
                {
                    mode = block.Mode;
                }
                if (itemTypes == null)
                {
                    itemTypes = new List<MyInventoryItemFilter>();
                    block.GetFilterList(itemTypes);
                }
                block.SetFilter(mode.Value, itemTypes);
            };

        public override Func<IMyConveyorSorter, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) => true;

        protected override string GetCompletionDetails(IMyConveyorSorter block, IDictionary<string, string> args)
        {
            string result;

            MyConveyorSorterMode? mode;
            List<MyInventoryItemFilter> itemTypes;
            Parse(args, out mode, out itemTypes);
            if (mode == null)
            {
                mode = block.Mode;
            }
            if (itemTypes == null)
            {
                itemTypes = new List<MyInventoryItemFilter>();
                block.GetFilterList(itemTypes);
            }
            result = $"{mode};{string.Join(",", itemTypes.Select(x => x.ItemId.SubtypeName))}";
            return result;
        }

        static void Parse(IDictionary<string, string> args, out MyConveyorSorterMode? mode, out List<MyInventoryItemFilter> itemTypes)
        {
            string itemNames;
            string modeName;
            MyDefinitionId itemDefintion;

            if (args.TryGetValue("MODE", out modeName))
            {
                modeName = modeName.Trim().ToUpper();
                switch (modeName)
                {
                    case "BLACK":
                    case "BLACKLIST":
                        mode = MyConveyorSorterMode.Blacklist;
                        break;
                    case "WHITE":
                    case "WHITELIST":
                        mode = MyConveyorSorterMode.Whitelist;
                        break;
                    default:
                        throw new ArgumentException($"Invalid mode '{modeName}'. Use BLACK/WHITE modes.", "MODE");
                }
            }
            else
            {
                mode = null;
            }
            if (args.TryGetValue("ITEMS", out itemNames))
            {
                itemTypes = new List<MyInventoryItemFilter>();
                foreach (var itemName in itemNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (MyDefinitionId.TryParse(itemName, out itemDefintion))
                    {
                        var itemType = new MyInventoryItemFilter(itemDefintion);

                        itemTypes.Add(itemType);
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown item with name '{itemName}'.", "ITEMS");
                    }
                }
            }
            else
            {
                itemTypes = null;
            }
        }

    }
}
