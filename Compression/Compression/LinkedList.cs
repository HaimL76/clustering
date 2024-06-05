using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
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
        public Func<T, string> Format { get; set; }

        protected LType head, tail;

        public void Print()
        {
            LType current = head;

            while (current != null)
            {
                string str = Format?.Invoke(current.Value) ?? $"{current.Value}";

                //Console.WriteLine($"[{counter++}], {current.Value}");
                Console.Write($"{str},");

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

        public long GetCount()
        {
            int counter = 0;

            var link = head;

            while (link != null)
            {
                counter++;

                link = (LType) link.Next;
            }

            return counter;
        }
    }

    public class DoubleLinkedList<T> : LinkedList<T, DoubleLink<T>>
    {
    }

    public class SortedOneWayLinkedList<T> : SortedLinkedList<T, Link<T>>
    {
        public SortedOneWayLinkedList(IComparer<T> comp) : base(comp) => _ = 0;
    }

    public class SortedDoubleLinkedList<T> : SortedLinkedList<T, DoubleLink<T>>
    {
        public SortedDoubleLinkedList(IComparer<T> comp) : base(comp) => _ = 0;

        public void Update(DoubleLink<T> link)
        {
            var next = link?.Next;

            if (next != null)
            {
                int comp = Comparer.Compare(link.Value, next.Value);

                if (comp > 0)
                {
                    var prev = link.Prev ?? head;

                    prev.SetNext(prev.Next);

                    AddSorted(link.Value, prev);
                }
            }
        }
    }

    public class SortedBuffer<T> : SortedLinkedList<T, DoubleLink<T>>
    {
        private int size;

        private int count;

        public SortedBuffer(IComparer<T> comp, int sz) : base(comp) => size = sz;

        public override Link<T> AddSorted(T val, DoubleLink<T> start = null)
        {
            DoubleLink<T> newLink = null;

            if (head == null)
            {
                if (start != null)
                    throw new ApplicationException(nameof(head));

                head = tail = new DoubleLink<T>(val);
            }
            else
            {
                DoubleLink <T> current = start ?? head;

                bool addVal = count < size || (Comparer?.Compare(val, head.Value))
                    .GetValueOrDefault() > 0;

                if (addVal)
                {
                    newLink = new DoubleLink<T>(val);

                    if (count > size)
                    {
                        _ = RemoveFirst();

                        count--;
                    }

                    int counter = 0;

                    int count0 = count;

                    while (count0 == count && current != null && counter++ < size)
                    {
                        int comp = (Comparer?.Compare(val, current.Value))
                            .GetValueOrDefault();

                        if (comp < 0)
                        {
                            var prev = current.Prev;

                            newLink.SetNext(current);
                            newLink.SetPrev(prev);

                            prev?.SetNext(newLink);

                            if (current == head)
                                head = newLink;

                            count++;
                        }
                    }

                    if (count0 == count)
                    {
                        tail.SetNext(newLink);
                        newLink.SetPrev(tail);

                        tail = newLink;

                        count++;
                    }
                }
            }

            return newLink;
        }

        public void Replace(DoubleLink<T> link)
        {
            var next = link?.Next;

            if (next != null)
            {
                int comp = Comparer.Compare(link.Value, next.Value);

                if (comp > 0)
                {
                    var prev = link.Prev ?? head;

                    prev.SetNext(prev.Next);

                    AddSorted(link.Value, prev);
                }
            }
        }
    }

    public class SortedLinkedList<T, LType> : LinkedList<T, LType>
        where LType : Link<T>, new()
    {
        private IComparer<T> comparer;

        public IComparer<T> Comparer => comparer;

        public SortedLinkedList(IComparer<T> comp) => comparer = comp;

        private int counter;

        public virtual Link<T> AddSorted(T val, LType start = null)
        {
            var newLink = new LType();
            newLink.SetValue(val);

            int counter0 = counter++;

            if ((counter0 % 100000) == 0)
                Console.WriteLine($"[{counter0}], {nameof(val)}: {val}");

            if (head == null)
            {
                if (start != null)
                    throw new ApplicationException(nameof(head));

                head = tail = newLink;
            }
            else
            {
                LType current = start ?? head;
                LType previous = current;

                bool added = false;

                bool finished = false;

                while (!finished && current != null)
                {                                                                  
                    int c0 = comparer.Compare(val, current.Value);

                    if (c0 < 0)
                    {
                        newLink.SetNext(current);

                        if (previous.Next == current)
                            previous.SetNext(newLink);

                        if (head == current)
                            head = (LType)newLink;

                        finished = added = true;
                    }
                    else if (c0 > 0)
                    {
                        previous = current;

                        current = (LType)current.Next;
                    }
                    else
                    {
                        finished = true;
                    }
                }

                if (!added)
                {
                    tail.SetNext(newLink);

                    tail = newLink;
                }
            }

            return newLink;
        }
    }
}