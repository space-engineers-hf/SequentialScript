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
    sealed class ActionProfileMotorStatorForward : ActionProfile<IMyMotorStator>
    {

        public override IEnumerable<string> ActionNames => new[] { "Forward" };
        public override string GroupName => "Direction";

        public override Action<IMyMotorStator, IDictionary<string, string>> OnActionCallback =>
            (block, args) => block.TargetVelocityRPM = Math.Abs(block.TargetVelocityRPM);

        public override Func<IMyMotorStator, IDictionary<string, string>, bool> IsCompleteCallback =>
            // Rotor is moving in positive direction. If upper limit is set, it checks that angle has arrived to the limit.
            (block, args) => block.TargetVelocityRad > 0 && (block.UpperLimitRad == float.MaxValue || block.Angle >= block.UpperLimitRad);

    }
}
