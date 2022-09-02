using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;

namespace Nino.Shared
{
    [Serializable]
    public class UncheckedStack<T> : IEnumerable<T>,
        System.Collections.ICollection,
        IReadOnlyCollection<T>
    {
        private T[] _array; // Storage for UncheckedStack elements
        private int _size; // Number of items in the UncheckedStack.
        private int _version; // Used to keep enumerator in sync w/ collection.
        private Object _syncRoot;

        private const int _defaultCapacity = 4;
        static T[] _emptyArray = new T[0];

        public UncheckedStack()
        {
            _array = _emptyArray;
            _size = 0;
            _version = 0;
        }

        // Create a UncheckedStack with a specific initial capacity.  The initial capacity
        // must be a non-negative number.
        public UncheckedStack(int capacity)
        {
            _array = new T[capacity];
            _size = 0;
            _version = 0;
        }

        // Fills a UncheckedStack with the contents of a particular collection.  The items are
        // pushed onto the UncheckedStack in the same order they are read by the enumerator.
        public UncheckedStack(IEnumerable<T> collection)
        {
            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                int count = c.Count;
                _array = new T[count];
                c.CopyTo(_array, 0);
                _size = count;
            }
            else
            {
                _size = 0;
                _array = new T[_defaultCapacity];

                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Push(en.Current);
                    }
                }
            }
        }

        public int Count => _size;

        bool System.Collections.ICollection.IsSynchronized => false;

        Object System.Collections.ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }

                return _syncRoot;
            }
        }

        // Removes all Objects from the UncheckedStack.
        public void Clear()
        {
            Array.Clear(_array, 0,
                _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            _size = 0;
            _version++;
        }

        public bool Contains(T item)
        {
            int count = _size;

            EqualityComparer<T> c = EqualityComparer<T>.Default;
            while (count-- > 0)
            {
                if (((Object)item) == null)
                {
                    if (((Object)_array[count]) == null)
                        return true;
                }
                else if (_array[count] != null && c.Equals(_array[count], item))
                {
                    return true;
                }
            }

            return false;
        }

        // Copies the UncheckedStack into an array.
        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_array, 0, array, arrayIndex, _size);
            Array.Reverse(array, arrayIndex, _size);
        }

        void System.Collections.ICollection.CopyTo(Array array, int arrayIndex)
        {
            Array.Copy(_array, 0, array, arrayIndex, _size);
            Array.Reverse(array, arrayIndex, _size);
        }

        // Returns an IEnumerator for this UncheckedStack.
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void TrimExcess()
        {
            int threshold = (int)(((double)_array.Length) * 0.9);
            if (_size < threshold)
            {
                T[] newarray = new T[_size];
                Array.Copy(_array, 0, newarray, 0, _size);
                _array = newarray;
                _version++;
            }
        }

        // Returns the top object on the UncheckedStack without removing it.  If the UncheckedStack
        // is empty, Peek throws an InvalidOperationException.
        public T Peek()
        {
            return _array[_size - 1];
        }

        // Pops an item from the top of the UncheckedStack.  If the UncheckedStack is empty, Pop
        // throws an InvalidOperationException.
        public T Pop()
        {
            _version++;
            T item = _array[--_size];
            _array[_size] = default(T); // Free memory quicker.
            return item;
        }

        // Pushes an item to the top of the UncheckedStack.
        public void Push(T item)
        {
            if (_size == _array.Length)
            {
                T[] newArray = new T[(_array.Length == 0) ? _defaultCapacity : 2 * _array.Length];
                Array.Copy(_array, 0, newArray, 0, _size);
                _array = newArray;
            }

            _array[_size++] = item;
            _version++;
        }

        // Copies the UncheckedStack to an array, in the same order Pop would return the items.
        public T[] ToArray()
        {
            T[] objArray = new T[_size];
            int i = 0;
            while (i < _size)
            {
                objArray[i] = _array[_size - i - 1];
                i++;
            }

            return objArray;
        }

        [Serializable()]
        [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes",
            Justification = "not an expected scenario")]
        public struct Enumerator : IEnumerator<T>,
            System.Collections.IEnumerator
        {
            private UncheckedStack<T> _stack;
            private int _index;
            private int _version;
            private T currentElement;

            internal Enumerator(UncheckedStack<T> UncheckedStack)
            {
                _stack = UncheckedStack;
                _version = _stack._version;
                _index = -2;
                currentElement = default(T);
            }

            public void Dispose()
            {
                _index = -1;
            }

            public bool MoveNext()
            {
                bool retval;
                if (_index == -2)
                {
                    // First call to enumerator.
                    _index = _stack._size - 1;
                    retval = (_index >= 0);
                    if (retval)
                        currentElement = _stack._array[_index];
                    return retval;
                }

                if (_index == -1)
                {
                    // End of enumeration.
                    return false;
                }

                retval = (--_index >= 0);
                if (retval)
                    currentElement = _stack._array[_index];
                else
                    currentElement = default(T);
                return retval;
            }

            public T Current => currentElement;

            Object System.Collections.IEnumerator.Current => currentElement;

            void System.Collections.IEnumerator.Reset()
            {
                _index = -2;
                currentElement = default(T);
            }
        }
    }
}