// JAction.cs
// Chainable action execution framework with object pooling
//
// Author: JasonXuDeveloper <jason@xgamedev.net>
// Copyright (c) 2025 JEngine - MIT License

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JEngine.Util.Internal;
using UnityEngine;

namespace JEngine.Util
{
    /// <summary>
    /// High-performance chainable action execution framework with object pooling and minimal GC.
    /// </summary>
    /// <remarks>
    /// <para>
    /// JAction provides a fluent API for sequencing actions, delays, conditions, and loops.
    /// It uses object pooling and polymorphic state storage to minimize garbage collection.
    /// </para>
    /// <para>
    /// <b>Key Features:</b>
    /// <list type="bullet">
    /// <item>Fluent chainable API for building action sequences</item>
    /// <item>Object pooling via <see cref="JObjectPool{T}"/> for zero-allocation reuse</item>
    /// <item>Zero-boxing state storage for value types via polymorphic dispatch</item>
    /// <item>Async/await support with <see cref="JActionAwaitable"/></item>
    /// <item>Time-based delays, frame delays, and conditional waits</item>
    /// <item>Repeat loops with conditions or fixed counts</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// Basic usage:
    /// <code>
    /// // Create a simple action sequence
    /// JAction.Create()
    ///     .Do(() => Debug.Log("Hello"))
    ///     .Delay(1f)
    ///     .Do(() => Debug.Log("World"))
    ///     .Execute();
    /// </code>
    ///
    /// Using state to avoid closures:
    /// <code>
    /// // Pass state directly to avoid closure allocations
    /// int counter = 0;
    /// JAction.Create()
    ///     .Do(static (int c) => Debug.Log($"Count: {c}"), counter)
    ///     .Execute();
    /// </code>
    ///
    /// Async/await pattern with auto-dispose:
    /// <code>
    /// async Task MyAsyncMethod()
    /// {
    ///     using var action = await JAction.Create()
    ///         .Do(() => Debug.Log("Start"))
    ///         .Delay(1f)
    ///         .Do(() => Debug.Log("End"))
    ///         .ExecuteAsync();
    /// }
    /// </code>
    ///
    /// Conditional loops:
    /// <code>
    /// int hp = 100;
    /// JAction.Create()
    ///     .RepeatUntil(
    ///         () => hp -= 10,
    ///         () => hp &lt;= 0,
    ///         frequency: 0.5f
    ///     )
    ///     .Do(() => Debug.Log("Game Over"))
    ///     .Execute();
    /// </code>
    ///
    /// Manual disposal (without using keyword):
    /// <code>
    /// var action = JAction.Create()
    ///     .Do(() => Debug.Log("Task"))
    ///     .Execute();
    ///
    /// // When done, dispose to return to pool
    /// action.Dispose();
    /// </code>
    /// </example>
    public sealed class JAction : IDisposable
    {
        #region Constants

        private const int DefaultTaskCapacity = 8;
        private const int MaxTaskCapacity = 256;

        #endregion

        #region Static Pool

        private static readonly JObjectPool<JAction> Pool = new(
            maxSize: 32,
            onReturn: static a => a.Reset(true)
        );

        #endregion

        #region Instance Fields

        private readonly List<JActionTask> _tasks;
        private int _currentTaskIndex;
        internal bool IsExecuting;
        private bool _cancelled;
        private bool _parallel;
        private bool _disposed;
        private bool _blockingMode;

        private Action _onCancel;
        private Delegate _onCancelDelegate;
        private IStateStorage _onCancelState;

        internal Action ContinuationCallback;
        private float _executeStartTime;
        private float _timeoutEndTime;

        private float _delayEndTime;
        private int _delayEndFrame;
        private int _repeatCounter;
        private float _repeatLastTime;

        // Async task state
        private JActionAwaitable _pendingAwaitable;
        private bool _awaitingAsync;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether this JAction is currently executing.
        /// </summary>
        public bool Executing => IsExecuting;

