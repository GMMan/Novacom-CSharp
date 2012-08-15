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
using System.Security.Cryptography;

namespace Palm.Novacom.Internal
{
    class NovacomDevice : INovacomDevice
    {
        enum HashCalcType
        {
            HashCalc_FullCycle,
            HashCalc_ReuseNP
        };

        static readonly Encoding ENCODING = Encoding.ASCII;
        readonly string host;
        string hashNP;
        string hashNPS;
        NovaDeviceInfo devInfo;

        internal NovacomDevice(string host, NovaDeviceInfo devInfo)
        {
            this.host = host;
            this.devInfo = devInfo;
        }

        public void Close()
        { }

        public string Host
        {
            get { return host; }
        }

        public int Port
        {
            get { return devInfo.Port; }
        }

        public NovaDeviceInfo DeviceInfo
        {
            get { return devInfo; }
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        string socketReadLine(TcpClient socket)
        {
            Stream input = socket.GetStream();
            if (!input.CanRead) throw new IOException("Stream cannot be read from.");
            byte[] reply = new byte[256];
            int c;
            int length;

            /* Read up to the newline, and nothing more -- data comes right after */
            for (length = 0; length < reply.Length; )
            {
                c = input.ReadByte();
                if (c == -1)
                {
                    throw new NovacomException(Novacom.ERROR_SOCKET_READ, NovacomException.EXIT_IO, "No data to read from socket");
                }
                reply[length] = (byte)c;
                length++;
                if (c == '\n')
                {
                    break;
                }
            }

            return Encoding.Default.GetString(reply, 0, length);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        bool HandleReply(TcpClient socket)
        {
            string strReply = socketReadLine(socket);

            /* Check to see if the reply is "ok" */
            if (strReply.StartsWith("ok"))
            {
                return true;
            }
            if (strReply.StartsWith("err"))
            {
                return false;
            }
            if (strReply.StartsWith("req:auth"))
            {
                throw new NovacomException(-13, NovacomException.EXIT_ACCESS, "Access denied");
            }
            
            /* Not ok, then we have an error message to toss */
            throw new NovacomException(-71, NovacomException.EXIT_IO, "Unsupported reply: " + strReply);
        }

        string prepareCommand(string verb, string scheme, params string[] args)
        {
            StringBuilder cmd = new StringBuilder(verb);
            cmd.Append(" ");
            cmd.Append(scheme);
            if (args != null)
            {
                foreach (string arg in args)
                {
                    cmd.Append(" ");
                    cmd.Append(arg);
                }
            }
            cmd.Append("\n");
            return cmd.ToString();
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        TcpClient sendCommand(int port, string cmd)
        {
            TcpClient socket = null;
            try
            {
                socket = new TcpClient(host, port);
                Stream output = socket.GetStream();
                byte[] outBytes = ENCODING.GetBytes(cmd.ToString());
                output.Write(outBytes, 0, outBytes.Length);
            }
            catch (Exception e)
            {
                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }

                if (!IsConnected)
                {
                    throw new NovacomException(-107, NovacomException.EXIT_CONNECT, "Device has been disconnected. Command: " + cmd.Replace("\n", ""), e);
                }
                throw e;
            }

            return socket;
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        internal INovacomStream IssueCommand(string verb, string scheme, params string[] args)
        {
            string cmd = prepareCommand(verb, scheme, args);
            // Console.Error.WriteLine("Cmd: " + cmd);
            TcpClient socket = sendCommand(devInfo.Port, cmd);
            try
            {
                bool result = HandleReply(socket);
                if (!result)
                {
                    throw new NovacomException(-74, NovacomException.EXIT_IO, "Command processing error...");
                }
            }
            catch (NovacomException e)
            {
                socket.Close();
                throw new NovacomException(e.ErrorCode, e.ExitCode, "Command: " + cmd.Replace("\n", "") + ". Reply: " + e.Message.Replace("\n", ""));
            }
            return new NovacomSocketStream(socket, scheme);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        bool issueCtrlCommand(string verb, string scheme, params string[] args)
        {
            if (devInfo.SessionId == null)
            {
                return false;
            }

            string cmd = prepareCommand(verb, scheme, args);

            TcpClient socket = sendCommand(devInfo.CtrlPort, cmd);
            bool result;
            try
            {
                result = HandleReply(socket);
            }
            catch (NovacomException e)
            {
                socket.Close();
                throw new NovacomException(e.ErrorCode, e.ExitCode, "Command: " + cmd.Replace("\n", "") + ". Reply: " + e.Message.Replace("\n", ""));
            }
            return result;
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public INovacomStream GetFile(string path)
        {
            return IssueCommand("get", "file://" + path, null);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public INovacomStream PutFile(string path)
        {
            return IssueCommand("put", "file://" + path, null);
        }

        public bool CheckFile(string path)
        {
            try
            {
                GetFile(path);
                return true;
            }
            catch (NovacomException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public INovacomStream OpenTerminal(int tty)
        {
            return IssueCommand("open", "tty://" + tty, null);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public INovacomStream RunProgram(string path, params string[] args)
        {
            return IssueCommand("run", "file://" + path, args);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public void RunProgramAndWait(string path, params string[] args)
        {
            INovacomStream stream = RunProgram(path, args);
            stream.WaitForReturnCode();
            stream.Close();
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public INovacomStream RunScript(string script, string path)
        {
            INovacomStream stream = PutFile(path);
            stream.Write(ENCODING.GetBytes(script));
            stream.CloseInput();
            stream.CloseOutput();
            stream.WaitForReturnCode();
            stream.Close();

            return RunProgram("/bin/sh", "-c", path);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public INovacomStream PutInMemory(long address, bool boot)
        {
            return PutInMemory(address.ToString("x"), boot);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public INovacomStream PutInMemory(string address, bool boot)
        {
            return IssueCommand(boot ? "boot" : "put", "mem://" + address, null);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public INovacomStream ConnectDevicePort(int port)
        {
            return IssueCommand("connect", "tcp-port://" + port, null);
        }

        /// <exception cref="System.IO.IOException" />
        public Novacom.DeviceState State
        {
            get
            {
                INovacomStream stream = null;
                /*
                 * Try to open a file that the bootloader won't support.
                 */
                try
                {
                    stream = GetFile("/proc/mounts");
                    try
                    {
                        stream.Close();
                    }
                    catch (Exception)
                    { }

                    /*
                     * try to determine if os was booted from ram (installer)
                     */
                    stream = GetFile("/proc/cmdline");
                    string cmdline = stream.ReadLine();
                    if (cmdline.IndexOf("ram0") > 0)
                    {
                        return Novacom.DeviceState.Installer;
                    }
                    return Novacom.DeviceState.OS;
                }
                catch (NovacomException e)
                {
                    if (e.ErrorCode == -13)
                    {
                        return Novacom.DeviceState.Unknown;
                    }
                    return Novacom.DeviceState.Bootloader;
                }
                finally
                {
                    if (stream != null)
                    {
                        try
                        {
                            stream.Close();
                        }
                        catch (Exception)
                        { }
                    }
                }
            }
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public void KillConnection()
        {
            Novacom.DeviceState deviceState = State;
            if (deviceState == Novacom.DeviceState.OS || deviceState == Novacom.DeviceState.Installer)
            {
                /* novacomd won't be restarted on a SIGTERM */
                try
                {
                    RunProgram("/usr/bin/killall", "-15", "novacomd");
                }
                catch (NovacomException e)
                {
                    /* We expect this socket read error */
                    if (e.ErrorCode != Novacom.ERROR_SOCKET_READ)
                    {
                        throw;
                    }
                    return;
                }
            }
            else
            {
                throw new NovacomException(Novacom.ERROR, NovacomException.EXIT_RAMDISK_ERR, "Cannot kill connection in anything but OS mode.");
            }
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public void WaitForDeviceToAppear()
        {
            WaitForDeviceToAppear(3 * 60 * 1000); // 3 minute
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public void WaitForDeviceToAppear(int timeout)
        {
            INovacomDevice newdev = null;
            INovacomController controller = Novacom.GetController(host, Novacom.DEFAULT_PORT);
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            TimeSpan timeToTimeoutOn = TimeSpan.FromMilliseconds(timeout);
            stopwatch.Start();
            /* Loop until we find a new device show up */
            do
            {
                NovaDeviceInfo[] devices = controller.GetDeviceList();
                foreach (NovaDeviceInfo device in devices)
                {
                    if (device.UIDString == devInfo.UIDString && device.Port != devInfo.Port)
                    {
                        newdev = controller.ConnectToDevice(device);
                        break;
                    }
                }
                System.Threading.Thread.Sleep(250);
                if (stopwatch.Elapsed > timeToTimeoutOn)
                {
                    throw new NovacomException(Novacom.ERROR, NovacomException.EXIT_NO_DEVICE, "Timeout waiting for device to appear");
                }
            } while (newdev == null);

            /* Now clone ourselves to be the same as the new device info */
            this.devInfo = newdev.DeviceInfo;

            if ((devInfo.SessionId != null) && (hashNP != null))
            {
                calcHash(null, HashCalcType.HashCalc_ReuseNP);
            }
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public bool IsConnected
        {
            get
            {
                bool devConnected = false;
                INovacomController controller = Novacom.GetController(host, Novacom.DEFAULT_PORT);

                NovaDeviceInfo[] devices = controller.GetDeviceList();
                foreach (NovaDeviceInfo device in devices)
                {
                    if (device.UIDString == devInfo.UIDString && device.Port == devInfo.Port)
                    {
                        devConnected = true;
                        break;
                    }
                }

                return devConnected;
            }
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        internal void StreamToFile(string hostFileName, INovacomStream deviceStream)
        {
            FileStream fileStream = new FileStream(hostFileName, FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[1024 * 1024];

            long total = 0;
            while (true)
            {
                int count = deviceStream.Read(buffer);
                if (count == -1)
                {
                    break;
                }
                fileStream.Write(buffer, 0, count);
                total += count;
                if (count < buffer.Length)
                {
                    break;
                }
            }
            fileStream.Close();
        }

        string bytesToHexStr(byte[] hash)
        {
            StringBuilder hex = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                hex.Append(hash[i].ToString("x2"));
            }
            return hex.ToString();
        }

        /// <exception cref="Palm.Novacom.NovacomException" />
        string calcHash(string devicePass, HashCalcType calcType)
        {
            if (calcType == HashCalcType.HashCalc_FullCycle)
            {
                if (devicePass == null)
                {
                    throw new NovacomException(-22, "Empty password string...");
                }
            }
            else if (hashNP == null)
            {
                throw new NovacomException(-22, "Invalid call...");
            }

            if (devInfo.SessionId == null)
            {
                throw new NovacomException(-56, "Restricted access mode is not active...");
            }

            SHA1 sha = new SHA1CryptoServiceProvider();

            if (calcType == HashCalcType.HashCalc_FullCycle)
            {
                string str = devInfo.UIDString.ToLower() + devicePass;

                byte[] hash = sha.ComputeHash(Encoding.Default.GetBytes(str));
                hashNP = bytesToHexStr(hash);
            }

            string strs = hashNP + devInfo.SessionId;

            byte[] hashs = sha.ComputeHash(Encoding.Default.GetBytes(strs));
            return hashNPS = bytesToHexStr(hashs);
        }

        /// <exception cref="Palm.Novacom.NovacomException" />
        public void DevPass(string devicePass)
        {
            calcHash(devicePass, HashCalcType.HashCalc_FullCycle);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public bool DevLogin(string devicePass)
        {
            if ((devicePass == null) && (hashNPS == null))
            {
                return false;
            }
            if (devicePass != null)
            {
                calcHash(devicePass, HashCalcType.HashCalc_FullCycle);
            }
            return issueCtrlCommand("login", "dev://" + devInfo.UIDString, hashNPS);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public bool DevLogout()
        {
            return issueCtrlCommand("logout", "dev://" + devInfo.UIDString, null);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public bool DevAddToken(string devicePass)
        {
            if ((devicePass == null) && (hashNPS == null))
            {
                return false;
            }
            if (devicePass != null)
            {
                calcHash(devicePass, HashCalcType.HashCalc_FullCycle);
            }
            return issueCtrlCommand("add", "dev://" + devInfo.UIDString, hashNPS);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public bool DevRmToken(string devicePass)
        {
            if ((devicePass == null) && (hashNPS == null))
            {
                return false;
            }
            if (devicePass != null)
            {
                calcHash(devicePass, HashCalcType.HashCalc_FullCycle);
            }
            return issueCtrlCommand("remove", "dev://" + devInfo.UIDString, hashNPS);
        }

        public override string ToString()
        {
            return DeviceInfo.ToString();
        }
    }
}
