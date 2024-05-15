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

            inputPath = @"c:\gpp\bookmarks_3_25_24.html";
            //inputPath = @"c:\gpp\MissingCardsSupreme-output.txt";
            //inputPath = @"c:\gpp\kishkush.txt";

            Compression.ReadFile.ReadFileAsync(inputPath);

            _ = Console.ReadKey();
        }
    }
}
