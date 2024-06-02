using System;
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
        public Link(T val0) => val = val0;

        private Link<T> next;

        public Link<T> Next => next;

        private T val;

        public T Value => val;

        public void SetValue(T val0) => val = val0;

        public void SetNext(Link<T> next0) => next = next0;
    }

    public class DoubleLink<T>
    {
        public DoubleLink(T val0) => val = val0;

        private DoubleLink<T> next;
        private DoubleLink<T> prev;

        public DoubleLink<T> Next => next;
        public DoubleLink<T> Prev => prev;

        private T val;

        public T Value => val;

        public void SetValue(T val0) => val = val0;

        public void SetNext(DoubleLink<T> next0) => next = next0;
        public void SetPrev(DoubleLink<T> prev0) => prev = prev0;
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
                //Console.WriteLine($"[{counter++}], {current.Value}");
                Console.Write($"{current.Value},");

                current = current.Next;
            }

            Console.WriteLine();
        }

        public Link<T> RemoveFirst() => RemoveFirst(1)?.FirstOrDefault();

        public IList<Link<T>> RemoveFirst(int count)
        {
            Link<T> current = head;
            //Link<T> previous = current;

            IList<Link<T>> list = null;

            int counter = count;

            while (current != null && count-- > 0)
            {
                (list = list ?? new List<Link<T>>()).Add(current);

                //previous = current;
                //Console.WriteLine($"[{counter++}], {current.Value}");

                current = current.Next;
            }

            head = current;

            return list;
        }
    }

    public class DoubleLinkedList<T>
    {
        protected DoubleLink<T> head, tail;

        public void Print()
        {
            int counter = 0;

            DoubleLink<T> current = head;

            while (current != null)
            {
                //Console.WriteLine($"[{counter++}], {current.Value}");
                Console.Write($"{current.Value},");

                current = current.Next;
            }

            Console.WriteLine();
        }

        public DoubleLink<T> RemoveFirst() => RemoveFirst(1)?.FirstOrDefault();

        public IList<DoubleLink<T>> RemoveFirst(int count)
        {
            DoubleLink<T> current = head;
            //Link<T> previous = current;

            IList<DoubleLink<T>> list = null;

            int counter = count;

            while (current != null && count-- > 0)
            {
                (list = list ?? new List<DoubleLink<T>>()).Add(current);

                //previous = current;
                //Console.WriteLine($"[{counter++}], {current.Value}");

                current = current.Next;
            }

            head = current;
            head.SetPrev(null);

            return list;
        }
    }

    public class SortedLinkedList<T> : LinkedList<T>
    {
        private IComparer<T> comparer;

        public SortedLinkedList(IComparer<T> comp) => comparer = comp;

        private int counter;

        public Link<T> AddSorted(T val)
        {
            var newLink = new Link<T>(val);

            int counter0 = counter++;

            if ((counter0 % 1000) == 0)
                Console.WriteLine($"[{counter0}], {nameof(val)}: {val}");

            if (head == null)
            {
                head = tail = newLink;
            }
            else
            {
                Link<T> current = head;
                Link<T> previous = current;

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
                            head = newLink;

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

            return newLink;
        }
    }

    public class SortedDoubleLinkedList<T> : DoubleLinkedList<T>
    {
        private IComparer<T> comparer;

        public SortedDoubleLinkedList(IComparer<T> comp) => comparer = comp;

        private int counter;

        public void Update(DoubleLink<T> doubleLink)
        {
            if (doubleLink.Next != null && comparer.Compare(doubleLink.Value, doubleLink.Next.Value) > 0)
            {
                var prev = doubleLink.Prev ?? head;

                prev.SetNext(doubleLink.Next);

                DoubleLink<T> current = prev;
                DoubleLink<T> previous = current;

                bool added = false;

                while (!added && current != null)
                {
                    int c0 = comparer.Compare(doubleLink.Value, current.Value);

                    if (c0 < 0)
                    {
                        doubleLink.SetNext(current);
                        current.SetPrev(doubleLink);

                        if (previous.Next == current)
                        {
                            previous.SetNext(doubleLink);
                            doubleLink.SetPrev(previous);
                        }

                        if (head == current)
                            head = doubleLink;

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
                    tail.SetNext(doubleLink);
                    doubleLink.SetPrev(tail);

                    tail = doubleLink;
                }
            }

            if (doubleLink.Prev != null && comparer.Compare(doubleLink.Value, doubleLink.Prev.Value) < 0)
            {

            }
        }

        public DoubleLink<T> AddSorted(T val)
        {
            var newLink = new DoubleLink<T>(val);

            int counter0 = counter++;

            if ((counter0 % 1000) == 0)
                Console.WriteLine($"[{counter0}], {nameof(val)}: {val}");

            if (head == null)
            {
                head = tail = newLink;
            }
            else
            {
                DoubleLink<T> current = head;
                DoubleLink<T> previous = current;

                bool added = false;

                while (!added && current != null)
                {
                    int c0 = comparer.Compare(val, current.Value);

                    if (c0 < 0)
                    {
                        newLink.SetNext(current);
                        current.SetPrev(newLink);

                        if (previous.Next == current)
                        {
                            previous.SetNext(newLink);
                            newLink.SetPrev(previous);
                        }

                        if (head == current)
                            head = newLink;

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
                    newLink.SetPrev(tail);

                    tail = newLink;
                }
            }

            return newLink;
        }
    }
}