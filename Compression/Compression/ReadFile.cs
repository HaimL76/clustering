using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Compression
{
    internal class ReadFile
    {
        public static void ProcessCharsBuffer(char[] charsBuffer, long index, int readChars,
            Dictionary<long, TreeNode<(long Val, long Count)>> dictionary, 
            ref long charsCount)
        {
            Console.WriteLine($"Processing buffer, from {index} to {index + readChars - 1}");

            long loopCharsCount = 0;

            for (int i = 0; i < readChars; i++)
            {
                var ch = charsBuffer[i];

                loopCharsCount++;

                if (ch > 256)
                    ch = '-';//TODO:

                int val = ch;

                TreeNode<(long Val, long Count)> treeNode = null;

                lock (dictionary)
                {
                    if (!dictionary.ContainsKey(val))
                    {
                        treeNode = new TreeNode<(long Val, long Count)>((Val: val, Count: 0));

                        dictionary.Add(val, treeNode);
                    }

                    treeNode = dictionary[val];

                    treeNode.SetValue((treeNode.Value.Val, Count: treeNode.Value.Count + 1));
                }
            }

            _ = Interlocked.Add(ref charsCount, loopCharsCount);
        }

        public const int BufferSize = 1024 * 1024;

        public static async void ReadFileAsync(string inputPath)
        {
            // Collect all the characters from the input file,
            // and prepare a dictionary of their statistics. 
            var dictionary = new Dictionary<long, TreeNode<(long Val, long Count)>>();

            bool finished = false;

            var treeNodeComparer = new TreeNodeCountComparer<(long Val, long Count)>();

            var sortedLinkedList = new SortedLinkedList<TreeNode<(long Val, long Count)>>(treeNodeComparer);

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

            long sum = dictionary.Sum(x => (x.Value?.Value.Count).GetValueOrDefault());

            dictionary.Values.ToList().ForEach(x => sortedLinkedList.AddSorted(x));

            Console.WriteLine($"{nameof(dictionary)}: {dictionary.Count}");

            finished = false;

            TreeNode<(long Val, long Count)> parent = null;

            while (!finished)
            {
                long totalCount = 0;

                var left = sortedLinkedList.RemoveFirst();

                Link<TreeNode<(long Val, long Count)>> right = null;

                totalCount += (left?.Value.Value.Count).GetValueOrDefault();

                if (left != null)
                    right = sortedLinkedList.RemoveFirst();

                totalCount += (right?.Value.Value.Count).GetValueOrDefault();

                if (right == null)
                {
                    finished = true;
                }
                else
                {
                    parent = new TreeNode<(long Val, long Count)>((Val: 0, Count: totalCount));

                    parent.SetChild(left.Value, Side.Left);
                    parent.SetChild(right.Value, Side.Right);

                    sortedLinkedList.AddSorted(parent);
                }
            }

            parent?.Print();

            var arr = new (ulong Val, int Length)[256];

            parent.Traverse(new Stack<ulong>(), (stack, tup) =>
            {
                ulong bits = 0;

                int len = 0;

                foreach (var item in stack.Reverse().ToList())
                {
                    bits <<= 1;

                    bits |= (ulong)item;

                    len++;
                }

                if (tup.Val > 256)
                    _ = 0;

                arr[tup.Val] = (Val: bits, Length: len);
            });

            int counterBits = 0;

            int counterBytes = 0;

            byte[] writeBuffer = null;

            int tableCounter = 0;

            using (var fs = new FileStream($@"{inputPath}.huffman", FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                var chars = new byte[8];

                for (int i = 0; i < 8; i++)
                {
                    chars[i] = (byte)charsCount;

                    charsCount >>= 8;
                }

                bw.Write(chars);

                var translationTableBuffer = new byte[9 * 256];

                for (int i = 0; i < arr.Length; i++)
                {
                    var tup = arr[i];

                    ulong val = tup.Val;

                    translationTableBuffer[tableCounter++] = (byte)tup.Length;

                    for (int j = 0; j < 8; j++)
                    {
                        translationTableBuffer[tableCounter++] = (byte)val;

                        val >>= 8;
                    }
                }

                bw.Write(translationTableBuffer);

                byte pack = 0;

                charsIndex = 0;

                using (var sr = new StreamReader(inputPath))
                using (var sw = new StreamWriter($"{inputPath}.debug.txt"))
                    while (!sr.EndOfStream)
                    {
                        var charsBuffer = new char[BufferSize];

                        int readChars = await sr.ReadAsync(charsBuffer, 0, charsBuffer.Length);

                        charsIndex += readChars;

                        Console.WriteLine($"Read {charsIndex} characters from file {inputPath}");

                        for (int i = 0; i < charsBuffer.Length; i++)
                        {
                            ch = charsBuffer[i];

                            if (ch > 0 && ch < arr.Length)
                            {
                                var tup = arr[ch];

                                sw.WriteLine($"{ch},0x{tup.Val:X},{tup.Length}");

                                ulong val = tup.Val;
                                int len = tup.Length;

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

            writeBuffer = new byte[BufferSize];
            long writeIndex = 0, totalCounter = 0;

            using (var fs = new FileStream($@"{inputPath}.huffman", FileMode.Open))
            using (var fsw = new FileStream($@"{inputPath}.huffman.txt", FileMode.Create))
            using (var br = new BinaryReader(fs))
            using (var bw = new BinaryWriter(fsw))
            {
                var chars = br.ReadBytes(8);

                chars = chars.Reverse().ToArray();

                charsCount = 0;

                for (int i = 0; i < 8; i++)
                {
                    charsCount <<= 8;

                    charsCount |= chars[i];
                }

                arr = new (ulong Val, int Length)[256];

                int bytesCounter = 0;

                var translationTableBuffer = br.ReadBytes(9 * 256);

                long counter = 0;

                while (bytesCounter < translationTableBuffer.Length)
                {
                    byte length = translationTableBuffer[bytesCounter++];

                    ulong val = 0;

                    for (int i = 0; i < 8; i++)
                    {
                        ulong byteVal = translationTableBuffer[bytesCounter++];

                        for (int j = 0; j < i; j++)
                            byteVal <<= 8;

                        val |= byteVal;
                    }

                    arr[counter++] = (Val: val, Length: length);
                }

                Tree<char> tree = new Tree<char>();

                for (int i = 0; i < arr.Length; i++)
                {
                    var tup = arr[i];

                    int[] bits = null;

                    if (tup.Length > 0)
                    {
                        ulong val = tup.Val;

                        for (int j = 0; j < tup.Length; j++)
                        {
                            int bit = (int)(val & 0x1);

                            val >>= 1;

                            (bits = bits ?? new int[tup.Length])[j] = bit;
                        }
                    }
                    
                    if (bits?.Length > 0)
                        tree.Add((char)i, bits.Reverse().ToArray());
                }

                tree.Print();

                var bytes = br.ReadBytes(BufferSize);

                charsIndex = 0;

                finished = false;

                var visitor = new TreeVisitor<char>(tree);

                while (!finished && charsIndex < charsCount)
                {
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
                                ch = state.Val;

                                //Console.Write(ch);

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