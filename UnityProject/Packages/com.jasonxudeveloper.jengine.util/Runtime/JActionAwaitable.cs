// JActionAwaitable.cs
// Zero-allocation async/await support for JAction
//
// Author: JasonXuDeveloper

using System;
using System.Runtime.CompilerServices;

namespace JEngine.Util
{
    /// <summary>
    /// Zero-allocation awaitable struct for <see cref="JAction"/> async operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This struct enables completion tracking for <see cref="JAction"/> without heap allocations.
    /// It is used internally by <see cref="JAction.ExecuteAsync"/>.
    /// </para>
    /// <para>
    /// Users should call <see cref="JAction.ExecuteAsync"/> which returns <c>ValueTask&lt;JAction&gt;</c>
    /// for proper compiler warnings when not awaited.
    /// </para>
    /// </remarks>
    public readonly record struct JActionAwaitable(JAction Action)
    {
        /// <summary>
        /// Gets the awaiter for this awaitable.
        /// </summary>
        /// <returns>A <see cref="JActionAwaiter"/> instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JActionAwaiter GetAwaiter() => new(Action);
    }

    /// <summary>
    /// Zero-allocation awaiter struct for <see cref="JAction"/>.
    /// Implements the awaiter pattern for C# async/await support.
    /// </summary>
    /// <remarks>
    /// This struct implements <see cref="ICriticalNotifyCompletion"/> to support
    /// both regular and unsafe continuations, enabling efficient async state machine behavior.
    /// </remarks>
    public readonly record struct JActionAwaiter(JAction Action) : ICriticalNotifyCompletion
    {
        /// <summary>
        /// Gets whether the <see cref="JAction"/> has completed execution.
        /// </summary>
        /// <value>
        /// <c>true</c> if the action is null, disposed, or has finished executing;
        /// <c>false</c> if still in progress.
        /// </value>
        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Action == null || !Action.IsExecuting;
        }

        /// <summary>
        /// Gets the result of the awaited operation.
        /// </summary>
        /// <remarks>
        /// <see cref="JAction"/> returns void, so this method has no return value.
        /// It exists to satisfy the awaiter pattern.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult()
        {
            // Result is void, just need to handle completion
        }

        /// <summary>
        /// Schedules the continuation action to be invoked when the operation completes.
        /// </summary>
        /// <param name="continuation">The action to invoke upon completion.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation)
        {
            if (Action == null || !Action.IsExecuting)
            {
                continuation?.Invoke();
                return;
            }
            Action.ContinuationCallback = continuation;
        }

        /// <summary>
        /// Schedules the continuation action without capturing the execution context.
        /// </summary>
        /// <param name="continuation">The action to invoke upon completion.</param>
        /// <remarks>
        /// This method is used by the async infrastructure for performance optimization.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }
    }
}
