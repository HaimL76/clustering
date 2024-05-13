using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    public class Link<T>
    {
        public Link(T val0) => val = val0;

        private Link<T> next;

        public Link<T> Next => next;

        private T val;

        public T Value => val;

        public void SetValue(T val0) => val = val0;

        public void SetNext(Link<T> next0) => next = next0;
    }

    public class LinkedList<T>
    {
        protected Link<T> root;

        public void Print()
        {
            Link<T> current = root;

            while (current != null)
            {
                Console.WriteLine(current.Value);

                current = current.Next;
            }    
        }
    }

    public class SortedLinkedList<T> : LinkedList<T>
    {
        private IComparer<T> comparer;

        public SortedLinkedList(IComparer<T> comp) => comparer = comp;

        public void AddSorted(T val)
        {
            if (root == null)
            {
                root = new Link<T>(val);
            }
            else
            {
                Link<T> current = root;
                Link<T> previous = null;

                bool finished = false;

                while (!finished)
                {
                    int c0 = comparer.Compare(val, current.Value);

                    if (c0 > 0)
                    {
                        finished = true;
                    }
                    else
                    {
                        previous = current;

                        current = current.Next;

                        if (current == null)
                            finished = true;
                    }
                }

                if (previous != null)
                {
                    Link<T> link = new Link<T>(val);

                    link.SetNext(previous.Next);

                    previous.SetNext(link);
                }
            }
        }
    }
}