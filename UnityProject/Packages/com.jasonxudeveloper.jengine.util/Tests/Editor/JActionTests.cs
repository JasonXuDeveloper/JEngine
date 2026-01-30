// JActionTests.cs
// EditMode unit tests for JAction (synchronous execution)

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace JEngine.Util.Tests
{
    #region JObjectPool Tests

    [TestFixture]
    public class JObjectPoolTests
    {
        private class TestPoolItem
        {
            public int Value;
            public bool WasRented;
            public bool WasReturned;
        }

        [Test]
        public void Shared_ReturnsSameInstanceForType()
        {
            var pool1 = JObjectPool.Shared<TestPoolItem>();
            var pool2 = JObjectPool.Shared<TestPoolItem>();

            Assert.AreSame(pool1, pool2);
        }

        [Test]
        public void Shared_IsolatesBetweenTypes()
        {
            var pool1 = JObjectPool.Shared<TestPoolItem>();
            var pool2 = JObjectPool.Shared<List<int>>();

            // Can't directly compare different generic types, but we can verify
            // they maintain separate state by checking they're both functional
            Assert.IsNotNull(pool1);
            Assert.IsNotNull(pool2);

            // Rent from each - they should not interfere
            var item1 = pool1.Rent();
            var item2 = pool2.Rent();

            Assert.IsInstanceOf<TestPoolItem>(item1);
            Assert.IsInstanceOf<List<int>>(item2);

            pool1.Return(item1);
            pool2.Return(item2);
        }

        [Test]
        public void Prewarm_CreatesSpecifiedCount()
        {
            var pool = new JObjectPool<TestPoolItem>(maxSize: 10);

            Assert.AreEqual(0, pool.Count);

            pool.Prewarm(5);

            Assert.AreEqual(5, pool.Count);
        }

        [Test]
        public void Prewarm_RespectsMaxSize()
        {
            var pool = new JObjectPool<TestPoolItem>(maxSize: 3);

            pool.Prewarm(10); // Request more than max

            Assert.AreEqual(3, pool.Count); // Should cap at maxSize
        }

        [Test]
        public void Return_IgnoresNull()
        {
            var pool = new JObjectPool<TestPoolItem>(maxSize: 10);

            Assert.DoesNotThrow(() => pool.Return(null));
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void Return_DiscardsWhenAtCapacity()
        {
            var pool = new JObjectPool<TestPoolItem>(maxSize: 2);

            pool.Prewarm(2); // Fill to capacity
            Assert.AreEqual(2, pool.Count);

            var extraItem = new TestPoolItem();
            pool.Return(extraItem); // Should be discarded

            Assert.AreEqual(2, pool.Count); // Still at capacity
        }

        [Test]
        public void OnRent_CallbackInvoked()
        {
            int rentCallCount = 0;
            var pool = new JObjectPool<TestPoolItem>(
                maxSize: 10,
                onRent: item =>
                {
                    rentCallCount++;
                    item.WasRented = true;
                }
            );

            var item = pool.Rent();

            Assert.AreEqual(1, rentCallCount);
            Assert.IsTrue(item.WasRented);

            pool.Return(item);
        }

        [Test]
        public void OnReturn_CallbackInvoked()
        {
            int returnCallCount = 0;
            var pool = new JObjectPool<TestPoolItem>(
                maxSize: 10,
                onReturn: item =>
                {
                    returnCallCount++;
                    item.WasReturned = true;
                }
            );

            var item = pool.Rent();
            Assert.IsFalse(item.WasReturned);

            pool.Return(item);

            Assert.AreEqual(1, returnCallCount);
            Assert.IsTrue(item.WasReturned);
        }

        [Test]
        public void Rent_CreatesNewInstance_WhenPoolEmpty()
        {
            var pool = new JObjectPool<TestPoolItem>(maxSize: 10);

            Assert.AreEqual(0, pool.Count);

            var item = pool.Rent();

            Assert.IsNotNull(item);
            Assert.IsInstanceOf<TestPoolItem>(item);
        }

        [Test]
        public void Rent_ReusesPooledInstance()
        {
            var pool = new JObjectPool<TestPoolItem>(maxSize: 10);

            var item1 = pool.Rent();
            item1.Value = 42;
            pool.Return(item1);

            var item2 = pool.Rent();

            Assert.AreSame(item1, item2);
            Assert.AreEqual(42, item2.Value); // Value preserved
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            var pool = new JObjectPool<TestPoolItem>(maxSize: 10);

            pool.Prewarm(5);
            Assert.AreEqual(5, pool.Count);

            pool.Clear();

            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void Count_ReflectsPoolState()
        {
            var pool = new JObjectPool<TestPoolItem>(maxSize: 10);

            Assert.AreEqual(0, pool.Count);

            var item1 = pool.Rent();
            Assert.AreEqual(0, pool.Count); // Rented, not in pool

            pool.Return(item1);
            Assert.AreEqual(1, pool.Count);

            var item2 = pool.Rent();
            Assert.AreEqual(0, pool.Count); // Rented again

            pool.Return(item2);
            Assert.AreEqual(1, pool.Count);
        }

        [Test]
        public void Constructor_WithDefaultMaxSize_Uses64()
        {
            var pool = new JObjectPool<TestPoolItem>();

            // Fill beyond default size
            pool.Prewarm(100);

            // Default maxSize is 64
            Assert.AreEqual(64, pool.Count);
        }
    }

    #endregion

    #region JAction Tests

    [TestFixture]
    public class JActionTests
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

        #region Factory and Pool Tests

        [Test]
        public void Create_ReturnsNonNullInstance()
        {
            var action = JAction.Create();

            Assert.IsNotNull(action);
            Assert.IsFalse(action.Executing);
            Assert.IsFalse(action.Cancelled);

            action.Dispose();
        }

        [Test]
        public void Dispose_ReturnsToPool()
        {
            var action = JAction.Create();
            action.Dispose();

            Assert.AreEqual(1, JAction.PooledCount);
        }

        [Test]
        public void Create_AfterDispose_ReusesInstance()
        {
            var action1 = JAction.Create();
            action1.Dispose();

            var action2 = JAction.Create();

            Assert.AreSame(action1, action2);

            action2.Dispose();
        }

        #endregion

        #region Synchronous Execution Tests (onMainThread: false)

        [Test]
        public void Do_ExecutesSynchronousAction()
        {
            int counter = 0;

            JAction.Create()
                .Do(() => counter++)
                .Execute();

            Assert.AreEqual(1, counter);
        }

        [Test]
        public void Do_ChainsMultipleActions()
        {
            int counter = 0;

            JAction.Create()
                .Do(() => counter++)
                .Do(() => counter++)
                .Do(() => counter++)
                .Execute();

            Assert.AreEqual(3, counter);
        }

        [Test]
        public void Do_WithState_PassesStateToAction()
        {
            int result = 0;

            JAction.Create()
                .Do(x => result = x, 42)
                .Execute();

            Assert.AreEqual(42, result);
        }

        [Test]
        public void Do_WithReferenceState_PassesStateToAction()
        {
            var data = new TestData { Value = 10 };
            int result = 0;

            JAction.Create()
                .Do(d => result = d.Value, data)
                .Execute();

            Assert.AreEqual(10, result);
        }

        [Test]
        public void Repeat_ExecutesActionMultipleTimes()
        {
            int counter = 0;

            JAction.Create()
                .Repeat(() => counter++, count: 5)
                .Execute();

            Assert.AreEqual(5, counter);
        }

        [Test]
        public void Repeat_WithState_PassesStateEachTime()
        {
            int sum = 0;

            JAction.Create()
                .Repeat(x => sum += x, state: 10, count: 3)
                .Execute();

            Assert.AreEqual(30, sum);
        }

        #endregion

        #region Cancel Tests

        [Test]
        public void Cancel_InvokesOnCancelCallback_WhenTimeoutExceeded()
        {
            bool cancelled = false;

            // Use timeout to trigger cancellation
            using var action = JAction.Create()
                .Delay(1f) // Long delay
                .OnCancel(() => cancelled = true)
                .Execute(timeout: 0.01f); // Short timeout triggers cancel

            Assert.IsTrue(cancelled);
            Assert.IsTrue(action.Cancelled);
        }

        [Test]
        public void Cancel_WithState_PassesStateToCallback()
        {
            int result = 0;

            // Use timeout to trigger cancellation with state callback
            using var action = JAction.Create()
                .Delay(1f) // Long delay
                .OnCancel(x => result = x, 42)
                .Execute(timeout: 0.01f); // Short timeout triggers cancel

            Assert.AreEqual(42, result);
        }

        [Test]
        public void Cancel_NotExecuting_DoesNothing()
        {
            bool cancelled = false;

            using var action = JAction.Create()
                .Do(() => { })
                .OnCancel(() => cancelled = true);

            // Cancel without executing - should not invoke callback
            action.Cancel();

            Assert.IsFalse(cancelled);
        }

        #endregion

        #region Timeout Tests

        [Test]
        public void Execute_WithTimeout_CancelsWhenExceeded()
        {
            bool completed = false;
            bool cancelled = false;

            var action = JAction.Create()
                .Delay(1f) // 1 second delay
                .Do(() => completed = true)
                .OnCancel(() => cancelled = true)
                .Execute(timeout: 0.05f); // 50ms timeout

            Assert.IsFalse(completed);
            Assert.IsTrue(action.Cancelled);
            Assert.IsTrue(cancelled);

            action.Dispose();
        }

        [Test]
        public void Execute_WithTimeout_CompletesIfFastEnough()
        {
            bool completed = false;

            var action = JAction.Create()
                .Delay(0.01f) // 10ms delay
                .Do(() => completed = true)
                .Execute(timeout: 1f); // 1 second timeout

            Assert.IsTrue(completed);
            Assert.IsFalse(action.Cancelled);

            action.Dispose();
        }

        [Test]
        public void Execute_WithoutTimeout_NoPreemption()
        {
            bool completed = false;

            var action = JAction.Create()
                .Delay(0.05f)
                .Do(() => completed = true)
                .Execute(); // no timeout

            Assert.IsTrue(completed);
            Assert.IsFalse(action.Cancelled);

            action.Dispose();
        }

        #endregion

        #region Reset Tests

        [Test]
        public void Reset_ClearsAllTasks()
        {
            int counter = 0;

            using var action = JAction.Create()
                .Do(() => counter++)
                .Do(() => counter++);

            action.Reset();
            action.Execute();

            Assert.AreEqual(0, counter);
        }

        [Test]
        public void Reset_AllowsReuse()
        {
            int counter = 0;

            var action = JAction.Create()
                .Do(() => counter++);

            action.Execute();
            Assert.AreEqual(1, counter);

            action.Reset();
            action.Do(() => counter += 10)
                .Execute();

            Assert.AreEqual(11, counter);

            action.Dispose();
        }

        #endregion

        #region State Storage Tests

        [Test]
        public void Do_WithIntState_WorksCorrectly()
        {
            int result = 0;

            JAction.Create()
                .Do(x => result = x, 123)
                .Execute();

            Assert.AreEqual(123, result);
        }

        [Test]
        public void Do_WithFloatState_WorksCorrectly()
        {
            float result = 0;

            JAction.Create()
                .Do(x => result = x, 3.14f)
                .Execute();

            Assert.AreEqual(3.14f, result, 0.001f);
        }

        [Test]
        public void Do_WithBoolState_WorksCorrectly()
        {
            bool result = false;

            JAction.Create()
                .Do(x => result = x, true)
                .Execute();

            Assert.IsTrue(result);
        }

        #endregion

        #region Edge Cases

        [Test]
        public void Execute_EmptyAction_CompletesImmediately()
        {
            var action = JAction.Create().Execute();

            Assert.IsFalse(action.Executing);
            Assert.IsFalse(action.Cancelled);

            action.Dispose();
        }

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            var action = JAction.Create()
                .Do(() => { });

            // Multiple dispose calls should not throw
            Assert.DoesNotThrow(() =>
            {
                action.Dispose();
                action.Dispose();
                action.Dispose();
            });
        }

        [Test]
        public void Execute_WithNullAction_SkipsGracefully()
        {
            int counter = 0;

            JAction.Create()
                .Do(null)
                .Do(() => counter++)
                .Execute();

            Assert.AreEqual(1, counter);
        }

        [Test]
        public void Delay_ZeroValue_SkipsDelay()
        {
            int counter = 0;

            JAction.Create()
                .Delay(0f) // Zero delay should be skipped
                .Do(() => counter++)
                .Execute();

            Assert.AreEqual(1, counter);
        }

        [Test]
        public void Delay_NegativeValue_SkipsDelay()
        {
            int counter = 0;

            JAction.Create()
                .Delay(-1f) // Negative delay should be skipped
                .Do(() => counter++)
                .Execute();

            Assert.AreEqual(1, counter);
        }

        [Test]
        public void DelayFrame_ZeroValue_SkipsDelay()
        {
            int counter = 0;

            JAction.Create()
                .DelayFrame(0) // Zero frames should be skipped
                .Do(() => counter++)
                .Execute();

            Assert.AreEqual(1, counter);
        }

        [Test]
        public void DelayFrame_NegativeValue_SkipsDelay()
        {
            int counter = 0;

            JAction.Create()
                .DelayFrame(-1) // Negative frames should be skipped
                .Do(() => counter++)
                .Execute();

            Assert.AreEqual(1, counter);
        }

        [Test]
        public void TaskCapacity_ThrowsWhenExceeded()
        {
            using var action = JAction.Create();

            // Add 256 tasks (the max capacity)
            for (int i = 0; i < 256; i++)
            {
                action.Do(() => { });
            }

            // The 257th task should throw
            Assert.Throws<InvalidOperationException>(() => action.Do(() => { }));
        }

        [Test]
        public void Reset_ClearsTasks()
        {
            int counter = 0;

            using var action = JAction.Create()
                .Do(() => counter++)
                .Do(() => counter++);

            // Reset should clear the tasks
            action.Reset();

            // Execute after reset - should do nothing (no tasks)
            action.Execute();

            Assert.AreEqual(0, counter);
        }

        [Test]
        public void Reset_ClearsParallelMode()
        {
            using var action = JAction.Create()
                .Parallel();

            Assert.IsTrue(action.IsParallel);

            action.Reset();

            Assert.IsFalse(action.IsParallel);
        }

        #endregion

        #region Timeout Tests for Conditional Operations

        [Test]
        public void WaitWhile_TimeoutStopsWaiting()
        {
            bool completed = false;
            float startTime = UnityEngine.Time.realtimeSinceStartup;

            // WaitWhile with a condition that's always true, but with timeout
            JAction.Create()
                .WaitWhile(() => true, frequency: 0, timeout: 0.1f)
                .Do(() => completed = true)
                .Execute();

            float elapsed = UnityEngine.Time.realtimeSinceStartup - startTime;

            Assert.IsTrue(completed);
            Assert.GreaterOrEqual(elapsed, 0.1f); // Should have waited for timeout
            Assert.Less(elapsed, 0.5f); // But not too long
        }

        [Test]
        public void RepeatWhile_TimeoutStopsRepeating()
        {
            int repeatCount = 0;
            float startTime = UnityEngine.Time.realtimeSinceStartup;

            // RepeatWhile with a condition that's always true, but with timeout
            JAction.Create()
                .RepeatWhile(
                    () => repeatCount++,
                    () => true, // Always true
                    frequency: 0,
                    timeout: 0.1f
                )
                .Execute();

            float elapsed = UnityEngine.Time.realtimeSinceStartup - startTime;

            Assert.Greater(repeatCount, 0); // Should have run at least once
            Assert.GreaterOrEqual(elapsed, 0.1f);
            Assert.Less(elapsed, 0.5f);
        }

        [Test]
        public void RepeatUntil_TimeoutStopsRepeating()
        {
            int repeatCount = 0;
            float startTime = UnityEngine.Time.realtimeSinceStartup;

            // RepeatUntil with a condition that's never true, but with timeout
            JAction.Create()
                .RepeatUntil(
                    () => repeatCount++,
                    () => false, // Never true
                    frequency: 0,
                    timeout: 0.1f
                )
                .Execute();

            float elapsed = UnityEngine.Time.realtimeSinceStartup - startTime;

            Assert.Greater(repeatCount, 0); // Should have run at least once
            Assert.GreaterOrEqual(elapsed, 0.1f);
            Assert.Less(elapsed, 0.5f);
        }

        [Test]
        public void WaitUntil_TimeoutStopsWaiting()
        {
            bool completed = false;
            float startTime = UnityEngine.Time.realtimeSinceStartup;

            // WaitUntil with a condition that's never true, but with timeout
            JAction.Create()
                .WaitUntil(() => false, frequency: 0, timeout: 0.1f)
                .Do(() => completed = true)
                .Execute();

            float elapsed = UnityEngine.Time.realtimeSinceStartup - startTime;

            Assert.IsTrue(completed);
            Assert.GreaterOrEqual(elapsed, 0.1f);
            Assert.Less(elapsed, 0.5f);
        }

        #endregion

        #region Complex Chaining

        [Test]
        public void ComplexChain_ExecutesInOrder()
        {
            var order = new List<int>();

            JAction.Create()
                .Do(static o => o.Add(1), order)
                .Delay(0.01f)
                .Do(static o => o.Add(2), order)
                .Repeat(static o => o.Add(3), order, count: 2)
                .Do(static o => o.Add(4), order)
                .Execute();

            Assert.AreEqual(5, order.Count);
            Assert.AreEqual(1, order[0]);
            Assert.AreEqual(2, order[1]);
            Assert.AreEqual(3, order[2]);
            Assert.AreEqual(3, order[3]);
            Assert.AreEqual(4, order[4]);
        }

        [Test]
        public void ComplexChain_WithState_PassesCorrectState()
        {
            int sum = 0;

            JAction.Create()
                .Do(x => sum += x, 1)
                .Do(x => sum += x, 10)
                .Repeat(x => sum += x, state: 100, count: 2)
                .Do(x => sum += x, 1000)
                .Execute();

            Assert.AreEqual(1211, sum); // 1 + 10 + 100 + 100 + 1000
        }

        [Test]
        public void ComplexChain_MixedStaticAndState_WorksCorrectly()
        {
            var results = new List<string>();

            // Mix static lambdas (no closure) with state overloads
            using var action = JAction.Create()
                .Do(static () => { }) // static lambda, no state needed
                .Do(static r => r.Add("step1"), results)
                .Delay(0.01f)
                .Do(static r => r.Add("step2"), results)
                .Repeat(static r => r.Add("repeat"), results, count: 2)
                .Execute();

            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("step1", results[0]);
            Assert.AreEqual("step2", results[1]);
            Assert.AreEqual("repeat", results[2]);
            Assert.AreEqual("repeat", results[3]);
        }

        [Test]
        public void Using_AutoDisposesAction()
        {
            int initialPoolCount = JAction.PooledCount;

            using (JAction.Create().Do(() => { }).Execute())
            {
                Assert.AreEqual(initialPoolCount, JAction.PooledCount);
            }

            // After using block, should be back in pool
            Assert.AreEqual(initialPoolCount + 1, JAction.PooledCount);
        }

        #endregion

        #region Conditional With State (Synchronous)

        [Test]
        public void WaitUntil_WithState_PassesStateToCondition()
        {
            var data = new TestData { Value = 0 };
            bool completed = false;

            JAction.Create()
                .WaitUntil(d =>
                {
                    d.Value++;
                    return d.Value >= 3;
                }, data)
                .Do(() => completed = true)
                .Execute();

            Assert.IsTrue(completed);
            Assert.GreaterOrEqual(data.Value, 3);
        }

        [Test]
        public void WaitWhile_WithState_PassesStateToCondition()
        {
            var data = new TestData { Value = 0 };
            bool completed = false;

            JAction.Create()
                .WaitWhile(d =>
                {
                    d.Value++;
                    return d.Value < 3;
                }, data)
                .Do(() => completed = true)
                .Execute();

            Assert.IsTrue(completed);
            Assert.GreaterOrEqual(data.Value, 3);
        }

        [Test]
        public void RepeatWhile_WithState_PassesStateToActionAndCondition()
        {
            var data = new TestData { Value = 0 };
            int actionCount = 0;

            JAction.Create()
                .RepeatWhile(
                    d => { actionCount++; d.Value++; },
                    d => d.Value < 5,
                    data)
                .Execute();

            Assert.AreEqual(5, actionCount);
            Assert.AreEqual(5, data.Value);
        }

        [Test]
        public void RepeatUntil_WithState_PassesStateToActionAndCondition()
        {
            var data = new TestData { Value = 0 };
            int actionCount = 0;

            JAction.Create()
                .RepeatUntil(
                    d => { actionCount++; d.Value++; },
                    d => d.Value >= 5,
                    data)
                .Execute();

            Assert.AreEqual(5, actionCount);
            Assert.AreEqual(5, data.Value);
        }

        #endregion

        private class TestData
        {
            public int Value;
        }
    }

    #endregion
}
