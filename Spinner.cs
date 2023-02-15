namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class that displays a little spinner widget to the user
    /// </summary>
    public class Spinner
    {
        /// <summary>
        /// Positional counter
        /// </summary>
        static int counter = 0;

        /// <summary>
        /// Turn the widget
        /// </summary>
        /// <param name="segment"></param>
        public static void Turn(int segment)
        {
            string prefix = $"Nabu requesting segment {segment:X06}";
            
            counter++;
            switch (counter % 4)
            {
                case 0: Console.Write($"{prefix} /"); break;
                case 1: Console.Write($"{prefix} -"); break;
                case 2: Console.Write($"{prefix} \\"); break;
                case 3: Console.Write($"{prefix} |"); break;
            }

            Console.SetCursorPosition(0, Console.CursorTop);
        }
    }
}
