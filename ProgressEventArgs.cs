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
