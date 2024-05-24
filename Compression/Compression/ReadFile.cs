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

            long charsCounter = 0, charsCount = 0;

            char ch = '\0';

            var tasks = new List<Task>();

            using (var sr = new StreamReader(inputPath))
                while (!sr.EndOfStream)
                {
                    var charsBuffer = new char[BufferSize];

                    int readChars = await sr.ReadAsync(charsBuffer, 0, charsBuffer.Length);

                    charsCounter += readChars;

                    Console.WriteLine($"Read {charsCounter} characters from file {inputPath}");

                    tasks.Add(Task.Run(() => ProcessCharsBuffer(charsBuffer, charsCounter, readChars, dictionary,
                        ref charsCount)));
                }

            Task.WaitAll(tasks.ToArray());

            long sum = dictionary.Sum(x => (x.Value?.Value.Count).GetValueOrDefault());

            dictionary.Values.ToList().ForEach(x => sortedLinkedList.AddSorted(x));

            Console.WriteLine($"{nameof(dictionary)}: {dictionary.Count}");

            finished = false;

            int counter1 = 0;

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

                    //sortedLinkedList.Print();
                }

                int counter3 = counter1++;

                if (false)//(counter3 % 1000) == 0)
                    Console.WriteLine($"{nameof(counter3)}: {counter3}");
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

            byte[] arr2 = null;

            int totalCounter = 0;

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

                var bytes0 = new byte[9 * 256];

                for (int i = 0; i < arr.Length; i++)
                {
                    var tup = arr[i];

                    ulong val0 = tup.Val;

                    bytes0[tableCounter++] = (byte)tup.Length;

                    for (int j = 0; j < 8; j++)
                    {
                        bytes0[tableCounter++] = (byte)val0;

                        val0 >>= 8;
                    }
                }

                bw.Write(bytes0);

                byte pack = 0;

                charsCounter = 0;

                using (var sr = new StreamReader(inputPath))
                    while (!sr.EndOfStream)
                    //while ((ch = (char)sr.Read()) != -1)
                    {
                        var chars11 = new char[BufferSize];

                        int readChars = await sr.ReadAsync(chars11, 0, chars11.Length);

                        charsCounter += readChars;

                        Console.WriteLine($"Read {charsCounter} characters from file {inputPath}");

                        for (int i = 0; i < chars11.Length; i++)
                        {
                            ch = chars11[i];

                            if (ch < arr.Length)
                            {
                                var tup = arr[ch];

                                ulong val = tup.Val;
                                int len = tup.Length;

                                var arr22 = new byte[len];

                                for (int j = 0; j < len; j++)
                                {
                                    byte bit = (byte)(val & 0x1);

                                    arr22[j] = bit;

                                    val >>= 1;
                                }

                                arr22 = arr22.Reverse().ToArray();

                                for (int j = 0; j < len; j++)
                                {
                                    byte bit = arr22[j];

                                    pack <<= 1;

                                    pack |= bit;

                                    counterBits++;

                                    if (counterBits == 8)
                                    {
                                        counterBits = 0;

                                        int totalCounter0 = totalCounter++;

                                        if (false)//(totalCounter0 % 1000) == 0)
                                            Console.WriteLine($"[{totalCounter0}]: {pack}");

                                        (arr2 = arr2 ?? new byte[BufferSize])[counterBytes++] = pack;

                                        if (counterBytes == arr2?.Length)
                                        {
                                            bw.Write(arr2);

                                            counterBytes = 0;
                                            arr2 = null;
                                        }

                                        pack = 0;
                                    }
                                }
                            }
                        }
                    }

                if (counterBits > 0)
                {
                    int totalCounter0 = totalCounter++;

                    (arr2 = arr2 ?? new byte[1])[counterBytes++] = pack;
                }

                if (counterBytes > 0)
                    bw.Write(arr2);
            }

            using (var fs = new FileStream($@"{inputPath}.huffman", FileMode.Open))
            using (var br = new BinaryReader(fs))
            {
                var chars = br.ReadBytes(8);

                chars = chars.Reverse().ToArray();

                charsCount = 0;

                for (int i = 0; i < 8; i++)
                {
                    charsCount <<= 8;

                    charsCount |= chars[i];
                }

                var arr0 = new (ulong Val, int Length)[256];

                int bytesCounter = 0;

                var bytes0 = br.ReadBytes(9 * 256);

                int counter4 = 0;

                while (bytesCounter < bytes0.Length)
                {
                    byte length = bytes0[bytesCounter++];

                    ulong val0 = 0;

                    for (int i = 0; i < 8; i++)
                    {
                        ulong val1 = bytes0[bytesCounter++];

                        for (int j = 0; j < i; j++)
                            val1 <<= 8;

                        val0 |= val1;
                    }

                    arr0[counter4++] = (Val: val0, Length: length);
                }

                Tree<char> tree = new Tree<char>();

                for (int i = 0; i < arr0.Length; i++)
                {
                    var tup = arr0[i];

                    int[] arr5 = null;

                    if (tup.Length > 0)
                    {
                        ulong val5 = tup.Val;

                        for (int j = 0; j < tup.Length; j++)
                        {
                            int bit = (int)(val5 & 0x1);

                            val5 >>= 1;

                            (arr5 = arr5 ?? new int[tup.Length])[j] = bit;
                        }
                    }
                    
                    if (arr5?.Length > 0)
                        tree.Add((char)i, arr5.Reverse().ToArray());
                }

                tree.Print();

                var bytes3 = br.ReadBytes(BufferSize);

                charsCounter = 0;

                finished = false;

                var visitor = new TreeVisitor<char>(tree);

                while (!finished && charsCounter < charsCount)
                {
                    int i = 0;

                    while (charsCounter < charsCount && i < bytes3.Length)
                    {
                        byte byte0 = bytes3[i++];

                        var arr11 = new int[8];

                        for (int j = 0; j < 8; j++)
                        {
                            arr11[j] = byte0 & 0x1;

                            byte0 >>= 1;
                        }

                        arr11 = arr11.Reverse().ToArray();

                        int j0 = 0;

                        while (charsCounter < charsCount && j0 < 8)
                        {
                            var bit = arr11[j0++];

                            Side side = bit == 0 ? Side.Left : Side.Right;

                            visitor.Visit(side);

                            var state = visitor.Value;

                            if (state.Status)
                            {
                                ch = state.Val;

                                Console.Write(ch);

                                charsCounter++;
                            }
                        }
                    }
                }
            }
        }
    }
}