using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            using (var fs = new FileStream(inputPath, FileMode.Open))
                while (!finished && fs.CanRead)
                //using (var ms = new MemoryStream(bytes, 0, bytes.Length, false, true))
                {
                    var buffer = new byte[1024 * 1024];

                    int read = await fs.ReadAsync(buffer, 0, buffer.Length);

                    if (read < 1)
                        finished = true;

                    int index = 0;

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

                        if (dictionary.ContainsKey(val))
                            _ = 0;

                        TreeNode<(long Val, long Count)> treeNode = null;

                        if (!dictionary.ContainsKey(val))
                        {
                            treeNode = new TreeNode<(long Val, long Count)>((Val: val, Count: 0));

                            //sortedLinkedList.AddSorted(treeNode);

                            dictionary.Add(val, treeNode);
                        }

                        treeNode = dictionary[val];

                        treeNode.SetValue((treeNode.Value.Val, Count: treeNode.Value.Count + 1));

                        //Console.WriteLine(val);

                        int counter0 = counter++;

                        if (counter0 % 1000 == 0)
                            Console.WriteLine(counter0);
                    }
                }

            dictionary.Values.ToList().ForEach(x => sortedLinkedList.AddSorted(x));

            sortedLinkedList.Print();

            Console.WriteLine($"{nameof(dictionary)}: {dictionary.Count}");

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

                            if (counter0 % 1000 == 0)
                                Console.WriteLine(counter0);
                        }
                    }
                }
            }
        }
    }
}