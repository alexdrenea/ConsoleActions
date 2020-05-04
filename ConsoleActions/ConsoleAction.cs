using ConsoleActions.Attributes;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleActions
{
    public class ConsoleAction
    {
        /// <summary>
        /// Creates a new ConsoleAction based on a Func reference
        /// </summary>
        /// <param name="action">Func to execute when the action is called</param>
        /// <param name="description">Description of command to be displayed when the help command is invoked</param>
        /// <param name="commands">List of keywords/commands that trigger the function (e.g. help, ?)</param>
        public ConsoleAction(Func<string, Task> action, string description, params string[] commands)
            : this(action, description, int.MaxValue, commands)
        {
        }

        /// <summary>
        /// Creates a new ConsoleAction based on a Func reference
        /// </summary>
        /// <param name="action">Func to execute when the action is called</param>
        /// <param name="description">Description of command to be displayed when the help command is invoked</param>
        /// <param name="order">Order in the help menu</param>
        /// <param name="commands">List of keywords/commands that trigger the function (e.g. help, ?)</param>
        public ConsoleAction(Func<string, Task> action, string description, int order, params string[] commands)
        {
            ActionFunc = action;
            Action = null;
            ActionContext = null;
            Commands = commands;
            Description = description;
            Order = order;
        }

        /// <summary>
        /// Creates a new ConsoleAction based on a MethodInfo and the instance of the object the method is part of.
        /// The given action, must be a public, async Function that takes a string parameter ( public async void MyMethod(string param) )
        /// The given action, must be decorated with the <see cref="ActionAttribute"/> so that metadata can be passed accordingly
        /// </summary>
        /// <param name="action">Reflection Mehtod info of the method that needs to be executed when the action is called</param>
        /// <param name="actionContext">Instance of the object where the method is executed on.</param>
        public ConsoleAction(MethodInfo action, object actionContext)
        {
            ActionFunc = null;
            Action = action;
            ActionContext = actionContext;
            var configuration = action.GetCustomAttribute<ActionAttribute>();
            try
            {
                var parameters = action.GetCustomAttributes<ActionParameterAttribute>();
                Parameters = parameters.Select(_ => new ConsoleActionParameter
                {
                    ParameterName = _.PropertyName,
                    Variants = _.Variants.ToArray(),
                    Type = _.Type,
                    DefaultValue = _.DefaultValue
                });
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new Exception($"Error processing '{action.Name}' Action. {e.ParamName}", e);
            }
            if (Parameters.GroupBy(k => k.ParameterName).Any(g => g.Count() > 1))
                throw new Exception($"Error processing '{action.Name}' Action. Found duplicate parameter name.");
            if (Parameters.SelectMany(p => p.Variants).GroupBy(_ => _).Any(g => g.Count() > 1))
                throw new Exception($"Error processing '{action.Name}' Action. Found multiple parametes that share the same variant. If a variant is not explicitly specified, the first letter of the parameter will be used. Check if you have multiple parametes that start with the same letter.");

            Commands = configuration.Triggers;
            Description = configuration.Description ?? "";
            Order = configuration.DisplayOrder == 0 ? int.MaxValue : configuration.DisplayOrder;
            MeasureExecutionTime = configuration.MeasureExexutionTime;
        }

        /// <summary>
        /// Gets or sets the list of commands that will trigger the action
        /// </summary>
        public IEnumerable<string> Commands { get; set; }


        /// <summary>
        /// Shows up as the description of the command when run help
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value used to sort the commands when displayed in the help system.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Action should print the total execution time after the action is complete.
        /// </summary>
        public bool MeasureExecutionTime { get; set; }


        private IEnumerable<ConsoleActionParameter> Parameters { get; set; }

        /// <summary>
        /// Action to be executed when a the command is called.
        /// The function gets a string that represents whatever the user entered after the command
        /// It is the method's responsability to parse that input into whatever it needs.
        /// </summary>
        private Func<string, Task> ActionFunc { get; set; }

        private MethodInfo Action { get; set; }
        private object ActionContext { get; set; }


        internal Task ExecuteAction(object parameter)
        {
            if (ActionFunc != null)
            {
                return ActionFunc(parameter.ToString());
            }
            if (Action != null && ActionContext != null)
            {
                var parameters = new object[Action.GetParameters().Count()];
                if (parameters.Length > 0)
                    parameters[0] = parameter.ToString();
                if (parameters.Length > 1)
                    parameters[1] = ParseParameters(parameter.ToString());

                var res = Action.Invoke(ActionContext, parameters);
                return (res is Task) ? (Task)res : Task.FromResult(res);
            }

            throw new InvalidOperationException("Either ActionFunc or Action must be defined");
        }

        /// <summary>
        /// Given an input string, will parse it according to the Parameters defined in this Console Action
        /// </summary>
        /// <param name="input">input string to parse</param>
        /// <returns>dynamic object with parsed results</returns>
        /// <exception cref="ArgumentException">The method will throw an ArgumentException if a parameter is defined multiple times in the input string, if the input string is malformed or if a paramter's value cannot be parsed correctly into the desired type.</exception>
        public dynamic ParseParameters(string input)
        {
            input += " "; //this is to ensure we find the end index for the last parameter (i.e. "-f file" -> "-f file " so that " " is found at the end)

            dynamic res = new ExpandoObject();
            var resDic = res as IDictionary<string, object>;

            foreach (var p in Parameters)
            {
                resDic.Add(p.ParameterName, p.DefaultValue != null ? Convert.ChangeType(p.DefaultValue, p.Type) : (p.Type.IsValueType ? Activator.CreateInstance(p.Type) : null));

                //Find a match for the either of the variants of this parameter. Ensure to add a space after to get a better match.
                var matchedArgs = p.Variants.Where(_ => input.IndexOf($"{_} ") > -1).ToArray();

                //Ensure that we have exactly one variant match (i.e. can't specify both -f and --file in a command
                if (matchedArgs.Length > 1) throw new ArgumentException($"cannot parse parameter {p.ParameterName} from input string {input}. Found multiple instances");
                if (matchedArgs.Length == 0) continue; //parameter not referenced - set it to default 

                var startIndex = input.IndexOf($"{matchedArgs[0]} ") + matchedArgs[0].Length + 1;
                //handle the case where the argument value contains spaces and is wrapped in " "
                var endIndexChar = input[startIndex] == '\"' ? '\"' : ' ';
                var indexOffset = input[startIndex] == '\"' ? 1 : 0;
                var endIndex = input.IndexOf(endIndexChar, startIndex + indexOffset);
                if (endIndex == -1)
                    throw new ArgumentException($"cannot parse parameter {p.ParameterName} from input string {input}. No ending found");

                var argValue = input.Substring(startIndex + indexOffset, endIndex - startIndex - indexOffset);
                try
                {
                    resDic[p.ParameterName] = Convert.ChangeType(argValue, p.Type);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"cannot parse parameter {p.ParameterName} from input string {input}. Format exception - {ex.Message}", ex);
                }
            }

            return res;
        }
    }
}
