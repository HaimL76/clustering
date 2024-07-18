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
            Comparer<char>.Default.Compare((x?.Value?.keyPart).GetValueOrDefault(), (y?.Value?.keyPart).GetValueOrDefault());

        private readonly SortedLinkedList<DictionaryTreeNode<T>, Link<DictionaryTreeNode<T>>> list
            = new SortedLinkedList<DictionaryTreeNode<T>, Link<DictionaryTreeNode<T>>>(func);

        private char keyPart;

        public char KeyPart => keyPart;

        public (DictionaryTreeNode<T> TreeNodeObject, bool IsNew) Add(T val0, char[] path, int begin, int end)
            => Add(val0, path, begin, end, false);

        public static int counter0;

        private (DictionaryTreeNode<T> TreeNodeObject, bool IsNew) Add(T val0, char[] path, int begin, int end, bool isNew)
        {
            if (end >= (path?.Length).GetValueOrDefault())
                throw new ArgumentException(nameof(end));

            if (begin <= end)
            {
                char ch = path[begin];

                var tup = list.AddSorted(new DictionaryTreeNode<T>(ch, val0));

                var node = tup.LinkObject.Value;

                if (tup.IsNew)
                    isNew = true;

                return (node?.Add(val0, path, begin + 1, end, isNew))
                    .GetValueOrDefault();
            }
            else
            {
                //val = val0;

                NodeAction?.Invoke(this);

                int counter1 = counter0++;

                if ((counter1 % 100000) == 0)
                    Console.WriteLine($"[{counter1}], {this}");
            }

            return (TreeNodeObject: this, IsNew: isNew);
        }

        public override IEnumerable<TreeNode<T>> GetChildElements()
        {
            foreach (var link in list.GetElements())
                yield return link.Value;
        }

        public override void Traverse(Stack<ulong> stack, Action<Stack<ulong>, T> action)
        {
            foreach (var node in GetChildElements())
            {
                var node0 = (DictionaryTreeNode<T>) node;

                Console.WriteLine(node0.keyPart);
            }
        }
    }

    public class DictionaryTree<T> : Tree<T>
    {
        public (DictionaryTreeNode<T> TreeNodeObject, bool IsNew) Add(T val0, char[] path, int index, int length)
        {
            root = root ?? new DictionaryTreeNode<T>('\0', default);

            var root0 = root as DictionaryTreeNode<T>;

            return root0.Add(val0, path, index, length);
        }
    }
}
