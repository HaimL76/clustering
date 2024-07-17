using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Compression
{
    public class DictionaryTreeNode<T> : TreeNode<T>
    {
        public DictionaryTreeNode(char keyPart0, T val) : base(val) => keyPart = keyPart0;

        private int numNodes;

        private DictionaryTreeNode<T>[] arr;

        private static Func<Link<DictionaryTreeNode<T>>, Link<DictionaryTreeNode<T>>, int> func = (x, y) =>
        {
            return 0;
        };

        private readonly SortedLinkedList<DictionaryTreeNode<T>, Link<DictionaryTreeNode<T>>> list
            = new SortedLinkedList<DictionaryTreeNode<T>, Link<DictionaryTreeNode<T>>>(func);

        private T val;

        private char keyPart;

        public void Add(T val0, params char[] path)
            => Add(val0, 0, path);

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
                    result = arr[index] = new DictionaryTreeNode<T>(keyPart0, val);
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

                var node = list.AddSorted(new DictionaryTreeNode<T>(ch, val0))?.Value;

                node?.Add(val0, index + 1, path);
            }
            else
            {
                val = val0;
            }
        }
    }

    public class DictionaryTree<T>
    {
        private readonly DictionaryTreeNode<T> root = new DictionaryTreeNode<T>('\0', default);

        public void Add(T val0, params char[] path)
            => root.Add(val0, path);
    }
}
