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

namespace Palm.Novacom.Tests
{
    public class CheckStartupTime
    {
        public static void Main(string[] args)
        {
            INovacomController controller = Novacom.GetController();
            INovacomDevice device = controller.ConnectDefaultDevice();

            Console.WriteLine("Check reboot time: reports how fast connection is established after reboot");
            if (device.State != Novacom.DeviceState.Bootloader)
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

                Console.WriteLine("---   device connected: " + device.IsConnected);

                Console.WriteLine("--- rebooting device");
                INovacomStream stream = device.RunProgram("/sbin/tellbootie", "reboot");

                while (stream.Read() > 0) ;
                stream.Close();

                Console.WriteLine("---   device connected: " + device.IsConnected);
                stopwatch.Start();

                device.WaitForDeviceToAppear();
                stopwatch.Stop();

                Console.WriteLine("--- device started, novacomd online");
                Console.WriteLine("---   device connected: " + device.IsConnected);
                Console.WriteLine(" connection established after " + stopwatch.ElapsedMilliseconds + " millisec; (" + stopwatch.Elapsed.TotalSeconds + "sec)");
            }
            else
            {
                Console.WriteLine("Device must be booted in normal mode");
            }
        }
    }
}
