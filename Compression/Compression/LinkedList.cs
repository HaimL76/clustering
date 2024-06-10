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

        public virtual void Disconnect() => next = null;
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

        public override void Disconnect()
        {
            base.Disconnect();

            prev = null;
        }
    }

    public class LinkedList<T, LType>
        where LType : Link<T>
    {
        public Func<T, string> Format { get; set; }

        protected LType head, tail;

        public List<T> ToList()
        {
            var list = new List<T>();

            LType current = head;

            while (current != null)
            {
                list.Add(current.Value);

                current = (LType)current.Next;
            }

            return list;
        }

        public void Print(bool sepearetLines = true)
        {
            LType current = head;

            long counter = 0;

            while (current != null)
            {
                string str = Format?.Invoke(current.Value) ?? $"{current.Value}";

                Console.Write($"{str}");

                str = sepearetLines ? $"[{counter++}]{str}{Environment.NewLine}" : $"{str},";

                Console.Write(str);

                current = (LType)current.Next;
            }

            Console.WriteLine();
        }

        public LType RemoveFirst() => RemoveFirst(1)?.FirstOrDefault();

        public IList<LType> RemoveFirst(int count)
        {
            LType current = head;

            IList<LType> list = null;

            while (current != null && count-- > 0)
            {
                var next = (LType)current.Next;

                current.Disconnect();

                (list = list ?? new List<LType>()).Add(current);

                current = next;
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

        public override void AddSorted(DoubleLink<T> link, DoubleLink<T> start = null)
        {
            if (head == null)
            {
                if (start != null)
                    throw new ApplicationException(nameof(head));

                head = tail = link;

                count++;
            }
            else
            {
                DoubleLink<T> current = start ?? head;

                int count0 = count;

                bool finished = false;

                while (!finished && count0 == count && current != null)
                {
                    int comp = (Comparer?.Compare(link.Value, current.Value))
                        .GetValueOrDefault();

                    var prev = current.Prev;

                    if (comp < 0)
                    {
                        if (prev == null && count > size)
                        {
                            finished = true;
                        }
                        else
                        {
                            link.SetNext(current);

                            if (prev == null)//This means current is the head.
                            {
                                head = link;
                            }
                            else
                            {
                                prev.SetNext(link);
                                link.SetPrev(prev);

                                link.SetNext(current);
                                current.SetPrev(link);
                            }

                            count++;
                        }
                    }
                    else if (comp > 0)
                    {
                        current = (DoubleLink<T>)current.Next;
                    }
                    else
                    {
                        finished = true;
                    }
                }

                if (count0 == count)
                {
                    tail.SetNext(link);
                    link.SetPrev(tail);

                    tail = link;

                    count++;
                }
            }

            if (count > size)
            {
                var next = (DoubleLink<T>)head.Next;

                head.SetNext(null);

                head = next;
                head.SetPrev(null);

                count--;
            }
        }

        public void Replace(DoubleLink<T> link)
        {
            return;
            if (link.Next == null)
            {
                AddSorted(link);
            }
            else
            {
                var next = link?.Next;

                if (next != null)
                {
                    int comp = Comparer.Compare(link.Value, next.Value);

                    if (comp > 0)
                    {
                        var prev = link.Prev;

                        prev?.SetNext(link.Next);
                        (link.Next as DoubleLink<T>)?.SetPrev(prev);

                        link.Disconnect();

                        AddSorted(link);
                    }
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

        public Link<T> AddSorted(T val, LType start = null)
        {
            var newLink = new LType();
            newLink.SetValue(val);

            AddSorted(newLink, start);

            return newLink;
        }

        public virtual void AddSorted(LType link, LType start = null)
        {
            var val = link.Value;

            int counter0 = counter++;

            if ((counter0 % 100000) == 0)
                Console.WriteLine($"[{counter0}], {nameof(val)}: {val}");

            if (head == null)
            {
                if (start != null)
                    throw new ApplicationException(nameof(head));

                head = tail = link;
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
                        link.SetNext(current);

                        if (previous.Next == current)
                            previous.SetNext(link);

                        if (head == current)
                            head = link;

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
                    tail.SetNext(link);

                    tail = link;
                }
            }
        }
    }
}