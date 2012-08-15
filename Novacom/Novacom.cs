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

using Palm.Novacom.Internal;

namespace Palm.Novacom
{
    /// <summary>
    /// Factory class for getting a Controller object
    /// </summary>
    public class Novacom
    {
        public enum DiskAreas
        {
            DiskMode,
            Root,
            User,
            SDCard
        };

        public enum DeviceState
        {
            Unknown,
            Bootloader,
            Installer,
            OS
        };

        public const string NOVACOM_VERSION = "Novacom submission 26 (GMWare, from Device Browser bundled source and webOS Doctor disassembly)";

        /// <summary>
        /// The default port that host side novacomd listens on for device lists
        /// </summary>
        public const int DEFAULT_PORT = 6968;

        /// <summary>
        /// The default host to try to connect to for all sockets
        /// </summary>
        public const string DEFAULT_HOST = "127.0.0.1";

        public const int ERROR = -1;
        public const int ERROR_SOCKET_READ = -2;

        /// <summary>
        /// Get a Controller object on a specific host and port combination
        /// </summary>
        /// <param name="host">The host running novacomd</param>
        /// <param name="port">The port novacomd is listening on</param>
        /// <returns>Controller object to work with to get at Devices</returns>
        public static INovacomController GetController(string host, int port)
        {
            return new NovacomController(host, port);
        }

        /// <summary>
        /// Get a Controller object on a specific host with the default port.
        /// </summary>
        /// <param name="host">The host running novacomd on the default port.</param>
        /// <returns>Controller object to work with to get at Devices</returns>
        public static INovacomController GetController(string host)
        {
            return GetController(host, DEFAULT_PORT);
        }

        /// <summary>
        /// Get the 'default' controller.
        /// Uses DEFAULT_PORT and DEFAULT_HOST and calls getController(host, port)
        /// </summary>
        /// <returns>Controller object to work with to get at Devices</returns>
        public static INovacomController GetController()
        {
            return GetController(DEFAULT_HOST, DEFAULT_PORT);
        }
    }
}
