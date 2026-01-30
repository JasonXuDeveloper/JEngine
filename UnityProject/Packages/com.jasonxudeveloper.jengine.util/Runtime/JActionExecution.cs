// JActionExecution.cs
// Result struct for JAction execution with per-execution state
//
// Author: JasonXuDeveloper

using System;
using System.Runtime.CompilerServices;

namespace JEngine.Util
{
    /// <summary>
    /// Represents the result of a JAction execution.
    /// Captures per-execution state (like cancellation) that is specific to one execution,
    /// even when the same JAction is executed multiple times in parallel.
    /// </summary>
    public readonly struct JActionExecution : IDisposable
    {
        /// <summary>
        /// The JAction that was executed.
        /// </summary>
        public JAction Action
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// Gets whether this specific execution was cancelled.
        /// </summary>
        public bool Cancelled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// Gets whether the action is still executing.
        /// Note: This reflects the JAction's overall state, not this specific execution.
        /// </summary>
        public bool Executing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Action?.Executing ?? false;
        }

        /// <summary>
        /// Creates a new JActionExecution result.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal JActionExecution(JAction action, bool cancelled)
        {
            Action = action;
            Cancelled = cancelled;
        }

        /// <summary>
        /// Disposes the underlying JAction, returning it to the pool.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Action?.Dispose();
        }

        /// <summary>
        /// Implicit conversion to JAction for backwards compatibility.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator JAction(JActionExecution execution) => execution.Action;
    }
}
