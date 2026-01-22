// JActionTask.cs
// Task struct and enums for JAction execution
//
// Author: JasonXuDeveloper

using System;
using System.Runtime.CompilerServices;

namespace JEngine.Util.Internal
{
    /// <summary>
    /// Defines the type of operation a <see cref="JActionTask"/> represents.
    /// </summary>
    internal enum JActionTaskType : byte
    {
        /// <summary>Synchronous action execution.</summary>
        Action,
        /// <summary>Asynchronous function execution.</summary>
        AsyncFunc,
        /// <summary>Time-based delay in seconds.</summary>
        Delay,
        /// <summary>Frame-based delay.</summary>
        DelayFrame,
        /// <summary>Wait until a condition becomes true.</summary>
        WaitUntil,
        /// <summary>Wait while a condition remains true.</summary>
        WaitWhile,
        /// <summary>Repeat while a condition is true.</summary>
        RepeatWhile,
        /// <summary>Repeat until a condition becomes true.</summary>
        RepeatUntil,
        /// <summary>Repeat a fixed number of times.</summary>
        Repeat
    }

    /// <summary>
    /// Lightweight struct representing a single task item in a <see cref="JAction"/> chain.
    /// Uses polymorphic <see cref="IStateStorage"/> for typed state without boxing.
    /// </summary>
    /// <remarks>
    /// This struct is designed for minimal memory footprint and efficient execution.
    /// State is stored externally via <see cref="IStateStorage"/> to avoid boxing value types.
    /// </remarks>
    internal record struct JActionTask
    {
        /// <summary>The type of task operation.</summary>
        internal JActionTaskType Type;

        /// <summary>Stateless synchronous action (used when <see cref="State"/> is null).</summary>
        internal Action SyncAction;

        /// <summary>Stateless async function (used when <see cref="State"/> is null).</summary>
        internal Func<JActionAwaitable> AsyncFunc;

        /// <summary>Stateless condition function (used when <see cref="State"/> is null).</summary>
        internal Func<bool> Condition;

        /// <summary>Stateful action/async delegate (cast based on <see cref="State"/> type).</summary>
        internal Delegate ActionDelegate;

        /// <summary>Stateful condition delegate (cast based on <see cref="State"/> type).</summary>
        internal Delegate ConditionDelegate;

        /// <summary>Polymorphic state storage (null if stateless).</summary>
        internal IStateStorage State;

        /// <summary>Primary float parameter (delay time, frequency, or duration).</summary>
        internal float FloatParam1;

        /// <summary>Secondary float parameter (timeout).</summary>
        internal float FloatParam2;

        /// <summary>Integer parameter (frame count or repeat count).</summary>
        internal int IntParam;

        /// <summary>
        /// Invokes the action delegate (stateless or stateful).
        /// </summary>
        /// <remarks>
        /// If <see cref="State"/> is null, invokes <see cref="SyncAction"/>.
        /// Otherwise, invokes <see cref="ActionDelegate"/> via <see cref="IStateStorage.InvokeAction"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InvokeAction()
        {
            if (State == null)
            {
                SyncAction?.Invoke();
            }
            else
            {
                State.InvokeAction(ActionDelegate);
            }
        }

        /// <summary>
        /// Invokes the async function delegate (stateless or stateful).
        /// </summary>
        /// <returns>The awaitable result, or default if no function is set.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal JActionAwaitable InvokeAsyncFunc()
        {
            if (State == null)
            {
                return AsyncFunc?.Invoke() ?? default;
            }
            return State.InvokeAsyncFunc(ActionDelegate);
        }

        /// <summary>
        /// Invokes the condition delegate (stateless or stateful).
        /// </summary>
        /// <returns>The condition result, or false if no condition is set.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool InvokeCondition()
        {
            if (State == null)
            {
                return Condition?.Invoke() ?? false;
            }
            return State.InvokeCondition(ConditionDelegate);
        }
    }
}
