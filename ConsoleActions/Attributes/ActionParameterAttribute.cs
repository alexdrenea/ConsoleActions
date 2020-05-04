using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleActions.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ActionParameterAttribute : Attribute
    {
        public ActionParameterAttribute(string name, params string[] variants)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentOutOfRangeException("must provive parameter name");
            if (name.Any(_ => !char.IsLetterOrDigit(_))) throw new ArgumentOutOfRangeException("parameter name must only contain letters and digits");
            if (!char.IsLetter(name.Trim()[0])) throw new ArgumentOutOfRangeException("parameter name must start with letter");

            PropertyName = name;
            Variants = variants;
            if (!Variants.Any())
                Variants = new[] { $"-{PropertyName[0]}", $"--{PropertyName}" };

            if (Variants.Any(v => !v.StartsWith("-"))) throw new ArgumentOutOfRangeException("Parameter variant must start with -");
            //TODO - nicer condition
            if (Variants.Any(v => v.StartsWith("-") && v.Length > 1 && v[1] != '-' && v.Length != 2)) throw new ArgumentOutOfRangeException("If parameter starts with -, it must be a single letter");
            if (Variants.Any(v => v.StartsWith("--") && v.Length <= 3)) throw new ArgumentOutOfRangeException("Parameter variant must start with -- must be more than one letter");
            
            Type = typeof(string);
        }

        /// <summary>
        /// Array of strings possible ways to declare this parameter (i.e (-f, --file))
        /// If array is not declared, name of parameter will be used.
        /// </summary>
        public IEnumerable<string> Variants { get; set; }

        /// <summary>
        /// Gets or sets the value of the parameter in the parsed argumnts.
        /// Name of parameter must be unique across all parameters of the method
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter provided. Defaults to string.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the default value of the parameter if not provided. Defaults to null.
        /// </summary>
        public object DefaultValue { get; set; }
    }
}
