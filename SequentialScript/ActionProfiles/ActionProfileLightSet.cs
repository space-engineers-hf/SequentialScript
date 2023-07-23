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
    sealed class ActionProfileLightSet : ActionProfile<IMyLightingBlock>
    {

        public override IEnumerable<string> ActionNames => new[] { "Set" };

        public override Action<IMyLightingBlock, IDictionary<string, string>> OnActionCallback =>
            (block, args) =>
            {
                string value;

                if (args.TryGetValue("COLOR", out value))
                {
                    block.Color = Helper.ParseColor(value);
                }
            };

        public override Func<IMyLightingBlock, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) =>
            {
                bool result = true;
                string value;

                if (args.TryGetValue("COLOR", out value))
                {
                    result &= (block.Color == Helper.ParseColor(value));
                }
                return result;
            };
        
    }
}
