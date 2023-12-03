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
    public class Instruction
    {

        /// <summary>
        /// Gets or sets the <see cref="IMyTerminalBlock"/> of the block where execute the <see cref="ActionName"/>.
        /// </summary>
        public string BlockName { get; set; }

        /// <summary>
        /// Gets or sets the name of the action that <see cref="BlockName"/> must execute.
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// Gets or sets the list of arguments for the instruction.
        /// </summary>
        public IDictionary<string, string> Arguments { get; set; }

        /// <summary>
        /// Gets or sets if this <see cref="ActionGroup"/> was parsed successfully.
        /// </summary>
        public bool IsValid { get; set; }

    }
}
