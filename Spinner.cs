using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabuAdaptor
{
    public class Spinner
    {
        static int counter = 0;

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