        /// <summary>
        /// Gets whether this JAction has been cancelled.
        /// </summary>
        public bool Cancelled => _cancelled;

        /// <summary>
        /// Gets whether parallel execution mode is enabled.
        /// </summary>
        public bool IsParallel => _parallel;

        /// <summary>
        /// Gets the approximate number of pooled JAction instances available.
        /// </summary>
        public static int PooledCount => Pool.Count;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new JAction instance. Prefer using <see cref="Create"/> for pooled instances.
        /// </summary>
        public JAction()
        {
            _tasks = new(DefaultTaskCapacity);
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Creates or retrieves a pooled JAction instance ready for use.
        /// </summary>
        /// <returns>A JAction instance from the pool or newly created.</returns>
        /// <example>
        /// <code>
        /// var action = JAction.Create()
        ///     .Do(() => Debug.Log("Hello"))
        ///     .Execute();
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JAction Create()
        {
            var action = Pool.Rent();
            action._disposed = false;
            return action;
        }

        /// <summary>
        /// Clears all instances from the object pool.
        /// </summary>
        public static void ClearPool() => Pool.Clear();

        #endregion

        #region Fluent API - Action Registration

        /// <summary>
        /// Queues a synchronous action for execution.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>This JAction for method chaining.</returns>
        /// <example>
        /// <code>
        /// JAction.Create()
        ///     .Do(() => Debug.Log("Step 1"))
        ///     .Do(() => Debug.Log("Step 2"))
        ///     .Execute();
        /// </code>
        /// </example>
        public JAction Do(Action action)
        {
            if (action == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.Action,
                SyncAction = action
            });
            return this;
        }

        /// <summary>
        /// Queues an async function for execution.
        /// </summary>
        /// <param name="asyncFunc">The async function returning a <see cref="JActionAwaitable"/>.</param>
        /// <returns>This JAction for method chaining.</returns>
        public JAction Do(Func<JActionAwaitable> asyncFunc)
        {
            if (asyncFunc == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.AsyncFunc,
                AsyncFunc = asyncFunc
            });
            return this;
        }

