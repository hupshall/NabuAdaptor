namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class SegmentManager
    {
        /// <summary>
        /// Create the time segment that the nabu can parse.
        /// </summary>
        /// <returns></returns>
        public static NabuPak CreateTimePak()
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

            return new NabuPak(new List<NabuSegment> { new NabuSegment(0, list.ToArray()) }.ToArray(), "0x7FFFFF");
        }

        /// <summary>
        /// Load the segments imbedded in the PAK file
        /// </summary>
        /// <param name="pakName">Name of the PAK file</param>
        /// <param name="data">contents of the file as a byte array</param>
        /// <returns>Nabu Pak object which contains the segments</returns>
        public static NabuPak LoadSegments(string pakName, byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                if (memoryStream.Length > 0xFFFFL)
                {
                    throw new ArgumentOutOfRangeException("data", "File too large");
                }

                List<NabuSegment> list = new List<NabuSegment>();

                byte segmentNumber = 0;

                // Ok, read in the PAK file into it's constituent segments
                while (memoryStream.Position < memoryStream.Length)
                {
                    // Get the first two bytes, this is the length of this segment
                    int segmentLength = memoryStream.ReadByte() + (memoryStream.ReadByte() << 8);

                    if (segmentLength > 0 && segmentLength <= NabuSegment.MaxSegmentSize)
                    {
                        // ok, Read this segment
                        byte[] segmentData = new byte[segmentLength];
                        memoryStream.Read(segmentData, 0, segmentLength);
                        NabuSegment segment = new NabuSegment(segmentNumber, segmentData);
                        ValidateSegment(segment.Data);
                        list.Add(segment);
                        segmentNumber++;
                    }
                }

                return new NabuPak(list.ToArray(), pakName);
            }
        }

        /// <summary>
        /// Create segments for a compiled program
        /// </summary>
        /// <param name="pakName">name of pak file</param>
        /// <param name="data">binary data to make into segments</param>
        /// <returns></returns>
        public static NabuPak CreateSegments(string pakName, byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                if (memoryStream.Length > 0xFFFFL)
                {
                   throw new ArgumentOutOfRangeException("data", "File too large");
                }

                List<NabuSegment> segments = new List<NabuSegment>();

                byte segmentNumber = 0;
                while (true)
                {
                    long offset = memoryStream.Position;
                    byte[] buffer = new byte[NabuSegment.SegmentDataLength];
                    int bytesRead = memoryStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // We're done
                        break;
                    }

                    // If we are at the EOF, then this is the last segment
                    bool lastSegment = memoryStream.Position == memoryStream.Length;

                    // Create the segment
                    segments.Add(new NabuSegment(segmentNumber, CreateSegment(segmentNumber, (ushort)offset, lastSegment, buffer.Take(bytesRead).ToArray()))); 
                } 

                return new NabuPak(segments.ToArray(), pakName);
            }
        }

        /// <summary>
        /// Create an individual segment
        /// </summary>
        /// <param name="segmentNumber">Segment number</param>
        /// <param name="offset">offset </param>
        /// <param name="lastSegment"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] CreateSegment(byte segmentNumber, ushort offset, bool lastSegment, byte[] data)
        {
            List<byte> list = new List<byte>();

            // Cobble together the header
            list.Add(0x0);
            list.Add(0x0);
            list.Add(0x1);
            list.Add(segmentNumber);

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
            list.Add(segmentNumber);
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
        /// Validate the segment CRC
        /// </summary>
        /// <param name="segmentData">segment data</param>
        private static void ValidateSegment(byte[] segmentData)
        {
            byte[] data = new byte[segmentData.Length - 2];
            Array.Copy(segmentData, data, segmentData.Length - 2);
            byte[] crcData = CRC.CalculateCRC(data);

            if (segmentData[segmentData.Length - 2] != crcData[0] || segmentData[segmentData.Length - 1] != crcData[1])
            {
                Logger.Log($"CRC Bad, Calculated 0x{crcData[0]}, 0x{crcData[1]}, but read 0x{segmentData[segmentData.Length - 2]:X02}, 0x{segmentData[segmentData.Length - 1]:X02}", Logger.Target.file);

                // Fix the CRC so that the nabu will load.
                segmentData[segmentData.Length - 2] = crcData[0];
                segmentData[segmentData.Length - 1] = crcData[1];
            }
        }
    }
}
