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

    /// <summary>
    /// Class to load nabu files on the local machine
    /// </summary>
    class LocalLoader : ILoader
    {
        /// <summary>
        /// Try to get the contents of the nabu file located at the specified path
        /// </summary>
        /// <param name="path">Path to nabu file</param>
        /// <param name="data">contents of file</param>
        /// <returns>returns true/false if successful or not</returns>
        public bool TryGetData(string path, out byte[] data)
        {
            data = null;

            try
            {
                data = File.ReadAllBytes(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Try to get the parent directory of the specified file
        /// </summary>
        /// <param name="path">Path to get the parent directory</param>
        /// <param name="directoryPath">Parent directory path</param>
        /// <returns>returns true/false if successful or not</returns>
        public bool TryGetDirectory(string path, out string directoryPath)
        {
            directoryPath = string.Empty;

            try
            {
                directoryPath = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
