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
    sealed class ActionProfileProgrammableBlockRun : ActionProfile<IMyProgrammableBlock>
    {

        public override IEnumerable<string> ActionNames => new[] { "Run" };

        public override Action<IMyProgrammableBlock, IDictionary<string, string>> OnActionCallback =>
            (block, args) =>
            {
                string argument;

                // If exsits "/ARG:<argument>" use it; else, use default argument.
                if (args.TryGetValue("ARG", out argument))
                {
                    block.TryRun(argument);
                }
                else
                {
                    block.TryRun(block.TerminalRunArgument);
                }
            };

        public override Func<IMyProgrammableBlock, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) => block.IsRunning;

    }
}
