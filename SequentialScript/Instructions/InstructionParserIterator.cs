using IngameScript.Instructions;
using Sandbox.Game.EntityComponents;
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
    public class InstructionParserIterator
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

        readonly double _millisecondsLimit;
        readonly IEnumerator<System.Text.RegularExpressions.Match> _commandMatchesEtor;
        System.Text.RegularExpressions.Match _currentCommandMatch;
        IEnumerator<System.Text.RegularExpressions.Match> _bodyMatchesEtor;
        //System.Text.RegularExpressions.Match _currentBodyMatch;

        /// <summary> Command list building. </summary>
        readonly Dictionary<string, ICommandInstruction> _commands;
        string _commandName;
        System.Text.RegularExpressions.Group _bodyGroup;
        string _bodyContent;
        /// <summary> Body for instruction blocks </summary>
        IDictionary<string, InstructionBlock> _instructionBlockBody;
        /// <summary> Body for condition blocks </summary>
        List<ConditionBlockInstruction> _conditionBlockBody;

        int _previousIndex;
        int _lineCount;
        bool _ifCondition;
        bool _elseCondition;


        public InstructionParserIterator(IEnumerable<System.Text.RegularExpressions.Match> commandMatches, double millisecondsLimit)
        {
            _commandMatchesEtor = commandMatches.GetEnumerator();
            _commands = new Dictionary<string, ICommandInstruction>(StringComparer.OrdinalIgnoreCase);
            _millisecondsLimit = millisecondsLimit;
        }

        public IDictionary<string, ICommandInstruction> Continue(StringBuilder debug = null)
        {
            Dictionary<string, ICommandInstruction> result = null;
            bool done = false;
            var momento = DateTime.Now;

            do
            {
                if (_currentCommandMatch == null)
                {
                    CommandMoveNext(debug);
                }

                if (_currentCommandMatch == null)
                {
                    InstructionParser.ValidateCommandDependences(_commands);
                    done = true;
                    result = _commands;
                }
                else if (_instructionBlockBody != null)
                {
                    debug.AppendLine($"{_commandName} - instructionBlock");
                    InstructionBlockMoveNext();
                }
                else if (_conditionBlockBody != null)
                {
                    debug.AppendLine($"{_commandName} - conditionBlock");
                    ConditionBlockMoveNext();
                }
                else
                {
                    throw new FormatException($"Unknown command body format.");
                }
            } while (!done && (DateTime.Now - momento).TotalMilliseconds < _millisecondsLimit);
            return result;
        }

        private void CommandMoveNext(StringBuilder debug)
        {
            if (_commandMatchesEtor.MoveNext())
            {
                System.Text.RegularExpressions.MatchCollection bodyMatchesCollection;
                IEnumerable<System.Text.RegularExpressions.Match> bodyMatches;

                _currentCommandMatch = _commandMatchesEtor.Current;
                _commandName = _currentCommandMatch.Groups["command"].Value.Trim();
                _bodyGroup = _currentCommandMatch.Groups["body"];
                _bodyContent = _bodyGroup.Value.Trim();
                _previousIndex = -1;
                _lineCount = -1;

                bodyMatchesCollection = bodyRegex.Matches(_bodyContent);
                bodyMatches = bodyMatchesCollection.OfType<System.Text.RegularExpressions.Match>();
                if (bodyMatches.Any())
                {
                    // Block command
                    debug?.AppendLine("Block command");
                    _bodyMatchesEtor = bodyMatches.GetEnumerator();
                    _instructionBlockBody = new Dictionary<string, InstructionBlock>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    bodyMatches = ifRegex.Matches(_bodyContent).OfType<System.Text.RegularExpressions.Match>();
                    if (bodyMatches.Any())
                    {
                        // Condition command
                        debug?.AppendLine("Condition command");
                        _bodyMatchesEtor = bodyMatches.GetEnumerator();
                        _conditionBlockBody = new List<ConditionBlockInstruction>();
                        _ifCondition = false;
                        _elseCondition = false;
                    }
                    else
                    {
                        debug?.AppendLine("Unknown commnad");
                    }
                }
            }
        }

        private void InstructionBlockMoveNext()
        {
            if (_bodyMatchesEtor.MoveNext())
            {
                InstructionParser.MatchBody(_bodyMatchesEtor.Current, _instructionBlockBody, _bodyContent, ref _previousIndex, ref _lineCount);
            }
            else
            {
                // Search invalid strings from previous body match to the end of the string.
                try
                {
                    InstructionParser.CheckSyntax(_bodyContent, _previousIndex, _bodyContent.Length);
                }
                catch (SyntaxException ex)
                {
                    throw new SyntaxException(ex.OriginalMessage, _lineCount + ex.Line, ex.Pos, ex.InnerException);
                }

                // Check dependences.
                foreach (var item in _instructionBlockBody.Values)
                {
                    InstructionParser.CheckDependences(_instructionBlockBody, item);
                }

                // Insert command.
                _commands.Add(_commandName, new InstructionCommand
                {
                    CommandName = _commandName,
                    Body = _instructionBlockBody.Values.ToArray()
                });
                _instructionBlockBody.Clear();
                _instructionBlockBody = null;
                _bodyMatchesEtor.Dispose();
                _bodyMatchesEtor = null;
                _currentCommandMatch = null;
            }
        }

        private void ConditionBlockMoveNext()
        {
            if (_bodyMatchesEtor.MoveNext())
            {
                InstructionParser.MatchCondition(_bodyMatchesEtor.Current, _conditionBlockBody, ref _ifCondition, ref _elseCondition);
            }
            else
            {
                _commands.Add(_commandName, new ConditionCommandInstruction
                {
                    CommandName = _commandName,
                    Body = _conditionBlockBody.ToArray()
                });
                _conditionBlockBody.Clear();
                _conditionBlockBody = null;
                _bodyMatchesEtor.Dispose();
                _bodyMatchesEtor = null;
                _currentCommandMatch = null;
            }
        }

        internal static InstructionParserIterator Create(string text, double millisecondsLimit)
        {
            var result = new Dictionary<string, ICommandInstruction>(StringComparer.OrdinalIgnoreCase);
            var commandMatches = commandRegex.Matches(text).OfType<System.Text.RegularExpressions.Match>();

            return new InstructionParserIterator(commandMatches, millisecondsLimit);
        }

    }
}
