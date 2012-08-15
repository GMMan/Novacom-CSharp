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
    class PacketWriter
    {
        Stream stream;
        internal PacketHeader packetHeader;

        internal protected PacketWriter(Stream stream)
        {
            if (!stream.CanWrite) throw new IOException("Stream cannot be written to.");
            this.stream = stream;
            packetHeader = new PacketHeader();
        }

        internal void StuffAnInt(int val, byte[] array, int offset)
        {
            array[offset + 0] = (byte)((val >> 0) & 0xFF);
            array[offset + 1] = (byte)((val >> 8) & 0xFF);
            array[offset + 2] = (byte)((val >> 16) & 0xFF);
            array[offset + 3] = (byte)((val >> 24) & 0xFF);
        }

        /// <exception cref="System.IO.IOException" />
        internal void WriteOutOfBand(byte[] data)
        {
            byte[] header = packetHeader.MakeHeader(data.Length, PacketHeader.PACKET_HEADER_TYPE_OOB);
            stream.Write(header, 0, header.Length);
            stream.Write(data, 0, data.Length);
        }

        /// <exception cref="System.IO.IOException" />
        internal void Write(byte[] data)
        {
            byte[] header = packetHeader.MakeHeader(data.Length, PacketHeader.PACKET_HEADER_TYPE_DATA);
            stream.Write(header, 0, header.Length);
            stream.Write(data, 0, data.Length);
        }

        /// <exception cref="System.IO.IOException" />
        internal void Write(int b)
        {
            Write(new byte[] { (byte)(b & 0xFF) });
        }
    }
}
