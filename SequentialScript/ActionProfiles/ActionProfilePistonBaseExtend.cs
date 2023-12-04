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
    sealed class ActionProfilePistonBaseExtend : ActionProfile<IMyPistonBase>
    {

        public override IEnumerable<string> ActionNames => new[] { "Extend" };
        public override string GroupName => "Direction";

        public override Action<IMyPistonBase, IDictionary<string, string>> OnActionCallback =>
            (block, args) => block.Extend();

        public override Func<IMyPistonBase, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) => block.Status == PistonStatus.Extended || (block.MaxLimit - block.CurrentPosition) < 0.05;

        protected override string GetCompletionDetails(IMyPistonBase block, IDictionary<string, string> arguments)
            => $"Position: {block.CurrentPosition:N2} ({(block.MaxLimit - block.CurrentPosition):N2}); Status: {block.Status}";

    }
}
