/*
*
*      Copyright (c) 2012 Yukai Li
*      Copyright (c) 2008-2012 Hewlett-Packard Development Company, L.P.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Palm.Novacom
{
    /// <summary>
    /// An interface to be used to read/write data from a stream.
    /// If reads or writes are meaningful depends on how the stream was created.
    /// </summary>
    /// <see cref="Palm.Novacom.INovacomDevice"/>
    public interface INovacomStream
    {
        /// <summary>
        /// Cleans up any resources associated with this stream.
        /// </summary>
        /// <exception cref="System.IO.IOException" />
        void Close();

        /// <summary>
        /// Close the input stream from the other side of the connection.
        /// </summary>
        /// <exception cref="System.IO.IOException" />
        void CloseInput();

        /// <summary>
        /// Close the output stream to the other side of the connection (gives an EOF to the spawned process)
        /// </summary>
        /// <exception cref="System.IO.IOException" />
        void CloseOutput();

        /// <summary>
        /// Wait for the operation to complete and get back the status code.
        /// In the case of a spawned process this is the return code of the program.
        /// </summary>
        /// <returns>The return code of the operation performed</returns>
        /// <exception cref="System.IO.IOException" />
        int WaitForReturnCode();

        /// <exception cref="System.IO.IOException" />
        void Flush();

        /// <summary>
        /// Return the next byte of the stream
        /// </summary>
        /// <returns>The next byte, or -1 if the stream is EOF</returns>
        /// <exception cref="System.IO.IOException" />
        int Read();

        /// <exception cref="System.IO.IOException" />
        int Read(byte[] b, int minBytesToRead);

        /// <summary>
        /// Return the next 'n' bytes of the stream
        /// </summary>
        /// <param name="b">Array to hold data, whose .Length becomes 'n'</param>
        /// <returns>The actual number of bytes read into b</returns>
        /// <exception cref="System.IO.IOException" />
        int Read(byte[] b);
        /// <summary>
        /// Read a line of data from the stream
        /// Only useful if the stream is ASCII text (like a terminal or program being run)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.IO.IOException" />
        string ReadLine();
        /// <summary>
        /// Write a byte of data into the stream
        /// </summary>
        /// <param name="b">The byte to write</param>
        /// <exception cref="System.IO.IOException" />
        void Write(int b);
        /// <summary>
        /// Write a series of bytes into the stream
        /// </summary>
        /// <param name="b">Array of bytes to write</param>
        /// <exception cref="System.IO.IOException" />
        void Write(byte[] b);
        /// <summary>
        /// Write bytes into the stream from a given offset and count
        /// </summary>
        /// <param name="b">Array of byte to write</param>
        /// <param name="off">Offset into the array to start reading from</param>
        /// <param name="count">Number of bytes to write</param>
        /// <exception cref="System.IO.IOException" />
        void Write(byte[] b, int off, int count);
        /// <summary>
        /// Write the bytes from the given file into the stream.
        /// </summary>
        /// <param name="file">The file to stream</param>
        /// <returns>Number of bytes written into the stream</returns>
        /// <exception cref="System.IO.IOException" />
        long Write(FileInfo file);
        /// <exception cref="System.IO.IOException" />
        long Write(Stream stream);

        /// <exception cref="System.IO.IOException" />
        void SendSignal(int i);

        bool ReadMightBlock { get; }

        /// <exception cref="System.IO.IOException" />
        void SendResize(int cols, int rows);
    }
}
