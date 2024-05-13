using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    public enum Side { Left, Right }

    internal class TreeNode<T>
    {
        public TreeNode(T val0) => val = val0;

        private TreeNode<T> left;

        private TreeNode<T> right;

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
