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
    sealed class ActionProfileThrusterSet : ActionProfile<IMyThrust>
    {

        public override IEnumerable<string> ActionNames => new[] { "Set" };

        public override Action<IMyThrust, IDictionary<string, string>> OnActionCallback =>
            (block, args) =>
            {
                string value;

                if (args.TryGetValue("OVERRIDE", out value))
                {
                    block.ThrustOverridePercentage = float.Parse(value);
                }
            };

        public override Func<IMyThrust, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) =>
            {
                bool result = true;
                string value;

                if (args.TryGetValue("OVERRIDE", out value))
                {
                    result &= (block.ThrustOverridePercentage == float.Parse(value));
                }
                return result;
            };
        
    }
}
