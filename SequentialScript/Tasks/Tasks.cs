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

        enum TaskStatusMode
        {
            /// <summary>
            /// Checks if task is already done.
            /// </summary>
            Condition,
            /// <summary>
            /// Checks if every actions in the task have run and finished.
            /// </summary>
            Run
        }

        /// <summary>
        /// Creates a list of tasks from a list of <see cref="InstructionBlock"/>.
        /// </summary>
        /// <param name="instructions">List of <see cref="InstructionBlock"/>.</param>
        /// <param name="grid">The <see cref="IMyBlockGroup"/> where all the blocks are placed.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static IEnumerable<Task> CreateTasks(IEnumerable<InstructionBlock> instructions, IDictionary<string, IEnumerable<IMyTerminalBlock>> blockDictionary)
        {
            var result = new List<Task>();

            foreach (var instructionBlock in instructions)
            {
                var actions = new List<TaskAction>();

                foreach (var instruction in instructionBlock.Instructions)
                {
                    IEnumerable<IMyTerminalBlock> blocks = null;

                    // Try get blocks.
                    if (blockDictionary.TryGetValue(instruction.BlockName, out blocks))
                    {
                        foreach (var block in blocks)
                        {
                            string argumentValue;
                            bool check;
                            int wait;

                            // Check parameters
                            check = instruction.Arguments.TryGetValue("CHECK", out argumentValue) && (string.IsNullOrWhiteSpace(argumentValue) || argumentValue.Equals("true", StringComparison.OrdinalIgnoreCase));
                            if (instruction.Arguments.TryGetValue("MAXWAIT", out argumentValue))
                            {
                                if (string.IsNullOrWhiteSpace(argumentValue) || !int.TryParse(argumentValue, out wait))
                                {
                                    throw new FormatException($"'/MAXWAIT' must have a numeric value.");
                                }
                            }
                            else if (instruction.Arguments.TryGetValue("WAIT", out argumentValue))
                            {
                                if (string.IsNullOrWhiteSpace(argumentValue) || !int.TryParse(argumentValue, out wait))
                                {
                                    throw new FormatException($"'/WAIT' must have a numeric value.");
                                }
                            }
                            else if (instruction.Arguments.TryGetValue("NOWAIT", out argumentValue) && (string.IsNullOrWhiteSpace(argumentValue) || argumentValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
                            {
                                wait = 0;
                            }
                            else
                            {
                                wait = -1;
                            }

                            // Add action.
                            actions.Add(new TaskAction
                            {
                                Block = block,
                                ActionProfile = ActionProfiles.GetActionProfile(block, instruction.ActionName),
                                Arguments = instruction.Arguments,
                                Check = check,
                                Wait = wait
                            });
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
                    PreviousTasks = result.Join(instructionBlock.PreviousAlias, x => x.Alias, y => y, (x, y) => x, StringComparer.OrdinalIgnoreCase),
                });
            }
            // Calculate dependences.
            foreach (var task in result)
            {
                task.PreviousTasks = task.PreviousTasks.ToArray();
            }
            return result;
        }

        public static IEnumerable<Task> Run(this IEnumerable<Task> tasks, StringBuilder debug = null)
        {
            var momento = DateTime.UtcNow;

            debug?.AppendLine("Running:");
            foreach (var task in tasks)
            {
                var status = GetStatus(task, TaskStatusMode.Run, momento, debug);

                if (status == TaskStatus.Completed && task.IsRunning)
                {
                    // Update task status.
                    task.IsRunning = false;
                    task.IsDone = true;
                }
                else if (status == TaskStatus.Pending)
                {
                    // Search if this task can start.
                    var previousCompleted = task.PreviousTasks.All(previous => previous.IsDone);

                    debug?.AppendLine($"{task.Alias} previous completed: {previousCompleted} ({string.Join(", ", task.PreviousTasks.Select(x => x.Alias))})");
                    if (previousCompleted)
                    {
                        foreach (var action in task.Actions)
                        {
                            action.ActionProfile.OnActionCallback(action.Block, action.Arguments);
                            action.StartTime = momento;
                        }
                        task.IsRunning = true;
                    }
                }
            }
            return tasks.Where(task => task.IsRunning == true);
        }

        /// <summary>
        /// Change all <see cref="Task"/> in <paramref name="tasks"/> status to pending.
        /// </summary>
        public static void Cancel(this IEnumerable<Task> tasks)
        {
            tasks.ForEach(task =>
            {
                task.IsRunning = false;
                task.Actions.ForEach(action => action.StartTime = null);
            });
        }

        /// <summary>
        /// Returns if all <paramref name="tasks"/> are completed.
        /// </summary>
        public static bool IsCompleted(this IEnumerable<Task> tasks, StringBuilder debug = null)
        {
            bool result;

            debug?.AppendLine("Checking:");
            result = IsDone(GetLastActions(tasks), TaskStatusMode.Condition, null, debug);
            debug?.AppendLine($"Result: {result}");
            return result;
        }

        /// <summary>
        /// Returns the task status.
        /// </summary>
        /// <remarks>
        /// !IsRunning && !Completed = not started
        /// IsRunning && !Completed = started
        /// Completed = finished
        /// </remarks>
        private static TaskStatus GetStatus(Task task, TaskStatusMode mode, DateTime? momento = null, StringBuilder debug = null)
        {
            TaskStatus status;

            debug?.AppendLine($" -> {task.Alias} ({(task.IsRunning ? "Running" : task.IsDone ? "Done" : "Pending")})");
            if (task.IsDone)
            {
                status = TaskStatus.Completed;
            }
            else if (task.IsRunning)
            {
                if (IsDone(task, mode, momento, debug))
                {
                    status = TaskStatus.Completed;
                }
                else
                {
                    status = TaskStatus.Running;
                }
            }
            else
            {
                status = TaskStatus.Pending;
            }
            return status;
        }

        /// <summary>
        /// Checks if all elements in a <see cref="Task"/> have been completed.
        /// </summary>
        private static bool IsDone(Task task, TaskStatusMode mode, DateTime? momento = null, StringBuilder debug = null)
        {
            return IsDone(task.Actions, mode, momento, debug);
        }

        /// <summary>
        /// Checks if all <see cref="TaskAction"/> have been completed.
        /// </summary>
        private static bool IsDone(IEnumerable<TaskAction> actions, TaskStatusMode mode, DateTime? momento = null, StringBuilder debug = null)
        {
            return actions.All(action =>
            {
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
                            sufixDebug = $"(wait:{action.Wait})";
                        }
                        else if (action.Wait > -1) // -1 wait until done
                        {
                            isCompleted = (action.StartTime != null && (((momento ?? DateTime.UtcNow) - action.StartTime.Value).TotalMilliseconds > action.Wait)); // Wait for maximun...
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
            });
        }

        /// <summary>
        /// Returns the last action for each block taking in mind the running sequence.
        /// </summary>
        /// <param name="tasks">List of instruction blocks.</param>
        public static IEnumerable<TaskAction> GetLastActions(this IEnumerable<Task> tasks)
        {
            var actionDictionary = new Dictionary<string, TaskAction>(StringComparer.OrdinalIgnoreCase);
            var validatedTasks = new List<Task>(); // All tasks already validated.
            IEnumerable<Task> validationTasks; // Tasks to validate for each iteration.
            var i = 0;

            validationTasks = tasks.Where(x => !x.PreviousTasks.Any()); // Start with tasks without parents.
            while (validationTasks.Any())
            {
                // Get all tasks and save its actions in the dictionary.
                foreach (var task in validationTasks)
                {
                    foreach (var action in task.Actions)
                    {
                        actionDictionary[$"{action.Block.EntityId}\t{action.ActionProfile.GroupName}"] = action;
                    }
                    validatedTasks.Add(task);
                }
                // Get tasks pending to validate whose previous tasks are already validated.
                validationTasks = tasks
                    .Except(validatedTasks)
                    .Where(t => t.PreviousTasks.All(x => validatedTasks.Contains(x)));
                i++;
            }
            return actionDictionary.Values;
        }

    }
}
