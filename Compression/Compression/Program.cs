using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string inputPath = @"c:\html\Introducing WinZip _ Get the all-new WinZip today.html";

            Compression.ReadFile.ReadFileAsync(inputPath);

            _ = Console.ReadKey();
        }
    }
}
