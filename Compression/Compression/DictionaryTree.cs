using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    public class DictionaryTreeNode<T>
    {
        private int numNodes;

        private DictionaryTreeNode<T>[] arr;

        private T val;

        public void Add(T val0, params char[] path)
            => Add(val0, 0, path);

        private static Func<char, int, int> HashKey = (ch, num) =>
        {
            if (num < 10)
                return ch % 10;

            return (int)ch;
        };

        private void Add(T val0, int index, params char[] path)
        {
            if (index < path.Length)
            {
                char ch = path[index];

                int hash = HashKey(ch, numNodes);

                int length = (arr?.Length).GetValueOrDefault();

                if (hash >= length)
                {
                    var arr0 = new DictionaryTreeNode<T>[hash + 1];

                    for (int i = 0; i < length; i++)
                        arr0[i] = arr[i];

                    arr = arr0;
                }

                int numNodes0 = numNodes;

                int index0 = hash;

                DictionaryTreeNode<T> node = null;

                int counter = 0;

                while (numNodes0 == numNodes && counter++ < arr.Length)
                {
                    node = arr[index0];

                    if (node == null)
                    {
                        node = arr[index0] = new DictionaryTreeNode<T>();

                        numNodes++;
                    }

                    index0++;

                    if (index0 == arr.Length)
                        index0 = 0;
                }

                if (numNodes0 == numNodes)
                    _ = 0;

                node.Add(val0, index + 1, path);
            }
            else
            {
                val = val0;
            }
        }
    }

    public class DictionaryTree<T>
    {
        private readonly DictionaryTreeNode<T> root = new DictionaryTreeNode<T>();

        public void Add(T val0, params char[] path)
            => root.Add(val0, path);
    }
}
