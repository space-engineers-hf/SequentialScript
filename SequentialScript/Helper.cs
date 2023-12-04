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
    static class Helper
    {

        public static Color ParseColor(string value)
        {
            Color? color;

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException("Color argument must have some value.");
            }
            else if (value.StartsWith("#"))
            {
                color = ColorHelper.FromHtml(value.Trim());
                if (color == null)
                {
                    throw new FormatException($"Color argument is not a valid HTML value: '{value}'");
                }
            }
            else
            {
                var rgb = value.Trim().Split(new[] { ';', ',' }, StringSplitOptions.None);
                int a, r, g, b;

                if (rgb.Length == 1)
                {
                    color = ColorHelper.FromName(rgb[0]);
                    if (color == null)
                    {
                        throw new FormatException($"Color argument is not a valid color name: '{value}'");
                    }
                }
                else if (rgb.Length == 3 && int.TryParse(rgb[0], out r) && int.TryParse(rgb[1], out g) && int.TryParse(rgb[2], out b))
                {
                    color = ColorHelper.FromRGB(r, g, b);
                }
                else if (rgb.Length == 4 && int.TryParse(rgb[0], out a) && int.TryParse(rgb[1], out r) && int.TryParse(rgb[2], out g) && int.TryParse(rgb[3], out b))
                {
                    color = ColorHelper.FromARGB(a, r, g, b);
                }
                else
                {
                    throw new FormatException($"Color argument is not a valid RGB value: '{value}'.");
                }
            }
            return color.Value;
        }

        /// <summary>
        /// Returns a dictionary of <see cref="IMyTerminalBlock"/> by block names or block group names.
        /// </summary>
        /// <param name="blockNames">List of block names and block group names.</param>
        /// <param name="blockList">List of blocks.</param>
        /// <param name="blockGroup">List of block groups.</param>
        /// <exception cref="KeyNotFoundException">Block name or group name not found.</exception>
        /// <remarks>
        /// If name is between asteristks it will try to find in <paramref name="blockGroup"/> list, else, it will try to find it in <paramref name="blockList"/>. 
        /// </remarks>
        public static IDictionary<string, IEnumerable<IMyTerminalBlock>> CreateBlockDictionary(IEnumerable<string> blockNames, IEnumerable<IMyTerminalBlock> blockList, IEnumerable<IMyBlockGroup> blockGroup)
        {
            var result = new Dictionary<string, IEnumerable<IMyTerminalBlock>>(StringComparer.OrdinalIgnoreCase);
            IEnumerable<IMyTerminalBlock> blocks;

            foreach (var blockName in blockNames)
            {
                if (!result.TryGetValue(blockName, out blocks))
                {
                    if (blockName.StartsWith("*") && blockName.EndsWith("*"))
                    {
                        var groups = blockGroup.Where(x => x.Name.Equals(blockName.Substring(1, blockName.Length - 2), StringComparison.OrdinalIgnoreCase));

                        // Groups are between "*", but if it has not been found, it will check in block list.
                        if (groups.Any())
                        {
                            blocks = groups.SelectMany(group => group.GetBlocks());
                        }
                    }
                    if (blocks == null)
                    {
                        blocks = blockList.Where(x => x.DisplayNameText.Equals(blockName, StringComparison.OrdinalIgnoreCase));
                    }
                    if (blocks.Any())
                    {
                        result.Add(blockName, blocks.ToArray());
                    }
                    else
                    {
                        throw new KeyNotFoundException($"No blocks found with name '{blockName}'.");
                    }
                }
            }
            return result;
        }

    }
}
