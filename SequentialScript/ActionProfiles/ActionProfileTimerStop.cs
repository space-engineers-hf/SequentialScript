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
    sealed class ActionProfileTimerStop : ActionProfile<IMyTimerBlock>
    {

        public override IEnumerable<string> ActionNames => new[] { "Stop" };

        public override Action<IMyTimerBlock, IDictionary<string, string>> OnActionCallback =>
            (block, args) => block.StopCountdown();

        public override Func<IMyTimerBlock, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) => !block.IsCountingDown;

    }
}
