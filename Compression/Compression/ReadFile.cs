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
    internal class ReadFile
    {
        public static byte[] ConvertToBytes(long val, int numBytes)
        {
            var bytes = new byte[numBytes];

            for (int i = 0; i < numBytes; i++)
            {
                bytes[i] = (byte)val;

                val >>= 8;
            }

            return bytes;
        }

        public static void CopyToBytesArray(ulong val, byte[] buffer, int index, int numBytes)
        {
            for (int i = 0; i < numBytes; i++)
            {
                buffer[index + i] = (byte)val;

                val >>= 8;
            }
        }

        public static long ConvertFromBytes(byte[] arr, int index = 0, int length = 0)
        {
            long val = 0;

            if (length < 1)
                length = arr.Length;

            for (int i = length - 1; i >= 0; i--)
            {
                val <<= 8;

                try
                {
                    val |= arr[index + i];
                }
                catch(Exception exception)
                {
                    _ = exception;
                }
            }

            return val;
        }

        public static void ProcessCharsBuffer(char[] charsBuffer, long index, int readChars,
            Dictionary<long, TreeNode<(long Character, long NumOccurrences)>> dictionary,
            ref long charsCount)
        {
            Console.WriteLine($"Processing buffer, from {index} to {index + readChars - 1}");

            long loopCharsCount = 0;

            for (int i = 0; i < readChars; i++)
            {
                var ch = charsBuffer[i];

                loopCharsCount++;

                int val = ch;

                TreeNode<(long Character, long NumOccurrences)> treeNode = null;

                lock (dictionary)
                {
                    if (!dictionary.ContainsKey(val))
                    {
                        treeNode = new TreeNode<(long Character, long NumOccurrences)>((Character: val, NumOccurrences: 0));

                        dictionary.Add(val, treeNode);
                    }

                    treeNode = dictionary[val];

                    treeNode.SetValue((treeNode.Value.Character, NumOccurrences: treeNode.Value.NumOccurrences + 1));
                }
            }

            _ = Interlocked.Add(ref charsCount, loopCharsCount);
        }

        public const int BufferSize = 1024 * 1024;

        private const int LenKey = 2;
        private const int LenLength = 1;
        private const int LenCharacter = 8;

        public static async Task CompressFileAsync(string inputPath)
        {
            // Collect all the characters from the input file,
            // and prepare a dictionary of their statistics. 
            var dictionary = new Dictionary<long, TreeNode<(long Character, long NumBits)>>();

            bool finished = false;

            var treeNodeComparer = new TreeNodeCountComparer<(long Character, long NumOccurrences)>();

            var sortedLinkedList = new SortedLinkedList<TreeNode<(long Character, long NumOccurrences)>>(treeNodeComparer);

            long charsIndex = 0, charsCount = 0;

            char ch = '\0';

            var tasks = new List<Task>();

            using (var sr = new StreamReader(inputPath))
                while (!sr.EndOfStream)
                {
                    var charsBuffer = new char[BufferSize];

                    int readChars = await sr.ReadAsync(charsBuffer, 0, charsBuffer.Length);

                    Console.WriteLine($"Read {charsIndex} characters from file {inputPath}");

                    // Need to capture this index value, otherwise all the threads
                    // refer to the same variable.
                    long capturedCharsIndex = charsIndex;

                    tasks.Add(Task.Run(() => ProcessCharsBuffer(charsBuffer, capturedCharsIndex, readChars, dictionary,
                        ref charsCount)));

                    charsIndex += readChars;
                }

            Task.WaitAll(tasks.ToArray());

            long sum = dictionary.Sum(x => (x.Value?.Value.NumBits).GetValueOrDefault());

            dictionary.Values.ToList().ForEach(x => sortedLinkedList.AddSorted(x));

            Console.WriteLine($"{nameof(dictionary)}: {dictionary.Count}");

            finished = false;

            TreeNode<(long Character, long NumOccurrences)> parent = null;

            while (!finished)
            {
                long totalCount = 0;

                var left = sortedLinkedList.RemoveFirst();

                Link<TreeNode<(long Character, long NumOccurrences)>> right = null;

                totalCount += (left?.Value.Value.NumOccurrences).GetValueOrDefault();

                if (left != null)
                    right = sortedLinkedList.RemoveFirst();

                totalCount += (right?.Value.Value.NumOccurrences).GetValueOrDefault();

                if (right == null)
                {
                    finished = true;
                }
                else
                {
                    parent = new TreeNode<(long Character, long NumOccurrences)>((Character: 0, NumOccurrences: totalCount));

                    parent.SetChild(left.Value, Side.Left);
                    parent.SetChild(right.Value, Side.Right);

                    sortedLinkedList.AddSorted(parent);
                }
            }

            parent?.Print();

            var table = new Dictionary<long, (ulong Bits, int NumBits)>();

            parent.Traverse(new Stack<ulong>(), (stack, tup) =>
            {
                ulong bits = 0;

                int len = 0;

                foreach (var item in stack.Reverse().ToList())
                {
                    bits <<= 1;

                    bits |= item;

                    len++;
                }

                table[tup.Character] = (Bits: bits, NumBits: len);
            });

            int counterBits = 0;

            int counterBytes = 0;

            byte[] writeBuffer = null;

            using (var fs = new FileStream($@"{inputPath}.huffman", FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                var buffer = ConvertToBytes(charsCount, 8);

                bw.Write(buffer);

                var tableArray = table.ToArray();

                buffer = ConvertToBytes(tableArray.Length, 2);

                bw.Write(buffer);

                var translationTableBuffer = new byte[(LenLength + LenKey + LenCharacter) * tableArray.Length];

                for (int i = 0; i < tableArray.Length; i++)
                {
                    int baseIndex = i * (LenLength + LenKey + LenCharacter);

                    var pair = tableArray[i];

                    var key = pair.Key;
                    var tup = pair.Value;

                    CopyToBytesArray((ulong)key, translationTableBuffer, baseIndex, LenKey);
                    baseIndex += LenKey;

                    ulong bits = tup.Bits;

                    CopyToBytesArray((ulong)tup.NumBits, translationTableBuffer, baseIndex, LenLength);
                    baseIndex += LenLength;

                    CopyToBytesArray(bits, translationTableBuffer, baseIndex, LenCharacter);
                }

                bw.Write(translationTableBuffer);

                byte pack = 0;

                charsIndex = 0;

                using (var sr = new StreamReader(inputPath))
                    while (!sr.EndOfStream)
                    {
                        var charsBuffer = new char[BufferSize];

                        int readChars = await sr.ReadAsync(charsBuffer, 0, charsBuffer.Length);

                        charsIndex += readChars;

                        Console.WriteLine($"Read {charsIndex} characters from file {inputPath}");

                        for (int i = 0; i < charsBuffer.Length; i++)
                        {
                            ch = charsBuffer[i];

                            if (ch > 0 && table.ContainsKey(ch))
                            {
                                var tup = table[ch];

                                ulong val = tup.Bits;
                                int len = tup.NumBits;

                                var bitsBuffer = new byte[len];

                                for (int j = 0; j < len; j++)
                                {
                                    byte bit = (byte)(val & 0x1);

                                    bitsBuffer[j] = bit;

                                    val >>= 1;
                                }

                                bitsBuffer = bitsBuffer.Reverse().ToArray();

                                for (int j = 0; j < len; j++)
                                {
                                    byte bit = bitsBuffer[j];

                                    pack <<= 1;

                                    pack |= bit;

                                    counterBits++;

                                    if (counterBits == 8)
                                    {
                                        counterBits = 0;

                                        (writeBuffer = writeBuffer ?? new byte[BufferSize])[counterBytes++] = pack;

                                        if (counterBytes == writeBuffer?.Length)
                                        {
                                            bw.Write(writeBuffer);

                                            counterBytes = 0;
                                            writeBuffer = null;
                                        }

                                        pack = 0;
                                    }
                                }
                            }
                        }
                    }

                if (counterBits > 0)
                    (writeBuffer = writeBuffer ?? new byte[1])[counterBytes++] = pack;

                if (counterBytes > 0)
                {
                    var copyBuffer = new byte[counterBytes];

                    for (int i = 0; i < counterBytes; i++)
                        copyBuffer[i] = writeBuffer[i];

                    bw.Write(copyBuffer);
                }
            }
        }

        private const string Banner = "decompressed by Haim Lavi software";

        public static async Task DecompressFileAsync(string inputPath)
        {
            await Task.Delay(1);

            var writeBuffer = new byte[BufferSize];
            long writeIndex = 0, totalCounter = 0;

            var queue = new Queue<long>();

            using (var fs = new FileStream($@"{inputPath}.huffman", FileMode.Open))
            using (var fsw = new FileStream($@"{inputPath}.huffman.decompressed.txt", FileMode.Create))
            using (var br = new BinaryReader(fs))
            using (var bw = new BinaryWriter(fsw))
            {
                string banner = $@"<<<{Banner}, {DateTime.Now}>>>{Environment.NewLine}";

                var bannerBytes = Encoding.ASCII.GetBytes(banner);

                bw.Write(bannerBytes);

                var buffer = br.ReadBytes(8);

                long charsCount = ConvertFromBytes(buffer);

                buffer = br.ReadBytes(2);

                int tableCount = (int)ConvertFromBytes(buffer);

                var table = new Dictionary<long, (ulong Bits, int NumBits)>();

                var translationTableBuffer = br.ReadBytes((LenKey + LenLength + LenCharacter) * tableCount);

                for (int i = 0; i < tableCount; i++)
                {
                    int baseindex = i * (LenKey + LenLength + LenCharacter);

                    int key = (int)ConvertFromBytes(translationTableBuffer, baseindex, LenKey);
                    baseindex += LenKey;

                    int length = (int)ConvertFromBytes(translationTableBuffer, baseindex, LenLength);
                    baseindex += LenLength;

                    ulong val = (ulong)ConvertFromBytes(translationTableBuffer, baseindex, LenCharacter);

                    table[key] = (Bits: val, NumBits: length);
                }

                Tree<char> tree = new Tree<char>();

                var tableArray = table.ToArray();

                for (int i = 0; i < tableArray.Length; i++)
                {
                    var pair = tableArray[i];

                    var tup = pair.Value;

                    int[] bitsArray = null;

                    long character = pair.Key;

                    ulong bits = tup.Bits;

                    int numBits = tup.NumBits;

                    if (tup.NumBits > 0)
                    {
                        for (int j = 0; j < numBits; j++)
                        {
                            int bit = (int)(bits & 0x1);

                            bits >>= 1;

                            (bitsArray = bitsArray ?? new int[numBits])[j] = bit;
                        }
                    }

                    if (bitsArray?.Length > 0)
                        tree.Add((char)character, bitsArray.Reverse().ToArray());
                }

                tree.Print();

                long charsIndex = 0;

                bool finished = false;

                var visitor = new TreeVisitor<char>(tree);

                while (!finished && charsIndex < charsCount)
                {
                    Console.WriteLine($"{nameof(charsIndex)}: {charsIndex}");

                    var bytes = br.ReadBytes(BufferSize);

                    long lenBytes = (bytes?.Length).GetValueOrDefault();

                    if (lenBytes < BufferSize)
                        finished = true;

                    int i = 0;

                    while (charsIndex < charsCount && i < bytes.Length)
                    {
                        byte byteVal = bytes[i++];

                        var byteBuffer = new int[8];

                        for (int j = 0; j < 8; j++)
                        {
                            byteBuffer[j] = byteVal & 0x1;

                            byteVal >>= 1;
                        }

                        byteBuffer = byteBuffer.Reverse().ToArray();

                        int index = 0;

                        while (charsIndex < charsCount && index < 8)
                        {
                            var bit = byteBuffer[index++];

                            Side side = bit == 0 ? Side.Left : Side.Right;

                            visitor.Visit(side);

                            var state = visitor.Value;

                            if (state.Status)
                            {
                                char ch = state.Val;

                                writeBuffer[writeIndex++] = (byte)ch;

                                if (writeIndex >= writeBuffer.Length)
                                {
                                    totalCounter += writeIndex;

                                    Console.WriteLine($"{nameof(totalCounter)}: {totalCounter}");

                                    writeIndex = 0;

                                    bw.Write(writeBuffer);

                                    writeBuffer = new byte[BufferSize];
                                }

                                charsIndex++;
                            }
                        }
                    }
                }

                if (writeIndex > 0)
                {
                    var copyBuffer = new byte[writeIndex];

                    for (int i = 0; i < writeIndex; i++)
                        copyBuffer[i] = writeBuffer[i];

                    bw.Write(copyBuffer);
                }

                Console.WriteLine(nameof(finished));
            }
        }
    }
}