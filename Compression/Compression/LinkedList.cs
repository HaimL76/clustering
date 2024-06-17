﻿using System;
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

        public long GetCount()
        {
            var current = head;

            long counter = 0;

            while (current != null)
            {
                counter++;

                current = current.Next;
            }

            return counter;
        }

        public bool IsEmpty() => head == null;

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

    public class SortedLinkedList<T> : LinkedList<T>
    {
        private IComparer<T> comparer;

        public SortedLinkedList(IComparer<T> comp) => comparer = comp;

        public void UpdateSorted(Link<T> link)
        {
            var val = link.Value;

            Link<T> current = link.Next;
            Link<T> previous = current;

            bool updated = false;

            while (!updated && current != null)
            {
                int c0 = comparer.Compare(val, current.Value);

                if (c0 < 0)
                {
                    link.SetNext(previous.Next);

                    previous.SetNext(link);

                    updated = true;
                }
                else
                {
                    previous = current;

                    current = current.Next;
                }
            }

            if (!updated)
            {
                tail.SetNext(link);

                tail = link;
            }
        }

        private int counter;

        public void AddSorted(T val)
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
        }
    }
}