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
    class OOBPacket
    {
        /* OOB messages */
        internal const int PACKET_OOB_EOF = 0;
        internal const int PACKET_OOB_SIGNAL = 1;
        internal const int PACKET_OOB_RETURN = 2;
        internal const int PACKET_OOB_RESIZE = 3;

        internal const int STDIN_FILENO = 0;
        internal const int STDOUT_FILENO = 1;
        internal const int STDERR_FILENO = 2;

        internal const int PACKET_OOB_BYTES = 20;

        /*
         * This is the C-style structure for the OOB messages
         */

        /*
            struct packet_oob_msg {
                uint32_t message;
                union {
                    uint32_t signo;
                    int32_t returncode;
                    int32_t fileno;
                    struct {
                        uint32_t rows;
                        uint32_t cols;
                    } resize;
                    uint32_t pad[4];
                } data;
            };
        */

        byte[] packetData;

        public OOBPacket()
        {
            packetData = new byte[PACKET_OOB_BYTES];
        }

        internal byte[] PacketData
        {
            get { return packetData; }
        }

        int makeAnInt(int i1, int i2, int i3, int i4)
        {
            i1 = i1 & 0xFF;
            i2 = i2 & 0xFF;
            i3 = i3 & 0xFF;
            i4 = i4 & 0xFF;
            return ((i1 << 0) | (i2 << 8) | (i3 << 16) | (i4 << 24));
        }

        public int MessageType
        {
            get { return makeAnInt(packetData[0], packetData[1], packetData[2], packetData[3]); }
        }

        public int MessagePayload
        {
            get { return makeAnInt(packetData[4], packetData[5], packetData[6], packetData[7]); }
        }

        static void stuffAnInt(int val, byte[] array, int offset)
        {
            array[offset + 0] = (byte)((val >> 0) & 0xFF);
            array[offset + 1] = (byte)((val >> 8) & 0xFF);
            array[offset + 2] = (byte)((val >> 16) & 0xFF);
            array[offset + 3] = (byte)((val >> 24) & 0xFF);
        }

        public static byte[] CloseFilenoHeader(int fileno)
        {
            byte[] oob = new byte[PACKET_OOB_BYTES];
            stuffAnInt(PACKET_OOB_EOF, oob, 0);
            stuffAnInt(fileno, oob, 4);
            return oob;
        }

        public static byte[] SendSignal(int signo)
        {
            byte[] oob = new byte[PACKET_OOB_BYTES];
            stuffAnInt(PACKET_OOB_SIGNAL, oob, 0);
            stuffAnInt(signo, oob, 4);
            return oob;
        }

        public static byte[] SendResize(int cols, int rows)
        {
            byte[] oob = new byte[PACKET_OOB_BYTES];
            stuffAnInt(PACKET_OOB_RESIZE, oob, 0);
            stuffAnInt(rows, oob, 4);
            stuffAnInt(cols, oob, 8);
            return oob;
        }
    }
}
