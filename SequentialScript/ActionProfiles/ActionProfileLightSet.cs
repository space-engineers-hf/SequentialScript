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
    sealed class ActionProfileLightSet : ActionProfile<IMyLightingBlock>
    {

        public override IEnumerable<string> ActionNames => new[] { "SET" };

        public override Action<IMyLightingBlock, IDictionary<string, string>> OnActionCallback =>
            (block, args) =>
            {
                string value;

                if (args.TryGetValue("COLOR", out value))
                {
                    block.Color = ParseColor(value);
                }
            };

        public override Func<IMyLightingBlock, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) =>
            {
                bool result = true;
                string value;

                if (args.TryGetValue("COLOR", out value))
                {
                    result &= (block.Color == ParseColor(value));
                }
                return result;
            };


        static Color ParseColor(string value)
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

    }
}
