namespace Nino.Shared.IO
{
    /// <summary>
    /// Thread safe object pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ObjectPool<T> where T: class, new()
    {
        /// <summary>
        /// A shared buffer queue
        /// </summary>
        private static readonly UncheckedStack<T> Pool = new UncheckedStack<T>(3);

        /// <summary>
        /// lock obj
        /// </summary>
        // ReSharper disable StaticMemberInGenericType
        private static readonly object Lock = new object();
        // ReSharper restore StaticMemberInGenericType
        
        /// <summary>
        /// Request an obj
        /// </summary>
        /// <returns></returns>
        public static T Request()
        {
            lock (Lock)
            {
                T ret;
                if (Pool.Count > 0)
                {
                    ret = Pool.Pop();
                    return ret;
                }
                else
                {
                    ret = new T();
                }

                return ret;
            }
        }
        
        /// <summary>
        /// Preview the next object from stack, wont take
        /// </summary>
        /// <returns></returns>
        public static T Peak()
        {
            lock (Lock)
            {
                if (Pool.Count > 0)
                {
                    return Pool.Peek();
                }

                return null;
            }
        }

        /// <summary>
        /// Return an obj to pool
        /// </summary>
        /// <param name="obj"></param>
        public static void Return(T obj)
        {
            lock (Lock)
            {
                Pool.Push(obj);
            }
        }
    }
}