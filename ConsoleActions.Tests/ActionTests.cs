using ConsoleActions.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ConsoleActions.Tests
{
    [TestClass]
    public class ActionTests
    {
        [Action("Action_With_MultipleParameters")]
        [ActionParameter("file")]
        [ActionParameter("keepOpen", Type = typeof(bool), DefaultValue = true)]
        [ActionParameter("recordsToRead", Type = typeof(int), DefaultValue = 1000)]
        [ActionParameter("maxMemory", Type = typeof(double), DefaultValue = 1.5)]
        public void Action_With_MultipleParameters(string parameter) { }

        [TestMethod]
        public void Test_Action_With_MultipleParameters()
        {
            var action = new ConsoleAction(typeof(ActionTests).GetMethod("Action_With_MultipleParameters"), this);
            Assert.IsNotNull(action);
        }


        [Action("Action_With_DuplicateParameter")]
        [ActionParameter("file")]
        [ActionParameter("file", Type = typeof(bool), DefaultValue = true)]
        [ActionParameter("recordsToRead", Type = typeof(int), DefaultValue = 1000)]
        [ActionParameter("maxMemory", Type = typeof(double), DefaultValue = 1.5)]
        public void Action_With_DuplicateParameter(string parameter) { }

        [TestMethod]
        public void Test_Action_With_DuplicateParameter()
        {
            Assert.ThrowsException<Exception>(() => new ConsoleAction(typeof(ActionTests).GetMethod("Action_With_DuplicateParameter"), this));
        }

        [Action("Action_With_DuplicateVariants")]
        [ActionParameter("recordsToRead")] // [ActionParameter("recordsToRead", "-r", "--read")]
        [ActionParameter("recordsToWrite")] // [ActionParameter("recordsToRead", "-w", "--write")]
        public void Action_With_DuplicateVariants(string parameter) { }

        [TestMethod]
        public void Test_Action_With_DuplicateVariants()
        {
            Assert.ThrowsException<Exception>(() => new ConsoleAction(typeof(ActionTests).GetMethod("Action_With_DuplicateVariants"), this));
        }

        [Action("Action_With_InvalidParameters")]
        [ActionParameter("1recordsToRead")]
        [ActionParameter("recordsToWrite")]
        public void Action_With_InvalidParameters(string parameter) { }

        [TestMethod]
        public void Test_Action_With_InvalidParameters()
        {
            Assert.ThrowsException<Exception>(() => new ConsoleAction(typeof(ActionTests).GetMethod("Action_With_InvalidParameters"), this));
        }
    }
}
