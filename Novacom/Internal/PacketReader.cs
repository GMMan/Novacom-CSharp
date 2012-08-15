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
    class PacketReader
    {
        Stream stream;
        internal PacketHeader packetHeader;
        byte[] packetData;
        int packetIndex;
        List<OOBPacket> oobPackets;

        internal protected PacketReader(Stream stream)
        {
            if (!stream.CanRead) throw new IOException("Stream cannot be read from.");
            this.stream = stream;
            packetHeader = new PacketHeader();
            packetData = new byte[32 * 1024];
            packetIndex = -1;
            oobPackets = new List<OOBPacket>();
        }

        internal void DumpByteArray(string prefix, byte[] b, int length)
        {
            // Note: submission 26 uses NovacomSocketStream's logger, which is not
            // available in .NET. Use old method and write to STDERR instead.
            Console.Error.Write(prefix + ":");
            for (int i = 0; i < length; i++)
            {
                Console.Error.Write(" 0x" + b[i].ToString("x2"));
            }
            Console.Error.WriteLine();
        }

        /// <exception cref="System.IO.IOException" />
        int readArray(byte[] data, int size)
        {
            int count = 0;
            try
            {
                while (count < size)
                {
                    int rc = stream.Read(data, count, size - count);
                    if (rc == 0)
                    {
                        return -1;
                    }
                    count += rc;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.Error.WriteLine("Out of Bounds! -- " + data.Length + "," + size + "," + count);
            }
            return count;
        }

        /// <exception cref="System.IO.IOException" />
        internal protected int ReadPacket()
        {
            packetIndex = -1;
            int rc = readArray(packetHeader.Header, PacketHeader.PACKET_HEADER_BYTES);
            if (rc != PacketHeader.PACKET_HEADER_BYTES)
            {
                return -1;
            }

            if (packetHeader.Magic != PacketHeader.PACKET_HEADER_MAGIC)
            {
                throw new IOException("Bad Magic Number in Packet!!! " + packetHeader.ToString());
            }

            int type = packetHeader.Type;
            switch (type)
            {
                case PacketHeader.PACKET_HEADER_TYPE_ERR:
                case PacketHeader.PACKET_HEADER_TYPE_DATA:
                    packetIndex = 0;
                    readArray(packetData, packetHeader.Size);
                    break;
                case PacketHeader.PACKET_HEADER_TYPE_OOB:
                    OOBPacket oob = new OOBPacket();
                    readArray(oob.PacketData, packetHeader.Size);
                    oobPackets.Add(oob);
                    break;
            }
            return type;
        }

        /// <exception cref="System.IO.IOException" />
        public int Read()
        {
            int rc;
            if (packetIndex == -1 || packetIndex >= packetHeader.Size)
            {
                do
                {
                    rc = ReadPacket();
                    if (rc == -1)
                    {
                        return -1;
                    }
                } while (rc == PacketHeader.PACKET_HEADER_TYPE_OOB);
            }
            if (packetIndex == -1)
            {
                return -1;
            }
            /* Sign extension must be stopped since it really is just a byte of data */
            rc = ((int)packetData[packetIndex] & 0xFF);
            packetIndex++;
            return rc;
        }

        /// <exception cref="System.IO.IOException" />
        public int Read(byte[] b)
        {
            int i;
            if (b == null)
            {
                return -1;
            }
            for (i = 0; i < b.Length; i++)
            {
                int val = Read();
                if (val == -1)
                {
                    break;
                }
                b[i] = (byte)val;
            }
            return i;
        }

        public List<OOBPacket> OOBPackets
        {
            get { return oobPackets; }
        }

        public bool AtPacketStart
        {
            get { return (packetIndex == -1) || (packetIndex >= packetHeader.Size); }
        }
    }
}
