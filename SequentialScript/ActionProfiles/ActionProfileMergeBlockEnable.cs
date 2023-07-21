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
    sealed class ActionProfileMergeBlockEnable : ActionProfile<IMyShipMergeBlock>
    {

        public override IEnumerable<string> ActionNames => new[] { "Enable", "On", "Lock" };

        public override Action<IMyShipMergeBlock> OnActionCallback => 
            block => block.Enabled = true;

        public override Func<IMyShipMergeBlock, bool> IsCompleteCallback => 
            block => block.State == MergeState.Locked;
    }
}
