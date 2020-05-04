using ConsoleActions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleActions
{
    public class ConsoleREPL
    {
        private object _executionContext;
        private List<ConsoleAction> _actionsList;
        private Dictionary<string, ConsoleAction> _actions;

        public ConsoleREPL(object executionContext)
        {
            _executionContext = executionContext;
        }

        public string Prompt { get; set; } = ":>";

        public async Task RunLoop()
        {
            SetupActions();
            ConsoleHelpers.WriteLine("Type '?' or 'help' for additional commands...", ConsoleColor.DarkGray);

            while (true)
            {
                Console.WriteLine();
                Prompt.WriteInLine(ConsoleColor.DarkYellow);
                var queryString = Console.ReadLine();
                if (queryString == "q") break;

                var queryCommand = queryString.Split(' ').FirstOrDefault();
                var queryParam = queryString.Substring(queryCommand.Length, queryString.Length - queryCommand.Length).Trim();
                if (_actions.ContainsKey(queryCommand))
                {
                    try
                    {
                        var start = DateTime.Now;
                        await _actions[queryCommand].ExecuteAction(queryParam);
                        ConsoleHelpers.WriteLine("");
                        if (_actions[queryCommand].MeasureExecutionTime)
                        {
                            ConsoleHelpers.WriteLine($"Method executed in {DateTime.Now.Subtract(start).TotalSeconds.ToString(".##")} sec");
                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleHelpers.WriteInLine($"Error:", ConsoleColor.DarkRed);
                        e.Message.WriteLine();
                    }
                }
                else
                {
                    ConsoleHelpers.WriteLine($"{Prompt} Unrecognized command '{queryCommand}'. Use ? or help for a list of available commands", ConsoleColor.DarkRed);
                }
            }
        }

        public void SetupActions()
        {
            var allMethods = _executionContext.GetType().GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            var allActionsAttributes = allMethods.Where(m => m.GetCustomAttribute<ActionAttribute>() != null);
            _actionsList = new List<ConsoleAction>();
            foreach (var actionAttribute in allActionsAttributes)
            {
                try
                {
                    _actionsList.Add(new ConsoleAction(actionAttribute, _executionContext));
                }
                catch(Exception e)
                {
                    $"WARNING: {e.Message}".WriteLine(ConsoleColor.DarkYellow);
                }
            }
            _actionsList.Insert(0, new ConsoleAction(Help, "Displays this message", 0, "h", "help", "?"));
            
            _actions = _actionsList.SelectMany(ca => ca.Commands.Select(c => new KeyValuePair<string, ConsoleAction>(c, ca)))
                               .ToDictionary(k => k.Key, v => v.Value);

        }

        private async Task Help(string input)
        {
            var actions = _actionsList.OrderBy(o => o.Order).ToDictionary(k => string.Join(", ", k.Commands), v => v.Description);
            var maxCommand = actions.Max(a => a.Key.Length);

            Console.WriteLine();
            Console.WriteLine($"Available commands:");
            foreach (var a in actions)
                Console.WriteLine($"{a.Key.PadLeft(maxCommand)} : {a.Value}");
            Console.WriteLine($"{"q".PadLeft(maxCommand)} : Exit");
            Console.WriteLine();
        }
    }
}
