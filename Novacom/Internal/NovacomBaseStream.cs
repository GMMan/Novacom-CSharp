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

namespace Palm.Novacom.Internal
{
    abstract class NovacomBaseStream : INovacomStream
    {
        /// <exception cref="System.IO.IOException" />
        public abstract void Write(byte[] b);

        /// <exception cref="System.IO.IOException" />
        public void Write(byte[] b, int off, int count)
        {
            if (off == 0 && count == b.Length)
            {
                Write(b);
            }
            else
            {
                byte[] shortb = new byte[count];

                Array.Copy(b, off, shortb, 0, count);
                Write(shortb);
            }
        }

        /// <exception cref="System.IO.IOException" />
        public abstract void Write(int b);

        /// <exception cref="System.IO.IOException" />
        public abstract int Read(byte[] b);

        /// <exception cref="System.IO.IOException" />
        public abstract int Read(byte[] b, int minBytesToRead);

        /// <exception cref="System.IO.IOException" />
        public abstract int Read();

        /// <exception cref="System.IO.IOException" />
        public string ReadLine()
        {
            StringBuilder buffer = new StringBuilder();
            int c;
            while ((c = Read()) != '\n')
            {
                /* EOF on Socket causes read() to return -1 */
                if (c == -1)
                {
                    break;
                }
                buffer.Append((char)c);
            }
            if (c != -1)
            {
                buffer.Append((char)c);
            }
            return buffer.ToString();
        }

        /// <exception cref="System.IO.IOException" />
        public long Write(Stream stream)
        {
            if (!stream.CanRead) throw new IOException("Stream cannot be read from.");
            BufferedStream buffstream = new BufferedStream(stream, 1024 * 1024);
            byte[] buffer = new byte[1024 * 16];
            int count;
            long total = 0;
            while (true)
            {
                count = stream.Read(buffer, 0, buffer.Length);
                if (count == 0)
                {
                    break;
                }
                Write(buffer, 0, count);
                total += count;
                if (count < buffer.Length)
                {
                    break;
                }
            }
            return total;
        }

        /// <exception cref="System.IO.IOException" />
        public long Write(FileInfo file)
        {
            return Write(file.OpenRead());
        }

        /// <exception cref="System.IO.IOException" />
        public abstract void Close();

        /// <exception cref="System.IO.IOException" />
        public abstract void CloseInput();

        /// <exception cref="System.IO.IOException" />
        public abstract void CloseOutput();

        /// <exception cref="System.IO.IOException" />
        public abstract int WaitForReturnCode();

        /// <exception cref="System.IO.IOException" />
        public abstract void Flush();

        /// <exception cref="System.IO.IOException" />
        public abstract void SendSignal(int i);

        public abstract bool ReadMightBlock { get; }

        /// <exception cref="System.IO.IOException" />
        public abstract void SendResize(int cols, int rows);
    }
}
