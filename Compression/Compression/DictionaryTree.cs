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

        public static Func<DictionaryTreeNode<T>, bool> NodeIsLeaf { get; set; }

        public DictionaryTreeNode(char keyPart0, T val) : base(val) => keyPart = keyPart0;

        private int numNodes;

        private DictionaryTreeNode<T>[] arr;

        private static Func<Link<DictionaryTreeNode<T>>, Link<DictionaryTreeNode<T>>, int> func = (x, y) =>
            Comparer<char>.Default.Compare((x?.Value?.keyPart).GetValueOrDefault(), (y?.Value?.keyPart).GetValueOrDefault());

        private readonly SortedLinkedList<DictionaryTreeNode<T>, Link<DictionaryTreeNode<T>>> list
            = new SortedLinkedList<DictionaryTreeNode<T>, Link<DictionaryTreeNode<T>>>(func);

        private char keyPart;

        public char KeyPart => keyPart;

        public (DictionaryTreeNode<T> TreeNodeObject, char[] WordChars, bool IsNew) Add(T val0, char[] path, int begin, int end)
            => Add(val0, path, begin, end, begin, false);

        public static int counter0;

        private (DictionaryTreeNode<T> TreeNodeObject, char[] WordChars, bool IsNew) Add(T val0, char[] path, int begin, int end, int index, bool isNew)
        {
            if (end >= (path?.Length).GetValueOrDefault())
                throw new ArgumentException(nameof(end));

            if (index <= end)
            {
                char ch = path[index];

                var tup = list.AddSorted(new DictionaryTreeNode<T>(ch, val0));

                var node = tup.LinkObject.Value;

                if (tup.IsNew)
                    isNew = true;

                return (node?.Add(val0, path, begin, end, index + 1, isNew))
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

            char[] wordChars = new char[end - begin + 1];

            Array.Copy(path, begin, wordChars, 0, wordChars.Length);

            return (TreeNodeObject: this, WordChars: wordChars, IsNew: isNew);
        }

        public override IEnumerable<TreeNode<T>> GetChildElements()
        {
            foreach (var link in list.GetElements())
                yield return link.Value;
        }

        public override void Traverse(Stack<ulong> stack, Action<Stack<ulong>, T> action, int level = 0)
        {
            var childNodes = GetChildElements()
                .OfType<DictionaryTreeNode<T>>().ToList();

            int count = (childNodes?.Count).GetValueOrDefault();

            for (int i = 0; i < count; i++)
            {
                var node0 = childNodes[i];

                stack.Push(node0.KeyPart);

                node0.Traverse(stack, action, level + 1);

                var keyPart = stack.Pop();
            }

            if ((NodeIsLeaf?.Invoke(this)).GetValueOrDefault())
                action?.Invoke(stack, Value);
        }
    }

    public class DictionaryTree<T> : Tree<T>
    {
        public (DictionaryTreeNode<T> TreeNodeObject, char[] WordChars, bool IsNew) Add(T val0, char[] path, int index, int length)
        {
            root = root ?? new DictionaryTreeNode<T>('\0', default);

            var root0 = root as DictionaryTreeNode<T>;

            return root0.Add(val0, path, index, length);
        }
    }
}
