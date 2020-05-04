using ConsoleActions.Attributes;
using ConsoleActions;
using System;
using System.Threading.Tasks;
using System.Dynamic;
using System.Collections.Generic;

namespace SampleREPL
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new ConsoleActions.ConsoleREPL(new Program()).RunLoop().Wait();
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        [Action("a", "methodA", Description = "Test method that does not need an input", DisplayOrder = 1, MeasureExexutionTime = true)]
        public void MethodA()
        {
            "MethodA".WriteInLine(ConsoleColor.DarkCyan);
            Console.WriteLine($"called. No inputs tracked.");
        }

        [Action("b", "methodB", Description = "Test method that gets just the raw input from user", DisplayOrder = 2, MeasureExexutionTime = true)]
        public async Task MethodB(string input)
        {
            "Method2".WriteInLine(ConsoleColor.DarkCyan);
            Console.WriteLine($"methods can also be async.");
            Console.WriteLine($"called with '{input}'");
        }

        [Action("c", "methodC", Description = "Test method that gets the raw input from user and the parsed input according to the declared parameters.", DisplayOrder = 3, MeasureExexutionTime = false)]
        public void MethodC(string input, dynamic parsedInput)
        {
            "MethodC".WriteInLine(ConsoleColor.DarkCyan);
            Console.WriteLine($"called with '{input}'. No parsed inputs since no inputs defined in the action parameter attributes");
        }

        [Action("d", "methodD", Description = "Test method that declares parameters to be parsed. Parsed parameters come in as a dynamic type", DisplayOrder = 4, MeasureExexutionTime = true)]
        [ActionParameter("fileName", "-f", "--file")]
        [ActionParameter("keepOpen", Type = typeof(bool), DefaultValue = true)]
        [ActionParameter("recordsToRead", Type = typeof(int), DefaultValue = 1000)]
        public void MethodD(string input, dynamic parsed)
        {
            "MethodD".WriteInLine(ConsoleColor.DarkCyan);
            Console.WriteLine($"called with input='{input}'. Parsed into: file='{parsed.fileName}' and keepOpen={parsed.keepOpen} and recordsToRead={parsed.recordsToRead}");
        }

        [Action("e", "methodE", Description = "Test method that declares parameters to be parsed.  Parsed parameters come in as a Dictionary of values", DisplayOrder = 5, MeasureExexutionTime = true)]
        [ActionParameter("fileName", "-f", "--file")]
        [ActionParameter("keepOpen", Type = typeof(bool), DefaultValue = true)]
        [ActionParameter("recordsToRead", Type = typeof(int), DefaultValue = 1000)]
        public void MethodE(string input, IDictionary<string,object> parsed)
        {
            "MethodE".WriteInLine(ConsoleColor.DarkCyan);
            Console.WriteLine($"called with input='{input}'. Parsed into: file='{parsed["fileName"]}' and keepOpen={parsed["keepOpen"]} and recordsToRead={parsed["recordsToRead"]}");
        }


        [Action("f", "methodF", Description = "Test method fails to index because it has duplicate paramter names", DisplayOrder = 6, MeasureExexutionTime = true)]
        [ActionParameter("fileName")]
        [ActionParameter("fileName")]
        public void MethodF(string input)
        {
        }

        [Action("g", "methodG", Description = "Test method fails to index because it has duplicate parameter variants", DisplayOrder = 7, MeasureExexutionTime = true)]
        [ActionParameter("recordsToRead")] // [ActionParameter("recordsToRead", "-r", "--read")]
        [ActionParameter("recordsToWrite")] // [ActionParameter("recordsToRead", "-w", "--write")]
        public void MethodG(string input)
        {
        }


        [Action("h", "methodH", Description = "Test method fails to index because it has invalid parameter name", DisplayOrder = 8, MeasureExexutionTime = true)]
        [ActionParameter("1recordsToRead")]
        public void MethodH(string input)
        {
        }

    }
}
