using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequentialScript.Test
{
    sealed class AssertInstrucion
    {


        public static void AreEqual(IDictionary<string, IngameScript.ICommandInstruction> expected, IDictionary<string, IngameScript.ICommandInstruction> actual)
        {
            var expectedJson = JsonConvert.SerializeObject(expected);
            var actualJson = JsonConvert.SerializeObject(actual);

            Assert.AreEqual(expectedJson, actualJson);
        }

    }
}
