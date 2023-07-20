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
                                IsValid = true,
                                Ignore = false
                            }},
                        } }
                    }.Values
                }}
            };

            commands = IngameScript.InstructionParser.Parse(CustomData.Resources.TestSimple);
            AssertInstrucion.AreEqual(expected, commands);
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
                                IsValid = true,
                                Ignore = false
                            }},
                        } }
                    }.Values
                }}
            };

            commands = IngameScript.InstructionParser.Parse(CustomData.Resources.TestComments);
            AssertInstrucion.AreEqual(expected, commands);
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
                                IsValid = true,
                                Ignore = false
                            }},
                        } }
                    }.Values
                }}
            };

            commands = IngameScript.InstructionParser.Parse(CustomData.Resources.TestDependences);
            AssertInstrucion.AreEqual(expected, commands);
        }

        [TestMethod]
        public void TestDependences_Recursivity()
        {
            Assert.ThrowsException<IngameScript.StackOverflowException>(() =>
            {
                IngameScript.InstructionParser.Parse(CustomData.Resources.TestDependences_Recursivity);
            });
        }

    }
}
