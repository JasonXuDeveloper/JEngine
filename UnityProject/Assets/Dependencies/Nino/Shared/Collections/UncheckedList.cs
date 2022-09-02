using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable CognitiveComplexity
namespace Nino.Shared
{
    [Serializable]
    public class UncheckedList<T> : IList<T>, System.Collections.IList, IReadOnlyList<T>
    {
        private const int DefaultCapacity = 4;
        /// <summary>
        /// here we made it public for faster access than indexer, but we dont recommend modify this
        /// direct calling is way faster in debug mode, when in release mode, calling indexer has same performance as direct call
        /// https://stackoverflow.com/questions/17105773/poor-c-sharp-optimizer-performance
        /// </summary>
        internal T[] items;
        private int size;
        private int version;
        [NonSerialized] private Object syncRoot;

        static readonly T[]
            EmptyArray =
                Array.Empty<T>(); // Constructs a UncheckedList. The UncheckedList is initially empty and has a capacity

        // of zero. Upon adding the first element to the UncheckedList the capacity is
        // increased to 16, and then increased in multiples of two as required.
        public UncheckedList()
        {
            items = EmptyArray;
        } // Constructs a UncheckedList with a given initial capacity. The UncheckedList is

        // initially empty, but will have room for the given number of elements
        // before any reallocations are required.
        // 
        public UncheckedList(int capacity)
        {
            if (capacity == 0)
                items = EmptyArray;
            else
                items = new T[capacity];
        } // Constructs a UncheckedList, copying the contents of the given collection. The

