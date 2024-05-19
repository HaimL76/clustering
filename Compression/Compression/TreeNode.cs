using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    public enum Side { Left, Right }

    public class TreeNode<T>
    {
        public TreeNode(T val0) => val = val0;

        private TreeNode<T> left;

        private TreeNode<T> right;

        public TreeNode<T> Left => left;
        public TreeNode<T> Right => right;

        private T val;

        public T Value => val;

        public void SetValue(T val0) => val = val0;

        public void SetChild(TreeNode<T> child, Side side)
        {
            if (side == Side.Left)
                left = child;
            else
                right = child;
        }

        public override string ToString() => val?.ToString();

        public void Print() => Traverse(new Stack<ulong>(), (stack, val) =>
        {
            string str = string.Join(string.Empty, stack.Reverse());

            Console.WriteLine($"{str}, {val}");
        });

        public void Traverse(Stack<ulong> stack, Action<Stack<ulong>, T> action)
        {
            if (left == null && right == null)
            {
                action?.Invoke(stack, val);
            }
            else
            {
                if (left != null)
                {
                    stack.Push(0);

                    left.Traverse(stack, action);

                    _ = stack.Pop();
                }

                if (right != null)
                {
                    stack.Push(1);

                    right.Traverse(stack, action);

                    _ = stack.Pop();
                }
            }
        }
    }

    public class Tree<T>
    {
        private TreeNode<T> root;

        public TreeNode<T> Root => root;

        public (T Val, bool Found)  GetValue(params int[] sides)
        {
            (T Val, bool Found) result = (Val: default(T), false);

            if (root != null)
            {
                var next = root;

                int index = 0;

                while (next != null && index < sides.Length)
                {
                    var side = sides[index++];

                    next = side == 0 ? next.Left : next.Right;
                }

                if (next != null)
                    result = (Val: next.Value, true);
            }

            return result;
        }

        public void Add(T val, params int[] sides)
        {
            root = root ?? new TreeNode<T>(default);

            var node = root;

            for (int i = 0; i < sides.Length; i++)
            {
                int side = sides[i];

                TreeNode<T> next = side == 0 
                    ? node.Left : node.Right;

                if (next == null)
                {
                    next = new TreeNode<T>(default);

                    if (side == 0)
                        node.SetChild(next, Side.Left);
                    else
                        node.SetChild(next, Side.Right);
                }

                node = next;
            }

            node.SetValue(val);
        }

        public void Print() => root?.Print();
    }

    public class TreeVisitor<T>
    {
        public TreeVisitor(Tree<T> tree0) => tree = tree0;

        private Tree<T> tree;

        private TreeNode<T> current;

        private (T Val, bool Status)? val0;

        public (T Val, bool Status) Value => val0.GetValueOrDefault();

        public void Visit(Side side)
        {
            val0 = null;

            current = current ?? tree.Root;

            current = side == Side.Left ? current.Left : current.Right;

            if (current.Left == null && current.Right == null)
            {
                val0 = (Val: current.Value, Status: true);

                current = null;
            }
        }
    }

    public class TreeNodeCountComparer<T> : IComparer<TreeNode<(long Val, long Count)>>
    {
        int IComparer<TreeNode<(long Val, long Count)>>.Compare(TreeNode<(long Val, long Count)> x, TreeNode<(long Val, long Count)> y)
        {
            long countX = (x?.Value.Count).GetValueOrDefault();

            long countY = (y?.Value.Count).GetValueOrDefault();

            long countDifference = countX - countY;

            return countDifference == 0
                ? 0 : countDifference > 0
                ? 1 : -1;
        }
    }
}
