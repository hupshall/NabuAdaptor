namespace NabuAdaptor
{
    using System;

    /// <summary>
    /// Class to define a nabu segment
    /// </summary>
    public class NabuSegment
    {
        /// <summary>
        /// This is the size of a nabu header
        /// </summary>
        public const int SegmentHeaderLength = 0x10;

        /// <summary>
        /// This is the maximum size of data that can be in a nabu segment
        /// </summary>
        public const int SegmentDataLength = 0x3E1;

        /// <summary>
        /// This is the size of the CRC at the end of the segment
        /// </summary>
        public const int CrcLength = 0x2;

        /// <summary>
        /// The maximum size of a nabu segment, header + data + crc
        /// </summary>
        public static int MaxSegmentSize
        {
            get
            {
                return SegmentHeaderLength + SegmentDataLength + CrcLength;
            }
        }

        /// <summary>
        /// Gets this segment's sequence number
        /// </summary>
        public byte SequenceNumber
        {
            get; private set;
        }

        /// <summary>
        /// Gets this segments data (what actually gets sent to the nabu
        /// </summary>
        public byte[] Data
        {
            get; private set;
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="NabuSegment"/> class.
        /// </summary>
        /// <param name="sequenceNumber">Sequence number of this segment</param>
        /// <param name="data">data for this segment</param>
        public NabuSegment(byte sequenceNumber, byte[] data)
        {
            this.SequenceNumber = sequenceNumber;
            this.Data = data;
        }
    }
}
