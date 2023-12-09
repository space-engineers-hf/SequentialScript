﻿using Sandbox.Game.EntityComponents;
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
    sealed class ActionProfileConnectorUnlock : ActionProfile<IMyShipConnector>
    {

        public override IEnumerable<string> ActionNames => new[] { "Unlock", "Disconnect" };
        public override string GroupName => "Status";

        public override Action<IMyShipConnector, IDictionary<string, string>> OnActionCallback =>
            (block, args) => block.Disconnect();

        public override Func<IMyShipConnector, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) =>
            {
                bool result = true;
                string value;

                result = (block.Status != MyShipConnectorStatus.Connected);
                if (args.TryGetValue("FULL", out value))
                {
                    result &= (block.Status == MyShipConnectorStatus.Unconnected);
                }
                return result;
            };
            

    }
}
