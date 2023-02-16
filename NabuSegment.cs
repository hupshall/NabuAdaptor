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

    /// <summary>
    /// Class to encapsulate a Nabu Segment
    /// A segment is a collection of packets.  IOS requests segments, either code to execute or data to provide.
    /// A segment number is analogous (comparable) to a filename.  It's just a compact way of making a directory entry.
    /// </summary>
    public class NabuSegment
    {
        /// <summary>
        /// List of all the packets in this segment
        /// </summary>
        public NabuPacket[] Packets
        {
            get; set;
        }

        /// <summary>
        /// Name of the segment
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NabuSegment"/> class.
        /// </summary>
        /// <param name="packets">packets for this segment</param>
        /// <param name="name">name of the segment</param>
        public NabuSegment(NabuPacket[] packets, string name)
        {
            this.Packets = packets;
            this.Name = name;
        }
    }
}
