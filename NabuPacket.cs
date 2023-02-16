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
