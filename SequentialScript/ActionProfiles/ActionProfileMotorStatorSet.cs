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
    sealed class ActionProfileMotorStatorSet : ActionProfile<IMyMotorStator>
    {

        public override IEnumerable<string> ActionNames => new[] { "Set" };

        public override Action<IMyMotorStator, IDictionary<string, string>> OnActionCallback =>
            (block, args) =>
            {
                string value;

                if (args.TryGetValue("MAX", out value))
                {
                    block.UpperLimitDeg = ParseValue(value) ?? float.MaxValue;
                }
                if (args.TryGetValue("MIN", out value))
                {
                    block.LowerLimitDeg = ParseValue(value) ?? float.MinValue;
                }
            };

        public override Func<IMyMotorStator, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) =>
            {
                bool result = true;
                string value;

                if (args.TryGetValue("MAX", out value))
                {
                    result &= (block.UpperLimitDeg == (ParseValue(value) ?? float.MaxValue));
                }
                if (args.TryGetValue("MIN", out value))
                {
                    result &= (block.LowerLimitDeg == (ParseValue(value) ?? float.MinValue));
                }
                return result;
            };

        static float? ParseValue(string value)
        {
            float? result;

            if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                result = null;
            }
            else
            {
                result = float.Parse(value);
            }
            return result;
        }

    }
}
