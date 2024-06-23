using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Compression
{
    public class DictionaryTreeNode<T>
    {
        public DictionaryTreeNode(char keyPart0) => keyPart = keyPart0;

        private int numNodes;

        private DictionaryTreeNode<T>[] arr;

        private T val;

        private char keyPart;

        public void Add(T val0, params char[] path)
            => Add(val0, 0, path);

        private static Func<char, int, int> HashKey = (ch, num) =>
        {
            if (num < 10)
                return ch % 10;

            return ch;
        };

        private DictionaryTreeNode<T> PutFirstAvailablePlace(char keyPart0, int startingIndex)
        {
            int length = (arr?.Length).GetValueOrDefault();

            if (length <= startingIndex)
            {
                var arr0 = new DictionaryTreeNode<T>[startingIndex + 1];

                for (int i = 0; i < length; i++)
                    arr0[i] = arr[i];

                arr = arr0;
            }

            DictionaryTreeNode<T> result = null;

            int counter = 0;

            int index = startingIndex;

            while (result == null && counter++ < arr.Length)
            {
                DictionaryTreeNode<T> node = arr[index];

                if (node == null)
                    result = arr[index] = new DictionaryTreeNode<T>(keyPart0);
                else if (node.keyPart == keyPart0)
                    result = node;

                index++;

                index %= arr.Length;
            }

            return result;
        }

        private void Add(T val0, int index, params char[] path)
        {
            if (index < path.Length)
            {
                char ch = path[index];

                int hash = HashKey(ch, numNodes);

                var node = PutFirstAvailablePlace(ch, hash);

                if (node == null)
                    _ = 0;
            }
            else
            {
                val = val0;
            }
        }
    }

    public class DictionaryTree<T>
    {
        private readonly DictionaryTreeNode<T> root = new DictionaryTreeNode<T>('\0');

        public void Add(T val0, params char[] path)
            => root.Add(val0, path);
    }
}
