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

namespace Palm.Novacom.Internal
{
    class PacketHeader
    {
        /* Constants for members */
        internal const int PACKET_HEADER_MAGIC = unchecked((int)0xDECAFBAD);
        internal const int PACKET_HEADER_VERSION = 1;

        /* packet types */
        internal const int PACKET_HEADER_TYPE_DATA = 0;
        internal const int PACKET_HEADER_TYPE_ERR = 1;
        internal const int PACKET_HEADER_TYPE_OOB = 2;

        internal const int PACKET_HEADER_BYTES = 16;

        byte[] header;

        public PacketHeader()
        {
            header = new byte[PACKET_HEADER_BYTES];
        }

        public byte[] Header
        {
            get { return header; }
        }

        public byte[] MakeHeader(int size, int type)
        {
            stuffAnInt(PACKET_HEADER_MAGIC, header, 0);
            stuffAnInt(PACKET_HEADER_VERSION, header, 4);
            stuffAnInt(size, header, 8);
            stuffAnInt(type, header, 12);
            return header;
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder("PacketHeader[");
            buffer.Append("magic 0x" + Magic.ToString("x"));
            buffer.Append(", version 0x" + Version.ToString("x"));
            buffer.Append(", size 0x" + Size.ToString("x"));
            buffer.Append(", type 0x" + Type.ToString("x"));
            buffer.Append("]");
            return buffer.ToString();
        }

        int makeAnInt(int i1, int i2, int i3, int i4)
        {
            i1 = i1 & 0xFF;
            i2 = i2 & 0xFF;
            i3 = i3 & 0xFF;
            i4 = i4 & 0xFF;
            return ((i1 << 0) | (i2 << 8) | (i3 << 16) | (i4 << 24));
        }

        void stuffAnInt(int val, byte[] array, int offset)
        {
            array[offset + 0] = (byte)((val >> 0) & 0xFF);
            array[offset + 1] = (byte)((val >> 8) & 0xFF);
            array[offset + 2] = (byte)((val >> 16) & 0xFF);
            array[offset + 3] = (byte)((val >> 24) & 0xFF);
        }

        public int Magic
        {
            get { return makeAnInt(header[0], header[1], header[2], header[3]); }
        }

        public int Version
        {
            get { return makeAnInt(header[4], header[5], header[6], header[7]); }
        }

        public int Size
        {
            get { return makeAnInt(header[8], header[9], header[10], header[11]); }
        }

        public int Type
        {
            get { return makeAnInt(header[12], header[13], header[14], header[15]); }
        }
    }
}
