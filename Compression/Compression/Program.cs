using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunAsync();

            _ = Console.ReadKey();
        }

        private static async void RunAsync()
        { 
            string inputPath = @"c:\html\Introducing WinZip _ Get the all-new WinZip today.html";

            inputPath = @"c:\html\dickens.txt";

            await Compression.ReadFileLempelZiv.CompressFileAsync(inputPath);

            //await Compression.ReadFile.DecompressFileAsync(inputPath);
        }
    }
}
