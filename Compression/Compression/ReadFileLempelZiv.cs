using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Compression
{
    internal class ReadFileLempelZiv
    {
        public static void ProcessBytesBuffer(byte[] bytesBuffer, long index, int readBytes,
            DictionaryTree tree,
            ref long bytesCount)
        {
            Console.WriteLine($"Processing buffer, from {index} to {index + readBytes - 1}");

            long loopBytesCount = 0;

            var queue = new Queue<int>();

            for (int i = 0; i < readBytes; i++)
            {
                var byte0 = bytesBuffer[i];

                var arr0 = new int[8];

                for (int j = 0; j < 8; j++)
                {
                    arr0[j] = byte0 & 0x1;
                    byte0 >>= 1;
                }

                arr0 = arr0.Reverse().ToArray();

                for (int j = 0; j < 8; j++)
                {
                    int bit = arr0[j];

                    queue.Enqueue(bit);

                    loopBytesCount++;

                    lock (tree)
                    {
                        var arr = queue.Select(x => x).ToArray();

                        bool added = tree.Add(arr);

                        if (added)
                            queue.Clear();
                    }
                }
            }

            _ = Interlocked.Add(ref bytesCount, loopBytesCount);
        }

        public const int BufferSize = 1024 * 1024;

        private const int LenKey = 2;
        private const int LenLength = 1;
        private const int LenCharacter = 8;

        public static async Task CompressFileAsync(string inputPath)
        {
            await Task.Delay(1);

            var tree = new DictionaryTree();

            long bytesIndex = 0, bytesCount = 0;

            bool finished = false;

            var tasks = new List<Task>();

            using (var fs = new FileStream(inputPath, FileMode.Open))
            using (var br = new BinaryReader(fs))
                while (!finished)
                {
                    byte[] buffer = br.ReadBytes(BufferSize);

                    int readBytes = (buffer?.Length).GetValueOrDefault();

                    if (readBytes < BufferSize)
                        finished = true;

                    Console.WriteLine($"Read {buffer?.Length} characters from file {inputPath}");

                    // Need to capture this index value, otherwise all the threads
                    // refer to the same variable.
                    long capturedBytesIndex = bytesIndex;

                    tasks.Add(Task.Run(() => ProcessBytesBuffer(buffer, capturedBytesIndex, readBytes, tree,
                        ref bytesCount)));

                    bytesIndex += readBytes;
                }

            Task.WaitAll(tasks.ToArray());

            _ = 0;
        }
    }
}