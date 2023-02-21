﻿// BSD 2-Clause License
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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Class to implement all of the NABU File system extensions as defined in
    /// https://github.com/DJSures/NABU-LIB/blob/main/NABULIB/RetroNET-FileStore.h
    /// only support HTTP(s) and Local files, no FTP
    /// </summary>
    public class FileStoreExtensions : IServerExtension
    {
        /// <summary>
        /// Instance of the server
        /// </summary>
        private Server server;

        /// <summary>
        /// We keep track of the file handles opened by NABU with a quick dictionary lookup
        /// </summary>
        private Dictionary<byte, NabuFileHandle> fileHandles;

        /// <summary>
        /// When FileList() is called, we create a list of the files which can then be returned one at a time with a call to FileListItem()
        /// </summary>
        private List<FileDetails> fileDetails;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStoreExtensions"/> class. 
        /// </summary>
        /// <param name="server"></param>
        public FileStoreExtensions(Server server)
        {
            this.server = server;
            this.Initialize();
        }

        /// <summary>
        /// This extension implements several new op codes - This function maps those codes to the appropriate function call.
        /// </summary>
        /// <param name="opCode">OP code to process</param>
        /// <returns>true if we acted on this opCode, otherwise false.</returns>
        public bool TryProcessCommand(byte opCode)
        {
            switch (opCode)
            {
                case 0xA3:
                    this.FileOpen();
                    return true;
                case 0xA4:
                    this.FileHandleSize();
                    return true;
                case 0xA5:
                    this.FileHandleRead();
                    return true;
                case 0xA7:
                    this.FileHandleClose();
                    return true;
                case 0xA8:
                    this.FileSize();
                    return true;
                case 0xA9:
                    this.FileHandleAppend();
                    return true;
                case 0xAA:
                    this.FileHandleInsert();
                    return true;
                case 0xAB:
                    this.FileHandleDeleteRange();
                    return true;
                case 0xAC:
                    this.FileHandleReplace();
                    return true;
                case 0xAD:
                    this.FileDelete();
                    return true;
                case 0xAE:
                    this.FileHandleCopy();
                    return true;
                case 0xAF:
                    this.FileHandleMove();
                    return true;
                case 0xB0:
                    this.FileHandleEmptyFile();
                    return true;
                case 0xB1:
                    this.FileList();
                    return true;
                case 0xB2:
                    this.FileListItem();
                    return true;
                case 0xB3:
                    this.FileDetails();
                    return true;
                case 0xB4:
                    this.FileHandleDetails();
                    return true;
                case 0xB5:
                    this.FileHandleReadSeq();
                    return true;
                case 0xB6:
                    this.FileHandleSeek();
                    return true;
            }

            // Op code not serviced by this extension
            return false;
        }

        /// <summary>
        /// Reset this extension.  If the Nabu starts over loading segment 0 and packet 0 - start over.
        /// </summary>
        public void Reset()
        {
            server.Logger.Log($"Reseting FileStoreExtension", Logger.Target.console);
            this.Initialize();
        }

        /// <summary>
        /// Initialize the extension - setup the member variables.
        /// </summary>
        private void Initialize()
        {
            this.fileHandles = new Dictionary<byte, NabuFileHandle>();
            this.fileDetails = new List<FileDetails>();

            // Just make sure that all the file handles are set to null (unused) - this is a just in case
            for (byte b = 0; b < byte.MaxValue; b++)
            {
                this.fileHandles[b] = null;
            }
        }
        
        /// <summary>
        /// File Open
        /// </summary>
        private void FileOpen()
        {
            // First byte is the string length
            byte length = this.server.ReadByte();

            // Read the filename
            string fileName = this.server.Connection.NabuStream.ReadString(length);

            if (fileName.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                // Download this file from wherever it is located to the curent directory.
                Uri uri = new Uri(fileName);
                fileName = Path.GetFileName(uri.LocalPath);

                string fullPathAndFilename = Path.Combine(this.server.GetWorkingDirectory(), fileName);
                if (!File.Exists(fullPathAndFilename))
                {
                    byte[] data;

                    WebLoader webLoader = new WebLoader();
                    webLoader.TryGetData(uri.AbsoluteUri, out data);
                    System.IO.File.WriteAllBytes(fullPathAndFilename, data);
                }
            }

            // Read the flags
            ushort fileFlags = this.server.Connection.NabuStream.ReadUshort();

            // Read the file handle
            byte fileHandle = this.server.ReadByte();

            // If this handle is the max value, or that this handle is already in use, find the first unused handle
            if (fileHandle == byte.MaxValue || this.fileHandles[fileHandle] != null)
            {
                KeyValuePair<byte, NabuFileHandle> kvp = this.fileHandles.First(handle => handle.Value == null);
                fileHandle = kvp.Key;
            }

            NabuFileHandle nabuFileHandle = new NabuFileHandle(this.server.GetWorkingDirectory(), fileName, (FileFlags)fileFlags, fileHandle);
            
            // Mark this handle as in use.
            this.fileHandles[fileHandle] = nabuFileHandle;

            // Let the NABU know what the real file handle is
            this.server.WriteBytes(fileHandle);
        }

        /// <summary>
        /// File Handle Size
        /// </summary>
        private void FileHandleSize()
        {
            // first byte, the file handle
            byte fileHandle = this.server.ReadByte();

            // Retrieve this file handle from the file handle list.
            NabuFileHandle nabuFileHandle = this.fileHandles[fileHandle];

            if (nabuFileHandle != null)
            {
                this.server.Connection.NabuStream.WriteInt(this.FileSize(nabuFileHandle.FullFileName));
            }
            else
            {
                server.Logger.Log($"FileHandleSize Requested file handle: {fileHandle:X06} but it was not found, returning -1", Logger.Target.console);
                this.server.Connection.NabuStream.WriteInt(-1);
            }
}

        /// <summary>
        /// File Handle Read
        /// </summary>
        private void FileHandleRead()
        {
            // first byte, the file handle
            byte fileHandle = this.server.ReadByte();

            // the offset
            uint offset = this.server.Connection.NabuStream.ReadUint();

            // the length
            ushort length = this.server.Connection.NabuStream.ReadUshort();

            // Retrieve this file handle from the file handle list.
            NabuFileHandle nabuFileHandle = this.fileHandles[fileHandle];
            
            if (nabuFileHandle != null)
            {
                byte[] data = File.ReadAllBytes(nabuFileHandle.FullFileName).Skip((int)offset).Take(length).ToArray();
                this.server.Connection.NabuStream.WriteUshort((ushort)data.Length);
                this.server.WriteBytes(data);
            }
            else
            {
                server.Logger.Log($"FileHandleRead Requested file handle to read: {fileHandle:X06} but it was not found", Logger.Target.console);

                // sending back 0, this tells the NABU there is no data to read
                this.server.Connection.NabuStream.WriteUshort(0);
            }
        }

        /// <summary>
        /// File Handle Close
        /// </summary>
        private void FileHandleClose()
        {
            // first byte, the file handle
            byte fileHandle = this.server.ReadByte();
            this.fileHandles[fileHandle] = null;
        }

        /// <summary>
        /// File Size
        /// </summary>
        private void FileSize()
        {
            // First byte is the string length
            byte length = this.server.ReadByte();

            // read filename
            string fileName = this.server.Connection.NabuStream.ReadString(length);
            fileName = Path.Combine(this.server.GetWorkingDirectory(), fileName);
            this.server.Connection.NabuStream.WriteInt(this.FileSize(fileName));
        }

        /// <summary>
        /// File Handle Append (write data to the end of the file)
        /// </summary>
        private void FileHandleAppend()
        {
            // first byte, the file handle
            byte fileHandle = this.server.ReadByte();

            // now the length of the data 
            ushort length = this.server.Connection.NabuStream.ReadUshort();

            // read the data into an array
            byte[] data = this.server.Connection.NabuStream.ReadBytes(length);

            // ok, get the specified file handle.
            NabuFileHandle nabuFileHandle = this.fileHandles[fileHandle];

            if (nabuFileHandle != null && ((nabuFileHandle.Flags & FileFlags.ReadWrite) == FileFlags.ReadWrite))
            {
                using (FileStream fileStream = new FileStream(nabuFileHandle.FullFileName, FileMode.Append))
                {
                    fileStream.Write(data, 0, data.Length);
                }
            }
            else
            {
                server.Logger.Log($"Requested file Append on {fileHandle:X06} but it was not found", Logger.Target.console);
            }
        }

        /// <summary>
        /// File Handle Insert (Insert data into the file)
        /// </summary>
        private void FileHandleInsert()
        {
            // first byte, the file handle
            byte fileHandle = this.server.ReadByte();

            // read the file index
            uint index = this.server.Connection.NabuStream.ReadUint();

            // read the data length
            ushort length = this.server.Connection.NabuStream.ReadUshort();

            // Read the data from the buffer
            byte[] data = this.server.Connection.NabuStream.ReadBytes(length);

            // Get the file handle
            NabuFileHandle nabuFileHandle = this.fileHandles[fileHandle];

            if (nabuFileHandle != null && ((nabuFileHandle.Flags & FileFlags.ReadWrite) == FileFlags.ReadWrite))
            {
                List<byte> bytes = File.ReadAllBytes(nabuFileHandle.FullFileName).ToList();
                bytes.InsertRange((int)index, data);
                File.WriteAllBytes(nabuFileHandle.FullFileName, bytes.ToArray());
            }
            else if ((nabuFileHandle.Flags & FileFlags.ReadWrite) == FileFlags.ReadWrite)
            {
                server.Logger.Log($"FileHandleInsert Requested handle insert on {fileHandle:X06} but it was not found", Logger.Target.console);
            }
        }

        /// <summary>
        /// File Handle Delete Range
        /// </summary>
        private void FileHandleDeleteRange()
        {
            // first byte, file handle
            byte fileHandle = this.server.ReadByte();

            // read the file offset
            uint index = this.server.Connection.NabuStream.ReadUint();

            // read the length
            ushort length = this.server.Connection.NabuStream.ReadUshort();

            // Get the file handle
            NabuFileHandle nabuFileHandle = this.fileHandles[fileHandle];

            if (nabuFileHandle != null && ((nabuFileHandle.Flags & FileFlags.ReadWrite) == FileFlags.ReadWrite))
            {
                List<byte> bytes = File.ReadAllBytes(nabuFileHandle.FullFileName).ToList();
                bytes.RemoveRange((int)index, length);
                File.WriteAllBytes(nabuFileHandle.FullFileName, bytes.ToArray());
            }
            else
            {
                server.Logger.Log($"Requested handle in HandleDeleteRange {fileHandle:X06} but it was not found", Logger.Target.console);
            }
        }

        /// <summary>
        /// File Handle Replace (replace data in a file)
        /// </summary>
        private void FileHandleReplace()
        {
            // Get the file handle
            byte fileHandle = this.server.ReadByte();

            // Get the file offset
            uint index = this.server.Connection.NabuStream.ReadUint();

            // get the data length
            ushort length = this.server.Connection.NabuStream.ReadUshort();

            // Get the data
            byte[] data = this.server.Connection.NabuStream.ReadBytes(length);

            // Get the file handle
            NabuFileHandle nabuFileHandle = this.fileHandles[fileHandle];

            if (nabuFileHandle != null && ((nabuFileHandle.Flags & FileFlags.ReadWrite) == FileFlags.ReadWrite))
            {
                List<byte> bytes = File.ReadAllBytes(nabuFileHandle.FullFileName).ToList();
                for (int i = 0; i < length; i++)
                {
                    bytes[(int)(i + index)] = data[i];
                }

                File.WriteAllBytes(nabuFileHandle.FullFileName, bytes.ToArray());
            }
            else
            {
                server.Logger.Log($"Requested handle in HandleDeleteReplace {fileHandle:X06} but it was not found", Logger.Target.console);
            }
        }

        /// <summary>
        /// File Delete
        /// </summary>
        private void FileDelete()
        {
            // Read the filename length
            byte length = this.server.ReadByte();

            // read the filename
            string fileName = this.server.Connection.NabuStream.ReadString(length);
            fileName = Path.Combine(this.server.GetWorkingDirectory(), fileName);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            // Must be a better way to do this - Find all instances of this file in the file handles and close them.
            for (byte i = 0; i <= byte.MaxValue; i++)
            {
                if (this.fileHandles[i] != null && this.fileHandles[i].FullFileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    // clear out this file handle
                    this.fileHandles[i] = null;
                }
            }
        }

        /// <summary>
        /// File Handle Copy
        /// </summary>
        private void FileHandleCopy()
        {
            // read filename length
            byte length = this.server.ReadByte();

            // read the source filename
            string sourceFilename = this.server.Connection.NabuStream.ReadString(length);
            length = this.server.ReadByte();

            // read the destination filename
            string destinationFilename = this.server.Connection.NabuStream.ReadString(length);

            // read the copy move flag
            byte flags = this.server.ReadByte();

            sourceFilename = Path.Combine(this.server.GetWorkingDirectory(), sourceFilename);
            destinationFilename = Path.Combine(this.server.GetWorkingDirectory(), destinationFilename);

            if (!File.Exists(destinationFilename) || (File.Exists(destinationFilename) && ((CopyMoveFlags)flags & CopyMoveFlags.YesReplace) == CopyMoveFlags.YesReplace))
            {
                File.Copy(sourceFilename, destinationFilename);
            }
        }

        /// <summary>
        /// File Handle Move
        /// </summary>
        private void FileHandleMove()
        {
            // read filename length
            byte length = this.server.ReadByte();

            // read the source filename
            string sourceFilename = this.server.Connection.NabuStream.ReadString(length);

            // read the destination file length
            length = this.server.ReadByte();

            // read the destination filename
            string destinationFilename = this.server.Connection.NabuStream.ReadString(length);

            // read the copy move flag
            byte flags = this.server.ReadByte();

            sourceFilename = Path.Combine(this.server.GetWorkingDirectory(), sourceFilename);
            destinationFilename = Path.Combine(this.server.GetWorkingDirectory(), destinationFilename);

            if (!File.Exists(destinationFilename) || (File.Exists(destinationFilename) && ((CopyMoveFlags)flags & CopyMoveFlags.YesReplace) == CopyMoveFlags.YesReplace))
            {
                File.Move(sourceFilename, destinationFilename);
            }
        }

        /// <summary>
        /// File Handle Empty File
        /// </summary>
        private void FileHandleEmptyFile()
        {
            // Read in the file handle.
            byte fileHandle = this.server.ReadByte();

            // Get the file handle
            NabuFileHandle nabuFileHandle = this.fileHandles[fileHandle];

            if (nabuFileHandle != null)
            {
                using (File.Create(nabuFileHandle.FullFileName)) ;
            }
            else
            {
                server.Logger.Log($"Requested handle in FileHandleEmptyFile {fileHandle:X06} but it was not found", Logger.Target.console);
            }
        }

        /// <summary>
        /// File List (basically, do a DIR based on the search pattern, store the results for use later
        /// </summary>
        private void FileList()
        {
            // Read the path length
            byte length = this.server.ReadByte();

            // Get the path
            string path = this.server.Connection.NabuStream.ReadString(length);

            // read the search pattern length
            length = this.server.ReadByte();

            // read the search pattern
            string searchPattern = this.server.Connection.NabuStream.ReadString(length);

            // Get the flags
            FileListFlags flags = (FileListFlags)this.server.ReadByte();
            this.fileDetails.Clear();

            if ((flags & FileListFlags.IncludeDirectories) == FileListFlags.IncludeDirectories)
            {
                string[] directories = Directory.GetDirectories(Path.Combine(this.server.GetWorkingDirectory(), path), searchPattern);
                foreach (string directory in directories)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                    this.fileDetails.Add(new FileDetails(directoryInfo));
                }
            }

            if ((flags & FileListFlags.IncludeFiles) == FileListFlags.IncludeFiles)
            {
                string[] files = Directory.GetFiles(Path.Combine(this.server.GetWorkingDirectory(), path), searchPattern);
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    this.fileDetails.Add(new FileDetails(fileInfo));
                }
            }

            this.server.Connection.NabuStream.WriteUshort((ushort)this.fileDetails.Count);
        }

        /// <summary>
        /// File List Item
        /// </summary>
        private void FileListItem()
        {
            // read in the index number for the flie list cache.
            ushort num = this.server.Connection.NabuStream.ReadUshort();
            this.server.WriteBytes(this.fileDetails[num].GetFileDetails());
        }

        /// <summary>
        /// File Details (this is with a filename)
        /// </summary>
        private void FileDetails()
        {
            // read in the length
            byte length = this.server.ReadByte();

            // read in the filename
            string filename = this.server.Connection.NabuStream.ReadString(length);
            filename = Path.Combine(this.server.GetWorkingDirectory(), filename);
            this.FileDetails(filename);
        }

        /// <summary>
        /// File Handle Details (this is with a file handle)
        /// </summary>
        private void FileHandleDetails()
        {
            // Read the file handle
            byte fileHandle = this.server.ReadByte();

            // Retrieve this file handle from the file handle list.
            NabuFileHandle nabuFileHandle = this.fileHandles[fileHandle];

            // if the file handle is null, what the heck?
            if (nabuFileHandle != null)
            {
                this.FileDetails(nabuFileHandle.FullFileName);
            }
            else
            {
                server.Logger.Log($"Requested file handle for FileHandleDetails: {fileHandle:X06} but it was not found", Logger.Target.console);
            }
        }

        /// <summary>
        /// Return a FileDetails based on the filename
        /// </summary>
        /// <param name="filename"></param>
        private void FileDetails(string filename)
        {
            FileDetails fileDetails;

            if (File.Exists(filename))
            {
                fileDetails = new FileDetails(new FileInfo(filename));
            }
            else if (Directory.Exists(filename))
            {
                fileDetails = new FileDetails(new DirectoryInfo(filename));
            }
            else
            {
                // fake it
                fileDetails = new FileDetails(DateTime.Now, DateTime.Now, @"\", -2);
            }

            this.server.WriteBytes(fileDetails.GetFileDetails());
        }

        /// <summary>
        /// File Handle Sequential Read
        /// </summary>
        private void FileHandleReadSeq()
        {
            // Read the file handle
            byte fileHandle = this.server.ReadByte();

            // Read the number of bytes to read
            ushort length = this.server.Connection.NabuStream.ReadUshort();

            // Retrieve this file handle from the file handle list.
            NabuFileHandle nabuFileHandle = this.fileHandles[fileHandle];

            // if the file handle is null, what the heck?
            if (nabuFileHandle != null)
            {
                byte[] data = File.ReadAllBytes(nabuFileHandle.FullFileName).Skip((int)nabuFileHandle.Index).Take(length).ToArray();
                nabuFileHandle.Index += data.Length;

                // write how much data we got
                this.server.Connection.NabuStream.WriteUshort((ushort)data.Length);

                // write the data
                this.server.WriteBytes(data);
            }
            else
            {
                server.Logger.Log($"Requested file handle for FileHandleReadSeq: {fileHandle:X06} but it was not found", Logger.Target.console);
            }
        }

        /// <summary>
        /// File Handle Seek
        /// </summary>
        private void FileHandleSeek()
        {
            // read the file handle
            byte fileHandle = this.server.ReadByte();

            // read the offset
            int offset = this.server.Connection.NabuStream.ReadInt();

            // read the seek options
            byte seekOption = this.server.ReadByte();
            SeekFlags seekFlags = (SeekFlags)seekOption;

            // Retrieve this file handle from the file handle list.
            NabuFileHandle nabuFileHandle = this.fileHandles[fileHandle];

            if (nabuFileHandle != null)
            {
                FileInfo fileInfo = new FileInfo(nabuFileHandle.FullFileName);

                if (seekFlags == SeekFlags.SET)
                {
                    // Seek from the start of the file
                    nabuFileHandle.Index = offset;
                }
                else if (seekFlags == SeekFlags.CUR)
                {
                    // Seek from the current position in the file.
                    nabuFileHandle.Index += offset;
                }
                else
                {
                    // Last option is from the end of the file.
                    nabuFileHandle.Index = fileInfo.Length - offset;
                }

                if (nabuFileHandle.Index < 0)
                {
                    nabuFileHandle.Index = 0L;
                }
                else if (nabuFileHandle.Index > fileInfo.Length)
                {
                    nabuFileHandle.Index = fileInfo.Length;
                }

                this.server.Connection.NabuStream.WriteInt((int)nabuFileHandle.Index);
            }
            else
            {
                server.Logger.Log($"Requested file handle for FileHandleSeek: {fileHandle:X06} but it was not found", Logger.Target.console);
            }
        }

        /// <summary>
        /// File Size
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <returns>file size</returns>
        private int FileSize(string fileName)
        {
            if (File.Exists(fileName))
            {
                FileInfo fileInfo = new FileInfo(fileName);
                return (int)fileInfo.Length;
            }
            else
            {
                server.Logger.Log($"FileSize, unable to find file {fileName}, returing -1", Logger.Target.console);
                return -1;
            }
        }
    }
}
