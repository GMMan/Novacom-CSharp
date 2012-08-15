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

namespace Palm.Novacom
{
    public class NovacomException : Exception
    {
        static readonly long serialVersionUID = 0xDEADBEEF;
        public const int EXIT_OK = 0;
        public const int EXIT_COMMAND_ERROR = 2;
        public const int EXIT_UNKNOWN = 3;
        public const int EXIT_XML = 4;
        public const int EXIT_CONNECT = 5;
        public const int EXIT_IO = 6;
        public const int EXIT_ACCESS = 7;
        public const int EXIT_COMPONENT_NOT_FOUND = 10;
        public const int EXIT_COMPONENT_NOT_ON_SERVER = 11;
        public const int EXIT_CUST_NOT_ON_SERVER = 13;
        public const int EXIT_SERVER_ERR = 14;
        public const int EXIT_IMAGE_FILE_NOT_FOUND = 15;
        public const int EXIT_CUST_FILE_NOT_FOUND = 16;
        public const int EXIT_BUILD_AUTH = 17;
        public const int EXIT_CARRIER = 18;
        public const int EXIT_NO_DEVICE = 20;
        public const int EXIT_DEVICE_BOOTIE = 21;
        public const int EXIT_DEVICE_RAMDISK = 22;
        public const int EXIT_DEVICE_BATTERY = 23;
        public const int EXIT_RAMDISK_ERR = 24;
        public const int EXIT_CHARGE = 25;
        public const int EXIT_CHARGER = 26;
        public const int EXIT_ROM_VERIFY = 30;
        public const int EXIT_NV_WRITE = 35;
        public const int EXIT_NV_READ = 36;
        public const int EXIT_TOKENS = 37;
        public const int EXIT_MISSING_PART = 40;
        public const int EXIT_MOUNT_ERR = 41;
        public const int EXIT_MODEM = 45;
        public const int EXIT_UPDATE_SITE = 50;
        public const int EXIT_BONUS_ERR = 60;
        public const int EXIT_TRENCHCOAT = 100;
        int errorCode;
        int exitCode;
        string errorMsg;

        public NovacomException(int errorCode, string errorMsg)
            : this(errorCode, EXIT_UNKNOWN, errorMsg, null)
        { }

        public NovacomException(int errorCode, int exitCode, string errorMsg)
            : this(errorCode, EXIT_UNKNOWN, errorMsg, null)
        { }

        public NovacomException(int errorCode, int exitCode, string errorMsg, Exception innerException)
            : base(errorMsg, innerException)
        {
            this.errorCode = errorCode;
            this.exitCode = exitCode;
            this.errorMsg = errorMsg;
        }

        public override string ToString()
        {
            return "err " + errorCode + " \"" + errorMsg + "\"";
        }

        public int ErrorCode
        {
            get { return errorCode; }
        }

        public int ExitCode
        {
            get { return exitCode; }
        }

        public string ErrorMessage
        {
            get { return errorMsg; }
        }
    }
}
