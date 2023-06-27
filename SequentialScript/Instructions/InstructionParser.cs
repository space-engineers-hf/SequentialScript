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
    class InstructionParser
    {

        // https://regex101.com/r/2Vysiy/4
        const string commandPattern = @"\[(?<command>.*?)\]\r?\n(?<body>[\s\S]*?)(?=\r?\n\[(.*?)\]|$)";
        // https://regex101.com/r/DEsq3A/1
        const string bodyPattern = @"(?:when\s+(?<when>.*?)\s+)?run\s+(?<run>.*?)\s+as\s+(?<var>@\w+|none)";
        const string dependencesPattern = @"@\w+";

        static readonly System.Text.RegularExpressions.Regex commandRegex
            = new System.Text.RegularExpressions.Regex(commandPattern, System.Text.RegularExpressions.RegexOptions.Singleline);
        static readonly System.Text.RegularExpressions.Regex bodyRegex
            = new System.Text.RegularExpressions.Regex(bodyPattern, System.Text.RegularExpressions.RegexOptions.Singleline);
        static readonly System.Text.RegularExpressions.Regex dependencesRegex
            = new System.Text.RegularExpressions.Regex(dependencesPattern);

        public static IDictionary<string, InstructionCommand> Parse(string text)
        {
            var result = new Dictionary<string, InstructionCommand>();
            var commandMatches = commandRegex.Matches(text);

            foreach (System.Text.RegularExpressions.Match commandMatch in commandMatches)
            {
                var commandName = commandMatch.Groups["command"].Value.Trim();
                var bodyContent = commandMatch.Groups["body"].Value.Trim();
                var instructionBlocks = new Dictionary<string, InstructionBlock>(StringComparer.OrdinalIgnoreCase);
                var bodyMatches = bodyRegex.Matches(bodyContent);

                // Match blocks and its instructions.
                foreach (System.Text.RegularExpressions.Match bodyMatch in bodyMatches)
                {
                    var whenGroup = bodyMatch.Groups["when"]?.Value;
                    var runGroup = bodyMatch.Groups["run"].Value;
                    var alias = bodyMatch.Groups["var"].Value.Trim();

                    if (string.IsNullOrEmpty(alias) || !(alias == "none" || alias.StartsWith("@")))
                    {
                        throw new FormatException("Invalid text after 'as' clausule.");
                    }
                    else if (instructionBlocks.ContainsKey(alias))
                    {
                        throw new ArgumentException($"'{alias}' has been declared several times.");
                    }
                    else
                    {
                        var actions = runGroup
                            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select((line, index) =>
                            {
                                var lineItems = line.Trim().Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                                string blockName = null, actionName = null;
                                bool isValid;

                                if (lineItems.Length > 0)
                                {
                                    blockName = lineItems[0].Trim();
                                }
                                if (lineItems.Length > 1)
                                {
                                    actionName = lineItems[1].Trim();
                                }
                                isValid = lineItems.Length == 2;
                                return new
                                {
                                    Line = index + 1,
                                    BlockName = blockName,
                                    ActionName = actionName,
                                    IsValid = isValid
                                };
                            })
                        ;
                        var invalid = actions.Where(x => !x.IsValid);
                        if (invalid.Any())
                        {
                            throw new FormatException($"Invalid sentence in '{alias}' for lines: {string.Join(",", invalid.Select(x => x.Line))}");
                        }
                        else
                        {
                            var previousActionsMatch =
                                dependencesRegex
                                    .Matches(whenGroup)
                                    .OfType<System.Text.RegularExpressions.Match>()
                                    .Select(x => x.Value.Trim());

                            instructionBlocks.Add(alias, new InstructionBlock
                            {
                                Alias = alias,
                                PreviousAlias = previousActionsMatch,
                                Instructions = actions.Select(x => new Instruction
                                {
                                    BlockName = x.BlockName,
                                    ActionName = x.ActionName,
                                    IsValid = x.IsValid
                                })
                            });
                        }
                    }
                }

                // Check dependences.
                foreach (var item in instructionBlocks.Values)
                {
                    CheckDependences(instructionBlocks, item);
                }

                // Insert command.
                result.Add(commandName, new InstructionCommand
                {
                    CommandName = commandName,
                    Body = instructionBlocks.Values
                });
            }
            return result;
        }

        /// <summary>
        /// Checks if dependences exists and if there are cyclical references.
        /// </summary>
        /// <param name="collection">Full list of <see cref="InstructionBlock"/>.</param>
        /// <param name="item">The <see cref="InstructionBlock"/> to validate.</param>
        /// <param name="path">This is a list for check cyclical references during recursive calls.</param>
        /// <exception cref="Exception">There are cyclical references.</exception>
        /// <exception cref="NullReferenceException">The <see cref="InstructionBlock.PreviousAlias"/> contains some <see cref="InstructionBlock.Alias"/> that has not been declared.</exception>
        private static void CheckDependences(IDictionary<string, InstructionBlock> collection, InstructionBlock item, IList<string> path = null)
        {
            if (path == null)
            {
                path = new List<string>();
            }
            if (path.Contains(item.Alias, StringComparer.OrdinalIgnoreCase))
            {
                throw new Exception($"'{item.Alias}' calls itself in the following path: '{string.Join("/", path)}/{item.Alias}'. Please check 'when' clausules."); // TODO: Create StackOverflowException
            }
            else
            {
                path.Insert(0, item.Alias);
                foreach (var dependence in item.PreviousAlias)
                {
                    InstructionBlock previous;

                    if (collection.TryGetValue(dependence, out previous))
                    {
                        CheckDependences(collection, previous, path);
                    }
                    else
                    {
                        throw new NullReferenceException($"Unknown dependence '{dependence}' found in 'when' clausule.");
                    }
                }
            }
        }

    }
}
