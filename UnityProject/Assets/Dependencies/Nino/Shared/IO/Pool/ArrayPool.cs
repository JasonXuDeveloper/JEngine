using System;
using System.Collections.Generic;

namespace Nino.Shared.IO
{
    /// <summary>
    /// Thread safe array pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ArrayPool<T>
    {
        /// <summary>
        /// Shared pool
        /// </summary>
        private static readonly Dictionary<int, UncheckedStack<T[]>> Pool = new Dictionary<int, UncheckedStack<T[]>>(3);

        /// <summary>
        /// lock obj
        /// </summary>
        // ReSharper disable StaticMemberInGenericType
        private static readonly object Lock = new object();
        // ReSharper restore StaticMemberInGenericType
        
        /// <summary>
        /// Check pool size
        /// </summary>
        /// <param name="size"></param>
        private static void CheckPool(int size)
        {
            lock (Lock)
            {
                if (!Pool.TryGetValue(size, out _))
                {
                    //new queue
                    Pool.Add(size, new UncheckedStack<T[]>());
                }
            }
        }
        
        /// <summary>
        /// Request a T arr with internal length of size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static T[] Request(int size)
        {
            CheckPool(size);
            lock (Lock)
            {
                var queue = Pool[size];
                //get from queue
                if (queue.Count > 0)
                {
                    var ret = queue.Pop();
                    //double check
                    if (ret.Length != size)
                    {
                        Array.Resize(ref ret, size);
                    }
                    return ret;
                }
                //return new obj[]
                return new T[size];
            }
        }

        /// <summary>
        /// Return arr to pool
        /// </summary>
        /// <param name="size"></param>
        /// <param name="arr"></param>
        public static void Return(int size, T[] arr)
        {
            CheckPool(size);
            lock (Lock)
            {
                Pool[size].Push(arr);
            }
        }

        /// <summary>
        /// Return arr to pool
        /// </summary>
        /// <param name="arr"></param>
        public static void Return(T[] arr)
        {
            Return(arr.Length, arr);
        }
    }
}