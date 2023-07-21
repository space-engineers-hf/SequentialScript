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
    partial class Program : MyGridProgram
    {

        IEnumerable<Task> _tasks;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.None;
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {

            if (CommonHelper.IsCycle(updateSource))
            {
                IEnumerable<Task> tasksRunning;
                var debug = new StringBuilder();

                tasksRunning = _tasks.Run();
                if (tasksRunning.Any())
                {
                    // Debug running tasks and its pending instructions.
                    debug.AppendLine("Running:");
                    foreach (var task in tasksRunning)
                    {
                        debug.AppendLine($" -> {task.Alias}");
                        foreach (var action in task.Actions)
                        {
                            debug.AppendLine($"  - {action.Block.DisplayNameText}.{action.ActionProfile.ActionNames.First()} = {action.ActionProfile.IsCompleteCallback(action.Block, action.Arguments)}");
                        }
                    }
                    Echo($"{debug}");
                }
                else
                {
                    // Finish cycle.
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    Echo("Done.");
                }
            }
            else
            {
                IDictionary<string, ICommandInstruction> commands;
                ICommandInstruction command;

                try
                {
                    commands = InstructionParser.Parse(Me.CustomData);

                    if (string.IsNullOrWhiteSpace(argument))
                    {
                        // TODO: Compilar todo.
                    }
                    else if (commands.TryGetValue(argument, out command))
                    {
                        _tasks?.Cancel();
                        if (command is InstructionCommand)
                        {
                            _tasks = Tasks.CreateTasks(((InstructionCommand)command).Body, GridTerminalSystem.GetBlocks(), GridTerminalSystem.GetBlockGroups());
                            Runtime.UpdateFrequency = UpdateFrequency.Update10;
                            Echo("Started.");
                        }
                        else if (command is ConditionCommandInstruction)
                        {
                            var blocks = GridTerminalSystem.GetBlocks();
                            var groups = GridTerminalSystem.GetBlockGroups();
                            var conditionCommand = (ConditionCommandInstruction)command;
                            var conditionBlocks = conditionCommand.Body.GetEnumerator();
                            bool positive = false;

                            while (!positive && conditionBlocks.MoveNext())
                            {
                                var condition = conditionBlocks.Current;

                                // Check positive condition.
                                if (string.IsNullOrEmpty(condition.When))
                                {
                                    positive = true; // 'else' condition.
                                }
                                else
                                {
                                    InstructionCommand whenCommand;
                                    IEnumerable<Task> whenTasks;

                                    whenCommand = (InstructionCommand)commands[condition.When];
                                    whenTasks = Tasks.CreateTasks(whenCommand.Body, blocks, groups);
                                    if (Tasks.IsCompleted(whenTasks))
                                    {
                                        positive = true;
                                    }
                                }
                                if (positive)
                                {
                                    InstructionCommand thenCommand;

                                    // TODO: Multiple tasks.
                                    thenCommand = (InstructionCommand)commands[condition.Then.Single()];
                                    _tasks = Tasks.CreateTasks(thenCommand.Body, blocks, groups);
                                    Runtime.UpdateFrequency = UpdateFrequency.Update10;
                                    Echo("Started.");
                                }
                            }

                        }
                    }
                    else
                    {
                        Echo($"ERROR: Command not found: '{argument}'");
                    }
                }
                catch (Exception ex)
                {
                    Echo($"ERROR: {ex.Message}");
                }
            }
        }
    }
}
