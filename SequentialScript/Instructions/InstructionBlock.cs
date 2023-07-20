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
    public class InstructionBlock
    {

        /// <summary>
        /// Gets or sets the <see cref="ActionGroup"/> alias.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets a list of <see cref="Alias"/> of the previous <see cref="InstructionBlock"/> that must finish before estar this one.
        /// </summary>
        public IEnumerable<string> PreviousAlias { get; set; }

        /// <summary>
        /// Gets or sets a list of <see cref="Instruction"/> that this <see cref="InstructionBlock"/> must execute. 
        /// The action group is fully completed when all its actions are completed.
        /// </summary>
        public IEnumerable<Instruction> Instructions { get; set; }

    }
}
