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
    public class BasicTests
    {
        public static void Main(string[] args)
        {
            const string TEST_HEADER_LINE = "[test]-----------------------";
            const string TEST_PASSWORD = "test";

            Console.Error.WriteLine("Basic Novacom Test");
            INovacomController controller = Novacom.GetController();
            try
            {
                NovaDeviceInfo[] devices = controller.GetDeviceList();

                foreach (NovaDeviceInfo dev in devices)
                    Console.Error.WriteLine(dev);
                if (devices.Length <= 0)
                {
                    Console.Error.WriteLine("No Devices, No Tests.");
                    return;
                }

                INovacomDevice device = controller.ConnectDefaultDevice();
                INovacomStream stream;
                if (device.State == Novacom.DeviceState.Bootloader)
                {
                    Console.Error.WriteLine("BOOTLOADER Test Run");
                    stream = device.RunProgram("", null);

                    string command = "usb print ";
                    string call = "Hello World, via NovaCom";
                    string combined = command + call;
                    byte[] bytes = Encoding.ASCII.GetBytes(combined);
                    stream.Write(bytes);
                    stream.CloseOutput();

                    string reply = stream.ReadLine();
                    Console.Error.Write(reply);
                    stream.Close();
                }
                else
                {
                    Console.WriteLine(TEST_HEADER_LINE);
                    Console.WriteLine("logout");
                    try
                    {
                        device.DevLogout();
                    }
                    catch (NovacomException e)
                    {
                        Console.Error.WriteLine(e);
                    }

                    Console.WriteLine(TEST_HEADER_LINE);
                    Console.WriteLine("login");
                    try
                    {
                        device.DevLogin(TEST_PASSWORD);
                    }
                    catch (NovacomException e)
                    {
                        Console.Error.WriteLine(e);
                    }

                    Console.WriteLine(TEST_HEADER_LINE);
                    Console.WriteLine("add token");
                    try
                    {
                        device.DevAddToken(TEST_PASSWORD);
                    }
                    catch (NovacomException e)
                    {
                        Console.Error.WriteLine(e);
                    }

                    Console.WriteLine(TEST_HEADER_LINE);
                    Console.WriteLine("remove token");
                    try
                    {
                        device.DevRmToken(TEST_PASSWORD);
                    }
                    catch (NovacomException e)
                    {
                        Console.Error.WriteLine(e);
                    }

                    Console.WriteLine(TEST_HEADER_LINE);
                    Console.Error.Write("OS Test Run: ");
                    Console.Error.WriteLine("Get a file that doesn't exist.");
                    try
                    {
                        stream = device.GetFile("/foobar/is/geeky");
                        stream.Close();
                    }
                    catch (NovacomException e)
                    {
                        Console.Error.WriteLine(e);
                    }

                    Console.WriteLine(TEST_HEADER_LINE);
                    stream = device.RunProgram("/bin/cat", "/dev/urandom");
                    for (int i = 0; i < 50; i++)
                    {
                        int code = stream.Read();
                        Console.Write(" " + code);
                    }
                    Console.WriteLine();
                    stream.SendSignal(7);
                    while (stream.Read() > 0)
                    {
                        Console.WriteLine("Still alive");
                    }
                    Console.WriteLine("Got returncode " + stream.WaitForReturnCode());
                    stream.Close();

                    Console.WriteLine(TEST_HEADER_LINE);
                    stream = device.PutFile("/tmp/hello_world.txt");
                    for (int i = 0; i < 10; i++)
                    {
                        string hello = "Hello World, via NovaCom: " + i + "\n";
                        byte[] bytes = Encoding.ASCII.GetBytes(hello);
                        stream.Write(bytes);
                    }
                    stream.CloseOutput();
                    stream.Close();

                    Console.WriteLine(TEST_HEADER_LINE);
                    stream = device.GetFile("/tmp/hello_world.txt");
                    string line = null;
                    while (((line = stream.ReadLine()) != null) && (line.Length != 0))
                    {
                        Console.Write(line);
                    }
                    stream.Close();

                    Console.WriteLine("\n\"return code\" test");
                    stream = device.RunProgram("/bin/ls", "-l", "/mamba");
                    int retCode = stream.WaitForReturnCode();
                    if (retCode > 0)
                    {
                        Console.WriteLine("Correct return code " + retCode);
                    }
                    else
                    {
                        Console.Error.WriteLine("Incorrect return code!!!");
                    }

                    Console.WriteLine(TEST_HEADER_LINE);
                    Console.WriteLine("\n\"Broken pipe\" test");
                    stream = device.RunProgram("/bin/cat", null);
                    string hello2 = "Hello World";
                    byte[] bytes2 = Encoding.ASCII.GetBytes(hello2);
                    for (int i = 0; i < 5; i++)
                    {
                        stream.Write(bytes2);
                    }
                    stream.SendSignal(7);
                    try
                    {
                        for (int i = 0; i < 50000; i++)
                        {
                            stream.Write(bytes2);
                            System.Threading.Thread.Sleep(10);
                        }
                    }
                    catch (System.IO.IOException eS)
                    {
                        Console.Error.WriteLine("exception:");
                        Console.Error.WriteLine(eS);
                    }

                    Console.WriteLine(TEST_HEADER_LINE);
                    Console.WriteLine("connectDevicePort: 5858");
                    try
                    {
                        stream = device.ConnectDevicePort(5858);

                        for (int i = 0; i < 5; i++)
                        {
                            int c = stream.Read();
                            if (c < 0)
                            {
                                Console.WriteLine("disconnected");
                                break;
                            }
                            Console.WriteLine((char)c);
                        }
                        stream.CloseOutput();
                        stream.CloseInput();
                        stream.Close();
                    }
                    catch (NovacomException e)
                    {
                        Console.WriteLine("unable to connect to device port");
                        Console.Error.WriteLine(e);
                    }

                    Console.WriteLine(TEST_HEADER_LINE);
                    Console.WriteLine("Exception check on command handling: please DISCONNECT device within 10sec");
                    System.Threading.Thread.Sleep(10000);
                    if (!device.IsConnected)
                    {
                        try
                        {
                            stream = device.GetFile("/tmp/hello_world.txt");
                            while (((line = stream.ReadLine()) != null) && (line.Length != 0))
                            {
                                Console.Write(line);
                            }
                            stream.Close();
                        }
                        catch (NovacomException)
                        {
                            Console.WriteLine("Exception caught.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("device is connected, skip test...");
                    }

                    Console.WriteLine(TEST_HEADER_LINE);
                    Console.WriteLine("autologin test: please DISCONNECT device for 10sec");
                    System.Threading.Thread.Sleep(10000);
                    if (!device.IsConnected)
                    {
                        Console.WriteLine("autologin test: please CONNECT device");
                        device.WaitForDeviceToAppear();
                    }
                    else
                    {
                        Console.WriteLine("device is connected, skip test...");
                    }

                    Console.WriteLine(TEST_HEADER_LINE);
                    Console.WriteLine("logout");
                    try
                    {
                        device.DevLogout();
                    }
                    catch (NovacomException e)
                    {
                        Console.Error.WriteLine(e);
                    }
                }
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
