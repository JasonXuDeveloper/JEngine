// JActionExecutionHandle.cs
// Handle for controlling a specific JAction execution
//
// Author: JasonXuDeveloper

using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using JEngine.Util.Internal;

namespace JEngine.Util
{
    /// <summary>
    /// Handle for controlling a specific JAction execution.
    /// Allows cancelling individual executions when parallel mode is enabled.
    /// </summary>
    /// <remarks>
    /// This struct is returned by <see cref="JAction.ExecuteAsync"/> and can be:
    /// <list type="bullet">
    /// <item>Awaited to get the <see cref="JActionExecution"/> result</item>
    /// <item>Cancelled via <see cref="Cancel"/> to stop this specific execution</item>
    /// <item>Disposed to clean up the underlying JAction</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var action = JAction.Create().Parallel().Delay(5f).Do(() => Debug.Log("Done"));
    ///
    /// // Start multiple executions
    /// var handle1 = action.ExecuteAsync();
    /// var handle2 = action.ExecuteAsync();
    ///
    /// // Cancel only the first one
    /// handle1.Cancel();
    ///
    /// // Await both
    /// var result1 = await handle1; // result1.Cancelled == true
    /// var result2 = await handle2; // result2.Cancelled == false
    /// </code>
    /// </example>
    public readonly struct JActionExecutionHandle : IDisposable
    {
        private readonly JAction _action;
        private readonly JActionExecutionContext _context;

        /// <summary>
        /// Gets the JAction associated with this execution.
        /// </summary>
        public JAction Action => _action;

        /// <summary>
        /// Gets whether this execution has been cancelled.
        /// </summary>
        public bool Cancelled => _context?.Cancelled ?? false;

        /// <summary>
        /// Gets whether this execution is still running.
        /// </summary>
        public bool Executing => _context?.IsExecuting ?? false;

        /// <summary>
        /// Creates a new execution handle.
        /// </summary>
        internal JActionExecutionHandle(JAction action, JActionExecutionContext context)
        {
            _action = action;
            _context = context;
        }

        /// <summary>
        /// Cancels this specific execution.
        /// Does not affect other parallel executions of the same JAction.
        /// </summary>
        public void Cancel()
        {
            _context?.Cancel();
        }

        /// <summary>
        /// Gets the awaiter for this handle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JActionExecutionAwaiter GetAwaiter() => new(_action, _context);

        /// <summary>
        /// Converts this handle to a UniTask for advanced async operations.
        /// </summary>
        public async UniTask<JActionExecution> AsUniTask()
        {
            // Delegate to the handle's awaiter so that completion and cleanup
            // are managed in a single, centralized location.
            return await this;
        }

        /// <summary>
        /// Disposes the underlying JAction, returning it to the pool.
        /// </summary>
        public void Dispose()
        {
            _action?.Dispose();
        }

        /// <summary>
        /// Implicit conversion to JAction for backwards compatibility.
        /// </summary>
        public static implicit operator JAction(JActionExecutionHandle handle) => handle._action;
    }

    /// <summary>
    /// Awaiter for <see cref="JActionExecutionHandle"/>.
    /// </summary>
    public readonly struct JActionExecutionAwaiter : ICriticalNotifyCompletion
    {
        private readonly JAction _action;
        private readonly JActionExecutionContext _context;

        internal JActionExecutionAwaiter(JAction action, JActionExecutionContext context)
        {
            _action = action;
            _context = context;
        }

        /// <summary>
        /// Gets whether the execution has completed.
        /// </summary>
        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _context == null || !_context.IsExecuting;
        }

        /// <summary>
        /// Gets the result of the execution.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JActionExecution GetResult()
        {
            // Remove context from action's active list
            _action?.RemoveActiveContext(_context);

            // Capture cancelled state before returning context to pool
            bool cancelled = _context?.Cancelled ?? false;

            // Return context to pool
            JActionExecutionContext.Return(_context);

            return new JActionExecution(_action, cancelled);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation)
        {
            if (_context == null || !_context.IsExecuting)
            {
                continuation?.Invoke();
                return;
            }
            _context.ContinuationCallback = continuation;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }
    }
}
