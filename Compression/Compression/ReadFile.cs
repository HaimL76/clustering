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
            Dictionary<string, TreeNode<(string StringKey, long NumOccurances, object LinkObject)>> dictionaryStrings,
            Dictionary<char, TreeNode<(string StringKey, long NumOccurances, object LinkObject)>> dictionaryCharacters,
            SortedBuffer<TreeNode<(string StringKey, long NumOccurances, object LinkObject)>> sortedBuffer,
            ref long charsCount, int maxStringLength = 2)
        {
            Console.WriteLine($"Processing buffer, from {index} to {index + readChars - 1}");

            long loopCharsCount = 0;

            var queue = new Queue<char>(maxStringLength);

            for (int i = 0; i < readChars; i++)
            {
                var ch = charsBuffer[i];

                loopCharsCount++;

                if (ch > 256)
                    ch = '-';//TODO:

                string str = null;

                TreeNode<(string StringKey, long NumOccurances, object LinkObject)> treeNode = null;

                if (queue.Count >= maxStringLength)
                {
                    var arr = queue.ToArray();

                    for (int j = maxStringLength; j >= 2; j--)
                    {
                        str = new string(arr, 0, j);

                        lock (dictionaryStrings)
                        {
                            DoubleLink<TreeNode<(string StringKey, long NumOccurances, object LinkObject)>> doubleLink = null;

                            if (!dictionaryStrings.ContainsKey(str))
                            {
                                treeNode = new TreeNode<(string StringKey, long NumOccurances, object LinkObject)>((StringKey: str, NumOccurances: 0, LinkObject: null));

                                dictionaryStrings.Add(str, treeNode);
                                
                                doubleLink = sortedBuffer.AddSorted(treeNode) 
                                    as DoubleLink<TreeNode<(string StringKey, long NumOccurances, object LinkObject)>>;

                                treeNode.SetValue((treeNode.Value.StringKey, treeNode.Value.NumOccurances, LinkObject: doubleLink));
                            }

                            treeNode = dictionaryStrings[str];

                            ////////treeNode.SetValue((treeNode.Value.StringKey, 
                            ////////    NumOccurances: treeNode.Value.NumOccurances + treeNode.Value.StringKey.Length, 
                            ////////    treeNode.Value.LinkObject));

                            if (doubleLink == null)
                            {
                                doubleLink = treeNode.Value.LinkObject as DoubleLink<TreeNode<(string StringKey, long NumOccurances, object LinkObject)>>;

                                sortedBuffer.AddSorted(doubleLink);
                            }
                        }
                    }

                    _ = queue.Dequeue();
                }

                queue.Enqueue(ch);

                lock (dictionaryCharacters)
                {
                    string strChar = new string(new[] { ch });

                    if (!dictionaryCharacters.ContainsKey(ch))
                    {
                        treeNode = new TreeNode<(string StringKey, long NumOccurances, object LinkObject)>((StringKey: strChar, NumOccurances: 0, 
                            LinkObject: null));

                        dictionaryCharacters.Add(ch, treeNode);
                    }

                    treeNode = dictionaryCharacters[ch];

                    treeNode.SetValue((treeNode.Value.StringKey, NumOccurances: treeNode.Value.NumOccurances + 1, treeNode.Value.LinkObject));
                }
            }

            _ = Interlocked.Add(ref charsCount, loopCharsCount);
        }

        public const int BufferSize = 1024 * 1024;

        private const int SizeOfStringLengthField = 1;//we can have strings of up to 256 bytes
        private const int LenLength = 1;
        private const int LenCharacter = 8;

        public static async Task CompressFileAsync(string inputPath, int maxStringLength = 2)
        {
            // Collect all the characters from the input file,
            // and prepare a dictionary of their statistics. 
            var dictionaryStrings = new Dictionary<string, TreeNode<(string StringKey, long NumOccurances, object LinkObject)>>();
            var dictionaryCharacters = new Dictionary<char, TreeNode<(string StringKey, long NumOccurances, object LinkObject)>>();

            bool finished = false;

            var treeNodeComparer = new TreeNodeCountComparer<(string StringKey, long NumOccurances, object LinkObject)>();

            var sortedLinkedList = 
                new SortedOneWayLinkedList<TreeNode<(string StringKey, long NumOccurances, object LinkObject)>>(treeNodeComparer);

            var sortedBuffer = new SortedBuffer<TreeNode<(string StringKey, long NumOccurances, object LinkObject)>>(treeNodeComparer, 3);

            //sortedDoubleLinkedList.Format = node => node.Value.StringKey;

            long charsIndex = 0, charsCount = 0;

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

                    tasks.Add(Task.Run(() => ProcessCharsBuffer(charsBuffer, capturedCharsIndex, readChars, dictionaryStrings,
                        dictionaryCharacters, sortedBuffer, ref charsCount, maxStringLength: maxStringLength)));

                    charsIndex += readChars;
                }

            Task.WaitAll(tasks.ToArray());

            long count = sortedBuffer.GetCount();

            sortedBuffer.Print();

            //dictionaryStrings.Values.ToList().ForEach(x => sortedReverseLinkedList.AddSorted(x));

            //long sumCharacters = dictionaryCharacters.Sum(x => (x.Value?.Value.NumOccurances).GetValueOrDefault());

            //long sumStrings = dictionaryStrings.Sum(x => (x.Value?.Value.NumOccurances).GetValueOrDefault());

            double averageCharactersOccurances = dictionaryCharacters.Average(x => (x.Value?.Value.NumOccurances).GetValueOrDefault());

            ////////dictionaryStrings.ToList().ForEach(x =>
            ////////{
            ////////    var treeNode = x.Value;

            ////////    int len = treeNode.Value.StringKey.Length;

            ////////    x.Value.SetValue((x.Value.Value.StringKey, x.Value.Value.NumOccurances * len, x.Value.Value.LinkObject));
            ////////});

            dictionaryCharacters.ToList().ForEach(x => sortedLinkedList.AddSorted(x.Value));// dictionaryStrings[x.Value.Value.StringKey] = x.Value);

            sortedBuffer.ToList().ForEach(x => sortedLinkedList.AddSorted(x));

            Console.WriteLine($"{nameof(dictionaryStrings)}: {dictionaryStrings.Count}");

            finished = false;

            TreeNode<(string StringKey, long NumOccurances, object LinkObject)> parent = null;

            while (!finished)
            {
                long totalCount = 0;

                var left = sortedLinkedList.RemoveFirst();

                Link<TreeNode<(string StringKey, long NumOccurences, object LinkObject)>> right = null;

                totalCount += (left?.Value.Value.NumOccurances).GetValueOrDefault();

                if (left != null)
                    right = sortedLinkedList.RemoveFirst();

                totalCount += (right?.Value.Value.NumOccurences).GetValueOrDefault();

                if (right == null)
                {
                    finished = true;
                }
                else
                {
                    parent = new TreeNode<(string StringKey, long NumOccurances, object LinkObject)>((StringKey: string.Empty, NumOccurances: totalCount,
                        LinkObject: null));

                    parent.SetChild(left.Value, Side.Left);
                    parent.SetChild(right.Value, Side.Right);

                    sortedLinkedList.AddSorted(parent);
                }
            }

            parent?.Print();

            var table = new Dictionary<string, (ulong Bits, int NumBits)>();

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

                //////if (tup.Character > 256)
                //////    _ = 0;

                table[tup.StringKey] = (Bits: bits, NumBits: len);
            });

            int counterBits = 0;

            int counterBytes = 0;

            byte[] writeBuffer = null;

            using (var fs = new FileStream($@"{inputPath}.huffman", FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                var buffer = ConvertToBytes(charsCount, 8);

                bw.Write(buffer);

                var tableArray = new (byte[] StringBytes, int SizeStringBytes, ulong Bits, int NumBits)[table.Count];

                buffer = ConvertToBytes(tableArray.Length, 2);

                bw.Write(buffer);

                var translationTableBuffer = new byte[(LenLength + SizeOfStringLengthField + LenCharacter) * tableArray.Length];

                var tableToArray = table.ToArray();

                for (int i = 0; i < tableArray.Length; i++)
                {
                    int baseIndex = i * (LenLength + SizeOfStringLengthField + LenCharacter);

                    var pair = tableToArray[i];

                    var stringBytes = Encoding.ASCII.GetBytes(pair.Key);

                    var tup = tableArray[i] = (StringBytes: stringBytes, SizeStringBytes: stringBytes.Length, pair.Value.Bits, pair.Value.NumBits);

                    CopyToBytesArray((ulong)stringBytes.Length, translationTableBuffer, baseIndex, SizeOfStringLengthField);
                    baseIndex += SizeOfStringLengthField;

                    ulong bits = tup.Bits;

                    CopyToBytesArray((ulong)tup.NumBits, translationTableBuffer, baseIndex, LenLength);
                    baseIndex += LenLength;

                    CopyToBytesArray(bits, translationTableBuffer, baseIndex, LenCharacter);
                }

                bw.Write(translationTableBuffer);

                for (int i = 0; i < tableArray.Length; i++)
                    bw.Write(tableArray[i].StringBytes);
                
                byte pack = 0;

                charsIndex = 0;

                using (var sr = new StreamReader(inputPath))
                    while (!sr.EndOfStream)
                    {
                        var charsBuffer = new char[BufferSize];

                        int readChars = await sr.ReadAsync(charsBuffer, 0, charsBuffer.Length);

                        charsIndex += readChars;

                        Console.WriteLine($"Read {charsIndex} characters from file {inputPath}");

                        int index = 0;

                        while (index < charsBuffer.Length)
                        {
                            (ulong Bits, int NumOccurrances)? tup = null;

                            int numChars = Math.Min(maxStringLength, charsBuffer.Length - index);

                            while (!tup.HasValue && numChars > 0)
                            {
                                string str = new string(charsBuffer, index, numChars);

                                if (table.ContainsKey(str))
                                {
                                    tup = table[str];

                                    index += numChars;

                                    if (str.Length > 1)
                                        _ = 0;
                                }

                                numChars--;
                            }

                            if (!tup.HasValue)
                                index++;

                            if (tup.HasValue)
                            {
                                ulong val = tup.Value.Bits;
                                int len = tup.Value.NumOccurrances;

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

                var table = new Dictionary<string, (ulong Bits, int NumBits)>();

                var translationTableBuffer = br.ReadBytes((SizeOfStringLengthField + LenLength + LenCharacter) * tableCount);

                var arrTable = new (string StringValue, byte SizeString, ulong Bits, int NumBits)[tableCount];

                for (int i = 0; i < tableCount; i++)
                {
                    int baseindex = i * (SizeOfStringLengthField + LenLength + LenCharacter);

                    byte sizeString = (byte)ConvertFromBytes(translationTableBuffer, baseindex, SizeOfStringLengthField);
                    baseindex += SizeOfStringLengthField;

                    int length = (int)ConvertFromBytes(translationTableBuffer, baseindex, LenLength);
                    baseindex += LenLength;

                    ulong val = (ulong)ConvertFromBytes(translationTableBuffer, baseindex, LenCharacter);

                    arrTable[i] = (StringValue: null, SizeString: sizeString, Bits: val, NumBits: length);
                }

                Tree<byte[]> tree = new Tree<byte[]>();

                for (int i = 0; i < arrTable.Length; i++)
                {
                    var tup = arrTable[i];

                    var bytesString = br.ReadBytes(tup.SizeString);

                    string StringKey = Encoding.ASCII.GetString(bytesString);

                    int[] bitsArray = null;

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
                        tree.Add(bytesString, bitsArray.Reverse().ToArray());
                }

                tree.Print();

                long charsIndex = 0;

                bool finished = false;

                var visitor = new TreeVisitor<byte[]>(tree);

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
                                byte[] bytesString = state.Val;

                                //char ch = state.Val;
                                //string str = state.Val;

                                //writeBuffer[writeIndex++] = (byte)str[0];
                                //writeBuffer[writeIndex++] = (byte)str[1];

                                if (bytesString.Length > 1)
                                    _ = 0;

                                for (int j = 0; j < bytesString.Length; j++)
                                {
                                    writeBuffer[writeIndex++] = bytesString[j];

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