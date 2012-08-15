/*
*
*      Copyright (c) 2012 Yukai Li
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
using System.Reflection;

namespace TestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Type> tests;
            // Populate tests
            var q = from t in typeof(Palm.Novacom.Novacom).Assembly.GetTypes() where t.IsClass && t.Namespace.StartsWith("Palm.Novacom.Tests") select t;
            tests = q.ToList();

            Console.WriteLine("Novacom Test Launcher (C) 2012 GMMan");
            Console.WriteLine("====================================");

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Please enter test number to run, or 0 to quit.");
                Console.WriteLine();
                for (int i = 0; i < tests.Count; i++)
                {
                    Console.WriteLine(i + 1 + ". " + tests[i].Name);
                }
                Console.WriteLine();

                int n;
                bool inputValid = false;
                do
                {
                    Console.Write("Enter your selection: ");
                    inputValid = int.TryParse(Console.ReadLine(), out n);
                    if (!inputValid)
                    {
                        Console.WriteLine("Please enter a number.");
                    }
                } while (!inputValid);

                if (n == 0)
                {
                    break;
                }

                try
                {
                    Console.WriteLine();
                    Type test = tests[n - 1];
                    MethodInfo main = test.GetMethod("Main", BindingFlags.Static | BindingFlags.Public);
                    if (main == null)
                    {
                        Console.WriteLine("Failed to find test entry point.");
                        continue;
                    }
                    Console.WriteLine("Starting test...");
                    try
                    {
                        main.Invoke(null, new object[] { new string[] { } });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error during test: " + e.InnerException.ToString());
                    }
                    Console.WriteLine("Test complete.");
                }
                catch
                {
                    Console.WriteLine("Please enter a valid test number or 0.");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Bye!");
        }
    }
}
