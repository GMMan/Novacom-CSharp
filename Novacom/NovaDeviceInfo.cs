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
using System.Globalization;

namespace Palm.Novacom
{
    public class NovaDeviceInfo
    {
        protected int port;
        protected byte[] uid;
        protected string uidString;
        protected string transport;
        protected string name;
        protected string ssid;
        public const int DEFAULT_CTRL_PORT = 6971;
        protected int ctrlPort = DEFAULT_CTRL_PORT;

        public NovaDeviceInfo(string info)
        {
            string[] tokens = info.Split();
            port = int.Parse(tokens[0]);
            uid = new byte[20];
            tokens[1] = tokens[1].ToUpper();
            System.Diagnostics.Debug.Assert(tokens[1].Length == 40);
            uidString = tokens[1];
            try
            {
                for (int i = 0; i < uid.Length; i++)
                {
                    // As per http://stackoverflow.com/a/8235530/1180879
                    uid[i] = byte.Parse(uidString.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            transport = tokens[2];
            name = tokens[3];

            if (tokens.Length > 4)
            {
                this.ssid = tokens[4];
            }
        }

        public string Transport
        {
            get { return transport; }
        }

        public byte[] UID
        {
            get { return uid; }
        }

        public string UIDString
        {
            get { return uidString; }
        }

        public int Port
        {
            get { return port; }
        }

        public int CtrlPort
        {
            get { return ctrlPort; }
        }

        public string Name
        {
            get { return name; }
        }

        public string MachineName
        {
            get { return name.Split('-')[0]; }
        }

        public string SessionId
        {
            get { return ssid; }
        }

        public override string ToString()
        {
            return UIDString + " (" + Transport + ", " + Name + ")";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NovaDeviceInfo)) return false;
            return uidString == (obj as NovaDeviceInfo).UIDString;
        }

        public override int GetHashCode()
        {
            return uidString.GetHashCode();
        }
    }
}
