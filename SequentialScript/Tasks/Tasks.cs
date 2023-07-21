using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
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
    static class Tasks
    {

        enum TaskStatus
        {
            Pending,
            Running,
            Completed
        }

        /// <summary>
        /// Creates a list of tasks from a list of <see cref="InstructionBlock"/>.
        /// </summary>
        /// <param name="instructions">List of <see cref="InstructionBlock"/>.</param>
        /// <param name="grid">The <see cref="IMyBlockGroup"/> where all the blocks are placed.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static IEnumerable<Task> CreateTasks(IEnumerable<InstructionBlock> instructions, IEnumerable<IMyTerminalBlock> blockList, IEnumerable<IMyBlockGroup> blockGroup)
        {
            var result = new List<Task>();

            foreach (var instructionBlock in instructions)
            {
                var actions = new List<TaskAction>();

                foreach (var instruction in instructionBlock.Instructions)
                {
                    IEnumerable<IMyTerminalBlock> blocks = null;

                    // Try get blocks.
                    if (instruction.BlockName.StartsWith("*") || instruction.BlockName.EndsWith("*"))
                    {
                        var groups = blockGroup.Where(x => x.Name.Equals(instruction.BlockName.Substring(1, instruction.BlockName.Length - 2), StringComparison.OrdinalIgnoreCase));

                        // Groups are between "*", but if it has not been found, it will check in block list.
                        if (groups.Any())
                        {
                            blocks = groups.SelectMany(group => group.GetBlocks());
                        }
                    }
                    if (blocks == null)
                    {
                        blocks = blockList.Where(x => x.DisplayNameText.Equals(instruction.BlockName, StringComparison.OrdinalIgnoreCase));
                    }
                    if (blocks.Any())
                    {
                        foreach (var block in blocks)
                        {
                            actions.Add(new TaskAction { Block = block, ActionProfile = ActionProfiles.GetActionProfile(block, instruction.ActionName), Arguments = instruction.Arguments, Ignore = instruction.Ignore });
                        }
                    }
                    else
                    {
                        throw new KeyNotFoundException($"No blocks found with name '{instruction.BlockName}'.");
                    }
                }
                result.Add(new Task
                {
                    Alias = instructionBlock.Alias,
                    Actions = actions,
                    PreviousTasks = result.Join(instructionBlock.PreviousAlias, x => x.Alias, y => y, (x, y) => x),
                });
            }
            // Calculate dependences.
            foreach (var task in result)
            {
                task.PreviousTasks = task.PreviousTasks.ToArray();
            }
            return result;
        }

        public static IEnumerable<Task> Run(this IEnumerable<Task> tasks)
        {
            foreach (var task in tasks)
            {
                var status = GetStatus(task);

                if (status == TaskStatus.Completed && task.IsRunning)
                {
                    // Update task status.
                    task.IsRunning = false;
                    task.IsDone = true;
                }
                else if (status == TaskStatus.Pending)
                {
                    // Search if this task can start.
                    var previousCompleted = task.PreviousTasks.All(previous => GetStatus(previous) == TaskStatus.Completed);

                    if (previousCompleted)
                    {
                        foreach (var action in task.Actions)
                        {
                            action.ActionProfile.OnActionCallback(action.Block, action.Arguments);
                        }
                        task.IsRunning = true;
                    }
                }
            }
            return tasks.Where(task=> task.IsRunning == true);
        }

        /// <summary>
        /// Change all <see cref="Task"/> in <paramref name="tasks"/> status to pending.
        /// </summary>
        public static void Cancel(this IEnumerable<Task> tasks)
        {
            tasks.ForEach(task => task.IsRunning = false);
        }

        /// <summary>
        /// Returns if all <paramref name="tasks"/> are completed.
        /// </summary>
        public static bool IsCompleted(this IEnumerable<Task> tasks)
        {
            return tasks.Any(task => task.Actions.All(action => GetStatus(task) == TaskStatus.Completed));
        }

        /// <summary>
        /// Returns the task status.
        /// </summary>
        /// <remarks>
        /// !IsRunning && !Completed = not started
        /// IsRunning && !Completed = started
        /// Completed = finished
        /// </remarks>
        private static TaskStatus GetStatus(Task task)
        {
            TaskStatus status;

            if (task.IsDone || task.Actions.All(action => action.Ignore || action.ActionProfile.IsCompleteCallback(action.Block)))
            {
                status = TaskStatus.Completed;
            }
            else if (task.IsRunning)
            {
                status = TaskStatus.Running;
            }
            else
            {
                status = TaskStatus.Pending;
            }
            return status;
        }

    }
}
