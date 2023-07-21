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
    abstract class ActionProfile<TMyTerminalBlock> : IActionProfile where TMyTerminalBlock : class, IMyTerminalBlock
    {

        public abstract IEnumerable<string> ActionNames { get; }
        public abstract Action<TMyTerminalBlock, IDictionary<string, string>> OnActionCallback { get; }
        public abstract Func<TMyTerminalBlock, bool> IsCompleteCallback { get; }


        Action<IMyTerminalBlock, IDictionary<string, string>> IActionProfile.OnActionCallback =>
            (block, args) => OnActionCallback(GetMyTerminalBlock(block), args);

        Func<IMyTerminalBlock, bool> IActionProfile.IsCompleteCallback =>
            block => IsCompleteCallback(GetMyTerminalBlock(block));

        bool IActionProfile.IsAssignableFrom(IMyTerminalBlock block)
            => (block is TMyTerminalBlock);

        TMyTerminalBlock GetMyTerminalBlock(IMyTerminalBlock block)
        {
            if (block == null)
            {
                throw new NullReferenceException("Cannot run any action in a null block.");
            }
            else
            {
                var castBlock = block as TMyTerminalBlock;

                if (castBlock == null)
                {
                    throw new InvalidCastException($"Cannot block type '{block.GetType().Name}' does not support the action profile '{this.GetType().Name}'.");
                }
                else
                {
                    return castBlock;
                }
            }
        }

    }
}
