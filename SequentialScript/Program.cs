using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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

        static readonly bool DEBUG_IN_SCREEN = false;
        static readonly UpdateFrequency UPDATE_FREQUENCY = UpdateFrequency.Update10; // Update1, Update10, Update100
        static readonly int UPDATE_TICKS = 0; // Very slow mode multiplier (for debug)

        IDictionary<string, ICommandInstruction> _commands;
        InstructionCommand _command;
        IEnumerable<Task> _tasks;
        ConditionCommandInstruction _conditionCommand;
        int _checkIndex;
        int ticks;

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
                var debug = new StringBuilder();
                var now = DateTime.UtcNow;

                if (UPDATE_TICKS == 0 || ticks % UPDATE_TICKS == 0)
                {
                    if (_command != null)
                    {
                        IEnumerable<Task> thenTasks;

                        thenTasks = Tasks.CreateTasks(_command.Body, GridTerminalSystem.GetBlocks(), GridTerminalSystem.GetBlockGroups());
                        StartTasks(thenTasks, $"{debug}\nStarted.");
                    }
                    else if (_tasks != null)
                    {
                        IEnumerable<Task> tasksRunning;

                        tasksRunning = _tasks.Run(debug);
                        if (tasksRunning.Any())
                        {
                            AdvancedEcho($"{debug}");
                            AdvancedEcho($"Running tasks: {(DateTime.UtcNow - now).TotalMilliseconds:N0}", true);
                        }
                        else
                        {
                            // Finish cycle.
                            Runtime.UpdateFrequency = UpdateFrequency.None;
                            AdvancedEcho("Done.", true);
                        }
                    }
                    else if (_conditionCommand != null)
                    {
                        Check(_conditionCommand);
                        AdvancedEcho($"Condition command: {(DateTime.UtcNow - now).TotalMilliseconds:N0}", true);
                    }
                    else
                    {
                        throw new Exception("Invalid state.");
                    }
                    ticks = 0;
                }
                ticks++;
            }
            else
            {
                ICommandInstruction command;

                try
                {
                    _commands = InstructionParser.Parse(Me.CustomData);
                    if (string.IsNullOrWhiteSpace(argument))
                    {
                        // TODO: Compilar todo.
                    }
                    else if (_commands.TryGetValue(argument, out command))
                    {

                        _tasks?.Cancel();
                        if (command is InstructionCommand)
                        {
                            StartCommand((InstructionCommand)command);
                        }
                        else if (command is ConditionCommandInstruction)
                        {
                            StartCheck((ConditionCommandInstruction)command);
                        }
                    }
                    else
                    {
                        AdvancedEcho($"ERROR: Command not found: '{argument}'");
                    }
                }
                catch (Exception ex)
                {
                    AdvancedEcho($"ERROR: {ex.Message}");
                }
            }
        }

        
        void StartCommand(InstructionCommand command, string message = "Command started.")
        {
            _command = command;
            _tasks = null;
            _conditionCommand = null;
            _checkIndex = 0;
            Runtime.UpdateFrequency = UPDATE_FREQUENCY;
            ticks = 1;

            AdvancedEcho(message);
        }

        /// <summary>
        /// Starts tasks.
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="message"></param>
        void StartTasks(IEnumerable<Task> tasks, string message = "Started.")
        {
            _command = null;
            _tasks = tasks;
            _conditionCommand = null;
            Runtime.UpdateFrequency = UPDATE_FREQUENCY;
            ticks = 1;

            AdvancedEcho(message);
        }


        /// <summary>
        /// Starts to check a condition command.
        /// </summary>
        /// <param name="conditionCommand"></param>
        /// <param name="message"></param>
        void StartCheck(ConditionCommandInstruction conditionCommand, string message = "")
        {
            _command = null;
            _tasks = null;
            _conditionCommand = conditionCommand;
            _checkIndex = 0;
            Runtime.UpdateFrequency = UPDATE_FREQUENCY;
            ticks = 1;

            AdvancedEcho(message);
        }

        /// <summary>
        /// Checks if current command is done. 
        /// </summary>
        /// <param name="conditionCommand"></param>
        void Check(ConditionCommandInstruction conditionCommand)
        {
            var debug = new StringBuilder();
            var now = DateTime.UtcNow;

            if (conditionCommand != null && _checkIndex < _commands.Count)
            {
                var blocks = GridTerminalSystem.GetBlocks();
                var groups = GridTerminalSystem.GetBlockGroups();
                bool positive = false;

                var condition = conditionCommand.Body[_checkIndex];

                debug.AppendLine($"{_checkIndex}");

                // Check positive condition.
                if (string.IsNullOrEmpty(condition.When))
                {
                    debug.AppendLine($"ELSE");
                    positive = true; // 'else' condition.
                }
                else
                {
                    InstructionCommand whenCommand;
                    IEnumerable<Task> whenTasks;

                    whenCommand = (InstructionCommand)_commands[condition.When];
                    whenTasks = Tasks.CreateTasks(whenCommand.Body, blocks, groups);

                    debug.AppendLine($"{whenCommand.CommandName}");
                    if (Tasks.IsCompleted(whenTasks, debug))
                    {
                        positive = true;
                    }
                }

                if (positive)
                {
                    InstructionCommand thenCommand;

                    // TODO: Multiple tasks.
                    thenCommand = (InstructionCommand)_commands[condition.Then.Single()];
                    StartCommand(thenCommand);
                }
                else
                {
                    _checkIndex++;
                }
            }
        }

        void AdvancedEcho(string message, bool append = false)
        {
            string value = message;

            if (append)
            {
                var builder = new StringBuilder(Me.CustomInfo);

                builder.Append(value);
                message = builder.ToString();
            }
            Echo(message);
            if (DEBUG_IN_SCREEN)
            {
                var displays = DisplayHelper.GetTextSurfaces(new[] { Me });
                var display = displays.First().TextSurface;

                display.ContentType = ContentType.TEXT_AND_IMAGE;
                display.WriteText(message);
            }
        }

    }
}
