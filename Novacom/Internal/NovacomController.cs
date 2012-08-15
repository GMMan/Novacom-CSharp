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
    class NovacomController : INovacomController
    {
        readonly string host;
        readonly int port;

        public NovacomController(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public INovacomDevice ConnectDefaultDevice()
        {
            NovaDeviceInfo[] devices = GetDeviceList();
            if (devices.Length <= 0)
            {
                throw new NovacomException(Novacom.ERROR, NovacomException.EXIT_NO_DEVICE, "No devices attached on " + host + "@" + port);
            }
            return ConnectToDevice(devices[0]);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public INovacomDevice ConnectToDevice(NovaDeviceInfo devInfo)
        {
            if (devInfo == null)
            {
                throw new NovacomException(-22, NovacomException.EXIT_NO_DEVICE, "Invalid device info(null)");
            }
            return new NovacomDevice(host, devInfo);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public NovaDeviceInfo[] GetDeviceList()
        {
            TcpClient socket = new TcpClient(host, port);
            NovacomSocketStream stream = new NovacomSocketStream(socket, "Device List Stream");
            stream.IsPacketMode = false;
            try
            {
                NovacomDeviceList deviceList = new NovacomDeviceList(stream);
                NovaDeviceInfo[] list = deviceList.Devices;
                return list;
            }
            finally
            {
                try
                {
                    stream.Close();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
            }
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        NovaDeviceInfo[] getDeviceListOnTransport(string transport)
        {
            NovaDeviceInfo[] list = GetDeviceList();
            List<NovaDeviceInfo> arrayList = new List<NovaDeviceInfo>(list.Length);
            if (list.Length > 0)
            {
                foreach (NovaDeviceInfo device in list)
                {
                    if (device.Transport == transport)
                    {
                        arrayList.Add(device);
                    }
                }
            }
            return arrayList.ToArray();
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public NovaDeviceInfo[] GetDeviceListOnUsb()
        {
            return getDeviceListOnTransport("usb");
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        public NovaDeviceInfo[] GetDeviceListOnTcp()
        {
            return getDeviceListOnTransport("tcp");
        }
    }
}
