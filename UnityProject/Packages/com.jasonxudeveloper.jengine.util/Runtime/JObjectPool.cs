// JObjectPool.cs
// JEngine.Util - Thread-safe generic object pool
//
// Author: JasonXuDeveloper

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace JEngine.Util
{
    /// <summary>
    /// Thread-safe generic object pool using lock-free CAS operations.
    /// Provides efficient reuse of objects to minimize GC allocations.
    /// </summary>
    /// <typeparam name="T">The type of objects to pool. Must be a reference type with a parameterless constructor.</typeparam>
    /// <example>
    /// Basic usage:
    /// <code>
    /// // Create a pool for StringBuilder with max 32 items
    /// var pool = new JObjectPool&lt;StringBuilder&gt;(maxSize: 32);
    ///
    /// // Rent an object from the pool
    /// var sb = pool.Rent();
    /// sb.Append("Hello");
    ///
    /// // Return to pool when done (clears automatically if onReturn provided)
    /// pool.Return(sb);
    /// </code>
    ///
    /// With cleanup callbacks:
    /// <code>
    /// var pool = new JObjectPool&lt;List&lt;int&gt;&gt;(
    ///     maxSize: 16,
    ///     onReturn: static list => list.Clear()  // Clean up on return
    /// );
    ///
    /// var list = pool.Rent();
    /// list.Add(1);
    /// list.Add(2);
    /// pool.Return(list);  // List is cleared before pooling
    /// </code>
    ///
    /// Using shared pools:
    /// <code>
    /// // Access the global shared pool for a type
    /// var sb = JObjectPool.Shared&lt;StringBuilder&gt;().Rent();
    /// // ... use sb ...
    /// JObjectPool.Shared&lt;StringBuilder&gt;().Return(sb);
    /// </code>
    /// </example>
    public sealed class JObjectPool<T> where T : class, new()
    {
        private readonly ConcurrentStack<T> _pool;
        private readonly int _maxSize;
        private readonly Action<T> _onRent;
        private readonly Action<T> _onReturn;

        /// <summary>
        /// Creates a new object pool with the specified configuration.
        /// </summary>
        /// <param name="maxSize">Maximum number of objects to keep in the pool. Default is 64.</param>
        /// <param name="onRent">Optional callback invoked when an object is rented. Use for initialization.</param>
        /// <param name="onReturn">Optional callback invoked when an object is returned. Use for cleanup/reset.</param>
        public JObjectPool(int maxSize = 64, Action<T> onRent = null, Action<T> onReturn = null)
        {
            _pool = new();
            _maxSize = maxSize;
            _onRent = onRent;
            _onReturn = onReturn;
        }

        /// <summary>
        /// Gets the approximate number of objects currently in the pool.
        /// </summary>
        /// <remarks>
        /// This value is approximate due to the concurrent nature of the pool.
        /// </remarks>
        public int Count => _pool.Count;

        /// <summary>
        /// Rents an object from the pool, or creates a new one if the pool is empty.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/> ready for use.</returns>
        /// <remarks>
        /// This operation is lock-free and thread-safe.
        /// Remember to call <see cref="Return"/> when done to avoid memory allocation on next rent.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Rent()
        {
            if (!_pool.TryPop(out var item))
            {
                item = new();
            }
            _onRent?.Invoke(item);
            return item;
        }

        /// <summary>
        /// Returns an object to the pool for reuse.
        /// </summary>
        /// <param name="item">The object to return. Null values are ignored.</param>
        /// <remarks>
        /// If the pool is at max capacity, the item is discarded (left for GC).
        /// The <c>onReturn</c> callback (if provided) is invoked before pooling.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T item)
        {
            if (item == null) return;

            _onReturn?.Invoke(item);

            if (_pool.Count < _maxSize)
            {
                _pool.Push(item);
            }
        }

        /// <summary>
        /// Clears all objects from the pool.
        /// </summary>
        /// <remarks>
        /// Cleared objects are left for garbage collection.
        /// </remarks>
        public void Clear()
        {
            _pool.Clear();
        }

        /// <summary>
        /// Pre-warms the pool by creating the specified number of objects.
        /// </summary>
        /// <param name="count">Number of objects to pre-create.</param>
        /// <remarks>
        /// Useful for avoiding allocation spikes during gameplay.
        /// Will not exceed <c>maxSize</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// var pool = new JObjectPool&lt;MyClass&gt;(maxSize: 100);
        /// pool.Prewarm(50);  // Pre-create 50 instances
        /// </code>
        /// </example>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count && _pool.Count < _maxSize; i++)
            {
                _pool.Push(new());
            }
        }
    }

    /// <summary>
    /// Provides access to global shared object pools for each type.
    /// </summary>
    /// <remarks>
    /// Each type gets its own dedicated pool with default configuration (max 64 items).
    /// Use this for convenience when you don't need custom pool settings.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Rent from the shared pool
    /// var list = JObjectPool.Shared&lt;List&lt;string&gt;&gt;().Rent();
    /// list.Add("item");
    ///
    /// // Return when done
    /// JObjectPool.Shared&lt;List&lt;string&gt;&gt;().Return(list);
    /// </code>
    /// </example>
    public static class JObjectPool
    {
        /// <summary>
        /// Gets the shared pool instance for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of objects to pool.</typeparam>
        /// <returns>The shared <see cref="JObjectPool{T}"/> for this type.</returns>
        public static JObjectPool<T> Shared<T>() where T : class, new() => SharedPool<T>.Instance;

        private static class SharedPool<T> where T : class, new()
        {
            public static readonly JObjectPool<T> Instance = new();
        }
    }
}
