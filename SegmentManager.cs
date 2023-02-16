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
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Class to manage nabu segments (programs in a cycle).
    /// Code to create the time segment on the fly using the local computers clock
    /// </summary>
    public static class SegmentManager
    {
        /// <summary>
        /// Create the time segment that the nabu can parse.
        /// </summary>
        /// <returns></returns>
        public static NabuSegment CreateTimeSegment()
        {
            DateTime dateTime = DateTime.Now;

            List<byte> list = new List<byte>();
            list.Add(0x7F);
            list.Add(0xFF);
            list.Add(0xFF);
            list.Add(0x0);
            list.Add(0x0);
            list.Add(0x7F);
            list.Add(0xFF);
            list.Add(0xFF);
            list.Add(0xFF);
            list.Add(0x7F);
            list.Add(0x80);
            list.Add(0x30);
            list.Add(0x0);
            list.Add(0x0);
            list.Add(0x0);
            list.Add(0x0);
            list.Add(0x2);
            list.Add(0x2);
            list.Add((byte)(dateTime.DayOfWeek + 1));
            list.Add(0x54);
            list.Add((byte)dateTime.Month);
            list.Add((byte)dateTime.Day);
            list.Add((byte)dateTime.Hour);
            list.Add((byte)dateTime.Minute);
            list.Add((byte)dateTime.Second);
            list.Add(0x0);
            list.Add(0x0);

            byte[] crcData = CRC.CalculateCRC(list.ToArray());

            list.Add(crcData[0]);
            list.Add(crcData[1]);

            return new NabuSegment(new List<NabuPacket> { new NabuPacket(0, list.ToArray()) }.ToArray(), "0x7FFFFF");
        }

        /// <summary>
        /// Load the packets inside of the segment file (original Nabu cycle packet)
        /// </summary>
        /// <param name="segmentName">Name of the segment file</param>
        /// <param name="data">contents of the file as a byte array</param>
        /// <returns>Nabu Segment object which contains the packets</returns>
        public static NabuSegment LoadPackets(string segmentName, byte[] data, Logger logger)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                if (memoryStream.Length > 0xFFFFL)
                {
                    throw new ArgumentOutOfRangeException("data", "File too large");
                }

                List<NabuPacket> list = new List<NabuPacket>();

                byte packetNumber = 0;

                // Ok, read in the segment file into it's constituent packets
                while (memoryStream.Position < memoryStream.Length)
                {
                    // Get the first two bytes, this is the length of this segment
                    int segmentLength = memoryStream.ReadByte() + (memoryStream.ReadByte() << 8);

                    if (segmentLength > 0 && segmentLength <= NabuPacket.MaxPacketSize)
                    {
                        // ok, Read this segment
                        byte[] segmentData = new byte[segmentLength];
                        memoryStream.Read(segmentData, 0, segmentLength);
                        NabuPacket packet = new NabuPacket(packetNumber, segmentData);
                        ValidatePacket(packet.Data, logger);
                        list.Add(packet);
                        packetNumber++;
                    }
                }

                return new NabuSegment(list.ToArray(), segmentName);
            }
        }

        /// <summary>
        /// Create packets object for a compiled program
        /// </summary>
        /// <param name="segmentName">name of segment file</param>
        /// <param name="data">binary data to make into segments</param>
        /// <returns>Nabu Segment object which contains the packets</returns>
        public static NabuSegment CreatePackets(string segmentName, byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                if (memoryStream.Length > 0xFFFFL)
                {
                   throw new ArgumentOutOfRangeException("data", "File too large");
                }

                List<NabuPacket> packets = new List<NabuPacket>();

                byte segmentNumber = 0;
                while (true)
                {
                    long offset = memoryStream.Position;
                    byte[] buffer = new byte[NabuPacket.PacketDataLength];
                    int bytesRead = memoryStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // We're done
                        break;
                    }

                    // If we are at the EOF, then this is the last segment
                    bool lastSegment = memoryStream.Position == memoryStream.Length;

                    // Create the segment
                    packets.Add(new NabuPacket(segmentNumber, CreatePacket(segmentNumber, (ushort)offset, lastSegment, buffer.Take(bytesRead).ToArray()))); 
                } 

                return new NabuSegment(packets.ToArray(), segmentName);
            }
        }

        /// <summary>
        /// Create an individual segment
        /// </summary>
        /// <param name="packetNumber">Segment number</param>
        /// <param name="offset">offset </param>
        /// <param name="lastSegment"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] CreatePacket(byte packetNumber, ushort offset, bool lastSegment, byte[] data)
        {
            List<byte> list = new List<byte>();

            // Cobble together the header
            list.Add(0x0);
            list.Add(0x0);
            list.Add(0x1);
            list.Add(packetNumber);

            // Owner
            list.Add(0x1);

            // Tier
            list.Add(0x7F);
            list.Add(0xFF);
            list.Add(0xFF);
            list.Add(0xFF);

            list.Add(0x7F);
            list.Add(0x80);

            // type
            byte b = 0x20;
            if (offset < 0x80)
            {
                b = (byte)(b | 0x81u);
            }
            if (lastSegment)
            {
                b = (byte)(b | 0x10u);
            }

            list.Add(b);
            list.Add(packetNumber);
            list.Add(0x0);
            list.Add((byte)((uint)(offset + 0x12 >> 8) & 0xFFu));
            list.Add((byte)((uint)(offset + 0x12) & 0xFFu));

            // Payload
            list.AddRange(data);

            // CRC
            byte[] crcData = CRC.CalculateCRC(list.ToArray());

            list.Add(crcData[0]);
            list.Add(crcData[1]);
            return list.ToArray();
        }

        /// <summary>
        /// Validate the packet CRC
        /// </summary>
        /// <param name="packetData">segment data</param>
        private static void ValidatePacket(byte[] packetData, Logger logger)
        {
            byte[] data = new byte[packetData.Length - 2];
            Array.Copy(packetData, data, packetData.Length - 2);
            byte[] crcData = CRC.CalculateCRC(data);

            if (packetData[packetData.Length - 2] != crcData[0] || packetData[packetData.Length - 1] != crcData[1])
            {
                logger.Log($"CRC Bad, Calculated 0x{crcData[0]}, 0x{crcData[1]}, but read 0x{packetData[packetData.Length - 2]:X02}, 0x{packetData[packetData.Length - 1]:X02}", Logger.Target.file);

                // Fix the CRC so that the nabu will load.
                packetData[packetData.Length - 2] = crcData[0];
                packetData[packetData.Length - 1] = crcData[1];
            }
        }
    }
}
