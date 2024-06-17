using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    public class DictionaryTreeNode<T>
    {
        private DictionaryTreeNode<T>[] arr;

        private T val;

        public void Add(T val0, params char[] path)
            => Add(val0, 0, path);

        private void Add(T val0, int index, params char[] path)
        {
            if (index < path.Length)
            {
                char ch = path[index];

                int length = (arr?.Length).GetValueOrDefault();

                if (ch >= length)
                {
                    var arr0 = new DictionaryTreeNode<T>[ch + 1];

                    for (int i = 0; i < length; i++)
                        arr0[i] = arr[i];

                    arr = arr0;
                }

                arr[ch] = arr[ch] ?? new DictionaryTreeNode<T>();

                arr[ch].Add(val0, index + 1, path);
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
