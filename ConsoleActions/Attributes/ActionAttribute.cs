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

        /// <summary>
        /// Array of strings representing the keyworods / commands that will can be used to trigger the action.
        /// Each element must be unique across the entire applicateion
        /// </summary>
        public IEnumerable<string> Triggers { get; set; }

        /// <summary>
        /// Description of the action to be displayed in the help menu
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Order of action in the help menu listing.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Optional parameter to indicate if the an exection time measurement should be taken and displayed after the execution of the action.
        /// </summary>
        public bool MeasureExexutionTime { get; set; }
    }
}
