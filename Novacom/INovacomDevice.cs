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
    public interface INovacomDevice
    {
        void Close();
        /// <summary>
        /// Get a file from the device's file system
        /// </summary>
        /// <param name="path">The path of the file. ie: /var/log/messages</param>
        /// <returns>A stream object to read the file contents</returns>
        /// <exception cref="Palm.Novacom.NovacomException" />
        /// <exception cref="System.IO.IOException" />
        INovacomStream GetFile(string path);
        /// <summary>
        /// Put a file onto the device's file system
        /// </summary>
        /// <param name="path">The path of the file you want to write.  ie: /tmp/stuff</param>
        /// <returns>A stream object to write the file contents</returns>
        /// <exception cref="Palm.Novacom.NovacomException" />
        /// <exception cref="System.IO.IOException" />
        INovacomStream PutFile(string path);

        bool CheckFile(string path);

        /// <summary>
        /// Open a device side terminal session (like rsh)
        /// </summary>
        /// <param name="tty">The terminal number to open (0..3)</param>
        /// <returns>A stream object that you can read and write as a byte stream to the session</returns>
        /// <exception cref="Palm.Novacom.NovacomException" />
        /// <exception cref="System.IO.IOException" />
        INovacomStream OpenTerminal(int tty);
        /// <summary>
        /// Run something on the device and and wire stdout/stdin into the returned stream.
        /// </summary>
        /// <param name="path">The program to run.  ie: /bin/ls</param>
        /// <param name="args">The arguments to pass to the program being run.  ie: "-l", "-c", "-F"</param>
        /// <returns>A stream object that you can read and write for the stdout and stdin of the remote process</returns>
        /// <exception cref="Palm.Novacom.NovacomException" />
        /// <exception cref="System.IO.IOException" />
        INovacomStream RunProgram(string path, params string[] args);

        /// <exception cref="Palm.Novacom.NovacomException" />
        /// <exception cref="System.IO.IOException" />
        void RunProgramAndWait(string path, params string[] args);

        /// <exception cref="Palm.Novacom.NovacomException" />
        /// <exception cref="System.IO.IOException" />
        INovacomStream RunScript(string script, string path);

        /// <summary>
        /// Put a fixed length stream of data into a specific address location.
        /// This is a bootloader (bootie) only command.  This will fail if the system is running Linux.
        /// </summary>
        /// <param name="address">Location to load data</param>
        /// <param name="boot">"true" if we want to boot the system at the given address once the last byte is sent</param>
        /// <returns></returns>
        /// <exception cref="Palm.Novacom.NovacomException" />
        /// <exception cref="System.IO.IOException" />
        INovacomStream PutInMemory(long address, bool boot);
        /// <summary>
        /// Put a fixed length stream of data into a specific address location.
        /// This is a bootloader (bootie) only command.  This will fail if the system is running Linux.
        /// </summary>
        /// <param name="address">Location to load data</param>
        /// <param name="boot">boot "true" if we want to boot the system at the given address once the last byte is sent</param>
        /// <returns></returns>
        /// <exception cref="Palm.Novacom.NovacomException" />
        /// <exception cref="System.IO.IOException" />
        INovacomStream PutInMemory(string address, bool boot);

        /// <exception cref="Palm.Novacom.NovacomException" />
        /// <exception cref="System.IO.IOException" />
        INovacomStream ConnectDevicePort(int port);

        /// <exception cref="System.IO.IOException" />
        Novacom.DeviceState State { get; }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        void KillConnection();

        NovaDeviceInfo DeviceInfo { get; }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        void WaitForDeviceToAppear();

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        void WaitForDeviceToAppear(int timeout);

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        bool IsConnected {
            get;
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        void DevPass(string devicePass);

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        bool DevLogin(string devicePass);

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        bool DevLogout();

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        bool DevAddToken(string devicePass);

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        bool DevRmToken(string devicePass);
    }
}
