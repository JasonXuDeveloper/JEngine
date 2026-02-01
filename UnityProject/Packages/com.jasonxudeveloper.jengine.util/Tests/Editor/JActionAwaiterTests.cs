// JActionAwaiterTests.cs
// EditMode unit tests for JActionAwaiter and JActionAwaitable

using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace JEngine.Util.Tests
{
    /// <summary>
    /// Tests for <see cref="JActionAwaitable"/> struct via public API.
    /// </summary>
    [TestFixture]
    public class JActionAwaitableTests
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

        [Test]
        public void ExecuteAsync_ReturnsHandle()
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            Assert.IsNotNull(handle.Action);
        }

        [Test]
        public void ExecuteAsync_HandleHasCorrectAction()
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            Assert.AreSame(action, handle.Action);
        }

        [UnityTest]
        public IEnumerator ExecuteAsync_CanBeAwaited() => UniTask.ToCoroutine(async () =>
        {
            bool executed = false;
            using var action = JAction.Create().Do(() => executed = true);

            await action.ExecuteAsync();

            Assert.IsTrue(executed);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_WithDelay_CompletesAfterDelay() => UniTask.ToCoroutine(async () =>
        {
            bool executed = false;
            using var action = JAction.Create()
                .Delay(0.05f)
                .Do(() => executed = true);

            var handle = action.ExecuteAsync();

            // Initially not executed
            Assert.IsFalse(executed);

            // Wait for completion
            await handle;

            Assert.IsTrue(executed);
        });
    }

    /// <summary>
    /// Tests for <see cref="JActionAwaiter"/> struct via public API.
    /// Note: ExecuteAsync always uses async path, so tests use [UnityTest] with coroutines.
    /// </summary>
    [TestFixture]
    public class JActionAwaiterTests
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

        #region GetAwaiter Tests

        [Test]
        public void GetAwaiter_ReturnsAwaiterInstance()
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            var awaiter = handle.GetAwaiter();

            Assert.IsInstanceOf<JActionExecutionAwaiter>(awaiter);
        }

        #endregion

        #region IsCompleted Tests

        [UnityTest]
        public IEnumerator IsCompleted_AfterAwait_ReturnsTrue() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            // Wait for completion
            await handle;

            // Get a new awaiter after completion
            var awaiter = handle.GetAwaiter();
            Assert.IsTrue(awaiter.IsCompleted);
        });

        [UnityTest]
        public IEnumerator IsCompleted_WithDelay_InitiallyFalse() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Delay(0.1f).Do(() => { });
            var handle = action.ExecuteAsync();
            var awaiter = handle.GetAwaiter();

            // Should not be completed immediately
            Assert.IsFalse(awaiter.IsCompleted);

            // Cancel to clean up
            handle.Cancel();
            await UniTask.Yield();
        });

        #endregion

        #region GetResult Tests

        [UnityTest]
        public IEnumerator GetResult_ReturnsJActionExecution() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var result = await action.ExecuteAsync();

            Assert.IsInstanceOf<JActionExecution>(result);
        });

        [UnityTest]
        public IEnumerator GetResult_AfterCompletion_NotCancelled() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create()
                .Delay(0.01f)
                .Do(() => { });

            var result = await action.ExecuteAsync();

            Assert.IsFalse(result.Cancelled);
        });

        [UnityTest]
        public IEnumerator GetResult_AfterCancel_IsCancelled() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create()
                .Delay(10f)
                .Do(() => { });

            var handle = action.ExecuteAsync();
            handle.Cancel();

            var result = await handle;

            Assert.IsTrue(result.Cancelled);
        });

        #endregion

        #region OnCompleted Tests

        [UnityTest]
        public IEnumerator OnCompleted_AfterCompletion_InvokesContinuationImmediately() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            // Wait for completion first
            await handle;

            // Now get awaiter and check OnCompleted
            var awaiter = handle.GetAwaiter();
            bool invoked = false;
            awaiter.OnCompleted(() => invoked = true);

            Assert.IsTrue(invoked);
        });

        [UnityTest]
        public IEnumerator OnCompleted_DuringExecution_RegistersCallback() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Delay(0.05f).Do(() => { });
            var handle = action.ExecuteAsync();
            var awaiter = handle.GetAwaiter();

            bool invoked = false;
            awaiter.OnCompleted(() => invoked = true);

            // Not invoked immediately (still executing)
            Assert.IsFalse(invoked);

            // Wait for completion by polling (don't use await handle - it overwrites OnCompleted callback)
            await UniTask.WaitUntil(() => invoked || !handle.Executing);

            // Callback should have been invoked
            Assert.IsTrue(invoked);
        });

        [Test]
        public void OnCompleted_NullContinuation_HandlesGracefully()
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();
            var awaiter = handle.GetAwaiter();

            Assert.DoesNotThrow(() => awaiter.OnCompleted(null));
        }

        #endregion

        #region UnsafeOnCompleted Tests

        [UnityTest]
        public IEnumerator UnsafeOnCompleted_AfterCompletion_InvokesContinuationImmediately() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            // Wait for completion first
            await handle;

            // Now get awaiter and check UnsafeOnCompleted
            var awaiter = handle.GetAwaiter();
            bool invoked = false;
            awaiter.UnsafeOnCompleted(() => invoked = true);

            Assert.IsTrue(invoked);
        });

        [Test]
        public void UnsafeOnCompleted_NullContinuation_HandlesGracefully()
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();
            var awaiter = handle.GetAwaiter();

            Assert.DoesNotThrow(() => awaiter.UnsafeOnCompleted(null));
        }

        #endregion

        #region UnsafeOnCompleted During Execution Tests

        [UnityTest]
        public IEnumerator UnsafeOnCompleted_DuringExecution_RegistersCallback() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Delay(0.05f).Do(() => { });
            var handle = action.ExecuteAsync();
            var awaiter = handle.GetAwaiter();

            bool invoked = false;
            awaiter.UnsafeOnCompleted(() => invoked = true);

            // Not invoked immediately (still executing)
            Assert.IsFalse(invoked);

            // Wait for completion by polling
            await UniTask.WaitUntil(() => invoked || !handle.Executing);

            // Callback should have been invoked
            Assert.IsTrue(invoked);
        });

        #endregion
    }

    /// <summary>
    /// Tests for nested JAction execution patterns.
    /// </summary>
    [TestFixture]
    public class JActionNestedExecutionTests
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

        #region Sequential Nested Execution Tests

        [UnityTest]
        public IEnumerator NestedExecution_ExecutesInSequence() => UniTask.ToCoroutine(async () =>
        {
            int executionOrder = 0;
            int first = 0, second = 0, third = 0;

            using var action = JAction.Create()
                .Do(() => first = ++executionOrder)
                .Do(() => second = ++executionOrder)
                .Do(() => third = ++executionOrder);

            await action.ExecuteAsync();

            Assert.AreEqual(1, first);
            Assert.AreEqual(2, second);
            Assert.AreEqual(3, third);
        });

        [UnityTest]
        public IEnumerator NestedExecution_WithDelays_WaitsForEach() => UniTask.ToCoroutine(async () =>
        {
            bool step1 = false, step2 = false, step3 = false;

            using var action = JAction.Create()
                .Do(() => step1 = true)
                .Delay(0.02f)
                .Do(() => step2 = true)
                .Delay(0.02f)
                .Do(() => step3 = true);

            await action.ExecuteAsync();

            Assert.IsTrue(step1);
            Assert.IsTrue(step2);
            Assert.IsTrue(step3);
        });

        #endregion

        #region Parallel Execution Tests

        [UnityTest]
        public IEnumerator ParallelExecution_RunsMultipleHandles() => UniTask.ToCoroutine(async () =>
        {
            int count = 0;

            using var action = JAction.Create()
                .Parallel()
                .Delay(0.02f)
                .Do(() => System.Threading.Interlocked.Increment(ref count));

            var handle1 = action.ExecuteAsync();
            var handle2 = action.ExecuteAsync();
            var handle3 = action.ExecuteAsync();

            await UniTask.WhenAll(handle1.AsUniTask(), handle2.AsUniTask(), handle3.AsUniTask());

            Assert.AreEqual(3, count);
        });

        #endregion

        #region Cancellation Tests

        [UnityTest]
        public IEnumerator Cancel_DuringNestedExecution_StopsExecution() => UniTask.ToCoroutine(async () =>
        {
            bool step1 = false;
            bool step2 = false;

            using var action = JAction.Create()
                .Do(() => step1 = true)
                .Delay(10f)
                .Do(() => step2 = true);

            var handle = action.ExecuteAsync();

            // Wait for first step
            await UniTask.WaitUntil(() => step1);

            handle.Cancel();
            await handle;

            Assert.IsTrue(step1);
            Assert.IsFalse(step2);
        });

        #endregion
    }
}
