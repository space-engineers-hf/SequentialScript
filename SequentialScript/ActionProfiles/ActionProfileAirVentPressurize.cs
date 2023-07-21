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
    sealed class ActionProfileAirVentPressurize : ActionProfile<IMyAirVent>
    {

        public override IEnumerable<string> ActionNames => new[] { "Pressurize" };

        public override Action<IMyAirVent, IDictionary<string, string>> OnActionCallback =>
            (block, args) => block.Depressurize = false;

        public override Func<IMyAirVent, bool> IsCompleteCallback => 
            block => block.Status == VentStatus.Pressurized;

    }
}
