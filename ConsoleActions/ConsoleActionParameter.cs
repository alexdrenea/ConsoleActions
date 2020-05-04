using System;
using System.Collections.Generic;

namespace ConsoleActions
{
    public class ConsoleActionParameter
    {
        public string ParameterName { get; set; }
        public IEnumerable<string> Variants { get; set; }
        public Type Type { get; set; }
        public object DefaultValue { get; set; }
    }
}
