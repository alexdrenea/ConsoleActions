using Newtonsoft.Json;
using System;
using System.Text;

namespace ConsoleActions
{
    public static class ConsoleHelpers
    {
        public static void WriteLine<T>(this T entity, ConsoleColor? color = null)
        {
            if (color.HasValue)
                Console.ForegroundColor = color.Value;
            Console.WriteLine(entity != null ? JsonConvert.SerializeObject(entity, Formatting.Indented) : "null");
            Console.ResetColor();
        }

        public static void WriteLine(this string message, ConsoleColor? color = null)
        {
            if (color.HasValue)
                Console.ForegroundColor = color.Value;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteInLine(this string message, ConsoleColor? color = null)
        {
            if (color.HasValue)
                Console.ForegroundColor = color.Value;
            Console.Write(message.Trim() + " "); //make sure there's a space at the end of the message since it's inline.
            Console.ResetColor();
        }

    }
}
