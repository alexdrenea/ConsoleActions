using ConsoleActions.Attributes;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace ConsoleActions.Tests
{
    [TestClass]
    public class ActionParameterTests
    {


        #region Multi Parameters tests

        [Action("Action_With_MultipleParameters")]
        [ActionParameter("file")]
        [ActionParameter("keepOpen", Type = typeof(bool), DefaultValue = true)]
        [ActionParameter("recordsToRead", Type = typeof(int), DefaultValue = 1000)]
        [ActionParameter("maxMemory", Type = typeof(double), DefaultValue = 1.5)]
        public void Action_With_MultipleParameters(string parameter) { }

        [DataTestMethod]
        [DataRow("-f", "file1.txt", "-k", "false", "-r", "2000", "-m", "2.4")]
        public void Test_Action_With_MultipleParameters(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters);
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_MultipleParameters"), this);
            var res = action.ParseParameters(inputString);

            Assert.AreEqual(parameters[1].Trim('"'), res.file);
            Assert.AreEqual(typeof(string), res.file.GetType());
            Assert.AreEqual(bool.Parse(parameters[3].Trim('"')), res.keepOpen);
            Assert.AreEqual(typeof(bool), res.keepOpen.GetType());
            Assert.AreEqual(int.Parse(parameters[5].Trim('"')), res.recordsToRead);
            Assert.AreEqual(typeof(int), res.recordsToRead.GetType());
            Assert.AreEqual(double.Parse(parameters[7].Trim('"')), res.maxMemory);
            Assert.AreEqual(typeof(double), res.maxMemory.GetType());
        }

        [TestMethod]
        public void Test_Action_With_MultipleParameters_MoreCases()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_MultipleParameters"), this);
            dynamic res = null;
            var inputString = "";

            inputString = "-f file1.txt -k false -r 2000 -m 2.4";
            res = action.ParseParameters(inputString);
            Assert.AreEqual(typeof(string), res.file.GetType());
            Assert.AreEqual(typeof(bool), res.keepOpen.GetType());
            Assert.AreEqual(typeof(int), res.recordsToRead.GetType());
            Assert.AreEqual(typeof(double), res.maxMemory.GetType());
            Assert.AreEqual("file1.txt", res.file);
            Assert.AreEqual(false, res.keepOpen);
            Assert.AreEqual(2000, res.recordsToRead);
            Assert.AreEqual(2.4, res.maxMemory);

            inputString = "--file file1.txt";
            res = action.ParseParameters(inputString);
            Assert.AreEqual(typeof(string), res.file.GetType());
            Assert.AreEqual(typeof(bool), res.keepOpen.GetType());
            Assert.AreEqual(typeof(int), res.recordsToRead.GetType());
            Assert.AreEqual(typeof(double), res.maxMemory.GetType());
            Assert.AreEqual("file1.txt", res.file);
            Assert.AreEqual(true, res.keepOpen);
            Assert.AreEqual(1000, res.recordsToRead);
            Assert.AreEqual(1.5, res.maxMemory);

            inputString = "-f \"file1 with space.txt\" -k false -r 2000 -m 2.4";
            res = action.ParseParameters(inputString);
            Assert.AreEqual(typeof(string), res.file.GetType());
            Assert.AreEqual(typeof(bool), res.keepOpen.GetType());
            Assert.AreEqual(typeof(int), res.recordsToRead.GetType());
            Assert.AreEqual(typeof(double), res.maxMemory.GetType());
            Assert.AreEqual("file1 with space.txt", res.file);
            Assert.AreEqual(false, res.keepOpen);
            Assert.AreEqual(2000, res.recordsToRead);
            Assert.AreEqual(2.4, res.maxMemory);
        }

        [TestMethod]
        public void Test_Action_With_MultipleParameters_EdgeCase_FAIL()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_MultipleParameters"), this);
            dynamic res = null;
            var inputString = "";

            inputString = "-f \"file1-k test.txt\" -k false -r 2000 -m 2.4";
            res = action.ParseParameters(inputString);
            Assert.AreEqual(typeof(string), res.file.GetType());
            Assert.AreEqual(typeof(bool), res.keepOpen.GetType());
            Assert.AreEqual(typeof(int), res.recordsToRead.GetType());
            Assert.AreEqual(typeof(double), res.maxMemory.GetType());
            Assert.AreEqual("file1.txt", res.file);
            Assert.AreEqual(false, res.keepOpen);
            Assert.AreEqual(2000, res.recordsToRead);
            Assert.AreEqual(2.4, res.maxMemory);
        }

        #endregion

        #region Negative Tests

        [Action("Action_For_NegativeTests")]
        [ActionParameter("file")]
        [ActionParameter("keepOpen", Type = typeof(bool), DefaultValue = true)]
        [ActionParameter("recordsToRead", Type = typeof(int), DefaultValue = 1000)]
        [ActionParameter("maxMemory", Type = typeof(double), DefaultValue = 1.5)]
        public void Action_For_NegativeTests(string parameter) { }


        [TestMethod]
        public void Test_Action_For_NegativeTests_MultipleSameParameter_ShouldFail()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_MultipleParameters"), this);
            dynamic res = null;
            var inputString = "";

            inputString = "-f file1.txt --file anotherfile.txt";
            Assert.ThrowsException<ArgumentException>(() => action.ParseParameters(inputString));


            //TODO - shouldn't be able to send -f twice. Current code picks up first one. Should be a bug.... but is it????
            //inputString = "-f file1.txt -f anotherfile.txt";
            //Assert.ThrowsException<ArgumentException>(() => action.ParseParameters(inputString));
        }

        [TestMethod]
        public void Test_Action_For_NegativeTests_NotEndingQuotes_ShouldFail()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_MultipleParameters"), this);
            dynamic res = null;
            var inputString = "";

            inputString = "-f \"file1.txt --k false";
            Assert.ThrowsException<ArgumentException>(()=> action.ParseParameters(inputString));
        }

        [TestMethod]
        public void Test_Action_For_NegativeTests_ParameterConvertError_ShouldFail()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_MultipleParameters"), this);
            dynamic res = null;
            var inputString = "";

            inputString = "-f \"file1.txt --k notbool";
            Assert.ThrowsException<ArgumentException>(() => action.ParseParameters(inputString));

            inputString = "-f \"file1.txt --r notint";
            Assert.ThrowsException<ArgumentException>(() => action.ParseParameters(inputString));

            inputString = "-f \"file1.txt --m nodouble";
            Assert.ThrowsException<ArgumentException>(() => action.ParseParameters(inputString));
        }

        #endregion

        #region String tests

        [Action("Action_With_StringParameter")]
        [ActionParameter("file")]
        public void Action_With_StringParameter(string parameter) { }

        [DataTestMethod]
        [DataRow("-f","file1.txt")]
        [DataRow("-f","\"file1 with space.txt\"")]
        [DataRow("--file", "file1.txt")]
        [DataRow("--file", "\"file1 with space.txt\"")]
        public void Test_Action_With_StringParameter(params string[] parameters)
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_StringParameter"), this);
            var inputString = string.Join(" ", parameters);
            var res = action.ParseParameters(inputString);
            
            Assert.AreEqual(parameters[1].Trim('"'), res.file);
            Assert.AreEqual(typeof(string), res.file.GetType());
        }
     
        [TestMethod]
        public void Test_Action_With_StringParameter_Default()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_StringParameter"), this);
            
            var res1 = action.ParseParameters("");
            Assert.AreEqual(null, res1.file);

            var res2 = action.ParseParameters("-x something");
            Assert.AreEqual(null, res2.file);
        }


        [Action("Action_With_StringParameter_ExplicitDefaultValue")]
        [ActionParameter("file", DefaultValue = "test")]
        public void Action_With_StringParameter_ExplicitDefaultValue(string parameter) { }

        [TestMethod]
        public void Test_Action_With_StringParameter_ExplicitDefaultValue_Default()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_StringParameter_ExplicitDefaultValue"), this);

            var res1 = action.ParseParameters("");
            Assert.AreEqual("test", res1.file);
            var res2 = action.ParseParameters("-x something");
            Assert.AreEqual("test", res2.file);
        }


        [Action("Action_With_StringParameter_ExplicitType")]
        [ActionParameter("file", Type = typeof(string))]
        public void Action_With_StringParameter_ExplicitType(string parameter) { }
       
        [DataTestMethod]
        [DataRow("-f file1.txt")]
        public void Test_Action_With_StringParameter_ExplicitType_ExplicitDefaultValue(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters);
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_StringParameter_ExplicitType"), this);

            var res = action.ParseParameters(inputString);
            Assert.AreEqual(typeof(string), res.file.GetType());
        }


        [Action("Action_With_StringParameter_ExplicitVariants")]
        [ActionParameter("file", "-x", "--xxx")]
        public void Action_With_StringParameter_ExplicitVariants(string parameter) { }
        
        [DataTestMethod]
        [DataRow("-x", "file1.txt")]
        [DataRow("-x", "\"file1 with space.txt\"")]
        [DataRow("--xxx", "file1.txt")]
        [DataRow("--xxx", "\"file1 with space.txt\"")]
        public void Test_Action_With_StringParameter_ExplicitVariants(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters); 
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_StringParameter_ExplicitVariants"), this);
            var res = action.ParseParameters(inputString);

            Assert.ThrowsException<RuntimeBinderException>(() => res.x);
            Assert.AreEqual(parameters[1].Trim('"'), res.file);
            Assert.AreEqual(typeof(string), res.file.GetType());
        }

        [DataTestMethod]
        [DataRow("-f", "file1.txt")]
        [DataRow("-f", "\"file1 with space.txt\"")]
        [DataRow("--file", "file1.txt")]
        [DataRow("--file", "\"file1 with space.txt\"")]
        public void Test_Action_With_StringParameter_ExplicitVariants_ShouldFail_With_DefaultVariant(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters);
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_StringParameter_ExplicitVariants"), this);
            var res = action.ParseParameters(inputString);

            Assert.AreEqual(null, res.file);
        }

        #endregion

        #region Bool tests

        [Action("Action_With_BoolParameter")]
        [ActionParameter("value", Type=typeof(bool))]
        public void Action_With_BoolParameter(string parameter) { }

        [DataTestMethod]
        [DataRow("-v", "true")]
        [DataRow("-v", "false")]
        [DataRow("--value", "true")]
        [DataRow("--value", "false")]
        public void Test_Action_With_BoolParameter(params string[] parameters)
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_BoolParameter"), this);
            var inputString = string.Join(" ", parameters);
            var res = action.ParseParameters(inputString);

            Assert.AreEqual(bool.Parse(parameters[1].Trim('"')), res.value);
            Assert.AreEqual(typeof(bool), res.value.GetType());
        }

        [TestMethod]
        public void Test_Action_With_BoolParameter_Default()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_BoolParameter"), this);

            var res1 = action.ParseParameters("");
            Assert.AreEqual(default(bool), res1.value);

            var res2 = action.ParseParameters("-x something");
            Assert.AreEqual(default(bool), res2.value);
        }


        [Action("Action_With_BoolParameter_ExplicitDefaultValue")]
        [ActionParameter("value", Type = typeof(bool), DefaultValue = "true")]
        public void Action_With_BoolParameter_ExplicitDefaultValue(string parameter) { }

        [TestMethod]
        public void Test_Action_With_BoolParameter_ExplicitDefaultValue_Default()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_BoolParameter_ExplicitDefaultValue"), this);

            var res1 = action.ParseParameters("");
            Assert.AreEqual(true, res1.value);
            var res2 = action.ParseParameters("-x something");
            Assert.AreEqual(true, res2.value);
        }

        [Action("Action_With_BoolParameter_ExplicitVariants")]
        [ActionParameter("value", "-x", "--xxx", Type = typeof(bool))]
        public void Action_With_BoolParameter_ExplicitVariants(string parameter) { }

        [DataTestMethod]
        [DataRow("-x", "true")]
        [DataRow("-x", "\"true\"")]
        [DataRow("-x", "false")]
        [DataRow("--xxx", "true")]
        [DataRow("--xxx", "\"true\"")]
        [DataRow("--xxx", "false")]
        public void Test_Action_With_BoolParameter_ExplicitVariants(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters);
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_BoolParameter_ExplicitVariants"), this);
            var res = action.ParseParameters(inputString);

            Assert.ThrowsException<RuntimeBinderException>(() => res.x);
            Assert.AreEqual(bool.Parse(parameters[1].Trim('"')), res.value);
            Assert.AreEqual(typeof(bool), res.value.GetType());
        }

        [DataTestMethod]
        [DataRow("-v", "true")]
        [DataRow("-v", "false")]
        [DataRow("--value", "true")]
        [DataRow("--value", "false")]
        public void Test_Action_With_BoolParameter_ExplicitVariants_ShouldFail_With_DefaultVariant(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters);
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_BoolParameter_ExplicitVariants"), this);
            var res = action.ParseParameters(inputString);

            Assert.AreEqual(default(bool), res.value);
        }

        #endregion
       
        #region Integer tests

        [Action("Action_With_IntParameter")]
        [ActionParameter("value", Type = typeof(int))]
        public void Action_With_IntParameter(string parameter) { }

        [DataTestMethod]
        [DataRow("-v", "1")]
        [DataRow("-v", "0")]
        [DataRow("-v", "-1")]
        [DataRow("-v", "23434112")]
        [DataRow("--value", "1")]
        [DataRow("--value", "0")]
        [DataRow("--value", "-1")]
        [DataRow("--value", "2323232")]
        public void Test_Action_With_IntParameter(params string[] parameters)
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_IntParameter"), this);
            var inputString = string.Join(" ", parameters);
            var res = action.ParseParameters(inputString);

            Assert.AreEqual(int.Parse(parameters[1].Trim('"')), res.value);
            Assert.AreEqual(typeof(int), res.value.GetType());
        }

        [TestMethod]
        public void Test_Action_With_IntParameter_Default()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_IntParameter"), this);

            var res1 = action.ParseParameters("");
            Assert.AreEqual(default(int), res1.value);

            var res2 = action.ParseParameters("-x something");
            Assert.AreEqual(default(int), res2.value);
        }


        [Action("Action_With_IntParameter_ExplicitDefaultValue")]
        [ActionParameter("value", Type = typeof(int), DefaultValue = 100)]
        public void Action_With_IntParameter_ExplicitDefaultValue(string parameter) { }

        [TestMethod]
        public void Test_Action_With_IntParameter_ExplicitDefaultValue_Default()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_IntParameter_ExplicitDefaultValue"), this);

            var res1 = action.ParseParameters("");
            Assert.AreEqual(100, res1.value);
            var res2 = action.ParseParameters("-x something");
            Assert.AreEqual(100, res2.value);
        }

        [Action("Action_With_IntParameter_ExplicitVariants")]
        [ActionParameter("value", "-x", "--xxx", Type = typeof(int))]
        public void Action_With_IntParameter_ExplicitVariants(string parameter) { }

        [DataTestMethod]
        [DataRow("-x", "1")]
        [DataRow("-x", "\"00\"")]
        [DataRow("-x", "111")]
        [DataRow("--xxx", "1")]
        [DataRow("--xxx", "\"111\"")]
        [DataRow("--xxx", "-222")]
        public void Test_Action_With_IntParameter_ExplicitVariants(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters);
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_IntParameter_ExplicitVariants"), this);
            var res = action.ParseParameters(inputString);

            Assert.ThrowsException<RuntimeBinderException>(() => res.x);
            Assert.AreEqual(int.Parse(parameters[1].Trim('"')), res.value);
            Assert.AreEqual(typeof(int), res.value.GetType());
        }

        [DataTestMethod]
        [DataRow("-v", "1")]
        [DataRow("-v", "1")]
        [DataRow("--value", "22")]
        [DataRow("--value", "22")]
        public void Test_Action_With_IntParameter_ExplicitVariants_ShouldFail_With_DefaultVariant(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters);
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_IntParameter_ExplicitVariants"), this);
            var res = action.ParseParameters(inputString);

            Assert.AreEqual(default(int), res.value);
        }

        #endregion

        #region Integer tests

        [Action("Action_With_DecimalParameter")]
        [ActionParameter("value", Type = typeof(decimal))]
        public void Action_With_DecimalParameter(string parameter) { }

        [DataTestMethod]
        [DataRow("-v", "1")]
        [DataRow("-v", "0")]
        [DataRow("-v", "-1.6")]
        [DataRow("-v", "\"23434.112\"")]
        [DataRow("--value", "1.6")]
        [DataRow("--value", "0")]
        [DataRow("--value", "-1")]
        [DataRow("--value", "232.3232")]
        public void Test_Action_With_DecimalParameter(params string[] parameters)
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_DecimalParameter"), this);
            var inputString = string.Join(" ", parameters);
            var res = action.ParseParameters(inputString);

            Assert.AreEqual(decimal.Parse(parameters[1].Trim('"')), res.value);
            Assert.AreEqual(typeof(decimal), res.value.GetType());
        }

        [TestMethod]
        public void Test_Action_With_DecimalParameter_Default()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_DecimalParameter"), this);

            var res1 = action.ParseParameters("");
            Assert.AreEqual(default(decimal), res1.value);

            var res2 = action.ParseParameters("-x something");
            Assert.AreEqual(default(decimal), res2.value);
        }


        [Action("Action_With_DecimalParameter_ExplicitDefaultValue")]
        [ActionParameter("value", Type = typeof(double), DefaultValue = 10.0)]
        public void Action_With_DecimalParameter_ExplicitDefaultValue(string parameter) { }

        [TestMethod]
        public void Test_Action_With_DecimalParameter_ExplicitDefaultValue_Default()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_DecimalParameter_ExplicitDefaultValue"), this);

            var res1 = action.ParseParameters("");
            Assert.AreEqual(10.0, res1.value);
            var res2 = action.ParseParameters("-x something");
            Assert.AreEqual(10.0, res2.value);
        }

        [Action("Action_With_DecimalParameter_ExplicitVariants")]
        [ActionParameter("value", "-x", "--xxx", Type = typeof(decimal))]
        public void Action_With_DecimalParameter_ExplicitVariants(string parameter) { }

        [DataTestMethod]
        [DataRow("-x", "1.1")]
        [DataRow("-x", "\"1.22\"")]
        [DataRow("-x", "11.1")]
        [DataRow("--xxx", "1")]
        [DataRow("--xxx", "\"11.21\"")]
        [DataRow("--xxx", "-22.2")]
        public void Test_Action_With_DecimalParameter_ExplicitVariants(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters);
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_DecimalParameter_ExplicitVariants"), this);
            var res = action.ParseParameters(inputString);

            Assert.ThrowsException<RuntimeBinderException>(() => res.x);
            Assert.AreEqual(decimal.Parse(parameters[1].Trim('"')), res.value);
            Assert.AreEqual(typeof(decimal), res.value.GetType());
        }

        [DataTestMethod]
        [DataRow("-v", "1.3")]
        [DataRow("-v", "0.2")]
        [DataRow("--value", "2.2")]
        [DataRow("--value", "2.2")]
        public void Test_Action_With_DecimalParameter_ExplicitVariants_ShouldFail_With_DefaultVariant(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters);
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_DecimalParameter_ExplicitVariants"), this);
            var res = action.ParseParameters(inputString);

            Assert.AreEqual(default(decimal), res.value);
        }

        #endregion

        #region DateTime tests

        [Action("Action_With_DateTimeParameter")]
        [ActionParameter("value", Type = typeof(DateTime))]
        public void Action_With_DateTimeParameter(string parameter) { }

        [DataTestMethod]
        [DataRow("-v", "2020-01-31")]
        [DataRow("-v", "\"2020-1-31 13:00:12 \"")]
        [DataRow("-v", "\"2020-2-22 1:00:00 pm\"")]
        [DataRow("-v", "2020/12/30")]
        [DataRow("-v", "12/30/2020")]
        [DataRow("--value", "2020-01-31")]
        [DataRow("--value", "\"2020-1-31 13:00:12 \"")]
        [DataRow("--value", "\"2020-2-22 1:00:00 pm\"")]
        [DataRow("--value", "2020/12/30")]
        [DataRow("--value", "12/30/2020")]
        public void Test_Action_With_DateTimeParameter(params string[] parameters)
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_DateTimeParameter"), this);
            var inputString = string.Join(" ", parameters);
            var res = action.ParseParameters(inputString);

            Assert.AreEqual(DateTime.Parse(parameters[1].Trim('"')), res.value);
            Assert.AreEqual(typeof(DateTime), res.value.GetType());
        }

        [TestMethod]
        public void Test_Action_With_DateTimeParameter_Default()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_DateTimeParameter"), this);

            var res1 = action.ParseParameters("");
            Assert.AreEqual(default(DateTime), res1.value);

            var res2 = action.ParseParameters("-x something");
            Assert.AreEqual(default(DateTime), res2.value);
        }


        [Action("Action_With_DateTimeParameter_ExplicitDefaultValue")]
        [ActionParameter("value", Type = typeof(DateTime), DefaultValue = "2020-01-01")]
        public void Action_With_DateTimeParameter_ExplicitDefaultValue(string parameter) { }

        [TestMethod]
        public void Test_Action_With_DateTimeParameter_ExplicitDefaultValue_Default()
        {
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_DateTimeParameter_ExplicitDefaultValue"), this);

            var res1 = action.ParseParameters("");
            Assert.AreEqual(new DateTime(2020,01,01), res1.value);
            var res2 = action.ParseParameters("-x something");
            Assert.AreEqual(new DateTime(2020, 01, 01), res2.value);
        }

        [Action("Action_With_DateTimeParameter_ExplicitVariants")]
        [ActionParameter("value", "-x", "--xxx", Type = typeof(DateTime))]
        public void Action_With_DateTimeParameter_ExplicitVariants(string parameter) { }

        [DataTestMethod]
        [DataRow("-x", "2020/12/30")]
        [DataRow("-x", "12/30/2020")]
        [DataRow("--xxx", "2020-01-31")]
        [DataRow("--xxx", "\"2020-1-31 13:00:12 \"")]
        public void Test_Action_With_DateTimeParameter_ExplicitVariants(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters);
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_DateTimeParameter_ExplicitVariants"), this);
            var res = action.ParseParameters(inputString);

            Assert.ThrowsException<RuntimeBinderException>(() => res.x);
            Assert.AreEqual(DateTime.Parse(parameters[1].Trim('"')), res.value);
            Assert.AreEqual(typeof(DateTime), res.value.GetType());
        }

        [DataTestMethod]
        [DataRow("-v", "2020/12/30")]
        [DataRow("-v", "12/30/2020")]
        [DataRow("--value", "2020-01-31")]
        [DataRow("--value", "\"2020-1-31 13:00:12 \"")]
        public void Test_Action_With_DateTimeParameter_ExplicitVariants_ShouldFail_With_DefaultVariant(params string[] parameters)
        {
            var inputString = string.Join(" ", parameters);
            var action = new ConsoleAction(typeof(ActionParameterTests).GetMethod("Action_With_DateTimeParameter_ExplicitVariants"), this);
            var res = action.ParseParameters(inputString);

            Assert.AreEqual(default(DateTime), res.value);
        }

        #endregion
    }

}
