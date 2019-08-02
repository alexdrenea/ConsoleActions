using ConsoleActions.Attributes;
using ConsoleActions;
using System;
using System.Threading.Tasks;

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
        public async Task Method1(string parameter)
        {
            Console.WriteLine($"Method1 called with '{parameter}'");
        }

        [Action("n", "n1", Description = "Test method 2", DisplayOrder = 20, MeasureExexutionTime = false)]
        public async Task Method2(string parameter)
        {
            "Method2".WriteInLine(ConsoleColor.DarkCyan);
            Console.WriteLine($"called with '{parameter}'");
        }
    }
}
