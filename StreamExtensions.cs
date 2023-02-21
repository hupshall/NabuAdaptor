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
    using System.IO;
    using System.Text;

    /// <summary>
    /// Some extensions to the stream to make reading and writing larger values easier
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Read a string
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <param name="length">the length of the string</param>
        /// <returns>The String</returns>
        public static string ReadString(this Stream stream, int length)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                return Encoding.ASCII.GetString(br.ReadBytes(length));
            }
        }

        /// <summary>
        /// Read a byte array
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <param name="length">number of bytes to read</param>
        /// <returns>array of bytes</returns>
        public static byte[] ReadBytes(this Stream stream, int length)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                return br.ReadBytes(length);
            }
        }

        /// <summary>
        /// Read an unsigned int
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <returns>the unsigned int</returns>
        public static uint ReadUint(this Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                return br.ReadUInt32();
            } 
        }

        /// <summary>
        /// Read an unsigned short
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <returns>the unsigned short</returns>
        public static ushort ReadUshort(this Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                return br.ReadUInt16();
            }
        }

        /// <summary>
        /// Read an int
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <returns>the int</returns>
        public static int ReadInt(this Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                return br.ReadInt32();
            }
        }

        /// <summary>
        /// Write an unsigned short
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <param name="value">the short to write</param>
        public static void WriteUshort(this Stream stream, ushort value)
        {
            using (BinaryWriter bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                bw.Write(value);
            }
        }

        /// <summary>
        /// Write the int
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <param name="value">the int to write</param>
        public static void WriteInt(this Stream stream, int value)
        {
            using (BinaryWriter bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                bw.Write(value);
            }
        }

        /// <summary>
        /// Write an array of bytes
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <param name="bytes">the array of bytes</param>
        public static void WriteBytes(this Stream stream, params byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
