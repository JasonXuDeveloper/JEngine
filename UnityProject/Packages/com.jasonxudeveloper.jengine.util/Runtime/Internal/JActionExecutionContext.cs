// JActionExecutionContext.cs
// Per-execution state for JAction, enabling parallel execution support
//
// Author: JasonXuDeveloper

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace JEngine.Util.Internal
{
    /// <summary>
    /// Holds the execution state for a single JAction execution.
    /// Enables parallel execution by isolating state per-execution.
    /// </summary>
    internal sealed class JActionExecutionContext
    {
        #region Static Pool

        private static readonly JObjectPool<JActionExecutionContext> Pool = new(
            maxSize: 64,
            onReturn: static ctx => ctx.Reset()
        );

        /// <summary>
        /// Gets a context from the pool or creates a new one.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JActionExecutionContext Rent() => Pool.Rent();

        /// <summary>
        /// Returns a context to the pool.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(JActionExecutionContext context)
        {
            if (context != null) Pool.Return(context);
        }

        #endregion

        #region Instance Fields

        // Task snapshot
        private readonly List<JActionTask> _tasks = new(8);

        // Execution state
        private int _currentTaskIndex;
        internal bool IsExecuting;
        private bool _cancelled;
        private bool _blockingMode;

        // Timing
        private float _executeStartTime;
        private float _timeoutEndTime;
        private float _delayEndTime;
        private int _delayEndFrame;
        private int _repeatCounter;
        private float _repeatLastTime;

        // Async state
        private JActionAwaitable _pendingAwaitable;
        private bool _awaitingAsync;

        // Cancel callbacks (copied from JAction at execution start)
        private Action _onCancel;
        private Delegate _onCancelDelegate;
        private IStateStorage _onCancelState;

        // Continuation for awaitable
        internal Action ContinuationCallback;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether this execution has been cancelled.
        /// </summary>
        public bool Cancelled => _cancelled;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes this context for a new execution.
        /// </summary>
        /// <param name="tasks">The task list to snapshot.</param>
        /// <param name="onCancel">Cancel callback (stateless).</param>
        /// <param name="onCancelDelegate">Cancel callback delegate (stateful).</param>
        /// <param name="onCancelState">Cancel callback state.</param>
        /// <param name="timeout">Execution timeout in seconds (0 = no timeout).</param>
        /// <param name="blockingMode">Whether this is a blocking (sync) execution.</param>
        public void Initialize(
            List<JActionTask> tasks,
            Action onCancel,
            Delegate onCancelDelegate,
            IStateStorage onCancelState,
            float timeout,
            bool blockingMode)
        {
            // Snapshot the tasks
            _tasks.Clear();
            for (int i = 0; i < tasks.Count; i++)
            {
                _tasks.Add(tasks[i]);
            }

            // Copy cancel callbacks
            _onCancel = onCancel;
            _onCancelDelegate = onCancelDelegate;
            _onCancelState = onCancelState;

            // Initialize execution state
            _currentTaskIndex = 0;
            IsExecuting = true;
            _cancelled = false;
            _blockingMode = blockingMode;
            _executeStartTime = Time.realtimeSinceStartup;
            _timeoutEndTime = timeout > 0f ? _executeStartTime + timeout : 0f;
            _awaitingAsync = false;
            _pendingAwaitable = default;
            _delayEndTime = 0;
            _delayEndFrame = 0;
            _repeatCounter = 0;
            _repeatLastTime = 0;
            ContinuationCallback = null;
        }

        private void Reset()
        {
            _tasks.Clear();
            _currentTaskIndex = 0;
            IsExecuting = false;
            _cancelled = false;
            _blockingMode = false;
            _onCancel = null;
            _onCancelDelegate = null;
            _onCancelState = null;
            ContinuationCallback = null;
            _delayEndTime = 0;
            _delayEndFrame = 0;
            _repeatCounter = 0;
            _repeatLastTime = 0;
            _executeStartTime = 0;
            _timeoutEndTime = 0;
            _awaitingAsync = false;
            _pendingAwaitable = default;
        }

        #endregion

        #region Execution

        /// <summary>
        /// Processes one tick of execution.
        /// </summary>
        /// <returns>True if execution is complete, false if more ticks needed.</returns>
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

            return task.Type switch
            {
                JActionTaskType.Action => ProcessActionTask(task),
                JActionTaskType.AsyncFunc => ProcessAsyncFuncTask(task),
                JActionTaskType.Delay => ProcessDelayTask(task),
                JActionTaskType.DelayFrame => ProcessDelayFrameTask(task),
                JActionTaskType.WaitUntil => ProcessWaitUntilTask(task),
                JActionTaskType.WaitWhile => ProcessWaitWhileTask(task),
                JActionTaskType.RepeatWhile => ProcessRepeatWhileTask(task),
                JActionTaskType.RepeatUntil => ProcessRepeatUntilTask(task),
                JActionTaskType.Repeat => ProcessRepeatTask(task),
                _ => true
            };
        }

        private bool ProcessActionTask(JActionTask task)
        {
            try { task.InvokeAction(); }
            catch (Exception e) { Debug.LogException(e); }
            return true;
        }

        private bool ProcessAsyncFuncTask(JActionTask task)
        {
            if (!_awaitingAsync)
            {
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

            if (_pendingAwaitable.GetAwaiter().IsCompleted)
            {
                _awaitingAsync = false;
                _pendingAwaitable = default;
                return true;
            }
            return false;
        }

        private bool ProcessDelayTask(JActionTask task)
        {
            float currentTime = Time.realtimeSinceStartup;
            if (_delayEndTime <= 0)
                _delayEndTime = currentTime + task.FloatParam1;
            if (currentTime >= _delayEndTime)
            {
                _delayEndTime = 0;
                return true;
            }
            return false;
        }

        private bool ProcessDelayFrameTask(JActionTask task)
        {
            float currentTime = Time.realtimeSinceStartup;
            int currentFrame = Time.frameCount;

            if (_blockingMode)
            {
                if (_delayEndTime <= 0)
                {
                    float frameTime = Mathf.Max(Time.unscaledDeltaTime, 0.001f);
                    _delayEndTime = currentTime + (task.IntParam * frameTime);
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
        }

        private bool ProcessWaitUntilTask(JActionTask task)
        {
            float currentTime = Time.realtimeSinceStartup;

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
        }

        private bool ProcessWaitWhileTask(JActionTask task)
        {
            float currentTime = Time.realtimeSinceStartup;

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
        }

        private bool ProcessRepeatWhileTask(JActionTask task)
        {
            float currentTime = Time.realtimeSinceStartup;

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
        }

        private bool ProcessRepeatUntilTask(JActionTask task)
        {
            float currentTime = Time.realtimeSinceStartup;

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
        }

        private bool ProcessRepeatTask(JActionTask task)
        {
            float currentTime = Time.realtimeSinceStartup;

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
        }

        private void OnExecutionComplete()
        {
            IsExecuting = false;
            var continuation = ContinuationCallback;
            ContinuationCallback = null;
            continuation?.Invoke();

            // Note: Context is returned to pool by the caller (JAction or awaiter)
            // after they've captured the Cancelled state
        }

        #endregion

        #region Cancellation

        /// <summary>
        /// Cancels this execution and invokes the cancel callback.
        /// </summary>
        public void Cancel()
        {
            if (!IsExecuting) return;

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
        }

        #endregion
    }
}
