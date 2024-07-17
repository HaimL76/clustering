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
        private static Action<TreeNode<T>> action;

        public static Action<DictionaryTreeNode<T>> NodeAction { get; set; }

        public DictionaryTreeNode(char keyPart0, T val) : base(val) => keyPart = keyPart0;

        private int numNodes;

        private DictionaryTreeNode<T>[] arr;

        private static Func<Link<DictionaryTreeNode<T>>, Link<DictionaryTreeNode<T>>, int> func = (x, y) =>
        {
            return 0;
        };

        private readonly SortedLinkedList<DictionaryTreeNode<T>, Link<DictionaryTreeNode<T>>> list
            = new SortedLinkedList<DictionaryTreeNode<T>, Link<DictionaryTreeNode<T>>>(func);

        private char keyPart;

        public (DictionaryTreeNode<T> TreeNodeObject, bool IsNew) Add(T val0, params char[] path)
            => Add(val0, 0, false, path);

        private (DictionaryTreeNode<T> TreeNodeObject, bool IsNew) Add(T val0, int index, bool isNew, params char[] path)
        {
            if (index < path.Length)
            {
                char ch = path[index];

                var tup = list.AddSorted(new DictionaryTreeNode<T>(ch, val0));

                var node = tup.LinkObject.Value;

                if (tup.IsNew)
                    isNew = true;

                return (node?.Add(val0, index + 1, isNew, path))
                    .GetValueOrDefault();
            }
            else
            {
                //val = val0;

                NodeAction?.Invoke(this);
            }

            return (TreeNodeObject: this, IsNew: isNew);
        }
    }

    public class DictionaryTree<T>
    {
        private readonly DictionaryTreeNode<T> root = new DictionaryTreeNode<T>('\0', default);

        public (DictionaryTreeNode<T> TreeNodeObject, bool IsNew) Add(T val0, params char[] path)
            => root.Add(val0, path);
    }
}
