using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
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
    sealed class ActionProfileConnectorThrow : ActionProfile<IMyShipConnector>
    {

        public override IEnumerable<string> ActionNames => new[] { "Throw", "ThrowOut" };

        public override string GroupName => "ThrowOut";

        public override Action<IMyShipConnector, IDictionary<string, string>> OnActionCallback =>
            (block, args) => block.ThrowOut = true;

        public override Func<IMyShipConnector, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) =>
            {
                bool result;

                if (!block.HasInventory)
                {
                    result = false;
                }
                else
                {
                    IList<MyInventoryItem> items;

                    items = block.GetItems();
                    if (items == null)
                    {
                        throw new ArgumentException($"No inventory found in '{block.DisplayNameText}'.");
                    }
                    else
                    {
                        result = (block.ThrowOut && items.Count == 0);
                    }
                }
                return result;
            };

    }
}
