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
        public ConsoleAction(Func<object, Task> action, string description, params string[] commands)
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
        public ConsoleAction(Func<object, Task> action, string description, int order, params string[] commands)
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
            var parameters = action.GetCustomAttributes<ActionParameterAttribute>();

            Parameters = parameters.Select(_ => new ConsoleActionParameter
            {
                ParameterName = _.PropertyName,
                Variants = _.Variants.ToArray(),
                Type = _.Type,
                DefaultValue = _.DefaultValue
            });
            Parameters.ToDictionary(k => k.ParameterName); //this checks that we don't duplicate parameter names

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
        /// Action to be executed when a the command is called.
        /// The function gets a string that represents whatever the user entered after the command
        /// It is the method's responsability to parse that input into whatever it needs.
        /// </summary>
        public Func<object, Task> ActionFunc { get; set; }

        public IEnumerable<ConsoleActionParameter> Parameters { get; set; }
        public MethodInfo Action { get; set; }
        public object ActionContext { get; set; }
        public int Order { get; set; }

        public bool MeasureExecutionTime { get; set; }

        public Task ExecuteAction(object parameter)
        {
            if (ActionFunc != null)
            {
                return ActionFunc(parameter);
            }
            if (Action != null && ActionContext != null)
            {
                var res = Action.Invoke(ActionContext, new[] { ParseParameters(parameter.ToString()) });
                return (res is Task) ? (Task)res : Task.FromResult(res);
            }

            throw new InvalidOperationException("Either ActionFunc or Action must be defined");
        }


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


    public class ConsoleActionParameter
    {
        public string ParameterName { get; set; }
        public IEnumerable<string> Variants { get; set; }
        public Type Type { get; set; }
        public object DefaultValue { get; set; }

    }
}