        // size and capacity of the new UncheckedList will both be equal to the size of the
        // given collection.
        // 
        public UncheckedList(IEnumerable<T> collection)
        {
            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                int count = c.Count;
                if (count == 0)
                {
                    items = EmptyArray;
                }
                else
                {
                    items = new T[count];
                    c.CopyTo(items, 0);
                    size = count;
                }
            }
            else
            {
                size = 0;
                items = EmptyArray;
                // This enumerable could be empty.  Let Add allocate a new array, if needed.
                // Note it will also go to _defaultCapacity first, not 1, then 2, etc.                using (IEnumerator<T> en = collection.GetEnumerator())
                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Add(en.Current);
                    }
                }
            }
        } // Gets and sets the capacity of this UncheckedList.  The capacity is the size of

        // the internal array used to hold items.  When set, the internal 
        // array of the UncheckedList is reallocated to the given capacity.
        // 
        public int Capacity
        {
            get => items.Length;
            set
            {
                if (value != items.Length)
                {
                    if (value > 0)
                    {
                        T[] newItems = new T[value];
                        if (size > 0)
                        {
                            Array.Copy(items, 0, newItems, 0, size);
                        }

                        items = newItems;
                    }
                    else
                    {
                        items = EmptyArray;
                    }
                }
            }
        } // Read-only property describing how many elements are in the UncheckedList.

        public int Count => size;

        bool System.Collections.IList.IsFixedSize => false;

        // Is this UncheckedList read-only?
        bool ICollection<T>.IsReadOnly => false;

        bool System.Collections.IList.IsReadOnly => false; // Is this UncheckedList synchronized (thread-safe)?

        bool System.Collections.ICollection.IsSynchronized => false; // Synchronization root for this object.

        Object System.Collections.ICollection.SyncRoot
        {
            get
            {
                if (syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<Object>(ref syncRoot, new Object(), null);
                }

                return syncRoot;
            }
        } // Sets or Gets the element at the given index.

        // 
        public T this[int index]
        {
            get => items[index];
            set
            {
                items[index] = value;
                version++;
            }
        }

        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            return ((value is T) || (value == null && default(T) == null));
        }

        Object System.Collections.IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        } // Adds the given object to the end of this UncheckedList. The size of the UncheckedList is

        // increased by one. If required, the capacity of the UncheckedList is doubled
        // before adding the new element.
        //
        public void Add(T item)
        {
            if (size == items.Length) EnsureCapacity(size + 1);
            items[size++] = item;
            version++;
        }

        int System.Collections.IList.Add(Object item)
        {
            Add((T)item);
            return Count - 1;
        }

        // Adds the elements of the given collection to the end of this UncheckedList. If
        // required, the capacity of the UncheckedList is increased to twice the previous
        // capacity or the new size, whichever is larger.
        //
        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(size, collection);
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            return new ReadOnlyCollection<T>(this);
        } // Searches a section of the UncheckedList for a given element using a binary search

        // algorithm. Elements of the UncheckedList are compared to the search value using
        // the given IComparer interface. If comparer is null, elements of
        // the UncheckedList are compared to the search value using the IComparable
        // interface, which in that case must be implemented by all elements of the
        // UncheckedList and the given search value. This method assumes that the given
        // section of the UncheckedList is already sorted; if this is not the case, the
        // result will be incorrect.
        //
        // The method returns the index of the given value in the UncheckedList. If the
        // UncheckedList does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value. This is also the index at which
        // the search value should be inserted into the UncheckedList in order for the UncheckedList
        // to remain sorted.
        // 
        // The method uses the Array.BinarySearch method to perform the
        // search.
        // 
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return Array.BinarySearch(items, index, count, item, comparer);
        }

        public int BinarySearch(T item)
        {
            return BinarySearch(0, Count, item, null);
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return BinarySearch(0, Count, item, comparer);
        }

        // Clears the contents of UncheckedList.
        public void Clear()
        {
            if (size > 0)
            {
                Array.Clear(items, 0,
                    size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
                size = 0;
            }

            version++;
        } // Contains returns true if the specified element is in the UncheckedList.

        // It does a linear, O(n) search.  Equality is determined by calling
        // item.Equals().
        //
        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < size; i++)
                    if (items[i] == null)
                        return true;
                return false;
            }
            else
            {
                EqualityComparer<T> c = EqualityComparer<T>.Default;
                for (int i = 0; i < size; i++)
                {
                    if (c.Equals(items[i], item)) return true;
                }

                return false;
            }
        }

        bool System.Collections.IList.Contains(Object item)
        {
            if (IsCompatibleObject(item))
            {
                return Contains((T)item);
            }

            return false;
        }

        public UncheckedList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            UncheckedList<TOutput> uncheckedList = new UncheckedList<TOutput>(size);
            for (int i = 0; i < size; i++)
            {
                uncheckedList.items[i] = converter(items[i]);
            }

            uncheckedList.size = size;
            return uncheckedList;
        } // Copies this UncheckedList into array, which must be of a 

        // compatible array type.  
        //
        // Copies this UncheckedList into array, which must be of a 

        // compatible array type.  
        //
        void System.Collections.ICollection.CopyTo(Array array, int arrayIndex)
        {
            // Array.Copy will check for NULL.
            Array.Copy(items, 0, array, arrayIndex, size);
        } // Copies a section of this UncheckedList to the given array at the given index.

        // 
        // The method uses the Array.Copy method to copy the elements.
        // 
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            // Delegate rest of error checking to Array.Copy.
            Array.Copy(items, index, array, arrayIndex, count);
        }

        public void CopyTo(T[] array, int arrayIndex = 0)
        {
            // Delegate rest of error checking to Array.Copy.
            Array.Copy(items, 0, array, arrayIndex, size);
        } // Ensures that the capacity of this UncheckedList is at least the given minimum

        // value. If the currect capacity of the UncheckedList is less than min, the
        // capacity is increased to twice the current capacity or to min,
        // whichever is larger.
        private void EnsureCapacity(int min)
        {
            if (items.Length < min)
            {
                int newCapacity = items.Length == 0 ? DefaultCapacity : items.Length * 2;
                // Allow the UncheckedList to grow to maximum possible capacity (~2G elements) before encountering overflow.
                // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
                if ((uint)newCapacity > int.MaxValue) newCapacity = int.MaxValue;
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }

        public bool Exists(Predicate<T> match)
        {
            return FindIndex(match) != -1;
        }

        public T Find(Predicate<T> match)
        {
            for (int i = 0; i < size; i++)
            {
                if (match(items[i]))
                {
                    return items[i];
                }
            }

            return default(T);
        }

        public UncheckedList<T> FindAll(Predicate<T> match)
        {
            UncheckedList<T> uncheckedList = new UncheckedList<T>();
            for (int i = 0; i < size; i++)
            {
                if (match(items[i]))
                {
                    uncheckedList.Add(items[i]);
                }
            }

            return uncheckedList;
        }

        public int FindIndex(Predicate<T> match)
        {
            return FindIndex(0, size, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return FindIndex(startIndex, size - startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(items[i])) return i;
            }

            return -1;
        }

        public T FindLast(Predicate<T> match)
        {
            for (int i = size - 1; i >= 0; i--)
            {
                if (match(items[i]))
                {
                    return items[i];
                }
            }

            return default(T);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            return FindLastIndex(size - 1, size, match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return FindLastIndex(startIndex, startIndex + 1, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            int endIndex = startIndex - count;
            for (int i = startIndex; i > endIndex; i--)
            {
                if (match(items[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public void ForEach(Action<T> action)
        {
            int ver = this.version;
            for (int i = 0; i < size; i++)
            {
                if (ver != this.version)
                {
                    break;
                }

                action(items[i]);
            }
        } // Returns an enumerator for this UncheckedList with the given

        // permission for removal of elements. If modifications made to the UncheckedList 
        // while an enumeration is in progress, the MoveNext and 
        // GetObject methods of the enumerator will throw an exception.
        //
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <internalonly/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public UncheckedList<T> GetRange(int index, int count)
        {
            UncheckedList<T> uncheckedList = new UncheckedList<T>(count);
            Array.Copy(items, index, uncheckedList.items, 0, count);
            uncheckedList.size = count;
            return uncheckedList;
        }

        // Returns the index of the first occurrence of a given value in a range of
        // this UncheckedList. The UncheckedList is searched forwards from beginning to end.
        // The elements of the UncheckedList are compared to the given value using the
        // Object.Equals method.
        // 
        // This method uses the Array.IndexOf method to perform the
        // search.
        // 
        public int IndexOf(T item)
        {
            return Array.IndexOf(items, item, 0, size);
        }

        int System.Collections.IList.IndexOf(Object item)
        {
            if (IsCompatibleObject(item))
            {
                return IndexOf((T)item);
            }

            return -1;
        } // Returns the index of the first occurrence of a given value in a range of

        // this UncheckedList. The UncheckedList is searched forwards, starting at index
        // index and ending at count number of elements. The
        // elements of the UncheckedList are compared to the given value using the
        // Object.Equals method.
        // 
        // This method uses the Array.IndexOf method to perform the
        // search.
        // 
        public int IndexOf(T item, int index)
        {
            return Array.IndexOf(items, item, index, size - index);
        } // Returns the index of the first occurrence of a given value in a range of

        // this UncheckedList. The UncheckedList is searched forwards, starting at index
        // index and upto count number of elements. The
        // elements of the UncheckedList are compared to the given value using the
        // Object.Equals method.
        // 
        // This method uses the Array.IndexOf method to perform the
        // search.
        // 
        public int IndexOf(T item, int index, int count)
        {
            return Array.IndexOf(items, item, index, count);
        } // Inserts an element into this UncheckedList at a given index. The size of the UncheckedList

        // is increased by one. If required, the capacity of the UncheckedList is doubled
        // before inserting the new element.
        // 
        public void Insert(int index, T item)
        {
            if (size == items.Length) EnsureCapacity(size + 1);
            if (index < size)
            {
                Array.Copy(items, index, items, index + 1, size - index);
            }

            items[index] = item;
            size++;
            version++;
        }

        void System.Collections.IList.Insert(int index, Object item)
        {
            Insert(index, (T)item);
        } // Inserts the elements of the given collection at a given index. If

        // required, the capacity of the UncheckedList is increased to twice the previous
        // capacity or the new size, whichever is larger.  Ranges may be added
        // to the end of the UncheckedList by setting index to the UncheckedList's size.
        //
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection is ICollection<T> c)
            {
                // if collection is ICollection<T>
                int count = c.Count;
                if (count > 0)
                {
                    EnsureCapacity(size + count);
                    if (index < size)
                    {
                        Array.Copy(items, index, items, index + count, size - index);
                    } // If we're inserting a UncheckedList into itself, we want to be able to deal with that.

                    // ReSharper disable PossibleUnintendedReferenceComparison
                    if (this == c)
                        // ReSharper restore PossibleUnintendedReferenceComparison
                    {
                        // Copy first part of _items to insert location
                        Array.Copy(items, 0, items, index, index);
                        // Copy last part of _items back to inserted location
                        Array.Copy(items, index + count, items, index * 2, size - index);
                    }
                    else
                    {
                        T[] itemsToInsert = new T[count];
                        c.CopyTo(itemsToInsert, 0);
                        itemsToInsert.CopyTo(items, index);
                    }

                    size += count;
                }
            }
            else
            {
                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Insert(index++, en.Current);
                    }
                }
            }

            version++;
        } // Returns the index of the last occurrence of a given value in a range of

        // this UncheckedList. The UncheckedList is searched backwards, starting at the end 
        // and ending at the first element in the UncheckedList. The elements of the UncheckedList 
        // are compared to the given value using the Object.Equals method.
        // 
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        // 
        public int LastIndexOf(T item)
        {
            if (size == 0)
            {
                // Special case for empty UncheckedList
                return -1;
            }
            else
            {
                return LastIndexOf(item, size - 1, size);
            }
        } // Returns the index of the last occurrence of a given value in a range of

        // this UncheckedList. The UncheckedList is searched backwards, starting at index
        // index and ending at the first element in the UncheckedList. The 
        // elements of the UncheckedList are compared to the given value using the 
        // Object.Equals method.
        // 
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        // 
        public int LastIndexOf(T item, int index)
        {
            return LastIndexOf(item, index, index + 1);
        } // Returns the index of the last occurrence of a given value in a range of

        // this UncheckedList. The UncheckedList is searched backwards, starting at index
        // index and upto count elements. The elements of
        // the UncheckedList are compared to the given value using the Object.Equals
        // method.
        // 
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        // 
        public int LastIndexOf(T item, int index, int count)
        {
            if (size == 0)
            {
                // Special case for empty UncheckedList
                return -1;
            }

            return Array.LastIndexOf(items, item, index, count);
        } // Removes the element at the given index. The size of the UncheckedList is

        // decreased by one.
        // 
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        void System.Collections.IList.Remove(Object item)
        {
            if (IsCompatibleObject(item))
            {
                Remove((T)item);
            }
        } // This method removes all items which matches the predicate.

        // The complexity is O(n).   
        public int RemoveAll(Predicate<T> match)
        {
            int freeIndex =
                0; // the first free slot in items array            // Find the first item which needs to be removed.
            while (freeIndex < size && !match(items[freeIndex])) freeIndex++;
            if (freeIndex >= size) return 0;
            int current = freeIndex + 1;
            while (current < size)
            {
                // Find the first item which needs to be kept.
                while (current < size && match(items[current])) current++;
                if (current < size)
                {
                    // copy item to the free slot.
                    items[freeIndex++] = items[current++];
                }
            }

            Array.Clear(items, freeIndex, size - freeIndex);
            int result = size - freeIndex;
            size = freeIndex;
            version++;
            return result;
        } // Removes the element at the given index. The size of the UncheckedList is

        // decreased by one.
        // 
        public void RemoveAt(int index)
        {
            size--;
            if (index < size)
            {
                Array.Copy(items, index + 1, items, index, size - index);
            }

            items[size] = default(T);
            version++;
        } // Removes a range of elements from this UncheckedList.

        // 
        public void RemoveRange(int index, int count)
        {
            if (count > 0)
            {
                size -= count;
                if (index < size)
                {
                    Array.Copy(items, index + count, items, index, size - index);
                }

                Array.Clear(items, size, count);
                version++;
            }
        } // Reverses the elements in this UncheckedList.

        public void Reverse()
        {
            Reverse(0, Count);
        } // Reverses the elements in a range of this UncheckedList. Following a call to this

        // method, an element in the range given by index and count
        // which was previously located at index i will now be located at
        // index index + (index + count - i - 1).
        // 
        // This method uses the Array.Reverse method to reverse the
        // elements.
        // 
        public void Reverse(int index, int count)
        {
            Array.Reverse(items, index, count);
            version++;
        } // Sorts the elements in this UncheckedList.  Uses the default comparer and 

        // Array.Sort.
        public void Sort()
        {
            Sort(0, Count, null);
        } // Sorts the elements in this UncheckedList.  Uses Array.Sort with the

        // provided comparer.
        public void Sort(IComparer<T> comparer)
        {
            Sort(0, Count, comparer);
        } // Sorts the elements in a section of this UncheckedList. The sort compares the

        // elements to each other using the given IComparer interface. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented by all
        // elements of the UncheckedList.
        // 
        // This method uses the Array.Sort method to sort the elements.
        // 
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            Array.Sort(items, index, count, comparer);
            version++;
        }

        public void Sort(Comparison<T> comparison)
        {
            if (size > 0)
            {
                IComparer<T> comparer = new FunctorComparer<T>(comparison);
                Array.Sort(items, 0, size, comparer);
            }
        } // ToArray returns a new Object array containing the contents of the UncheckedList.

        // This requires copying the UncheckedList, which is an O(n) operation.
        public T[] ToArray()
        {
            T[] array = new T[size];
            Array.Copy(items, 0, array, 0, size);
            return array;
        } // Sets the capacity of this UncheckedList to the size of the UncheckedList. This method can

        // be used to minimize a UncheckedList's memory overhead once it is known that no
        // new elements will be added to the UncheckedList. To completely clear a UncheckedList and
        // release all memory referenced by the UncheckedList, execute the following
        // statements:
        // 
        // UncheckedList.Clear();
        // UncheckedList.TrimExcess();
        // 
        public void TrimExcess()
        {
            int threshold = (int)(items.Length * 0.9);
            if (size < threshold)
            {
                Capacity = size;
            }
        }

        public bool TrueForAll(Predicate<T> match)
        {
            for (int i = 0; i < size; i++)
            {
                if (!match(items[i]))
                {
                    return false;
                }
            }

            return true;
        }

        internal static IList<T> Synchronized(UncheckedList<T> uncheckedList)
        {
            return new SynchronizedList(uncheckedList);
        }

        [Serializable()]
        internal class SynchronizedList : IList<T>
        {
            private UncheckedList<T> list;
            private Object root;

            internal SynchronizedList(UncheckedList<T> uncheckedList)
            {
                list = uncheckedList;
                root = ((System.Collections.ICollection)uncheckedList).SyncRoot;
            }

            public int Count
            {
                get
                {
                    lock (root)
                    {
                        return list.Count;
                    }
                }
            }

            // ReSharper disable InconsistentlySynchronizedField
            public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;
            // ReSharper restore InconsistentlySynchronizedField

            public void Add(T item)
            {
                lock (root)
                {
                    list.Add(item);
                }
            }

            public void Clear()
            {
                lock (root)
                {
                    list.Clear();
                }
            }

            public bool Contains(T item)
            {
                lock (root)
                {
                    return list.Contains(item);
                }
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                lock (root)
                {
                    list.CopyTo(array, arrayIndex);
                }
            }

            public bool Remove(T item)
            {
                lock (root)
                {
                    return list.Remove(item);
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                lock (root)
                {
                    return list.GetEnumerator();
                }
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                lock (root)
                {
                    return ((IEnumerable<T>)list).GetEnumerator();
                }
            }

            public T this[int index]
            {
                get
                {
                    lock (root)
                    {
                        return list[index];
                    }
                }
                set
                {
                    lock (root)
                    {
                        list[index] = value;
                    }
                }
            }

            public int IndexOf(T item)
            {
                lock (root)
                {
                    return list.IndexOf(item);
                }
            }

            public void Insert(int index, T item)
            {
                lock (root)
                {
                    list.Insert(index, item);
                }
            }

            public void RemoveAt(int index)
            {
                lock (root)
                {
                    list.RemoveAt(index);
                }
            }
        }

        [Serializable]
        public struct Enumerator : IEnumerator<T>
        {
            private UncheckedList<T> uncheckedList;
            private int index;
            private int version;
            private T current;

            internal Enumerator(UncheckedList<T> uncheckedList)
            {
                this.uncheckedList = uncheckedList;
                index = 0;
                version = uncheckedList.version;
                current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                UncheckedList<T> localList = uncheckedList;
                if (version == localList.version && ((uint)index < (uint)localList.size))
                {
                    current = localList.items[index];
                    index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                index = uncheckedList.size + 1;
                current = default(T);
                return false;
            }

            public T Current => current;

            Object System.Collections.IEnumerator.Current => Current;

            void System.Collections.IEnumerator.Reset()
            {
                index = 0;
                current = default(T);
            }
        }
    }
    // ReSharper restore CognitiveComplexity
}