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
    sealed class ActionProfileConnectorLock : ActionProfile<IMyShipConnector>
    {

        public override IEnumerable<string> ActionNames => new[] { "Lock", "Connect" };
        public override string GroupName => "Status";

        public override Action<IMyShipConnector, IDictionary<string, string>> OnActionCallback =>
            (block, args) => block.Connect();

        public override Func<IMyShipConnector, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) => block.Status == MyShipConnectorStatus.Connected;

    }
}
