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

namespace Palm.Novacom.Tests
{
    public class KillConnectionTest
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Kill Connection Tests");
            INovacomController conroller = Novacom.GetController();
            try
            {
                INovacomDevice device = conroller.ConnectDefaultDevice();
                device.KillConnection();
                Console.WriteLine("Connection Killed!!!!");
            }
            catch (System.IO.IOException e)
            {
                Console.Error.WriteLine(e);
            }
            catch (NovacomException e)
            {
                Console.Error.WriteLine(e);
            }
        }
    }
}
