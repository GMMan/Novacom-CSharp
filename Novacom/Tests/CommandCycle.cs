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

namespace Palm.Novacom.Tests
{
    public class CommandCycle
    {
        static readonly string localFileName = Path.Combine(Path.GetTempPath(), "image.cmp");
        static readonly string deviceFileName = "/tmp/image.dat";
        const string RMFILE = "/bin/rm";
        string fileImage = Path.Combine(Path.GetTempPath(), "image.dat");
        int cycleCount = 1000;
        int loopCount = 100;
        INovacomDevice device;

        public static void Main(string[] args)
        {
            Console.Error.WriteLine("Novacom Command Cycle Tests");
            CommandCycle cycle = new CommandCycle();
            cycle.CommandCycler();
        }

        public void CommandCycler()
        {
            try
            {
                INovacomController controller = Novacom.GetController();

                NovaDeviceInfo[] devices = controller.GetDeviceList();
                foreach (NovaDeviceInfo dev in devices)
                    Console.Error.WriteLine(dev);
                if (devices.Length <= 0)
                {
                    Console.Error.WriteLine("No Devices, No Tests.");
                    return;
                }

                device = controller.ConnectDefaultDevice();
                if (device.State != Novacom.DeviceState.OS)
                {
                    Console.Error.WriteLine("Requires OS mode");
                    return;
                }
                for (int i = 0; i < cycleCount; i++)
                {
                    Console.WriteLine("-------------------------------------------");
                    Console.WriteLine("sequence " + (i + 1));
                    Console.WriteLine("-------------------------------------------");
                    commandSequence();
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e);
            }
            catch (NovacomException e)
            {
                Console.Error.WriteLine(e);
            }
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        void commandSequence()
        {
            FileInfo localFile = new FileInfo(localFileName);

            Console.WriteLine("Put file test");
            for (int i = 0; i < loopCount; i++)
            {
                runProgram(RMFILE, deviceFileName);
                putFile(fileImage, deviceFileName);
                Console.Write(".");
            }
            Console.WriteLine();

            Console.WriteLine("Get file test");
            for (int i = 0; i < loopCount; i++)
            {
                getFile(localFileName, deviceFileName);
                localFile.Delete();
                Console.Write(".");
            }
            Console.WriteLine();

            Console.WriteLine("Run test");
            for (int i = 0; i < loopCount; i++)
            {
                INovacomStream output = device.RunProgram("/bin/ls", "-l", "/bin");
                string line = null;
                while (((line = output.ReadLine()) != null) && (line.Length != 0)) ;
                output.Close();
                Console.Write(".");
            }
            Console.WriteLine();

            Console.WriteLine("Put/Get/Compare test");
            for (int i = 0; i < loopCount; i++)
            {
                runProgram(RMFILE, deviceFileName);
                putFile(fileImage, deviceFileName);
                getFile(localFileName, deviceFileName);
                bool result = compareFiles(fileImage, localFileName);
                localFile.Delete();
                if (result)
                {
                    Console.Write(".");
                }
                else
                {
                    Console.Write("!");
                }
            }
            Console.WriteLine();
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        bool compareFiles(string inFile1, string inFile2)
        {
            FileInfo file1 = new FileInfo(inFile1);
            FileInfo file2 = new FileInfo(inFile2);
            if (file1.Length != file2.Length)
            {
                return false;
            }
            FileStream fileInputStream1 = file1.OpenRead();
            FileStream fileInputStream2 = file2.OpenRead();
            int b1;
            int b2;
            do
            {
                b1 = fileInputStream1.ReadByte();
                b2 = fileInputStream2.ReadByte();
            } while ((b1 == b2) && (b1 != -1) && (b2 != -1));
            fileInputStream1.Close();
            fileInputStream2.Close();

            return (b1 == -1) && (b2 == -1);
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        void putFile(string fileName, string filePathOnDevice)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            INovacomStream deviceStream = device.PutFile(filePathOnDevice);
            deviceStream.Write(fileStream);

            deviceStream.CloseOutput();
            deviceStream.WaitForReturnCode();
            deviceStream.Close();
            fileStream.Close();
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        void getFile(string fileName, string filePathOnDevice)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            INovacomStream deviceStream = device.GetFile(filePathOnDevice);
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

            deviceStream.Close();
        }

        /// <exception cref="System.IO.IOException" />
        /// <exception cref="Palm.Novacom.NovacomException" />
        void runProgram(string path, params string[] args)
        {
            INovacomStream deviceStream = device.RunProgram(path, args);
            deviceStream.WaitForReturnCode();
        }
    }
}
