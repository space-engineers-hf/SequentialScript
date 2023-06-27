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
                int tasksRunning;

                tasksRunning = _tasks.Run();
                Echo($"Running {tasksRunning}.");
                if (tasksRunning == 0)
                {
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                }
            }
            else
            {
                IDictionary<string, InstructionCommand> commands;
                InstructionCommand command;

                try
                {
                    commands = InstructionParser.Parse(Me.CustomData);

                    if (commands.TryGetValue(argument, out command))
                    {
                        _tasks?.Cancel();
                        _tasks = Tasks.CreateTasks(command.Body, GridTerminalSystem.GetBlocks(), GridTerminalSystem.GetBlockGroups());
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                        Echo("Started");
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
