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
    class TaskAction
    {

        public IMyTerminalBlock Block { get; set; }
        public IActionProfile ActionProfile { get; set; }
        public IDictionary<string, string> Arguments { get; set; }
        
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets if this task action must be checked during "<see cref="Tasks.TaskStatusMode.Condition"/>" validation.
        /// </summary>
        public bool Check { get; set; }
        
        /// <summary>
        /// Gets or sets the number of milliseconds to wait before assume this task finished. 
        /// <see cref="TaskAction"/> can finish before if <see cref="IActionProfile.IsCompleteCallback"/> returns true.
        /// </summary>
        /// <remarks>
        /// [-1] Wait until it finish.
        /// [0] No wait.
        /// </remarks>
        public int Wait { get; set; }

    }
}
