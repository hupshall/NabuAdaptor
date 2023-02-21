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
namespace NabuAdaptor.FileStoreExtensions
{
    using System;
    using System.IO;

    /// <summary>
    /// Class to keep track of a FileDetails object used by the file extensions
    /// </summary>
    public class FileDetails
    {
        /// <summary>
        /// Gets or sets the Created time
        /// </summary>
        private DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the Modified time
        /// </summary>
        private DateTime Modified { get; set; }

        /// <summary>
        /// Gets or sets the Filename
        /// </summary>
        private string FileName { get; set; }

        /// <summary>
        /// Gets or sets the FileSize
        /// </summary>
        private long FileSize { get; set; }

        /// <summary>
        ///  Initializes a new instance of the <see cref="FileDetails"/> class. 
        /// </summary>
        /// <param name="created">Created Time</param>
        /// <param name="modified">Last Modified Time</param>
        /// <param name="fileName">File Name</param>
        /// <param name="fileSize">File Size</param>
        public FileDetails(DateTime created, DateTime modified, string fileName, long fileSize)
        {
            this.Created = created;
            this.Modified = modified;
            this.FileName = FileName;
            this.FileSize = FileSize;
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="FileDetails"/> class.
        /// </summary>
        /// <param name="directoryInfo">DirectoryInfo object</param>
        public FileDetails(DirectoryInfo directoryInfo)
        {
            this.Created = directoryInfo.CreationTime;
            this.Modified = directoryInfo.LastWriteTime;

            if (string.IsNullOrWhiteSpace(directoryInfo.Name))
            {
                this.FileName = @"\";
            }
            else
            {
                this.FileName = directoryInfo.Name;
            }

            this.FileSize = -1;
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="FileDetails"/> class.
        /// </summary>
        /// <param name="fileInfo">FileInfo object</param>
        public FileDetails(FileInfo fileInfo)
        {
            this.Created = fileInfo.CreationTime;
            this.Modified = fileInfo.LastWriteTime;

            if (string.IsNullOrWhiteSpace(fileInfo.Name))
            {
                this.FileName = @"\";
            }
            else
            {
                this.FileName = fileInfo.Name;
            }

            this.FileSize = fileInfo.Length;
        }

        /// <summary>
        /// Byte array details defined here:
        /// https://github.com/DJSures/NABU-LIB/blob/main/NABULIB/RetroNET-FileStore.h
        /// </summary>
        /// <returns>Returns this FileDetails object in the necessary byte array format for NABU</returns>
        public byte[] GetFileDetails()
        {
            byte[] fileDetails = new byte[83];

            fileDetails[0] = (byte)((uint)this.FileSize & 0xFFu);
            fileDetails[1] = (byte)((uint)(this.FileSize >> 8) & 0xFFu);
            fileDetails[2] = (byte)((uint)(this.FileSize >> 16) & 0xFFu);
            fileDetails[3] = (byte)((uint)(this.FileSize >> 24) & 0xFFu);
            fileDetails[4] = (byte)((ushort)this.Created.Year & 0xFFu);
            fileDetails[5] = (byte)((uint)((ushort)this.Created.Year >> 8) & 0xFFu);
            fileDetails[6] = (byte)this.Created.Month;
            fileDetails[7] = (byte)this.Created.Day;
            fileDetails[8] = (byte)this.Created.Hour;
            fileDetails[9] = (byte)this.Created.Minute;
            fileDetails[10] = (byte)this.Created.Second;
            fileDetails[11] = (byte)((ushort)this.Modified.Year & 0xFFu);
            fileDetails[12] = (byte)((uint)((ushort)this.Modified.Year >> 8) & 0xFFu);
            fileDetails[13] = (byte)this.Modified.Month;
            fileDetails[14] = (byte)this.Modified.Day;
            fileDetails[15] = (byte)this.Modified.Hour;
            fileDetails[16] = (byte)this.Modified.Minute;
            fileDetails[17] = (byte)this.Modified.Second;
            fileDetails[18] = (byte)Math.Min(this.FileName.Length, 64);

            for (int i = 0; i < fileDetails[18]; i++)
            {
                fileDetails[19 + i] = (byte)this.FileName[i];
            }

            return fileDetails;
        }
    }
}
