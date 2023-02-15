namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class to define a nabu packet
    /// </summary>
    public class NabuPacket
    {
        /// <summary>
        /// This is the size of a nabu header
        /// </summary>
        public const int PacketHeaderLength = 0x10;

        /// <summary>
        /// This is the maximum size of data that can be in a nabu packet
        /// </summary>
        public const int PacketDataLength = 0x3E1;

        /// <summary>
        /// This is the size of the CRC at the end of the packet
        /// </summary>
        public const int CrcLength = 0x2;

        /// <summary>
        /// The maximum size of a nabu packet, header + data + crc
        /// </summary>
        public static int MaxPacketSize
        {
            get
            {
                return PacketHeaderLength + PacketDataLength + CrcLength;
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
        /// Gets this segments data in escaped format
        /// </summary>
        public byte[] EscapedData
        {
            get
            {
                List<byte> escapedData = new List<byte>();

                foreach (byte b in this.Data)
                {
                    // need to escape 0x10
                    if (b == 0x10)
                    {
                        escapedData.Add(0x10);
                    }

                    escapedData.Add(b);
                }

                return escapedData.ToArray();
            }
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="NabuPacket"/> class.
        /// </summary>
        /// <param name="sequenceNumber">Sequence number of this segment</param>
        /// <param name="data">data for this segment</param>
        public NabuPacket(byte sequenceNumber, byte[] data)
        {
            this.SequenceNumber = sequenceNumber;
            this.Data = data;
        }
    }
}
