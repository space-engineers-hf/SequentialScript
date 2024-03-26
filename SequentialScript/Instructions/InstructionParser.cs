using IngameScript.Instructions;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems.Chat;
using Sandbox.Game.Gui;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Policy;
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
    public class InstructionParser
    {

        // https://regex101.com/r/2Vysiy/5
        const string commandPattern = @"\[(?<command>.*?)\](?:\r?\n)+(?<body>[\s\S]*?)(?=\r?\n\[(.*?)\]|$)";
        // https://regex101.com/r/DEsq3A/3
        const string bodyPattern = @"(?:when\s+(?:(?<when>[\w@,| ]*)\s+)(?:\/\/[ \w]+\s+)?)?run(?<run>\s?.*?)\s+as\s+(?<var>[\w@]+)(?:\r?|;)";
        // https://regex101.com/r/fu9UHl/6
        const string ifPattern = @"(?:(?<clausule>if\s+|else\s?if\s+|else\s?)(?<condition>#\w+)?[\r\n]+(?<body>.*?)(?=[\r\n]+(?<close>if|else if|else|end)))";

        static readonly System.Text.RegularExpressions.Regex commandRegex
            = new System.Text.RegularExpressions.Regex(commandPattern, System.Text.RegularExpressions.RegexOptions.Singleline);
        static readonly System.Text.RegularExpressions.Regex bodyRegex
            = new System.Text.RegularExpressions.Regex(bodyPattern, System.Text.RegularExpressions.RegexOptions.Singleline);
        static readonly System.Text.RegularExpressions.Regex ifRegex
            = new System.Text.RegularExpressions.Regex(ifPattern, System.Text.RegularExpressions.RegexOptions.Singleline);


        public static IDictionary<string, ICommandInstruction> Parse(string text)
        {
            var result = new Dictionary<string, ICommandInstruction>(StringComparer.OrdinalIgnoreCase);
            var commandMatches = commandRegex.Matches(text);

            // Create commands.
            foreach (System.Text.RegularExpressions.Match commandMatch in commandMatches)
            {
                ICommandInstruction instructionCommand;

                instructionCommand = CreateCommand(commandMatch, text);
                if (instructionCommand == null)
                {
                    instructionCommand = CreateConditionCommand(commandMatch);
                }
                if (instructionCommand == null)
                {
                    throw new FormatException($"Unknown command body format.");
                }
                result.Add(instructionCommand.CommandName, instructionCommand);
            }
            ValidateCommandDependences(result);
            return result;
        }

        private static InstructionCommand CreateCommand(System.Text.RegularExpressions.Match commandMatch, string text)
        {
            InstructionCommand result = null;
            var instructionBlocks = new Dictionary<string, InstructionBlock>(StringComparer.OrdinalIgnoreCase);
            var commandName = commandMatch.Groups["command"].Value.Trim();
            var bodyGroup = commandMatch.Groups["body"];
            var bodyContent = bodyGroup.Value.Trim();

            var bodyMatches = bodyRegex.Matches(bodyContent);

            if (bodyMatches.OfType<System.Text.RegularExpressions.Match>().Any())
            {
                var previousIndex = -1;
                var lineCount = 1;

                // Match blocks and its instructions.
                lineCount += GetLineCount(text, 0, bodyGroup.Index);
                foreach (System.Text.RegularExpressions.Match bodyMatch in bodyMatches)
                {
                    MatchBody(bodyMatch, instructionBlocks, bodyContent, ref previousIndex, ref lineCount);
                }

                // Search invalid strings from previous body match to the end of the string.
                try
                {
                    CheckSyntax(bodyContent, previousIndex, bodyContent.Length);
                }
                catch (SyntaxException ex)
                {
                    throw new SyntaxException(ex.OriginalMessage, lineCount + ex.Line, ex.Pos, ex.InnerException);
                }

                // Check dependences.
                foreach (var item in instructionBlocks.Values)
                {
                    CheckDependences(instructionBlocks, item);
                }

                // Insert command.
                result = new InstructionCommand
                {
                    CommandName = commandName,
                    Body = instructionBlocks.Values
                };
            }
            return result;
        }

        internal static void MatchBody(
            System.Text.RegularExpressions.Match bodyMatch,
            IDictionary<string, InstructionBlock> instructionBlocks,
            string bodyContent,
            ref int previousIndex, ref int lineCount)
        {
            int lineStart;

            try
            {
                lineCount += CheckSyntax(bodyContent, previousIndex + 1, bodyMatch.Index); // Search invalid strings from previous body match to new body match.
            }
            catch (SyntaxException ex)
            {
                throw new SyntaxException(ex.OriginalMessage, lineCount + ex.Line, ex.Pos, ex.InnerException);
            }
            lineStart = lineCount;
            lineCount += GetLineCount(bodyContent, bodyMatch.Index, bodyMatch.Length);

            var whenGroup = bodyMatch.Groups["when"]?.Value;
            var runGroup = bodyMatch.Groups["run"];
            var run = runGroup.Value;
            var aliasGroup = bodyMatch.Groups["var"];
            var alias = aliasGroup.Value.Trim();

            if (string.IsNullOrEmpty(alias) || !(alias == "none" || alias.StartsWith("@")))
            {
                throw new SyntaxException("Invalid text after 'as' clausule.",
                    lineStart + GetLineCount(bodyContent, bodyMatch.Index, aliasGroup.Index - bodyMatch.Index)
                );
            }
            else if (instructionBlocks.ContainsKey(alias))
            {
                throw new SyntaxException($"'{alias}' has been declared several times.",
                    lineStart + GetLineCount(bodyContent, bodyMatch.Index, aliasGroup.Index - bodyMatch.Index)
                );
            }
            else
            {
                var actions = run
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("//")) // Ignore empty lines and comments.
                    .Select((line, index) =>
                    {
                        // Remove comments
                        var lineComments = line.Split(new[] { "//" }, StringSplitOptions.None);
                        var lineWithoutComments = lineComments.First();

                        // Take BlockName and ActionName
                        var lineItems = lineWithoutComments.Trim().Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
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

                        // Arguments
                        var arguments = GetArguments(actionName);

                        return new
                        {
                            Line = index + 1,
                            BlockName = blockName,
                            ActionName = arguments[""].Trim(),
                            Arguments = arguments.Where(x => !string.IsNullOrEmpty(x.Key)).ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase),
                            IsValid = isValid
                        };
                    })
                ;

                var invalid = actions.Where(x => !x.IsValid);
                if (invalid.Any())
                {
                    throw new SyntaxException(
                        $"Invalid sentence in '{alias}' for lines: {string.Join(",", invalid.Select(x => x.Line))}",
                        lineStart + GetLineCount(bodyContent, bodyMatch.Index, runGroup.Index - bodyMatch.Index)
                    );
                }
                else
                {
                    var previousActionsMatch =
                        whenGroup
                            .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .GroupJoin(
                                instructionBlocks.Keys,
                                x => x,
                                y => $"@{y}",
                                (x, y) => new { Alias = x, Exists = y.Any(), IsValid = x.StartsWith("@") || x.Equals("none", StringComparison.OrdinalIgnoreCase) }
                            );

                    if (previousActionsMatch.Any(x => !x.IsValid))
                    {
                        throw new FormatException(
                            $"Invalid syntax near to 'when' clausule in '{alias}' (line {lineStart}): " +
                            $"{string.Join(",", previousActionsMatch.Where(x => !x.IsValid).Select(x => $"{x.Alias}"))} is not valid.\n" +
                            $"Maybe you forgot the '@'."
                        );
                    }
                    else
                    {
                        if (alias.Equals("none", StringComparison.OrdinalIgnoreCase))
                        {
                            alias = $"Unnamed_{instructionBlocks.Count:00}";
                        }
                        instructionBlocks.Add(alias, new InstructionBlock
                        {
                            Alias = alias,
                            PreviousAlias = previousActionsMatch.Select(x => x.Alias),
                            Instructions = actions.Select(x => new Instruction
                            {
                                BlockName = x.BlockName,
                                ActionName = x.ActionName,
                                Arguments = x.Arguments,
                                IsValid = x.IsValid
                            })
                        });
                    }
                }
            }
            previousIndex = bodyMatch.Index + bodyMatch.Length;
        }

        private static ConditionCommandInstruction CreateConditionCommand(System.Text.RegularExpressions.Match commandMatch)
        {
            ConditionCommandInstruction result = null;
            var instructionBlocks = new List<ConditionBlockInstruction>();
            var commandName = commandMatch.Groups["command"].Value.Trim();
            var bodyGroup = commandMatch.Groups["body"];
            var bodyContent = bodyGroup.Value.Trim();
            var bodyMatches = ifRegex.Matches(bodyContent);

            if (bodyMatches.OfType<System.Text.RegularExpressions.Match>().Any())
            {
                bool ifCondition = false, elseCondition = false;

                // Comprobar si se encontró una coincidencia
                foreach (System.Text.RegularExpressions.Match match in bodyMatches)
                {
                    MatchCondition(match, instructionBlocks, ref ifCondition, ref elseCondition);
                }

                // Insert command.
                result = new ConditionCommandInstruction
                {
                    CommandName = commandName,
                    Body = instructionBlocks
                };
            }
            return result;
        }

        internal static void MatchCondition(
            System.Text.RegularExpressions.Match match,
            List<ConditionBlockInstruction> instructionBlocks,
            ref bool ifCondition, ref bool elseCondition)
        {
            var clausule = match.Groups["clausule"].Value.Trim();
            var close = match.Groups["close"].Value.Trim();
            var condition = match.Groups["condition"].Value?.Trim();
            var body = match.Groups["body"].Value.Trim();

            if (clausule.Equals("if", StringComparison.OrdinalIgnoreCase))
            {
                if (!ifCondition)
                {
                    ifCondition = true;
                }
                else
                {
                    throw new Exception($"Syntact incorrect near '{clausule}'.");
                }
            }
            if (!ifCondition)
            {
                throw new Exception($"Syntact incorrect near '{clausule}': 'if' condition not found.");
            }
            else if (clausule.Equals("else", StringComparison.OrdinalIgnoreCase))
            {
                if (!elseCondition) // Else debe ser la última condición.
                {
                    if (close.Equals("end", StringComparison.OrdinalIgnoreCase))
                    {
                        elseCondition = true;
                    }
                    else
                    {
                        throw new Exception($"Syntact incorrect near '{clausule}': 'end' clausule not found.");
                    }
                }
                else
                {
                    throw new Exception($"Syntact incorrect near '{clausule}': there are already other 'else' clausule.");
                }
            }
            else if (!condition.StartsWith("#"))
            {
                throw new Exception($"'{condition}' is not a valid command name becaue it does not start with '#' character.");
            }
            else
            {
                condition = condition.Substring(1);
            }

            instructionBlocks.Add(new ConditionBlockInstruction
            {
                When = condition,
                Then = body
                        .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => line.Trim().Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries).First().Trim())
                        .Where(line =>
                        {
                            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                            {
                                return false;
                            }
                            else if (line.StartsWith("#"))
                            {
                                return true;
                            }
                            else
                            {
                                throw new Exception($"'{condition}' is not a valid command name becaue it does not start with '#' character.");
                            }
                        }) // Ignore empty lines and comments.
                        .Select(value => value.Substring(1))
            });
        }

        private static IDictionary<string, string> GetArguments(string value)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            int i;
            char c;
            int stage = 0; // 0: None; 1: Reading argument; 2: Reading Value; 3: Save;
            bool instring = false;
            var keyBuilder = new System.Text.StringBuilder();
            var valueBuilder = new System.Text.StringBuilder();

            for (i = 0; i < value.Length; i++)
            {
                c = value[i];

                if (c == '/' && !instring)
                {
                    result.Add(keyBuilder.ToString(), valueBuilder.ToString());
                    keyBuilder.Clear();
                    valueBuilder.Clear();
                    stage = 1; // Begin argument
                }
                else if (c == ':' && !instring)
                {
                    stage = 2; // Begin value
                }
                else if (c == ' ' && !instring && stage > 0)
                {
                    stage = 3; // Save.
                }
                else if (c == '\"')
                {
                    instring = !instring;
                }
                else if (stage == 1)
                {
                    keyBuilder.Append(c);
                }
                else if (stage == 2 || stage == 0)
                {
                    valueBuilder.Append(c);
                }
            }
            if (stage < 3)
            {
                if (instring)
                {
                    throw new FormatException("String not closed.");
                }
                else
                {
                    result.Add(keyBuilder.ToString(), valueBuilder.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// Calculates how many lines are in the specific substring.
        /// </summary>
        private static int GetLineCount(string content, int startIndex, int length)
        {
            int i = 0;
            char previous = char.MinValue;
            var substring = content.Substring(startIndex, length);

            foreach (var chr in substring)
            {
                if (chr == '\r')
                {
                    i++;
                }
                else if (chr == '\n' && previous != '\r')
                {
                    i++;
                }
                previous = chr;
            }
            return i;
        }

        /// <summary>
        /// Checks if there are unknown instructions in the specific string range and returns the number of lines.
        /// </summary>
        /// <returns>Number of lines</returns>
        internal static int CheckSyntax(string content, int startIndex, int endIndex)
        {
            var substring = content.Substring(startIndex, endIndex - startIndex);
            var lines = substring.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None).Select(x => x.Trim());
            var buffer = new System.Text.StringBuilder();
            char previous = char.MinValue;
            bool validate = false;
            var i = 0;

            foreach (var chr in substring)
            {
                if (chr == '\r')
                {
                    validate = true;
                }
                else if (chr == '\n')
                {
                    if (previous != '\r')
                    {
                        validate = true;
                    }
                }
                else
                {
                    buffer.Append(chr);
                }

                if (validate)
                {
                    var value = buffer.ToString().Trim();

                    if (string.IsNullOrEmpty(value))
                    {
                        // Empty line.
                    }
                    else if (value.StartsWith("//"))
                    {
                        // It is a comment.
                    }
                    else
                    {
                        throw new SyntaxException($"Syntaxt exception near '{value}'.", i);
                    }
                    buffer.Clear();
                    i++;
                    validate = false;
                }
                previous = chr;
            }
            return i;
        }

        /// <summary>
        /// Checks if dependences exists and if there are cyclical references.
        /// </summary>
        /// <param name="collection">Full list of <see cref="InstructionBlock"/>.</param>
        /// <param name="item">The <see cref="InstructionBlock"/> to validate.</param>
        /// <param name="path">This is a list for check cyclical references during recursive calls.</param>
        /// <exception cref="StackOverflowException">There are cyclical references.</exception>
        /// <exception cref="NullReferenceException">The <see cref="InstructionBlock.PreviousAlias"/> contains some <see cref="InstructionBlock.Alias"/> that has not been declared.</exception>
        internal static void CheckDependences(IDictionary<string, InstructionBlock> collection, InstructionBlock item, IEnumerable<string> path = null)
        {
            string stringPath;

            if (path == null)
            {
                path = new List<string>();
            }
            stringPath = string.Join("/", path.Append(item.Alias).Reverse());
            if (path.Contains(item.Alias, StringComparer.OrdinalIgnoreCase))
            {
                throw new StackOverflowException($"'{item.Alias}' calls itself in the following path: '{stringPath}'. Please check 'when' clausules.");
            }
            else
            {
                var newpath = path.Append(item.Alias);

                foreach (var dependence in item.PreviousAlias)
                {
                    InstructionBlock previous;

                    if (collection.TryGetValue(dependence, out previous))
                    {
                        CheckDependences(collection, previous, newpath);
                    }
                    else
                    {
                        throw new NullReferenceException($"Unknown dependence '{dependence}' found in 'when' clausule.");
                    }
                }
            }
        }

        internal static void ValidateCommandDependences(IDictionary<string, ICommandInstruction> result)
        {
            foreach (var instructionCommand in result.Values.OfType<ConditionCommandInstruction>())
            {
                foreach (var conditions in instructionCommand.Body)
                {
                    if (!string.IsNullOrEmpty(conditions.When))
                    {
                        ICommandInstruction command;
                        string whenName = conditions.When;

                        if (whenName.Equals(instructionCommand.CommandName, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new ArgumentException($"A command cannot call itself ({whenName}).");
                        }
                        if (!result.TryGetValue(whenName, out command))
                        {
                            throw new NullReferenceException($"Command with name '{whenName}' not found.");
                        }
                        else if (!(command is InstructionCommand))
                        {
                            throw new ArgumentException($"Conditional commands not allowed '({whenName})' in conditional commands.");
                        }
                    }
                    foreach (var then in conditions.Then)
                    {
                        ICommandInstruction command;
                        string thenName = then;

                        if (thenName.Equals(instructionCommand.CommandName, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new ArgumentException($"A command cannot call itself ({thenName}).");
                        }
                        else if (!result.TryGetValue(thenName, out command))
                        {
                            throw new NullReferenceException($"Command with name '{thenName}' not found.");
                        }
                        else if (!(command is InstructionCommand))
                        {
                            throw new ArgumentException($"Conditional commands not allowed '({thenName})' in conditional commands.");
                        }
                    }
                }
            }
        }

    }
}
