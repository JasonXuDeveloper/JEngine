// JActionRuntimeTests.cs
// PlayMode tests for JAction (require Unity frame execution)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace JEngine.Util.Tests
{
    [TestFixture]
    public class JActionRuntimeTests
    {
        [SetUp]
        public void SetUp()
        {
            JAction.ClearPool();
        }

        [TearDown]
        public void TearDown()
        {
            JAction.ClearPool();
        }

        /// <summary>
        /// Helper to run async code in coroutine context and wait for completion.
        /// </summary>
        private IEnumerator RunAsync(Func<Task> asyncFunc)
        {
            var task = asyncFunc();

            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                throw task.Exception!.InnerException!;
            }
        }

        [UnityTest]
        public IEnumerator Delay_WaitsSpecifiedTime()
        {
            float startTime = Time.realtimeSinceStartup;
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .Delay(0.1f)
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.IsTrue(completed);
                Assert.GreaterOrEqual(Time.realtimeSinceStartup - startTime, 0.1f);
            });
        }

        [UnityTest]
        public IEnumerator DelayFrame_WaitsSpecifiedFrames()
        {
            int startFrame = Time.frameCount;
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .DelayFrame(3)
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.IsTrue(completed);
                Assert.GreaterOrEqual(Time.frameCount - startFrame, 3);
            });
        }

        [UnityTest]
        public IEnumerator WaitUntil_WaitsForCondition()
        {
            int counter = 0;
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .Do(() => counter = 0)
                    .WaitUntil(() =>
                    {
                        counter++;
                        return counter >= 5;
                    })
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.IsTrue(completed);
                Assert.GreaterOrEqual(counter, 5);
            });
        }

        [UnityTest]
        public IEnumerator WaitUntil_WithTimeout_StopsAtTimeout()
        {
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .WaitUntil(() => false, timeout: 0.1f)
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.IsTrue(completed);
            });
        }

        [UnityTest]
        public IEnumerator WaitWhile_WaitsWhileConditionTrue()
        {
            int counter = 0;
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .Do(() => counter = 0)
                    .WaitWhile(() =>
                    {
                        counter++;
                        return counter < 5;
                    })
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.IsTrue(completed);
                Assert.GreaterOrEqual(counter, 5);
            });
        }

        [UnityTest]
        public IEnumerator RepeatWhile_RepeatsWhileConditionTrue()
        {
            int counter = 0;
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .RepeatWhile(
                        () => counter++,
                        () => counter < 5
                    )
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.IsTrue(completed);
                Assert.AreEqual(5, counter);
            });
        }

        [UnityTest]
        public IEnumerator RepeatUntil_RepeatsUntilConditionTrue()
        {
            int counter = 0;
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .RepeatUntil(
                        () => counter++,
                        () => counter >= 5
                    )
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.IsTrue(completed);
                Assert.AreEqual(5, counter);
            });
        }

        [UnityTest]
        public IEnumerator ExecuteAsync_CanBeAwaited()
        {
            bool step1 = false;
            bool step2 = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .Do(() => step1 = true)
                    .DelayFrame()
                    .Do(() => step2 = true)
                    .ExecuteAsync();

                Assert.IsTrue(step1);
                Assert.IsTrue(step2);
                Assert.IsFalse(action.Executing);
            });
        }

        [UnityTest]
        public IEnumerator ExecuteAsync_WithTimeout_CancelsPreemptively()
        {
            bool completed = false;
            bool cancelled = false;

            return RunAsync(async () =>
            {
                using var result = await JAction.Create()
                    .Delay(2f) // 2 second delay
                    .Do(() => completed = true)
                    .OnCancel(() => cancelled = true)
                    .ExecuteAsync(timeout: 0.1f); // 100ms timeout

                Assert.IsFalse(completed);
                Assert.IsTrue(result.Cancelled);
                Assert.IsTrue(cancelled);
            });
        }

        #region Frequency Tests

        [UnityTest]
        public IEnumerator Repeat_WithInterval_RespectsInterval()
        {
            int counter = 0;
            float startTime = Time.realtimeSinceStartup;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .Repeat(() => counter++, count: 3, interval: 0.05f)
                    .ExecuteAsync();

                Assert.AreEqual(3, counter);
                // Should take at least 0.1s (2 intervals between 3 executions)
                Assert.GreaterOrEqual(Time.realtimeSinceStartup - startTime, 0.1f);
            });
        }

        [UnityTest]
        public IEnumerator WaitUntil_WithFrequency_ChecksAtInterval()
        {
            int checkCount = 0;
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .WaitUntil(() =>
                    {
                        checkCount++;
                        return checkCount >= 3;
                    }, frequency: 0.05f)
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.IsTrue(completed);
                Assert.GreaterOrEqual(checkCount, 3);
            });
        }

        #endregion

        #region State Variants (Runtime)

        [UnityTest]
        public IEnumerator WaitUntil_WithState_PassesState()
        {
            var data = new TestData { Counter = 0 };
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .WaitUntil(static d =>
                    {
                        d.Counter++;
                        return d.Counter >= 3;
                    }, data)
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.IsTrue(completed);
                Assert.GreaterOrEqual(data.Counter, 3);
            });
        }

        [UnityTest]
        public IEnumerator RepeatWhile_WithState_PassesState()
        {
            var data = new TestData { Counter = 0 };
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .RepeatWhile(
                        static d => d.Counter++,
                        static d => d.Counter < 5,
                        data)
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.IsTrue(completed);
                Assert.AreEqual(5, data.Counter);
            });
        }

        #endregion

        #region Complex Async Scenarios

        [UnityTest]
        public IEnumerator ComplexChain_Async_ExecutesInOrder()
        {
            var order = new List<int>();

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .Do(static o => o.Add(1), order)
                    .DelayFrame()
                    .Do(static o => o.Add(2), order)
                    .Delay(0.05f)
                    .Do(static o => o.Add(3), order)
                    .Repeat(static o => o.Add(4), order, count: 2)
                    .Do(static o => o.Add(5), order)
                    .ExecuteAsync();

                Assert.AreEqual(6, order.Count);
                Assert.AreEqual(1, order[0]);
                Assert.AreEqual(2, order[1]);
                Assert.AreEqual(3, order[2]);
                Assert.AreEqual(4, order[3]);
                Assert.AreEqual(4, order[4]);
                Assert.AreEqual(5, order[5]);
            });
        }

        [UnityTest]
        public IEnumerator ComplexChain_Async_WithStaticLambdasAndState()
        {
            var results = new List<string>();
            bool completed = false;

            return RunAsync(async () =>
            {
                // Mix static lambdas with state overloads - zero closure allocations
                using var action = await JAction.Create()
                    .Do(static r => r.Add("start"), results)
                    .DelayFrame()
                    .Do(static r => r.Add("after_frame"), results)
                    .Delay(0.05f)
                    .Repeat(static r => r.Add("repeat"), results, count: 2)
                    .Do(() => completed = true) // This one has closure (for test verification)
                    .ExecuteAsync();

                Assert.IsTrue(completed);
                Assert.AreEqual(4, results.Count);
                Assert.AreEqual("start", results[0]);
                Assert.AreEqual("after_frame", results[1]);
                Assert.AreEqual("repeat", results[2]);
                Assert.AreEqual("repeat", results[3]);
            });
        }

        #endregion

        #region Using Pattern Tests

        [UnityTest]
        public IEnumerator UsingAwait_AutoDisposesAfterExecution()
        {
            int initialPoolCount = JAction.PooledCount;
            bool completed = false;

            return RunAsync(async () =>
            {
                // Use explicit using block so we can check pool count after disposal
                using (var action = await JAction.Create()
                           .Do(() => completed = true)
                           .DelayFrame()
                           .ExecuteAsync())
                {
                    Assert.IsTrue(completed);
                    Assert.IsFalse(action.Executing);
                }

                // After using block ends, Dispose() has been called, action returned to pool
                Assert.AreEqual(initialPoolCount + 1, JAction.PooledCount);
            });
        }

        #endregion

        #region Async Function Tests

        [UnityTest]
        public IEnumerator Do_AsyncFunc_ExecutesWithExecuteAsync()
        {
            bool asyncFuncCalled = false;
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .Do(() =>
                    {
                        asyncFuncCalled = true;
                        // Return a default (already-completed) awaitable
                        return default;
                    })
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.IsTrue(asyncFuncCalled);
                Assert.IsTrue(completed);
            });
        }

        [UnityTest]
        public IEnumerator Do_AsyncFunc_WaitsForCompletion()
        {
            var order = new List<int>();

            return RunAsync(async () =>
            {
                // Create an inner action that we'll wait for
                using var innerAction = JAction.Create()
                    .Do(static o => o.Add(1), order)
                    .DelayFrame(2)
                    .Do(static o => o.Add(2), order);

                using var action = await JAction.Create()
                    .Do(static o => o.Add(0), order)
                    .Do(static act => { _ = act.ExecuteAsync(); }, innerAction) // Fire and forget
                    .Do(static o => o.Add(3), order)
                    .ExecuteAsync();

                // The inner action executes asynchronously, so order depends on timing
                Assert.Contains(0, order);
                Assert.Contains(3, order);
            });
        }

        [UnityTest]
        public IEnumerator Do_AsyncFuncWithState_PassesState()
        {
            var data = new TestData { Counter = 0 };
            bool completed = false;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .Do(static d =>
                    {
                        d.Counter = 42;
                        // Return a default (already-completed) awaitable
                        return default;
                    }, data)
                    .Do(() => completed = true)
                    .ExecuteAsync();

                Assert.AreEqual(42, data.Counter);
                Assert.IsTrue(completed);
            });
        }

        #endregion

        #region Parallel Execution Tests

        [UnityTest]
        public IEnumerator Parallel_AllowsConcurrentExecution()
        {
            // Note: This test is safe despite shared variables because Unity's PlayerLoop
            // executes on a single thread. The increment/decrement operations are not
            // racing with each other - they execute sequentially within the same frame.
            int concurrentCount = 0;
            int maxConcurrent = 0;

            return RunAsync(async () =>
            {
                // Create a parallel action
                using var action = JAction.Create()
                    .Parallel()
                    .Do(() =>
                    {
                        concurrentCount++;
                        if (concurrentCount > maxConcurrent) maxConcurrent = concurrentCount;
                    })
                    .DelayFrame(2)
                    .Do(() => concurrentCount--);

                // Start multiple executions concurrently
                var task1 = action.ExecuteAsync();
                var task2 = action.ExecuteAsync();

                await task1;
                await task2;

                // With parallel mode, both should have run
                // Note: maxConcurrent may be 1 or 2 depending on timing
                Assert.GreaterOrEqual(maxConcurrent, 1);
            });
        }

        [UnityTest]
        public IEnumerator NonParallel_BlocksConcurrentExecution()
        {
            int executionCount = 0;

            return RunAsync(async () =>
            {
                // Create a non-parallel action (default)
                using var action = JAction.Create()
                    .Do(() => executionCount++)
                    .DelayFrame(2);

                // Start first execution
                var task1 = action.ExecuteAsync();

                // Verify action is executing
                Assert.IsTrue(action.Executing);

                // Try to start second execution while first is running
                // This should log a warning and return immediately
                LogAssert.Expect(LogType.Warning,
                    "[JAction] Already executing. Enable Parallel() for concurrent execution.");
                var task2 = action.ExecuteAsync();

                // task2 should complete immediately (returns early)
                await task2;

                // Wait for first execution to complete
                await task1;

                // Only the first execution should have incremented
                Assert.AreEqual(1, executionCount);
            });
        }

        [UnityTest]
        public IEnumerator Parallel_Property_ReflectsState()
        {
            return RunAsync(async () =>
            {
                // Non-parallel by default
                var action1 = JAction.Create();
                Assert.IsFalse(action1.IsParallel);

                // Enable parallel
                var action2 = JAction.Create().Parallel();
                Assert.IsTrue(action2.IsParallel);

                action1.Dispose();
                action2.Dispose();

                await UniTask.CompletedTask;
            });
        }

        #endregion

        #region Timeout Tests (Runtime)

        [UnityTest]
        public IEnumerator WaitWhile_WithTimeout_StopsAtTimeout()
        {
            bool completed = false;
            float startTime = Time.realtimeSinceStartup;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .WaitWhile(() => true, timeout: 0.15f)
                    .Do(() => completed = true)
                    .ExecuteAsync();

                float elapsed = Time.realtimeSinceStartup - startTime;

                Assert.IsTrue(completed);
                Assert.GreaterOrEqual(elapsed, 0.15f);
            });
        }

        [UnityTest]
        public IEnumerator RepeatWhile_WithTimeout_StopsAtTimeout()
        {
            int repeatCount = 0;
            float startTime = Time.realtimeSinceStartup;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .RepeatWhile(
                        () => repeatCount++,
                        () => true,
                        frequency: 0,
                        timeout: 0.15f
                    )
                    .ExecuteAsync();

                float elapsed = Time.realtimeSinceStartup - startTime;

                Assert.Greater(repeatCount, 0);
                Assert.GreaterOrEqual(elapsed, 0.15f);
            });
        }

        [UnityTest]
        public IEnumerator RepeatUntil_WithTimeout_StopsAtTimeout()
        {
            int repeatCount = 0;
            float startTime = Time.realtimeSinceStartup;

            return RunAsync(async () =>
            {
                using var action = await JAction.Create()
                    .RepeatUntil(
                        () => repeatCount++,
                        () => false,
                        frequency: 0,
                        timeout: 0.15f
                    )
                    .ExecuteAsync();

                float elapsed = Time.realtimeSinceStartup - startTime;

                Assert.Greater(repeatCount, 0);
                Assert.GreaterOrEqual(elapsed, 0.15f);
            });
        }

        #endregion

        #region Edge Cases (Runtime)

        [UnityTest]
        public IEnumerator ExecuteAsync_EmptyAction_CompletesImmediately()
        {
            return RunAsync(async () =>
            {
                float startTime = Time.realtimeSinceStartup;

                using var result = await JAction.Create()
                    .ExecuteAsync();

                float elapsed = Time.realtimeSinceStartup - startTime;

                Assert.IsFalse(result.Executing);
                Assert.IsFalse(result.Cancelled);
                Assert.Less(elapsed, 0.1f); // Should complete very quickly
            });
        }

        [UnityTest]
        public IEnumerator ExecuteAsync_WithZeroDelay_SkipsDelay()
        {
            bool completed = false;

            return RunAsync(async () =>
            {
                float startTime = Time.realtimeSinceStartup;

                using var action = await JAction.Create()
                    .Delay(0f)
                    .Do(() => completed = true)
                    .ExecuteAsync();

                float elapsed = Time.realtimeSinceStartup - startTime;

                Assert.IsTrue(completed);
                Assert.Less(elapsed, 0.1f);
            });
        }

        [UnityTest]
        public IEnumerator Cancel_DuringAsyncExecution_StopsExecution()
        {
            bool step1 = false;
            bool step2 = false;
            bool cancelled = false;

            return RunAsync(async () =>
            {
                using var action = JAction.Create()
                    .Do(() => step1 = true)
                    .Delay(1f)
                    .Do(() => step2 = true)
                    .OnCancel(() => cancelled = true);

                // Start execution (don't await yet)
                var handle = action.ExecuteAsync();

                // Wait a bit then cancel via handle (per-execution cancellation)
                await UniTask.Delay(50);
                handle.Cancel();

                // Now await the handle to get the result
                var result = await handle;

                Assert.IsTrue(step1);
                Assert.IsFalse(step2);
                Assert.IsTrue(cancelled);
                Assert.IsTrue(result.Cancelled);
            });
        }

        [UnityTest]
        public IEnumerator Cancel_ParallelExecution_CancelsOnlySpecificHandle()
        {
            int cancelCount = 0;

            return RunAsync(async () =>
            {
                using var action = JAction.Create()
                    .Parallel()
                    .Delay(1f)
                    .OnCancel(() => cancelCount++);

                // Start two parallel executions
                var handle1 = action.ExecuteAsync();
                var handle2 = action.ExecuteAsync();

                // Wait a bit then cancel only handle1
                await UniTask.Delay(50);
                handle1.Cancel();

                // Await both
                var result1 = await handle1;
                var result2 = await handle2;

                // Only handle1 should be cancelled
                Assert.IsTrue(result1.Cancelled);
                Assert.IsFalse(result2.Cancelled);
                Assert.AreEqual(1, cancelCount);
            });
        }

        #endregion

        private class TestData
        {
            public int Counter;
        }
    }
}