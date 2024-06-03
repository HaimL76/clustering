using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    public class Link<T>
    {
        public Link() => _ = 0;

        public Link(T val0) => val = val0;

        private Link<T> next;

        public Link<T> Next => next;

        private T val;

        public T Value => val;

        public void SetValue(T val0) => val = val0;

        public virtual void SetNext(Link<T> next0) => next = next0;
    }

    public class DoubleLink<T> : Link<T>
    {
        public DoubleLink() : base() => _ = 0;

        public DoubleLink(T val0) : base(val0) => _ = 0;

        private DoubleLink<T> prev;

        public DoubleLink<T> Prev => prev;

        public override void SetNext(Link<T> next0)
        {
            base.SetNext(next0);

            (next0 as DoubleLink<T>)?.SetPrev(this);
        }

        public void SetPrev(DoubleLink<T> prev0) => prev = prev0;
    }

    public class LinkedList<T, LType>
        where LType : Link<T>
    {
        protected LType head, tail;

        public void Print()
        {
            LType current = head;

            while (current != null)
            {
                //Console.WriteLine($"[{counter++}], {current.Value}");
                Console.Write($"{current.Value},");

                current = (LType)current.Next;
            }

            Console.WriteLine();
        }

        public LType RemoveFirst() => RemoveFirst(1)?.FirstOrDefault();

        public IList<LType> RemoveFirst(int count)
        {
            LType current = head;
            //Link<T> previous = current;

            IList<LType> list = null;

            int counter = count;

            while (current != null && count-- > 0)
            {
                (list = list ?? new List<LType>()).Add(current);

                //previous = current;
                //Console.WriteLine($"[{counter++}], {current.Value}");

                current = (LType)current.Next;
            }

            head = current;

            return list;
        }
    }

    public class DoubleLinkedList<T> : LinkedList<T, DoubleLink<T>>
    {
    }

    public class SortedDoubleLinkedList<T> : SortedLinkedList<T, DoubleLink<T>>
    {
        public SortedDoubleLinkedList(IComparer<T> comp) : base(comp) => _ = 0;

        public void Update(DoubleLink<T> link)
        {
            var prev = link.Prev ?? head;

            prev.SetNext(prev.Next);

            AddSorted(link.Value, prev);
        }
    }

    public class SortedLinkedList<T, LType> : LinkedList<T, LType>
        where LType : Link<T>, new()
    {
        private IComparer<T> comparer;

        public SortedLinkedList(IComparer<T> comp) => comparer = comp;

        private int counter;

        public Link<T> AddSorted(T val, LType start = null)
        {
            var newLink = new LType();
            newLink.SetValue(val);

            int counter0 = counter++;

            if ((counter0 % 1000) == 0)
                Console.WriteLine($"[{counter0}], {nameof(val)}: {val}");

            if (head == null)
            {
                if (start != null)
                    throw new ApplicationException(nameof(head));

                head = tail = (LType)newLink;
            }
            else
            {
                LType current = start ?? head;
                LType previous = current;

                bool added = false;

                while (!added && current != null)
                {                                                                  
                    int c0 = comparer.Compare(val, current.Value);

                    if (c0 < 0)
                    {
                        newLink.SetNext(current);

                        if (previous.Next == current)
                            previous.SetNext(newLink);

                        if (head == current)
                            head = (LType)newLink;

                        added = true;
                    }
                    else
                    {
                        previous = current;

                        current = (LType)current.Next;
                    }
                }

                if (!added)
                {
                    tail.SetNext(newLink);

                    tail = (LType)newLink;
                }
            }

            return newLink;
        }
    }
}