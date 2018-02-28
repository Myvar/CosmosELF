using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CosmosELFCore;
using Sys = Cosmos.System;

namespace CosmosELF
{
    public unsafe class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.Clear();

            fixed (byte* ptr = TestFile.test_so)
            {
                var exe = new UnmanagedExecutible(ptr);
                exe.Load();
                exe.Link();

                var args = new ArgumentWriter();
                args.Push(25);
                var x = exe.Invoke("dbl");
                Console.WriteLine($"25 * 2 = {x}");
            }
        }

        protected override void Run()
        {
            Console.ReadLine();
        }
    }
}