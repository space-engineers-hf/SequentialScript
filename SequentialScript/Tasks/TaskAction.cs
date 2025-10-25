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
    class TaskAction : ITaskAction
    {

        public string ActionKey
        {
            get { return $"{this.Block.EntityId}\t{this.ActionProfile.GroupName}"; }
        }

        public IMyTerminalBlock Block { get; set; }
        public IActionProfile ActionProfile { get; set; }
        public IDictionary<string, string> Arguments { get; set; }

        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets if this task action must be checked during "<see cref="Tasks.TaskStatusMode.Condition"/>" validation.
        /// </summary>
        public bool IsCommandCondition { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds to wait before assume this task finished. 
        /// <see cref="TaskAction"/> can finish before if <see cref="IActionProfile.IsCompleteCallback"/> returns true.
        /// </summary>
        /// <remarks>
        /// [-1] Wait until it finish.
        /// [0] No wait.
        /// </remarks>
        public int Wait { get; set; }


        public void Execute()
        {
            this.ActionProfile.OnActionCallback(this.Block, this.Arguments);
        }

        public bool Check(TaskStatusMode mode, DateTime? momento = null, StringBuilder debug = null)
        {
            var action = this;
            bool isCompleted = false;
            bool printDebug = false;
            string sufixDebug = null;

            switch (mode)
            {
                case TaskStatusMode.Condition:
                    // Condition wait is ignored.
                    if (action.Wait > -1)
                    {
                        sufixDebug = $"(wait:{action.Wait})";
                        isCompleted = true;
                        printDebug = false;
                    }
                    break;

                case TaskStatusMode.Run:
                    if (action.Wait == 0) // No wait.
                    {
                        isCompleted = true;
                    }
                    else if (action.Wait > -1) // -1 wait until done
                    {
                        isCompleted = (action.StartTime != null && (((momento ?? DateTime.UtcNow) - action.StartTime.Value).TotalMilliseconds >= action.Wait)); // Wait for maximun...
                    }
                    sufixDebug = $"(wait:{action.Wait})";
                    printDebug = true;
                    break;

                default:
                    break;
            }
            isCompleted |= action.ActionProfile.IsCompleteCallback(action.Block, action.Arguments);
            if (printDebug && !isCompleted)
            {
                var isCompletedText = (isCompleted ? "Done" : "Pending");
                var actionDebug = action.ActionProfile.GetCompletionDetails(action.Block, action.Arguments);

                if (!string.IsNullOrWhiteSpace(actionDebug))
                {
                    sufixDebug += $" ({actionDebug})";
                }
                debug?.AppendLine($"  - {action.Block.DisplayNameText}.{action.ActionProfile.ActionNames.First()} ({isCompletedText})");
                debug?.AppendLine($"    {sufixDebug?.Trim()} {action.StartTime?.ToString("HH:mm:ss")}");
            }
            return isCompleted;
        }

    }
}
