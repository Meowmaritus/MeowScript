using MeowScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowScript_Build
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                bool correctUsage = Commands.Build(args);
                if (!correctUsage)
                {
                    Console.WriteLine("No input file(s) specified. Please specify one or more files as command line arguments. Note that opening one or more files with this program via dragging & dropping etc in Windows will pass them as command line arguments.");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }

        }
    }
}
