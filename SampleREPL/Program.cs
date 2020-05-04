using ConsoleActions.Attributes;
using ConsoleActions;
using System;
using System.Threading.Tasks;
using System.Dynamic;

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

        [Action("m", "m1", Description = "Test method", DisplayOrder = 100, MeasureExexutionTime = true)]
        public async Task Method1(dynamic parameter)
        {
            Console.WriteLine($"Method1 called with '{parameter}'");
        }

        [Action("n", "n1", Description = "Test method 2", DisplayOrder = 20, MeasureExexutionTime = false)]
        public async Task Method2(dynamic parameter)
        {
            "Method2".WriteInLine(ConsoleColor.DarkCyan);
            Console.WriteLine($"called with '{parameter}'");
        }

        [Action("nd1", Description = "Test method with params 2", DisplayOrder = 20, MeasureExexutionTime = false)]
        [ActionParameter("file", "-f", "--file")]
        [ActionParameter("write", "-w", "--write", Type = typeof(bool), DefaultValue = true)]
        public async Task MethodParams1(dynamic parameter)
        {
            Console.WriteLine($"called with file='{parameter.file}' and write='{parameter.write}'");
        }

        [Action("nd2", Description = "Test method with params 2", DisplayOrder = 20, MeasureExexutionTime = false)]
        [ActionParameter("file")]
        [ActionParameter("write", Type = typeof(bool))]
        public async Task MethodParams2(ExpandoObject parameter)
        {
            Console.WriteLine($"called with '{parameter}'");
        }

        [Action("nd3", Description = "Test method with params 2", DisplayOrder = 20, MeasureExexutionTime = false)]
        public async Task MethodParams3(dynamic parameter)
        {
            Console.WriteLine($"called with '{parameter}'");
        }
    }
}
