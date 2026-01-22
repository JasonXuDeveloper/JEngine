// JActionRuntimeTests.cs
// PlayMode tests for JAction (require Unity frame execution)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                using var action = await JAction.Create()
                    .Delay(2f) // 2 second delay
                    .Do(() => completed = true)
                    .OnCancel(() => cancelled = true)
                    .ExecuteAsync(timeout: 0.1f); // 100ms timeout

                Assert.IsFalse(completed);
                Assert.IsTrue(action.Cancelled);
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
                    .Do(() => order.Add(1))
                    .DelayFrame()
                    .Do(() => order.Add(2))
                    .Delay(0.05f)
                    .Do(() => order.Add(3))
                    .Repeat(() => order.Add(4), count: 2)
                    .Do(() => order.Add(5))
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

        private class TestData
        {
            public int Counter;
        }
    }
}
