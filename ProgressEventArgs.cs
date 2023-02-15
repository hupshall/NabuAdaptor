namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to encapsulate all info necessary to fire an event for updating a progress bar when downloading a nabu segment
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Current segment number
        /// </summary>
        private readonly int curr;

        /// <summary>
        /// Total number of segments
        /// </summary>
        private readonly int max;

        /// <summary>
        /// Create an instance of the ProgressEventArgs class
        /// </summary>
        /// <param name="segment">Segment name</param>
        /// <param name="curr">current segment</param>
        /// <param name="max">Max number of segments</param>
        public ProgressEventArgs(int segment, int curr, int max)
        {
            this.Segment = segment;
            this.curr = curr;
            this.max = max;
        }

        /// <summary>
        /// Get/Set the segment number
        /// </summary>
        public int Segment
        {
            get; set;
        }

        /// <summary>
        /// Gets the maximum segment number
        /// </summary>
        public int Max
        {
            get { return this.max; }
        }

        /// <summary>
        /// Gets the current segment number
        /// </summary>
        public int Curr
        {
            get { return this.curr; }
        }
    }
}