        /// <summary>
        /// Queues a synchronous action with state. Avoids closure allocations.
        /// </summary>
        /// <typeparam name="TState">The type of state to pass.</typeparam>
        /// <param name="action">The action receiving the state.</param>
        /// <param name="state">The state value to pass.</param>
        /// <returns>This JAction for method chaining.</returns>
        /// <remarks>
        /// Using state parameters avoids closure allocations when capturing variables.
        /// For value types, no boxing occurs due to polymorphic state storage.
        /// </remarks>
        /// <example>
        /// <code>
        /// int counter = 42;
        /// JAction.Create()
        ///     .Do(static (int c) => Debug.Log($"Counter: {c}"), counter)
        ///     .Execute();
        /// </code>
        /// </example>
        public JAction Do<TState>(Action<TState> action, TState state)
        {
            if (action == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.Action,
                ActionDelegate = action,
                State = StateStorage<TState>.Rent(state)
            });
            return this;
        }

        /// <summary>
        /// Queues an async function with state. Avoids closure allocations.
        /// </summary>
        /// <typeparam name="TState">The type of state to pass.</typeparam>
        /// <param name="asyncFunc">The async function receiving the state.</param>
        /// <param name="state">The state value to pass.</param>
        /// <returns>This JAction for method chaining.</returns>
        public JAction Do<TState>(Func<TState, JActionAwaitable> asyncFunc, TState state)
        {
            if (asyncFunc == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.AsyncFunc,
                ActionDelegate = asyncFunc,
                State = StateStorage<TState>.Rent(state)
            });
            return this;
        }

        #endregion

        #region Fluent API - Timing

        /// <summary>
        /// Delays execution for the specified duration.
        /// </summary>
        /// <param name="seconds">The delay duration in seconds.</param>
        /// <returns>This JAction for method chaining.</returns>
        /// <example>
        /// <code>
        /// JAction.Create()
        ///     .Do(() => Debug.Log("Start"))
        ///     .Delay(2.5f)
        ///     .Do(() => Debug.Log("2.5 seconds later"))
        ///     .Execute();
        /// </code>
        /// </example>
        public JAction Delay(float seconds)
        {
            if (seconds <= 0) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.Delay,
                FloatParam1 = seconds
            });
            return this;
        }

        /// <summary>
        /// Delays execution for the specified number of frames.
        /// </summary>
        /// <param name="frames">The number of frames to wait. Default is 1.</param>
        /// <returns>This JAction for method chaining.</returns>
        /// <example>
        /// <code>
        /// JAction.Create()
        ///     .Do(() => Debug.Log("Frame N"))
        ///     .DelayFrame(5)
        ///     .Do(() => Debug.Log("Frame N+5"))
        ///     .Execute();
        /// </code>
        /// </example>
        public JAction DelayFrame(int frames = 1)
        {
            if (frames <= 0) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.DelayFrame,
                IntParam = frames
            });
            return this;
        }

        #endregion

        #region Fluent API - Conditional

        /// <summary>
        /// Waits until the condition returns true.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="frequency">How often to check in seconds. 0 = every frame.</param>
        /// <param name="timeout">Maximum wait time in seconds. 0 = no timeout.</param>
        /// <returns>This JAction for method chaining.</returns>
        /// <example>
        /// <code>
        /// bool dataLoaded = false;
        /// JAction.Create()
        ///     .Do(() => StartAsyncLoad())
        ///     .WaitUntil(() => dataLoaded, timeout: 10f)
        ///     .Do(() => Debug.Log("Data ready!"))
        ///     .Execute();
        /// </code>
        /// </example>
        public JAction WaitUntil(Func<bool> condition, float frequency = 0, float timeout = 0)
        {
            if (condition == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.WaitUntil,
                Condition = condition,
                FloatParam1 = frequency,
                FloatParam2 = timeout
            });
            return this;
        }

        /// <summary>
        /// Waits until the condition with state returns true. Avoids closure allocations.
        /// </summary>
        /// <typeparam name="TState">The type of state to pass.</typeparam>
        /// <param name="condition">The condition receiving the state.</param>
        /// <param name="state">The state value to pass.</param>
        /// <param name="frequency">How often to check in seconds. 0 = every frame.</param>
        /// <param name="timeout">Maximum wait time in seconds. 0 = no timeout.</param>
        /// <returns>This JAction for method chaining.</returns>
        public JAction WaitUntil<TState>(Func<TState, bool> condition, TState state, float frequency = 0, float timeout = 0)
        {
            if (condition == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.WaitUntil,
                ConditionDelegate = condition,
                State = StateStorage<TState>.Rent(state),
                FloatParam1 = frequency,
                FloatParam2 = timeout
            });
            return this;
        }

        /// <summary>
        /// Waits while the condition returns true (continues when false).
        /// </summary>
        /// <param name="condition">The condition to check. Waits while true.</param>
        /// <param name="frequency">How often to check in seconds. 0 = every frame.</param>
        /// <param name="timeout">Maximum wait time in seconds. 0 = no timeout.</param>
        /// <returns>This JAction for method chaining.</returns>
        /// <example>
        /// <code>
        /// bool isLoading = true;
        /// JAction.Create()
        ///     .Do(() => StartLoad())
        ///     .WaitWhile(() => isLoading, timeout: 10f)
        ///     .Do(() => Debug.Log("Loading complete!"))
        ///     .Execute();
        /// </code>
        /// </example>
        public JAction WaitWhile(Func<bool> condition, float frequency = 0, float timeout = 0)
        {
            if (condition == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.WaitWhile,
                Condition = condition,
                FloatParam1 = frequency,
                FloatParam2 = timeout
            });
            return this;
        }

        /// <summary>
        /// Waits while the condition with state returns true. Avoids closure allocations.
        /// </summary>
        /// <typeparam name="TState">The type of state to pass.</typeparam>
        /// <param name="condition">The condition receiving the state. Waits while true.</param>
        /// <param name="state">The state value to pass.</param>
        /// <param name="frequency">How often to check in seconds. 0 = every frame.</param>
        /// <param name="timeout">Maximum wait time in seconds. 0 = no timeout.</param>
        /// <returns>This JAction for method chaining.</returns>
        public JAction WaitWhile<TState>(Func<TState, bool> condition, TState state, float frequency = 0, float timeout = 0)
        {
            if (condition == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.WaitWhile,
                ConditionDelegate = condition,
                State = StateStorage<TState>.Rent(state),
                FloatParam1 = frequency,
                FloatParam2 = timeout
            });
            return this;
        }

        /// <summary>
        /// Repeats an action while the condition is true.
        /// </summary>
        /// <param name="action">The action to repeat.</param>
        /// <param name="condition">The condition to check. Repeats while true.</param>
        /// <param name="frequency">Interval between repeats in seconds. 0 = every frame.</param>
        /// <param name="timeout">Maximum duration in seconds. 0 = no timeout.</param>
        /// <returns>This JAction for method chaining.</returns>
        /// <example>
        /// <code>
        /// int count = 0;
        /// JAction.Create()
        ///     .RepeatWhile(
        ///         () => count++,
        ///         () => count &lt; 10,
        ///         frequency: 0.1f
        ///     )
        ///     .Execute();
        /// </code>
        /// </example>
        public JAction RepeatWhile(Action action, Func<bool> condition, float frequency = 0, float timeout = 0)
        {
            if (action == null || condition == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.RepeatWhile,
                SyncAction = action,
                Condition = condition,
                FloatParam1 = frequency,
                FloatParam2 = timeout
            });
            return this;
        }

        /// <summary>
        /// Repeats an action with state while the condition is true.
        /// </summary>
        /// <typeparam name="TState">The type of state to pass.</typeparam>
        /// <param name="action">The action receiving the state.</param>
        /// <param name="condition">The condition receiving the state. Repeats while true.</param>
        /// <param name="state">The state value to pass to both delegates.</param>
        /// <param name="frequency">Interval between repeats in seconds.</param>
        /// <param name="timeout">Maximum duration in seconds. 0 = no timeout.</param>
        /// <returns>This JAction for method chaining.</returns>
        public JAction RepeatWhile<TState>(Action<TState> action, Func<TState, bool> condition, TState state,
            float frequency = 0, float timeout = 0)
        {
            if (action == null || condition == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.RepeatWhile,
                ActionDelegate = action,
                ConditionDelegate = condition,
                State = StateStorage<TState>.Rent(state),
                FloatParam1 = frequency,
                FloatParam2 = timeout
            });
            return this;
        }

        /// <summary>
        /// Repeats an action until the condition becomes true.
        /// </summary>
        /// <param name="action">The action to repeat.</param>
        /// <param name="condition">The condition to check. Stops when true.</param>
        /// <param name="frequency">Interval between repeats in seconds. 0 = every frame.</param>
        /// <param name="timeout">Maximum duration in seconds. 0 = no timeout.</param>
        /// <returns>This JAction for method chaining.</returns>
        public JAction RepeatUntil(Action action, Func<bool> condition, float frequency = 0, float timeout = 0)
        {
            if (action == null || condition == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.RepeatUntil,
                SyncAction = action,
                Condition = condition,
                FloatParam1 = frequency,
                FloatParam2 = timeout
            });
            return this;
        }

        /// <summary>
        /// Repeats an action with state until the condition becomes true.
        /// </summary>
        /// <typeparam name="TState">The type of state to pass.</typeparam>
        /// <param name="action">The action receiving the state.</param>
        /// <param name="condition">The condition receiving the state. Stops when true.</param>
        /// <param name="state">The state value to pass to both delegates.</param>
        /// <param name="frequency">Interval between repeats in seconds.</param>
        /// <param name="timeout">Maximum duration in seconds. 0 = no timeout.</param>
        /// <returns>This JAction for method chaining.</returns>
        public JAction RepeatUntil<TState>(Action<TState> action, Func<TState, bool> condition, TState state,
            float frequency = 0, float timeout = 0)
        {
            if (action == null || condition == null) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.RepeatUntil,
                ActionDelegate = action,
                ConditionDelegate = condition,
                State = StateStorage<TState>.Rent(state),
                FloatParam1 = frequency,
                FloatParam2 = timeout
            });
            return this;
        }

        /// <summary>
        /// Repeats an action a fixed number of times.
        /// </summary>
        /// <param name="action">The action to repeat.</param>
        /// <param name="count">Number of times to repeat.</param>
        /// <param name="interval">Interval between repeats in seconds. 0 = every frame.</param>
        /// <returns>This JAction for method chaining.</returns>
        /// <example>
        /// <code>
        /// JAction.Create()
        ///     .Repeat(() => Debug.Log("Ping"), count: 5, interval: 1f)
        ///     .Execute();
        /// </code>
        /// </example>
        public JAction Repeat(Action action, int count, float interval = 0)
        {
            if (action == null || count <= 0) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.Repeat,
                SyncAction = action,
                IntParam = count,
                FloatParam1 = interval
            });
            return this;
        }

        /// <summary>
        /// Repeats an action with state a fixed number of times.
        /// </summary>
        /// <typeparam name="TState">The type of state to pass.</typeparam>
        /// <param name="action">The action receiving the state.</param>
        /// <param name="state">The state value to pass.</param>
        /// <param name="count">Number of times to repeat.</param>
        /// <param name="interval">Interval between repeats in seconds.</param>
        /// <returns>This JAction for method chaining.</returns>
        public JAction Repeat<TState>(Action<TState> action, TState state, int count, float interval = 0)
        {
            if (action == null || count <= 0) return this;
            EnsureTaskCapacity();
            _tasks.Add(new JActionTask
            {
                Type = JActionTaskType.Repeat,
                ActionDelegate = action,
                State = StateStorage<TState>.Rent(state),
                IntParam = count,
                FloatParam1 = interval
            });
            return this;
        }

        #endregion

        #region Fluent API - Configuration

        /// <summary>
        /// Enables parallel execution mode, allowing multiple concurrent executions.
        /// </summary>
        /// <returns>This JAction for method chaining.</returns>
        public JAction Parallel()
        {
            _parallel = true;
            return this;
        }

        /// <summary>
        /// Registers a callback to be invoked when this JAction is cancelled.
        /// </summary>
        /// <param name="callback">The callback to invoke on cancellation.</param>
        /// <returns>This JAction for method chaining.</returns>
        /// <example>
        /// <code>
        /// var action = JAction.Create()
        ///     .Delay(10f)
        ///     .OnCancel(() => Debug.Log("Cancelled!"))
        ///     .Execute();
        ///
        /// // Later...
        /// action.Cancel();  // Triggers the OnCancel callback
        /// </code>
        /// </example>
        public JAction OnCancel(Action callback)
        {
            _onCancel = callback;
            _onCancelDelegate = null;
            _onCancelState?.Return();
            _onCancelState = null;
            return this;
        }

        /// <summary>
        /// Registers a callback with state to be invoked when cancelled.
        /// </summary>
        /// <typeparam name="TState">The type of state to pass.</typeparam>
        /// <param name="callback">The callback receiving the state.</param>
        /// <param name="state">The state value to pass.</param>
        /// <returns>This JAction for method chaining.</returns>
        public JAction OnCancel<TState>(Action<TState> callback, TState state)
        {
            _onCancel = null;
            _onCancelDelegate = callback;
            _onCancelState?.Return();
            _onCancelState = StateStorage<TState>.Rent(state);
            return this;
        }

        #endregion

        #region Execution

        /// <summary>
        /// Executes all queued tasks synchronously (blocking).
        /// </summary>
        /// <param name="timeout">
        /// Maximum time in seconds to wait before cancelling.
        /// Use 0 or negative for no timeout (default).
        /// </param>
        /// <returns>This JAction for method chaining or status checking.</returns>
        /// <remarks>
        /// <para>
        /// This method blocks until all tasks complete or timeout is reached.
        /// Time-based delays (<see cref="Delay"/>) work correctly by spinning on real time.
        /// Frame-based delays (<see cref="DelayFrame"/>) are converted to time-based delays
        /// in blocking mode since Unity frames don't advance.
        /// </para>
        /// <para>
        /// For proper frame-based execution, use <see cref="ExecuteAsync"/> instead.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Blocking execution with 5 second timeout
        /// var action = JAction.Create()
        ///     .Do(() => Debug.Log("Step 1"))
        ///     .Delay(0.5f)
        ///     .Do(() => Debug.Log("Step 2"))
        ///     .Execute(timeout: 5f);
        ///
        /// if (action.Cancelled)
        ///     Debug.Log("Action timed out!");
        /// </code>
        /// </example>
        public JAction Execute(float timeout = 0f)
        {
            if (IsExecuting && !_parallel)
            {
                Debug.LogWarning("[JAction] Already executing. Enable Parallel() for concurrent execution.");
                return this;
            }

            if (_tasks.Count == 0) return this;

            IsExecuting = true;
            _cancelled = false;
            _currentTaskIndex = 0;
            _executeStartTime = Time.realtimeSinceStartup;
            _blockingMode = true;
            _timeoutEndTime = timeout > 0f ? _executeStartTime + timeout : 0f;
            _awaitingAsync = false;

            // Spin until complete (timeout checked in Tick)
            while (!Tick()) { }

            return this;
        }

        /// <summary>
        /// Executes all queued tasks asynchronously via the PlayerLoop.
        /// </summary>
        /// <param name="timeout">
        /// Maximum time in seconds before cancelling. Use 0 or negative for no timeout (default).
        /// </param>
        /// <returns>This JAction after execution completes, enabling the <c>using</c> pattern.</returns>
        /// <remarks>
        /// <para>
        /// Unlike <see cref="Execute"/>, this method returns immediately and processes
        /// tasks across frames. Use this for frame-based delays (<see cref="DelayFrame"/>)
        /// that require actual Unity frames to advance.
        /// </para>
        /// <para>
        /// <b>Important:</b> Always await this method and use the <c>using</c> pattern
        /// to ensure proper cleanup and pool return.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// async Task PlaySequenceAsync()
        /// {
        ///     using var action = await JAction.Create()
        ///         .Do(() => Debug.Log("Step 1"))
        ///         .Delay(1f)
        ///         .Do(() => Debug.Log("Step 2"))
        ///         .ExecuteAsync(timeout: 5f);
        /// }
        /// </code>
        /// </example>
        public async ValueTask<JAction> ExecuteAsync(float timeout = 0f)
        {
            StartAsync(timeout);
            await new JActionAwaitable(this);
            return this;
        }

        private void StartAsync(float timeout)
        {
            if (IsExecuting && !_parallel)
            {
                Debug.LogWarning("[JAction] Already executing. Enable Parallel() for concurrent execution.");
                return;
            }

            if (_tasks.Count == 0) return;

            IsExecuting = true;
            _cancelled = false;
            _currentTaskIndex = 0;
            _executeStartTime = Time.realtimeSinceStartup;
            _blockingMode = false;
            _timeoutEndTime = timeout > 0f ? _executeStartTime + timeout : 0f;
            _awaitingAsync = false;

            JActionRunner.Register(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Tick()
        {
            if (_cancelled)
            {
                OnExecutionComplete();
                return true;
            }

            // Check timeout (preemptive cancellation)
            if (_timeoutEndTime > 0f && Time.realtimeSinceStartup >= _timeoutEndTime)
            {
                Cancel();
                OnExecutionComplete();
                return true;
            }

            if (_currentTaskIndex >= _tasks.Count)
            {
                OnExecutionComplete();
                return true;
            }

            if (ProcessCurrentTask())
            {
                _currentTaskIndex++;
            }

            if (_currentTaskIndex >= _tasks.Count)
            {
                OnExecutionComplete();
                return true;
            }

            return false;
        }

        private bool ProcessCurrentTask()
        {
            if (_currentTaskIndex >= _tasks.Count) return true;

            var task = _tasks[_currentTaskIndex];
            float currentTime = Time.realtimeSinceStartup;
            int currentFrame = Time.frameCount;

            switch (task.Type)
            {
                case JActionTaskType.Action:
                    try { task.InvokeAction(); }
                    catch (Exception e) { Debug.LogException(e); }
                    return true;

                case JActionTaskType.AsyncFunc:
                    if (!_awaitingAsync)
                    {
                        // Start the async operation
                        try
                        {
                            _pendingAwaitable = task.InvokeAsyncFunc();
                            _awaitingAsync = true;
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            return true;
                        }
                    }
                    // Check if async operation completed
                    if (_pendingAwaitable.GetAwaiter().IsCompleted)
                    {
                        _awaitingAsync = false;
                        _pendingAwaitable = default;
                        return true;
                    }
                    return false;

                case JActionTaskType.Delay:
                    if (_delayEndTime <= 0)
                        _delayEndTime = currentTime + task.FloatParam1;
                    if (currentTime >= _delayEndTime)
                    {
                        _delayEndTime = 0;
                        return true;
                    }
                    return false;

                case JActionTaskType.DelayFrame:
                    if (_blockingMode)
                    {
                        // In blocking mode, convert frames to time using last known frame time
                        if (_delayEndTime <= 0)
                        {
                            float frameTime = Mathf.Max(Time.unscaledDeltaTime, 0.001f);
                            _delayEndTime = currentTime + task.IntParam * frameTime;
                        }
                        if (currentTime >= _delayEndTime)
                        {
                            _delayEndTime = 0;
                            return true;
                        }
                        return false;
                    }
                    if (_delayEndFrame <= 0)
                        _delayEndFrame = currentFrame + task.IntParam;
                    if (currentFrame >= _delayEndFrame)
                    {
                        _delayEndFrame = 0;
                        return true;
                    }
                    return false;

                case JActionTaskType.WaitUntil:
                    if (task.FloatParam1 > 0 && _repeatLastTime > 0)
                    {
                        if (currentTime - _repeatLastTime < task.FloatParam1)
                            return false;
                    }
                    _repeatLastTime = currentTime;

                    if (task.FloatParam2 > 0 && currentTime - _executeStartTime >= task.FloatParam2)
                    {
                        _repeatLastTime = 0;
                        return true;
                    }

                    try
                    {
                        if (task.InvokeCondition())
                        {
                            _repeatLastTime = 0;
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        _repeatLastTime = 0;
                        return true;
                    }
                    return false;

                case JActionTaskType.WaitWhile:
                    if (task.FloatParam1 > 0 && _repeatLastTime > 0)
                    {
                        if (currentTime - _repeatLastTime < task.FloatParam1)
                            return false;
                    }
                    _repeatLastTime = currentTime;

                    if (task.FloatParam2 > 0 && currentTime - _executeStartTime >= task.FloatParam2)
                    {
                        _repeatLastTime = 0;
                        return true;
                    }

                    try
                    {
                        if (!task.InvokeCondition())
                        {
                            _repeatLastTime = 0;
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        _repeatLastTime = 0;
                        return true;
                    }
                    return false;

                case JActionTaskType.RepeatWhile:
                    if (task.FloatParam2 > 0 && currentTime - _executeStartTime >= task.FloatParam2)
                    {
                        _repeatLastTime = 0;
                        return true;
                    }

                    try
                    {
                        if (!task.InvokeCondition())
                        {
                            _repeatLastTime = 0;
                            return true;
                        }

                        if (task.FloatParam1 > 0 && _repeatLastTime > 0)
                        {
                            if (currentTime - _repeatLastTime < task.FloatParam1)
                                return false;
                        }

                        _repeatLastTime = currentTime;
                        task.InvokeAction();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        _repeatLastTime = 0;
                        return true;
                    }
                    return false;

                case JActionTaskType.RepeatUntil:
                    if (task.FloatParam2 > 0 && currentTime - _executeStartTime >= task.FloatParam2)
                    {
                        _repeatLastTime = 0;
                        return true;
                    }

                    try
                    {
                        if (task.InvokeCondition())
                        {
                            _repeatLastTime = 0;
                            return true;
                        }

                        if (task.FloatParam1 > 0 && _repeatLastTime > 0)
                        {
                            if (currentTime - _repeatLastTime < task.FloatParam1)
                                return false;
                        }

                        _repeatLastTime = currentTime;
                        task.InvokeAction();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        _repeatLastTime = 0;
                        return true;
                    }
                    return false;

                case JActionTaskType.Repeat:
                    if (_repeatCounter >= task.IntParam)
                    {
                        _repeatCounter = 0;
                        _repeatLastTime = 0;
                        return true;
                    }

                    if (task.FloatParam1 > 0 && _repeatCounter > 0)
                    {
                        if (currentTime - _repeatLastTime < task.FloatParam1)
                            return false;
                    }

                    _repeatLastTime = currentTime;

                    try { task.InvokeAction(); }
                    catch (Exception e) { Debug.LogException(e); }

                    _repeatCounter++;
                    return _repeatCounter >= task.IntParam;

                default:
                    return true;
            }
        }

        private void OnExecutionComplete()
        {
            IsExecuting = false;
            var continuation = ContinuationCallback;
            ContinuationCallback = null;
            continuation?.Invoke();
        }

        #endregion

        #region Cancellation

        /// <summary>
        /// Cancels the current execution and invokes the OnCancel callback if set.
        /// </summary>
        /// <returns>This JAction for method chaining.</returns>
        public JAction Cancel()
        {
            if (!IsExecuting) return this;

            _cancelled = true;

            try
            {
                if (_onCancel != null)
                {
                    _onCancel.Invoke();
                }
                else if (_onCancelDelegate != null && _onCancelState != null)
                {
                    _onCancelState.InvokeAction(_onCancelDelegate);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return this;
        }

        #endregion

        #region Reset & Dispose

        /// <summary>
        /// Resets this JAction to its initial state, clearing all tasks.
        /// </summary>
        /// <param name="force">If true, forces reset even during execution.</param>
        /// <returns>This JAction for method chaining.</returns>
        public JAction Reset(bool force = false)
        {
            if (IsExecuting && !force)
            {
                Debug.LogWarning("[JAction] Cannot reset while executing. Use Reset(true) to force.");
                return this;
            }

            // Return state storage to pools before clearing
            for (int i = 0; i < _tasks.Count; i++)
            {
                _tasks[i].State?.Return();
            }
            _tasks.Clear();

            _currentTaskIndex = 0;
            IsExecuting = false;
            _cancelled = false;
            _parallel = false;
            _onCancel = null;
            _onCancelDelegate = null;
            _onCancelState?.Return();
            _onCancelState = null;
            ContinuationCallback = null;
            _delayEndTime = 0;
            _delayEndFrame = 0;
            _repeatCounter = 0;
            _repeatLastTime = 0;
            _executeStartTime = 0;
            _timeoutEndTime = 0;
            _blockingMode = false;
            _awaitingAsync = false;
            _pendingAwaitable = default;

            return this;
        }

        /// <summary>
        /// Disposes this JAction and returns it to the object pool.
        /// </summary>
        /// <remarks>
        /// If currently executing, cancels first. After disposal, the instance
        /// may be reused by future <see cref="Create"/> calls.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed) return;

            if (IsExecuting)
            {
                Cancel();
            }

            _disposed = true;
            Pool.Return(this);
        }

        #endregion

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureTaskCapacity()
        {
            if (_tasks.Count >= MaxTaskCapacity)
            {
                throw new InvalidOperationException(
                    $"[JAction] Maximum task capacity ({MaxTaskCapacity}) exceeded.");
            }
        }

        #endregion
    }
}
