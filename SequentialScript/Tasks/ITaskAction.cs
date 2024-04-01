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
    interface ITaskAction
    {

        string ActionKey { get; }

        /// <summary>
        /// Gets or sets if this task action must be checked during "<see cref="Tasks.TaskStatusMode.Condition"/>" validation.
        /// </summary>
        bool IsCommandCondition { get; set; }

        /// <summary>
        /// Gets or sets when this task started or null if it has not started yet.
        /// </summary>
        DateTime? StartTime { get; set; }

        /// <summary>
        /// Starts the task.
        /// </summary>
        void Execute();

        /// <summary>
        /// Checks if this task has been ended.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="momento"></param>
        /// <param name="debug"></param>
        bool Check(TaskStatusMode mode, DateTime? momento = null, StringBuilder debug = null);

    }
}
