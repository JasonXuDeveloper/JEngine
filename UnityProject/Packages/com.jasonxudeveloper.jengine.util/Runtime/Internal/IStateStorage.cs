// IStateStorage.cs
// JEngine.Util - Polymorphic state storage interface
//
// Author: JasonXuDeveloper

using System;
using System.Runtime.CompilerServices;

namespace JEngine.Util.Internal
{
    /// <summary>
    /// Interface for polymorphic state storage with typed invocation.
    /// Enables invoking delegates with correctly-typed state via interface dispatch,
    /// avoiding boxing allocations for value types.
    /// </summary>
    internal interface IStateStorage
    {
        /// <summary>
        /// Invokes an Action delegate with the stored state.
        /// </summary>
        /// <param name="action">The delegate to invoke (must be <c>Action&lt;T&gt;</c> where T matches stored type).</param>
        void InvokeAction(Delegate action);

        /// <summary>
        /// Invokes an async Func delegate with the stored state.
        /// </summary>
        /// <param name="func">The delegate to invoke (must be <c>Func&lt;T, JActionAwaitable&gt;</c>).</param>
        /// <returns>The awaitable result from the function.</returns>
        JActionAwaitable InvokeAsyncFunc(Delegate func);

        /// <summary>
        /// Invokes a condition Func delegate with the stored state.
        /// </summary>
        /// <param name="condition">The delegate to invoke (must be <c>Func&lt;T, bool&gt;</c>).</param>
        /// <returns>The boolean result of the condition.</returns>
        bool InvokeCondition(Delegate condition);

        /// <summary>
        /// Returns this storage instance to its type-specific pool.
        /// </summary>
        void Return();
    }

    /// <summary>
    /// Generic state storage with per-type object pooling via <see cref="JObjectPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of state to store.</typeparam>
    /// <remarks>
    /// <para>
    /// This class avoids boxing for value types because the CLR creates a specialized
    /// version of the generic class for each value type, storing T directly in the class layout.
    /// </para>
    /// <para>
    /// Each type parameter T gets its own dedicated object pool, ensuring efficient reuse.
    /// </para>
    /// </remarks>
    internal sealed class StateStorage<T> : IStateStorage
    {
        private static readonly JObjectPool<StateStorage<T>> Pool = new(
            maxSize: 64,
            onReturn: static s => s.Value = default
        );

        /// <summary>
        /// The stored state value.
        /// </summary>
        public T Value;

        /// <summary>
        /// Rents a storage instance from the pool and initializes it with the specified value.
        /// </summary>
        /// <param name="value">The state value to store.</param>
        /// <returns>A pooled <see cref="StateStorage{T}"/> instance containing the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StateStorage<T> Rent(T value)
        {
            var storage = Pool.Rent();
            storage.Value = value;
            return storage;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return()
        {
            Pool.Return(this);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InvokeAction(Delegate action)
        {
            ((Action<T>)action)(Value);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JActionAwaitable InvokeAsyncFunc(Delegate func)
        {
            return ((Func<T, JActionAwaitable>)func)(Value);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool InvokeCondition(Delegate condition)
        {
            return ((Func<T, bool>)condition)(Value);
        }

        /// <summary>
        /// Gets the approximate number of pooled instances for this type.
        /// </summary>
        public static int PoolCount => Pool.Count;

        /// <summary>
        /// Clears the pool for this type.
        /// </summary>
        public static void ClearPool() => Pool.Clear();
    }
}
