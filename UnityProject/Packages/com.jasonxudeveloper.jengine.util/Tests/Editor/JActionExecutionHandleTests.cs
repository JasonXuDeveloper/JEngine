// JActionExecutionHandleTests.cs
// EditMode unit tests for JActionExecutionHandle and JActionExecutionAwaiter

using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace JEngine.Util.Tests
{
    /// <summary>
    /// Tests for <see cref="JActionExecutionHandle"/> struct.
    /// Note: ExecuteAsync always uses async path, so completion tests use [UnityTest].
    /// </summary>
    [TestFixture]
    public class JActionExecutionHandleTests
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

        #region Action Property Tests

        [UnityTest]
        public IEnumerator Action_ReturnsAssociatedJAction() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            Assert.AreSame(action, handle.Action);
            await handle;
        });

        [UnityTest]
        public IEnumerator Action_MultipleHandles_ReturnSameAction() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Parallel().Do(() => { });
            var handle1 = action.ExecuteAsync();
            var handle2 = action.ExecuteAsync();

            Assert.AreSame(action, handle1.Action);
            Assert.AreSame(action, handle2.Action);

            await UniTask.WhenAll(handle1.AsUniTask(), handle2.AsUniTask());
        });

        #endregion

        #region Cancelled Property Tests

        [Test]
        public void Cancelled_DefaultHandle_ReturnsFalse()
        {
            var handle = default(JActionExecutionHandle);

            Assert.IsFalse(handle.Cancelled);
        }

        [UnityTest]
        public IEnumerator Cancelled_AfterCompletion_ReturnsFalse() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            await handle;

            Assert.IsFalse(handle.Cancelled);
        });

        [UnityTest]
        public IEnumerator Cancelled_AfterCancel_ReturnsTrue() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Delay(10f);
            var handle = action.ExecuteAsync();

            handle.Cancel();

            Assert.IsTrue(handle.Cancelled);
            await UniTask.Yield();
        });

        #endregion

        #region Executing Property Tests

        [Test]
        public void Executing_DefaultHandle_ReturnsFalse()
        {
            var handle = default(JActionExecutionHandle);

            Assert.IsFalse(handle.Executing);
        }

        [UnityTest]
        public IEnumerator Executing_AfterCompletion_ReturnsFalse() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            await handle;

            Assert.IsFalse(handle.Executing);
        });

        [UnityTest]
        public IEnumerator Executing_DuringExecution_ReturnsTrue() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Delay(10f);
            var handle = action.ExecuteAsync();

            Assert.IsTrue(handle.Executing);

            handle.Cancel();
            await UniTask.Yield();
        });

        [UnityTest]
        public IEnumerator Executing_AfterCancel_ReturnsFalse() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Delay(10f);
            var handle = action.ExecuteAsync();

            handle.Cancel();
            await handle;

            Assert.IsFalse(handle.Executing);
        });

        #endregion

        #region Cancel Tests

        [Test]
        public void Cancel_DefaultHandle_DoesNotThrow()
        {
            var handle = default(JActionExecutionHandle);

            Assert.DoesNotThrow(() => handle.Cancel());
        }

        [UnityTest]
        public IEnumerator Cancel_CompletedHandle_DoesNotThrow() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            await handle;

            Assert.DoesNotThrow(() => handle.Cancel());
        });

        [UnityTest]
        public IEnumerator Cancel_MultipleTimes_DoesNotThrow() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Delay(10f);
            var handle = action.ExecuteAsync();

            Assert.DoesNotThrow(() =>
            {
                handle.Cancel();
                handle.Cancel();
                handle.Cancel();
            });

            await handle;
        });

        [UnityTest]
        public IEnumerator Cancel_StopsExecution() => UniTask.ToCoroutine(async () =>
        {
            bool completed = false;
            using var action = JAction.Create()
                .Delay(10f)
                .Do(() => completed = true);

            var handle = action.ExecuteAsync();
            handle.Cancel();

            Assert.IsTrue(handle.Cancelled);
            Assert.IsFalse(completed);
            await UniTask.Yield();
        });

        [UnityTest]
        public IEnumerator Cancel_InvokesOnCancelCallback() => UniTask.ToCoroutine(async () =>
        {
            bool cancelled = false;
            using var action = JAction.Create()
                .Delay(10f)
                .OnCancel(() => cancelled = true);

            var handle = action.ExecuteAsync();
            handle.Cancel();

            Assert.IsTrue(cancelled);
            await UniTask.Yield();
        });

        #endregion

        #region GetAwaiter Tests

        [UnityTest]
        public IEnumerator GetAwaiter_ReturnsAwaiter() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            var awaiter = handle.GetAwaiter();

            Assert.IsInstanceOf<JActionExecutionAwaiter>(awaiter);
            await handle;
        });

        [UnityTest]
        public IEnumerator GetAwaiter_AfterCompletion_ReturnsCompletedAwaiter() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            await handle;

            var awaiter = handle.GetAwaiter();
            Assert.IsTrue(awaiter.IsCompleted);
        });

        #endregion

        #region Dispose Tests

        [UnityTest]
        public IEnumerator Dispose_ReturnsActionToPool() => UniTask.ToCoroutine(async () =>
        {
            int initialCount = JAction.PooledCount;
            var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            await handle;
            handle.Dispose();

            Assert.AreEqual(initialCount + 1, JAction.PooledCount);
        });

        [Test]
        public void Dispose_DefaultHandle_DoesNotThrow()
        {
            var handle = default(JActionExecutionHandle);

            Assert.DoesNotThrow(() => handle.Dispose());
        }

        [UnityTest]
        public IEnumerator Dispose_MultipleTimes_DoesNotThrow() => UniTask.ToCoroutine(async () =>
        {
            var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            await handle;

            Assert.DoesNotThrow(() =>
            {
                handle.Dispose();
                handle.Dispose();
            });
        });

        #endregion

        #region Implicit Operator Tests

        [UnityTest]
        public IEnumerator ImplicitOperator_ConvertsToJAction() => UniTask.ToCoroutine(async () =>
        {
            var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            JAction converted = handle;

            Assert.AreSame(action, converted);

            await handle;
            action.Dispose();
        });

        [Test]
        public void ImplicitOperator_DefaultHandle_ReturnsNull()
        {
            var handle = default(JActionExecutionHandle);

            JAction converted = handle;

            Assert.IsNull(converted);
        }

        #endregion

        #region AsUniTask Tests

        [UnityTest]
        public IEnumerator AsUniTask_ReturnsUniTask() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            var result = await handle.AsUniTask();

            Assert.IsInstanceOf<JActionExecution>(result);
        });

        [UnityTest]
        public IEnumerator AsUniTask_WithDelay_CompletesAfterDelay() => UniTask.ToCoroutine(async () =>
        {
            bool executed = false;
            using var action = JAction.Create()
                .Delay(0.05f)
                .Do(() => executed = true);

            var handle = action.ExecuteAsync();
            await handle.AsUniTask();

            Assert.IsTrue(executed);
        });

        #endregion
    }

    /// <summary>
    /// Tests for <see cref="JActionExecutionAwaiter"/> struct.
    /// </summary>
    [TestFixture]
    public class JActionExecutionAwaiterTests
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

        #region IsCompleted Tests

        [UnityTest]
        public IEnumerator IsCompleted_AfterCompletion_ReturnsTrue() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            await handle;

            var awaiter = handle.GetAwaiter();
            Assert.IsTrue(awaiter.IsCompleted);
        });

        [UnityTest]
        public IEnumerator IsCompleted_WithDelay_InitiallyFalse() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Delay(10f).Do(() => { });
            var handle = action.ExecuteAsync();
            var awaiter = handle.GetAwaiter();

            Assert.IsFalse(awaiter.IsCompleted);

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
            using var action = JAction.Create().Do(() => { });
            var result = await action.ExecuteAsync();

            Assert.IsFalse(result.Cancelled);
        });

        [UnityTest]
        public IEnumerator GetResult_ReturnsAssociatedAction() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var result = await action.ExecuteAsync();

            Assert.AreSame(action, result.Action);
        });

        [UnityTest]
        public IEnumerator GetResult_CancelledExecution_IsCancelled() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Delay(10f).Do(() => { });
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

            await handle;

            var awaiter = handle.GetAwaiter();
            bool invoked = false;
            awaiter.OnCompleted(() => invoked = true);

            Assert.IsTrue(invoked);
        });

        [UnityTest]
        public IEnumerator OnCompleted_NullContinuation_HandlesGracefully() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();
            var awaiter = handle.GetAwaiter();

            Assert.DoesNotThrow(() => awaiter.OnCompleted(null));
            await handle;
        });

        #endregion

        #region UnsafeOnCompleted Tests

        [UnityTest]
        public IEnumerator UnsafeOnCompleted_AfterCompletion_InvokesContinuationImmediately() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();

            await handle;

            var awaiter = handle.GetAwaiter();
            bool invoked = false;
            awaiter.UnsafeOnCompleted(() => invoked = true);

            Assert.IsTrue(invoked);
        });

        [UnityTest]
        public IEnumerator UnsafeOnCompleted_NullContinuation_HandlesGracefully() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var handle = action.ExecuteAsync();
            var awaiter = handle.GetAwaiter();

            Assert.DoesNotThrow(() => awaiter.UnsafeOnCompleted(null));
            await handle;
        });

        #endregion
    }

    /// <summary>
    /// Tests for <see cref="JActionExecution"/> struct.
    /// </summary>
    [TestFixture]
    public class JActionExecutionTests
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

        [UnityTest]
        public IEnumerator Action_ReturnsAssociatedJAction() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var result = await action.ExecuteAsync();

            Assert.AreSame(action, result.Action);
        });

        [UnityTest]
        public IEnumerator Cancelled_AfterCompletion_ReturnsFalse() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var result = await action.ExecuteAsync();

            Assert.IsFalse(result.Cancelled);
        });

        [UnityTest]
        public IEnumerator Cancelled_AfterCancel_ReturnsTrue() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Delay(10f).Do(() => { });
            var handle = action.ExecuteAsync();
            handle.Cancel();

            var result = await handle;

            Assert.IsTrue(result.Cancelled);
        });

        [UnityTest]
        public IEnumerator ImplicitOperator_ConvertsToJAction() => UniTask.ToCoroutine(async () =>
        {
            using var action = JAction.Create().Do(() => { });
            var result = await action.ExecuteAsync();

            JAction converted = result;

            Assert.AreSame(action, converted);
        });
    }
}
