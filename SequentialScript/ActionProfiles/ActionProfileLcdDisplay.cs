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
    sealed class ActionProfileLcdDisplay : ActionProfile<IMyTerminalBlock>
    {

        readonly List<string> Images = new List<string>();
        readonly List<string> Scripts = new List<string>();

        public override IEnumerable<string> ActionNames => new[] { "Display" };

        public override Action<IMyTerminalBlock, IDictionary<string, string>> OnActionCallback =>
            (block, args) =>
            {
                if (block is IMyTextSurfaceProvider)
                {
                    var index = GetTextSurfaceIndex(args);
                    var textSurface = GetTextSurface(block, index);
                    string value;

                    if (args.TryGetValue("BACKGROUND", out value))
                    {
                        textSurface.BackgroundColor = Helper.ParseColor(value);
                        textSurface.ScriptBackgroundColor = Helper.ParseColor(value);
                    }
                    if (args.TryGetValue("COLOR", out value))
                    {
                        textSurface.FontColor = Helper.ParseColor(value);
                        textSurface.ScriptForegroundColor = Helper.ParseColor(value);
                    }
                    if (args.TryGetValue("PADDING", out value))
                    {
                        textSurface.TextPadding = float.Parse(value);
                    }
                    if (args.TryGetValue("SIZE", out value))
                    {
                        textSurface.FontSize = float.Parse(value);
                    }
                    if (args.TryGetValue("TEXT", out value))
                    {
                        textSurface.ContentType = ContentType.TEXT_AND_IMAGE;
                        textSurface.WriteText(value.Replace("\\n", "\n"));
                    }
                    if (args.TryGetValue("IMAGE", out value))
                    {
                        textSurface.ContentType = ContentType.TEXT_AND_IMAGE;
                        textSurface.ClearImagesFromSelection();
                        foreach (var image in value.Split(','))
                        {
                            textSurface.AddImageToSelection(image);
                        }
                    }
                    if (args.TryGetValue("SCRIPT", out value))
                    {
                        Scripts.Clear();
                        textSurface.GetScripts(Scripts);
                        if (Scripts.Contains(value, StringComparer.OrdinalIgnoreCase))
                        {
                            textSurface.Script = value;
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid script '{value}'. Allowed scripts: {string.Join(", ", Scripts)}");
                        }
                    }
                    else if (args.TryGetValue("NONE", out value))
                    {
                        textSurface.ContentType = ContentType.NONE;
                    }
                }
                else
                {
                    throw new NotSupportedException($"Block '{block.DisplayNameText}' not supports displays.");
                }
            };

        public override Func<IMyTerminalBlock, IDictionary<string, string>, bool> IsCompleteCallback =>
            (block, args) =>
            {
                if (block is IMyTextSurfaceProvider)
                {
                    bool result = true;
                    var index = GetTextSurfaceIndex(args);
                    var textSurface = GetTextSurface(block, index);
                    string value;

                    if (args.TryGetValue("BACKGROUND", out value))
                    {
                        result &= (textSurface.BackgroundColor == Helper.ParseColor(value));
                    }
                    if (args.TryGetValue("COLOR", out value))
                    {
                        result &= (textSurface.FontColor == Helper.ParseColor(value));
                    }
                    if (args.TryGetValue("PADDING", out value))
                    {
                        result &= textSurface.TextPadding == float.Parse(value);
                    }
                    if (args.TryGetValue("SIZE", out value))
                    {
                        result &= textSurface.FontSize == float.Parse(value);
                    }
                    if (args.TryGetValue("TEXT", out value))
                    {
                        result &= (textSurface.ContentType == ContentType.TEXT_AND_IMAGE) && (textSurface.GetText() == value.Replace("\\n", "\n"));
                    }
                    if (args.TryGetValue("IMAGE", out value))
                    {
                        Images.Clear();
                        textSurface.GetSelectedImages(Images);
                        result &= ((textSurface.ContentType == ContentType.TEXT_AND_IMAGE) && (string.Join(",", Images) == value));
                    }
                    return result;
                }
                else
                {
                    throw new NotSupportedException($"Block '{block.DisplayNameText}' not supports displays.");
                }
            };

        protected override string GetCompletionDetails(IMyTerminalBlock block, IDictionary<string, string> args)
        {
            if (block is IMyTextSurfaceProvider)
            {
                string result = "";
                var index = GetTextSurfaceIndex(args);
                var textSurface = GetTextSurface(block, index);
                string value;

                if (args.TryGetValue("TEXT", out value))
                {
                    result += $"Text: '{textSurface.GetText().Replace("\n", "\\n")}' vs '{value}' ";
                }
                if (args.TryGetValue("IMAGE", out value))
                {
                    Images.Clear();
                    textSurface.GetSelectedImages(Images);
                    result += $"Count: {Images.Count}; '{string.Join(",", Images)}' vs '{value}' ";
                }
                return result.TrimEnd();
            }
            else
            {
                throw new NotSupportedException($"Block '{block.DisplayNameText}' not supports displays.");
            }
        }

        static int? GetTextSurfaceIndex(IDictionary<string, string> args)
        {
            int? result = null;
            string stringIndex;

            if (args.TryGetValue("INDEX", out stringIndex))
            {
                int index;

                if (int.TryParse(stringIndex, out index))
                {
                    result = index;
                }
                else
                {
                    throw new FormatException($"Invalid format for '/INDEX:<number>. Value '{stringIndex}' is not numeric.");
                }
            }
            return result;
        }


        static IMyTextSurface GetTextSurface(IMyTerminalBlock block, int? index)
        {
            IMyTextSurface result;
            var textSurfaces = GetTextSurfaces(block).Select((val, i) => new { Index = i, TextSurface = val });

            switch (textSurfaces.Count())
            {
                case 0:
                    throw new NotSupportedException($"Block '{block.DisplayNameText}' is not a supported display block.");

                case 1:
                    var first = textSurfaces.First();

                    if (index != null && first.Index != index)
                    {
                        throw new ArgumentException($"Display index '{index}' is not available for block '{block.DisplayNameText}'.");
                    }
                    else
                    {
                        result = first.TextSurface;
                    }
                    break;

                default:
                    if (index == null)
                    {
                        throw new ArgumentException("Missing parameter '/INDEX:<number>'");
                    }
                    else
                    {
                        var selected = textSurfaces.SingleOrDefault(x => x.Index == index.Value);

                        if (selected == null)
                        {
                            throw new ArgumentException($"Display index '{index}' is not available for block '{block.DisplayNameText}'.");
                        }
                        else
                        {
                            result = selected.TextSurface;
                        }
                    }
                    break;
            }
            return result;
        }

        static IEnumerable<IMyTextSurface> GetTextSurfaces(IMyTerminalBlock block)
        {
            var provider = (IMyTextSurfaceProvider)block;
            var textSurfaces = new List<IMyTextSurface>();

            for (int i = 0; i < provider.SurfaceCount; i++)
            {
                textSurfaces.Add(provider.GetSurface(i));
            }
            return textSurfaces;
        }


    }
}
