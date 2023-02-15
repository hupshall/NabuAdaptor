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
