using IngameScript.Instructions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SequentialScript.Test
{
    [TestClass]
    public class InstructionParserTest
    {

        [TestMethod]
        public void TestSimple()
        {
            IDictionary<string, IngameScript.ICommandInstruction> commands;
            var expected = new Dictionary<string, IngameScript.ICommandInstruction>()
            {
                { "OPEN", new IngameScript.InstructionCommand {
                    CommandName = "OPEN",
                    Body = new Dictionary<string, IngameScript.InstructionBlock> {
                        { "@open_door", new IngameScript.InstructionBlock {
                            Alias = "@open_door",
                            PreviousAlias = new string[] { },
                            Instructions = new []{ new IngameScript.Instruction {
                                BlockName = "Door",
                                ActionName = "Open",
                                Arguments = new Dictionary<string, string> { },
                                IsValid = true,
                                Ignore = false
                            }},
                        } }
                    }.Values
                }}
            };

            commands = IngameScript.InstructionParser.Parse(CustomData.Resources.TestSimple);
            InstrucionAssert.AreEqual(expected, commands);
        }

        [TestMethod]
        public void TestComments()
        {
            IDictionary<string, IngameScript.ICommandInstruction> commands;
            var expected = new Dictionary<string, IngameScript.ICommandInstruction>()
            {
                { "OPEN", new IngameScript.InstructionCommand {
                    CommandName = "OPEN",
                    Body = new Dictionary<string, IngameScript.InstructionBlock> {
                        { "@open_door", new IngameScript.InstructionBlock {
                            Alias = "@open_door",
                            PreviousAlias = new string[] { },
                            Instructions = new []{ new IngameScript.Instruction {
                                BlockName = "Door",
                                ActionName = "Open",
                                Arguments = new Dictionary<string, string> { },
                                IsValid = true,
                                Ignore = false
                            }},
                        } }
                    }.Values
                }}
            };

            commands = IngameScript.InstructionParser.Parse(CustomData.Resources.TestComments);
            InstrucionAssert.AreEqual(expected, commands);
        }

        [TestMethod]
        public void TestDependences()
        {
            IDictionary<string, IngameScript.ICommandInstruction> commands;
            var expected = new Dictionary<string, IngameScript.ICommandInstruction>()
            {
                { "OPEN", new IngameScript.InstructionCommand {
                    CommandName = "OPEN",
                    Body = new Dictionary<string, IngameScript.InstructionBlock> {
                        { "@open_door_1", new IngameScript.InstructionBlock {
                            Alias = "@open_door_1",
                            PreviousAlias = new string[] { },
                            Instructions = new []{ new IngameScript.Instruction {
                                BlockName = "Door Light",
                                ActionName = "Enable",
                                Arguments = new Dictionary<string, string> { },
                                IsValid = true,
                                Ignore = false
                            }},
                        } },
                        { "@open_door_2", new IngameScript.InstructionBlock {
                            Alias = "@open_door_2",
                            PreviousAlias = new string[] { "@open_door_1" },
                            Instructions = new []{ new IngameScript.Instruction {
                                BlockName = "Door",
                                ActionName = "Open",
                                Arguments = new Dictionary<string, string> { },
                                IsValid = true,
                                Ignore = false
                            }},
                        } },
                        { "@open_door_3", new IngameScript.InstructionBlock {
                            Alias = "@open_door_3",
                            PreviousAlias = new string[] { "@open_door_1", "@open_door_2" },
                            Instructions = new []{ new IngameScript.Instruction {
                                BlockName = "Door Light",
                                ActionName = "Disable",
                                Arguments = new Dictionary<string, string> { },
                                IsValid = true,
                                Ignore = false
                            }},
                        } }
                    }.Values
                }}
            };

            commands = IngameScript.InstructionParser.Parse(CustomData.Resources.TestDependences);
            InstrucionAssert.AreEqual(expected, commands);
        }

        [TestMethod]
        public void TestDependences_Recursivity()
        {
            Assert.ThrowsException<IngameScript.StackOverflowException>(() =>
            {
                IngameScript.InstructionParser.Parse(CustomData.Resources.TestDependences_Recursivity);
            });
        }

        [TestMethod]
        public void TestArguments()
        {
            IDictionary<string, IngameScript.ICommandInstruction> commands;
            var expected = new Dictionary<string, IngameScript.ICommandInstruction>()
            {
                { "OPEN", new IngameScript.InstructionCommand {
                    CommandName = "OPEN",
                    Body = new Dictionary<string, IngameScript.InstructionBlock> {
                        { "@action", new IngameScript.InstructionBlock {
                            Alias = "@action",
                            PreviousAlias = new string[] { },
                            Instructions = new []{ new IngameScript.Instruction {
                                BlockName = "Block",
                                ActionName = "Some Action",
                                Arguments = new Dictionary<string, string> {
                                    { "NoCheck", "" },
                                    { "Key1", "Value" },
                                    { "Key2", "Value with spaces" },
                                    { "Key3", "Value with special characters :/" }
                                },
                                IsValid = true,
                                Ignore = true
                            }},
                        } }
                    }.Values
                }}
            };

            commands = IngameScript.InstructionParser.Parse(CustomData.Resources.TestArguments);
            InstrucionAssert.AreEqual(expected, commands);
        }

        [TestMethod]
        public void TestSyntaxAliasInvalid()
        {

            try
            {
                IngameScript.InstructionParser.Parse(CustomData.Resources.TestSyntaxAliasInvalid);
                Assert.Fail($"Exception SyntaxException was expected.");
            }
            catch (SyntaxException ex) when (ex.Line == 10)
            {
                // Test sucessfull.
            }
        }

        [TestMethod]
        public void TestSyntaxAliasDuplicated()
        {

            try
            {
                IngameScript.InstructionParser.Parse(CustomData.Resources.TestSyntaxAliasDuplicated);
                Assert.Fail($"Exception SyntaxException was expected.");
            }
            catch (SyntaxException ex) when (ex.Line == 11)
            {
                // Test sucessfull.
            }
        }

        [TestMethod]
        public void TestHydrogen()
        {
            IDictionary<string, IngameScript.ICommandInstruction> commands;

            commands = IngameScript.InstructionParser.Parse(CustomData.Resources.TestHydrogen);
            commands.ToString();
        }
    }
}
