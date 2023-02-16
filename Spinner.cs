// BSD 2-Clause License
//
// Copyright(c) 2022, Huw Upshall
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
