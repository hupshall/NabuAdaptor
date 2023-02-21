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

    [Flags]
    public enum FileFlags
    {
        /// <summary>
        /// File access is read only
        /// </summary>
        ReadOnly = 0,

        /// <summary>
        /// File access is read and write
        /// </summary>
        ReadWrite = 1
    }

    [Flags]
    public enum CopyMoveFlags
    {
        /// <summary>
        /// Do not overwrite
        /// </summary>
        NoReplace = 0,

        /// <summary>
        /// Overwrite allowed
        /// </summary>
        YesReplace = 1
    }

    [Flags]
    public enum FileListFlags
    {
        /// <summary>
        /// Include files in the file listing
        /// </summary>
        IncludeFiles = 1,

        /// <summary>
        /// Include directoris in the file listing
        /// </summary>
        IncludeDirectories = 2
    }

    /// <summary>
    /// Seek flags for access into a file
    /// </summary>
    public enum SeekFlags
    {
        SET = 1,
        CUR = 2,
        END = 3
    }

    /// <summary>
    /// Class to describe what the server needs to keep in memory for a file handle opened by the Nabu 
    /// </summary>
    public class NabuFileHandle
    {
        /// <summary>
        /// Internal holder for index
        /// </summary>
        private long index;

        /// <summary>
        /// Gets the file handle assigned to this file
        /// </summary>
        public byte FileHandle { get; private set; }

        /// <summary>
        /// Gets the flags used when the file was opened
        /// </summary>
        public FileFlags Flags { get; private set; }

        /// <summary>
        /// Gets the filename
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the local working directory
        /// </summary>
        public string WorkingDirectory { get; private set; }

        /// <summary>
        /// The index into the file where we are currently reading/writing
        /// </summary>
        public long Index
        {
            get
            {
                return this.index;
            }
            set
            {
                this.index = value;

                // Make sure that Index is valid
                if (this.index < 0)
                {
                    this.index = 0L;
                }
                else
                {
                    FileInfo fileInfo = new FileInfo(this.FullFileName);

                    if (this.index > fileInfo.Length)
                    {
                        this.Index = fileInfo.Length;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the file working filename (path and filename)
        /// </summary>
        public string FullFileName
        {
            get
            {
                return Path.Combine(this.WorkingDirectory, this.FileName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NabuFileHandle"/> class. 
        /// </summary>
        /// <param name="workingDirectory">Working Directory</param>
        /// <param name="fileName">Filename</param>
        /// <param name="flags">File Flags</param>
        /// <param name="fileHandle">File Handle</param>
        public NabuFileHandle(string workingDirectory, string fileName, FileFlags flags, byte fileHandle)
        {
            this.WorkingDirectory = workingDirectory;
            this.Flags = flags;
            this.FileHandle = fileHandle;
            this.FileName = fileName;
            this.Index = 0;
        }
    }
}
