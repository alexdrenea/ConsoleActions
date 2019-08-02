using System;
using System.Collections.Generic;

namespace ConsoleActions.Attributes
{
    public class ActionAttribute : Attribute
    {
        public ActionAttribute(params string[] triggers)
        {
            Triggers = triggers;
        }
        public IEnumerable<string> Triggers { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool MeasureExexutionTime { get; set; }
    }
}
