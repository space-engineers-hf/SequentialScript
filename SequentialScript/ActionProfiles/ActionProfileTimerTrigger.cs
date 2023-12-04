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
    sealed class ActionProfileTimerTrigger : ActionProfile<IMyTimerBlock>
    {

        public override IEnumerable<string> ActionNames => new[] { "Trigger" };
        public override string GroupName => "Status";

        public override Action<IMyTimerBlock, IDictionary<string, string>> OnActionCallback =>
            (block, args) => block.Trigger();

        public override Func<IMyTimerBlock, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) => true;

    }
}
