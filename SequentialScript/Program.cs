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

        #region mdk preserve

        static readonly bool DEBUG_IN_SCREEN = false;
        static readonly UpdateFrequency UPDATE_FREQUENCY = UpdateFrequency.Update10; // Update1, Update10, Update100
        static readonly int UPDATE_TICKS = 0; // Very slow mode multiplier (for debug)


        /* ----------------------------------------------------------------------------------- */
        /* --- ¡¡¡IMPORTANT!!! Do not change anything below this line. --- */
        /* ----------------------------------------------------------------------------------- */

        #endregion

        DateTime _momento;
        IList<IMyTerminalBlock> _terminalBlocks;
        IList<IMyBlockGroup> _terminalGroups;
        IDictionary<string, IEnumerable<IMyTerminalBlock>> _blocksDictionary;
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

        public void Main(string argument, UpdateType updateSource)
        {
            AdvancedEchoReset();

            if (CommonHelper.IsCycle(updateSource))
            {
                var debug = new StringBuilder();
                var now = DateTime.UtcNow;

                try
                {
                    if (UPDATE_TICKS == 0 || ticks % UPDATE_TICKS == 0)
                    {
                        if (_terminalBlocks == null || _terminalGroups == null)
                        {
                            AdvancedEcho($"Getting terminal blocks", append: true);
                            _terminalBlocks = GridTerminalSystem.GetBlocks();
                            AdvancedEcho($" - OK", append: true);
                            AdvancedEcho($"Getting terminal groups", append: true);
                            _terminalGroups = GridTerminalSystem.GetBlockGroups();
                            AdvancedEcho($" - OK", append: true);
                        }
                        else if (_blocksDictionary == null)
                        {
                            var blockNames = _commands.Values
                                .OfType<InstructionCommand>()
                                .SelectMany(cmd => cmd.Body)
                                .SelectMany(body => body.Instructions)
                                .Where(instruction => instruction.BlockName != null)
                                .Select(instruction => instruction.BlockName)
                                .Distinct();

                            AdvancedEcho($"Building dictionary", append: true);
                            _blocksDictionary = Helper.CreateBlockDictionary(blockNames, _terminalBlocks, _terminalGroups);
                            AdvancedEcho($" - OK", append: true);
                        }
                        else if (_command != null)
                        {
                            IEnumerable<Task> thenTasks;

                            AdvancedEcho($"Running {_command.CommandName}", append: true);
                            thenTasks = Tasks.CreateTasks(_command.Body, _blocksDictionary);
                            StartTasks(thenTasks, $"{debug}\nStarted.", appendMessage: true);
                        }
                        else if (_tasks != null)
                        {
                            IEnumerable<Task> tasksRunning;

                            tasksRunning = _tasks.Run(debug);
                            if (tasksRunning.Any())
                            {
                                AdvancedEcho($"{debug}", append: true);
                                AdvancedEcho($"Running tasks: {(DateTime.UtcNow - now).TotalMilliseconds:N0}", append: true);
                            }
                            else
                            {
                                // Finish cycle.
                                EndCycle();
                                AdvancedEcho("Done.");
                            }
                        }
                        else if (_conditionCommand != null)
                        {
                            CheckNextCondition();
                            AdvancedEcho($"Condition command: {(DateTime.UtcNow - now).TotalMilliseconds:N0}", append: true);
                        }
                        else
                        {
                            throw new Exception("Invalid state.");
                        }
                        ticks = 0;
                    }
                    ticks++;
                }
                catch (Exception ex)
                {
                    EndCycle();
                    AdvancedEcho($"ERROR: {ex.Message}", append: true);
                }

            }
            else
            {
                var debug = new StringBuilder();
                ICommandInstruction command;

                try
                {
                    _tasks?.Cancel();
                    _commands = InstructionParser.Parse(Me.CustomData);
                    if (string.IsNullOrWhiteSpace(argument))
                    {
                        // TODO: Compilar todo.
                    }
                    else if (_commands.TryGetValue(argument, out command))
                    {
                        _terminalBlocks = null;
                        _terminalGroups = null;
                        _blocksDictionary = null;
                        if (command is InstructionCommand)
                        {
                            StartCommand((InstructionCommand)command, $"Command '{command.CommandName}' started.");
                        }
                        else if (command is ConditionCommandInstruction)
                        {
                            StartCheck((ConditionCommandInstruction)command, $"Checking condition '{command.CommandName}'...");
                        }
                    }
                    else
                    {
                        AdvancedEcho($"ERROR: Command not found: '{argument}'");
                    }
                }
                catch (Exception ex)
                {
                    debug.AppendLine($"ERROR: {ex.Message}");
                }
                finally
                {
                    AdvancedEcho($"{debug}");
                }
            }
        }

        /// <summary>
        /// Starts a command.
        /// </summary>
        /// <param name="message">Message for the echo output.</param>
        void StartCommand(InstructionCommand command, string message = "Command started.", bool appendMessage = false)
        {
            _command = command;
            _tasks = null;
            _conditionCommand = null;
            _checkIndex = 0;
            Runtime.UpdateFrequency = UPDATE_FREQUENCY;
            ticks = 1;

            AdvancedEcho(message, appendMessage);
        }

        /// <summary>
        /// Starts tasks.
        /// </summary>
        /// <param name="message">Message for the echo output.</param>
        void StartTasks(IEnumerable<Task> tasks, string message = "Task started.", bool appendMessage = false)
        {
            _command = null;
            _tasks = tasks;
            _conditionCommand = null;
            Runtime.UpdateFrequency = UPDATE_FREQUENCY;
            ticks = 1;

            AdvancedEcho(message, appendMessage);
        }


        /// <summary>
        /// Starts to check a condition command.
        /// </summary>
        /// <param name="message">Message for the echo output.</param>
        void StartCheck(ConditionCommandInstruction conditionCommand, string message = "Checking condition...", bool appendMessage = false)
        {
            _command = null;
            _tasks = null;
            _conditionCommand = conditionCommand;
            _checkIndex = 0;
            Runtime.UpdateFrequency = UPDATE_FREQUENCY;
            ticks = 1;

            AdvancedEcho(message, appendMessage);
        }

        /// <summary>
        /// Checks if the next condition of the if current <see cref="ConditionCommandInstruction"/> is done.
        /// </summary>
        void CheckNextCondition()
        {
            var debug = new StringBuilder();

            if (_conditionCommand != null && _checkIndex < _commands.Count)
            {
                bool positive = false;

                var condition = _conditionCommand.Body[_checkIndex];

                debug.AppendLine($"Index condition: {_checkIndex}");

                // Check positive condition.
                if (string.IsNullOrEmpty(condition.When))
                {
                    debug.AppendLine($"Condition name: ELSE");
                    positive = true; // 'else' condition.
                }
                else
                {
                    InstructionCommand whenCommand;
                    IEnumerable<Task> whenTasks;

                    whenCommand = (InstructionCommand)_commands[condition.When];
                    debug.AppendLine($"Condition name: {whenCommand.CommandName}");

                    whenTasks = Tasks.CreateTasks(whenCommand.Body, _blocksDictionary);
                    if (Tasks.IsCompleted(whenTasks, debug))
                    {
                        positive = true;
                    }
                }
                AdvancedEcho(debug.ToString(), true);

                if (positive)
                {
                    InstructionCommand thenCommand;

                    // TODO: Multiple tasks.
                    thenCommand = (InstructionCommand)_commands[condition.Then.Single()];
                    StartCommand(thenCommand, $"Command started: {thenCommand.CommandName}.", appendMessage: true);
                }
                else
                {
                    _checkIndex++;
                }
            }
            else
            {
                _conditionCommand = null;
                _checkIndex = 0;
            }
        }

        void EndCycle()
        {
            _tasks = null;
            Runtime.UpdateFrequency = UpdateFrequency.None;
        }

        void AdvancedEchoReset()
        {
            _momento = DateTime.UtcNow;
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
            message = $"| Elapsed {(DateTime.UtcNow - _momento).TotalMilliseconds:00}ms |\n{message}";

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
