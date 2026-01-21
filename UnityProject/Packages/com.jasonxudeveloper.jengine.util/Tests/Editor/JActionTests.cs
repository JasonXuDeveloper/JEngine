// JActionTests.cs
// EditMode unit tests for JAction (synchronous execution)

using NUnit.Framework;

namespace JEngine.Util.Tests
{
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
        public void Cancel_InvokesOnCancelCallback()
        {
            bool cancelled = false;

            var action = JAction.Create()
                .Do(() => { })
                .OnCancel(() => cancelled = true);

            action.Execute();

            cancelled = false;
            var action2 = JAction.Create()
                .OnCancel(() => cancelled = true);

            // Force executing state and cancel
            typeof(JAction).GetField("IsExecuting",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                ?.SetValue(action2, true);

            action2.Cancel();
            Assert.IsTrue(cancelled);

            action.Dispose();
            action2.Dispose();
        }

        [Test]
        public void Cancel_WithState_PassesStateToCallback()
        {
            int result = 0;

            var action = JAction.Create()
                .OnCancel(x => result = x, 42);

            // Force executing state
            typeof(JAction).GetField("IsExecuting",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                ?.SetValue(action, true);

            action.Cancel();

            Assert.AreEqual(42, result);
            action.Dispose();
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

            var action = JAction.Create()
                .Do(() => counter++)
                .Do(() => counter++);

            action.Reset();
            action.Execute();

            Assert.AreEqual(0, counter);

            action.Dispose();
        }

        [Test]
        public void Reset_AllowsReuse()
        {
            int counter = 0;

            var action = JAction.Create()
                .Do(() => counter++)
                .Execute();

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
        public void Dispose_DuringExecution_CancelsFirst()
        {
            bool cancelled = false;

            var action = JAction.Create()
                .Delay(1f)
                .OnCancel(() => cancelled = true);

            // Start execution
            typeof(JAction).GetField("IsExecuting",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                ?.SetValue(action, true);

            action.Dispose();

            Assert.IsTrue(cancelled);
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

        #endregion

        #region Complex Chaining

        [Test]
        public void ComplexChain_ExecutesInOrder()
        {
            var order = new System.Collections.Generic.List<int>();

            JAction.Create()
                .Do(() => order.Add(1))
                .Delay(0.01f)
                .Do(() => order.Add(2))
                .Repeat(() => order.Add(3), count: 2)
                .Do(() => order.Add(4))
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
            var results = new System.Collections.Generic.List<string>();

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
                    d =>
                    {
                        actionCount++;
                        d.Value++;
                    },
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
                    d =>
                    {
                        actionCount++;
                        d.Value++;
                    },
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
}