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
using System.Net.Sockets;

namespace Palm.Novacom.Internal
{
    class NovacomSocketStream : NovacomBaseStream
    {
        TcpClient socket;
        Stream ioStream;
        PacketReader packetReader;
        PacketWriter packetWriter;
        bool packetMode;
        bool outputClosed;
        bool inputClosed;
        readonly string cmdUsedForStream;

        /// <exception cref="System.IO.IOException" />
        internal NovacomSocketStream(TcpClient socket, string cmdUsedForStream)
        {
            this.socket = socket;
            ioStream = this.socket.GetStream();
            packetReader = new PacketReader(ioStream);
            packetWriter = new PacketWriter(ioStream);
            packetMode = true;
            outputClosed = false;
            inputClosed = false;
            this.cmdUsedForStream = cmdUsedForStream;
        }

        /// <exception cref="System.IO.IOException" />
        public override void CloseOutput()
        {
            if (packetMode && !outputClosed)
            {
                try
                {
                    packetWriter.WriteOutOfBand(OOBPacket.CloseFilenoHeader(OOBPacket.STDIN_FILENO));
                }
                catch (IOException e)
                {
                    logWarning("Handled Exception Cmd: " + cmdUsedForStream + "\n" + e);
                }
                outputClosed = true;
            }
        }

        /// <exception cref="System.IO.IOException" />
        public override void CloseInput()
        {
            if (packetMode && !inputClosed)
            {
                try
                {
                    packetWriter.WriteOutOfBand(OOBPacket.CloseFilenoHeader(OOBPacket.STDOUT_FILENO));
                    packetWriter.WriteOutOfBand(OOBPacket.CloseFilenoHeader(OOBPacket.STDERR_FILENO));
                }
                catch (IOException e)
                {
                    logWarning("Handled Exception Cmd: " + cmdUsedForStream + "\n" + e);
                }
                inputClosed = true;
            }
        }

        void logWarning(string msg)
        {
            // Logging stub, output is to STDERR
            Console.Error.WriteLine(msg);
        }

        /// <exception cref="System.IO.IOException" />
        public override void Close()
        {
            socket.Close();
        }

        /// <exception cref="System.IO.IOException" />
        public override int WaitForReturnCode()
        {
            /* Read data until there is no interesting data left to read */
            while (true)
            {
                try
                {
                    int rc = packetReader.ReadPacket();
                    if (rc < 0)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    logWarning("Handled Exception Cmd: " + cmdUsedForStream + "\n" + e);
                    break;
                }
            }
            /* Iterate over all the OOB packets found in this session */
            List<OOBPacket> oobPackets = packetReader.OOBPackets;
            IEnumerator<OOBPacket> itr = oobPackets.GetEnumerator();
            while (itr.MoveNext())
            {
                OOBPacket oob = itr.Current;
                if (oob.MessageType == OOBPacket.PACKET_OOB_RETURN)
                {
                    /* Don't damage the state of the oobPackets, so we can call this function again */
                    return oob.MessagePayload;
                }
            }
            /* Since -1 is a valid return code for the process let's make our error case an exception */
            IOException ee = new IOException("No return code found in stream from " + cmdUsedForStream);
            logWarning("Throwing Exception Cmd: " + cmdUsedForStream + "\n" + ee);
            throw ee;
        }

        /// <exception cref="System.IO.IOException" />
        public override void Flush()
        {
            ioStream.Flush();
        }

        /// <exception cref="System.IO.IOException" />
        public override int Read()
        {
            try
            {
                if (IsPacketMode)
                {
                    return packetReader.Read();
                }
                else
                {
                    return ioStream.ReadByte();
                }
            }
            catch (IOException e)
            {
                handleSocketException(e);
            }
            return -1;
        }

        /// <exception cref="System.IO.IOException" />
        public override int Read(byte[] b, int minBytesToRead)
        {
            if (b == null)
            {
                return -1;
            }
            int i;
            for (i = 0; (i < b.Length) &&
                ((i < minBytesToRead) ||
                (!ReadMightBlock)); i++)
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

        /// <exception cref="System.IO.IOException" />
        public override int Read(byte[] b)
        {
            try
            {
                if (IsPacketMode)
                {
                    return packetReader.Read(b);
                }
                else
                {
                    return ioStream.Read(b, 0, b.Length);
                }
            }
            catch (IOException e)
            {
                handleSocketException(e);
            }
            return -1;
        }

        /// <exception cref="System.IO.IOException" />
        public override void Write(int b)
        {
            try
            {
                if (packetMode)
                {
                    packetWriter.Write(b);
                }
                else
                {
                    ioStream.WriteByte((byte)b);
                }
            }
            catch (IOException e)
            {
                handleSocketException(e);
            }
        }

        /// <exception cref="System.IO.IOException" />
        public override void Write(byte[] b)
        {
            try
            {
                if (packetMode)
                {
                    packetWriter.Write(b);
                }
                else
                {
                    ioStream.Write(b, 0, b.Length);
                }
            }
            catch (IOException e)
            {
                handleSocketException(e);
            }
        }

        /// <exception cref="System.IO.IOException" />
        void handleSocketException(IOException e)
        {
            // This method makes no sense in C#...
            logWarning("SocketException Cmd: " + cmdUsedForStream);
            if (e.Message.IndexOf("broken pipe", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                IOException e2;
                try
                {
                    int retCode = WaitForReturnCode();
                    e2 = new IOException("Broken pipe from " + cmdUsedForStream + ", return code: " + retCode, e);
                }
                catch (IOException eIO)
                {
                    e2 = new IOException("Broken pipe from " + cmdUsedForStream, eIO);
                }
                e = e2;
            }
            throw e;
        }

        public bool IsPacketMode
        {
            get { return packetMode; }
            set { packetMode = value; }
        }

        public override bool ReadMightBlock
        {
            get { return (!packetMode) || (packetReader.AtPacketStart); }
        }

        /// <exception cref="System.IO.IOException" />
        public override void SendSignal(int i)
        {
            if (!packetMode)
            {
                throw new IOException();
            }
            try
            {
                packetWriter.WriteOutOfBand(OOBPacket.SendSignal(i));
            }
            catch (SocketException e)
            {
                logWarning("Exception Returned from Out of Band Write.  Could be pipe closed too quickly\n" + e);
            }
        }

        /// <exception cref="System.IO.IOException" />
        public override void SendResize(int cols, int rows)
        {
            if (!packetMode)
            {
                throw new IOException();
            }
            try
            {
                packetWriter.WriteOutOfBand(OOBPacket.SendResize(cols, rows));
            }
            catch (SocketException e)
            {
                logWarning("Exception Returned from Out of Band Write.  Could be pipe closed too quickly\n" + e);
            }
        }
    }
}
