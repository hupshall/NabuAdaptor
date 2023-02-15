using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabuAdaptor
{
    public class ProgressEventArgs : EventArgs
    {
        private readonly int curr;
        private readonly int max;

        public ProgressEventArgs(int segment, int curr, int max)
        {
            this.Segment = segment;
            this.curr = curr;
            this.max = max;
        }

        public int Segment
        {
            get;set;
        }

        public int Max
        {
            get { return this.max; }
        }

        public int Curr
        {
            get { return this.curr; }
        }
    }
}
