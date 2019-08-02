using ConsoleActions.Attributes;
using System;
using System.Collections.Generic;
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
        public Func<string, Task> ActionFunc { get; set; }

        public MethodInfo Action { get; set; }
        public object ActionContext { get; set; }
        public int Order { get; set; }

        public bool MeasureExecutionTime { get; set; }

        public Task ExecuteAction(string parameter)
        {
            if (ActionFunc != null)
            {
                return ActionFunc(parameter);
            }
            if (Action != null && ActionContext != null)
            {
                var res = Action.Invoke(ActionContext, new[] { parameter });
                return (res is Task) ? (Task)res : Task.FromResult(res);
            }

            throw new InvalidOperationException("Either ActionFunc or Action must be defined");
        }
    }
}
