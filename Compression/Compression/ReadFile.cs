using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    internal class ReadFile
    {
        public static async void ReadFileAsync(string inputPath)
        {
            var dictionary = new Dictionary<long, TreeNode<(long Val, long Count)>>();

            bool finished = false;

            int counter = 0;

            string input = @"c:\gpp\MissingCardsSupreme-output.txt";

            input = @"c:\gpp\bookmarks_3_25_24.html";

            SortedLinkedList<TreeNode<(long Val, long Count)>> sortedLinkedList =
                new SortedLinkedList<TreeNode<(long Val, long Count)>>(new TreeNodeCountComparer<(long Val, long Count)>());

            //using (var fs = new FileStream(inputPath, FileMode.Open))
            //  while (!finished && fs.CanRead)
            //using (var ms = new MemoryStream(bytes, 0, bytes.Length, false, true))

            char ch = '\0';

            using (var sr = new StreamReader(inputPath))
                while (!sr.EndOfStream)
                //while ((ch = (char)sr.Read()) != -1)
                {
                    string line = await sr.ReadLineAsync();

                    for (int i = 0; i < line.Length; i++)
                    {
                        ////////////////////var buffer = new byte[1024 * 1024];

                        //////////////////byte[] buffer = null;

                        //////////////////    //int read = await fs.ReadAsync(buffer, 0, buffer.Length);

                        //////////////////    buffer = line?.ToCharArray().Select(ch => (byte)ch).ToArray();

                        //////////////////    int read = (buffer?.Length).GetValueOrDefault();

                        //////////////////    if (read < 1)
                        //////////////////        finished = true;

                        //////////////////    int index = 0;

                        //////////////////    while (!finished && index < read)
                        //////////////////    {
                        //////////////////        long val = 0;

                        //////////////////        for (int i = 0; i < 1; i++)
                        //////////////////        {
                        //////////////////            if (index < buffer.Length)
                        //////////////////            {
                        //////////////////                val <<= 8;

                        //////////////////                byte b = buffer[index++];

                        //////////////////                val |= b;
                        //////////////////            }
                        //////////////////        }

                        ch = line[i];

                        if (ch > 256)
                            ch = '-';//TODO:

                        int val = ch;

                        TreeNode<(long Val, long Count)> treeNode = null;

                        if (!dictionary.ContainsKey(val))
                        {
                            treeNode = new TreeNode<(long Val, long Count)>((Val: val, Count: 0));

                            //sortedLinkedList.AddSorted(treeNode);

                            dictionary.Add(val, treeNode);
                        }

                        treeNode = dictionary[val];

                        treeNode.SetValue((treeNode.Value.Val, Count: treeNode.Value.Count + 1));

                        //Console.WriteLine((char)val);

                        int counter0 = counter++;

                        if (false)//counter0 % 1000 == 0)
                            Console.WriteLine($"{nameof(counter0)}: {counter0}");
                        //////////////////    }
                    }
                }

            long count1 = dictionary.Sum(x => x.Value.Value.Count);

            dictionary.Values.ToList().ForEach(x => sortedLinkedList.AddSorted(x));

            //sortedLinkedList.Print();

            Console.WriteLine($"{nameof(dictionary)}: {dictionary.Count}");

            bool finished0 = false;

            int counter1 = 0;

            TreeNode<(long Val, long Count)> parent = null;

            while (!finished0)
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
                    finished0 = true;
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

            byte pack = 0;

            int counterBits = 0;

            int counterBytes = 0;

            byte[] arr2 = null;

            int totalCounter = 0;

            int tableCounter = 0;

            using (var fs = new FileStream($@"c:\html\{Path.GetFileNameWithoutExtension(inputPath)}.huffman", FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
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

                using (var sr = new StreamReader(inputPath))
                    while (!sr.EndOfStream)
                    //while ((ch = (char)sr.Read()) != -1)
                    {
                        string line = await sr.ReadLineAsync();

                        for (int i = 0; i < line.Length; i++)
                        {
                            ch = line[i];

                            if (ch < arr.Length)
                            {
                                var tup = arr[ch];

                                ulong val = tup.Val;
                                int len = tup.Length;

                                for (int j = 0; j < len; j++)
                                {
                                    byte bit = (byte)(val & 0x1);
                                    val >>= 1;
                                    pack <<= 1;

                                    pack |= bit;

                                    counterBits++;

                                    if (counterBits == 8)
                                    {
                                        counterBits = 0;

                                        int totalCounter0 = totalCounter++;

                                        if (false)//(totalCounter0 % 1000) == 0)
                                            Console.WriteLine($"[{totalCounter0}]: {pack}");

                                        (arr2 = arr2 ?? new byte[1024 * 1024])[counterBytes++] = pack;

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
            }

            using (var fs = new FileStream($@"c:\html\{Path.GetFileNameWithoutExtension(inputPath)}.huffman", FileMode.Open))
            using (var br = new BinaryReader(fs))
            {
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
            }

                return;

            var list = new List<TreeNode<(long Val, long Count)>>(dictionary.Values);

            while (list.Count > 0)
            {
                list.Sort(new TreeNodeCountComparer<TreeNode<(long Val, long Count)>>());

                var first = list[0];

                list.RemoveAt(0);

                var second = list[0];

                list.RemoveAt(0);



                long count0 = first.Value.Count + second.Value.Count;



                var newTreeNode = new TreeNode<(long Val, long Count)>((Val: 0, Count: count0));



                Console.WriteLine($"{nameof(list)}: {list.Count}");

            }



            long count = dictionary.Sum(pair => pair.Value.Value.Val);



            await NaiveCompressionAsync(dictionary.Keys.ToList());



            Console.WriteLine($"{nameof(count)}: {count}, {nameof(dictionary)}: {dictionary.Count}");

        }



        async private static Task NaiveCompressionAsync(IList<long> longs)

        {

            bool finished = false;



            int counter = 0;



            using (var fw = new FileStream(@"c:\gpp\MissingCardsSupreme-output.naive", FileMode.Create))

            {

                var buffer0 = new byte[12];



                int index = 0;



                var table = new Dictionary<long, int>();



                for (int i = 0; i < longs.Count; i++)

                {

                    long l = longs[i];

                    int index0 = index;



                    if (!table.ContainsKey(l))
                    {
                        table.Add(l, index0);

                        for (int j = 0; j < 8; j++)
                        {
                            buffer0[index++] = (byte)l;

                            l >>= 8;
                        }

                        for (int j = 0; j < 4; j++)
                        {
                            buffer0[index++] = (byte)index;

                            index >>= 8;
                        }
                        await fw.WriteAsync(buffer0, 0, buffer0.Length);
                    }
                }

                using (var fr = new FileStream(@"c:\gpp\MissingCardsSupreme-output.txt", FileMode.Open))
                {
                    while (!finished && fr.CanRead)

                    //using (var ms = new MemoryStream(bytes, 0, bytes.Length, false, true))
                    {
                        var buffer = new byte[1024 * 1024];

                        int read = await fr.ReadAsync(buffer, 0, buffer.Length);

                        if (read < 1)
                            finished = true;

                        index = 0;



                        long val = 0;

                        while (!finished && index < read)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                if (index < buffer.Length)
                                {
                                    val <<= 8;

                                    byte b = buffer[index++];

                                    val |= b;
                                }
                            }

                            if (table.ContainsKey(val))
                            {
                                int index0 = table[val];
                            }

                            if (!table.ContainsKey(val))
                                _ = 0;

                            //Console.WriteLine(val);

                            int counter0 = counter++;

                            if (false)//counter0 % 1000 == 0)
                                Console.WriteLine($"{nameof(counter0)}: {counter0}");
                        }
                    }
                }
            }
        }
    }
}