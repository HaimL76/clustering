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
        protected Link<T> head, tail;

        public void Print()
        {
            int counter = 0;

            Link<T> current = head;

            while (current != null)
            {
                Console.WriteLine($"[{counter++}], {current.Value}");

                current = current.Next;
            }    
        }
    }

    public class SortedLinkedList<T> : LinkedList<T>
    {
        private IComparer<T> comparer;

        public SortedLinkedList(IComparer<T> comp) => comparer = comp;

        public void AddSorted(T val, Link<T> start = null)
        {
            var newLink = new Link<T>(val);

            if (head == null)
            {
                head = tail = newLink;
            }
            else
            {
                Link<T> current = head;
                Link<T> previous = head;

                bool added = false;

                while (!added && current != null)
                {
                    int c0 = comparer.Compare(val, current.Value);

                    if (c0 < 0)
                    {
                        newLink.SetNext(previous.Next);

                        previous.SetNext(newLink);

                        added = true;
                    }
                    else
                    {
                        previous = current;

                        current = current.Next;
                    }
                }

                if (!added)
                {
                    tail.SetNext(newLink);

                    tail = newLink;
                }
            }
        }
    }
}