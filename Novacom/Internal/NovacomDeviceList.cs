﻿/*
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
    class NovacomDeviceList
    {
        List<NovaDeviceInfo> devices;

        /// <exception cref="System.IO.IOException" />
        internal protected NovacomDeviceList(INovacomStream stream)
        {
            string line;
            devices = new List<NovaDeviceInfo>();
            while (true)
            {
                line = stream.ReadLine();
                if (line == string.Empty)
                {
                    break;
                }
                devices.Add(new NovaDeviceInfo(line));
            }
        }

        internal int DeviceCount
        {
            get { return devices.Count; }
        }

        internal NovaDeviceInfo[] Devices
        {
            get { return devices.ToArray(); }
        }
    }
}
