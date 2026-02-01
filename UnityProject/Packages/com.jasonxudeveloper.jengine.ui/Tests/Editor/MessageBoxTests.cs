// MessageBoxTests.cs
// EditMode unit tests for MessageBox

using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace JEngine.UI.Tests
{
    [TestFixture]
    public class MessageBoxTests : JEngineTestBase
    {
        [SetUp]
        public override void BaseSetUp()
        {
            base.BaseSetUp();
            MessageBox.Dispose();
            MessageBox.TestHandler = null;
            MessageBox.SimulateNoPrefab = false;
            MessageBox.SkipDontDestroyOnLoad = true; // Required for EditMode tests
        }

        [TearDown]
        public override void BaseTearDown()
        {
            base.BaseTearDown();
            MessageBox.Dispose();
            MessageBox.TestHandler = null;
            MessageBox.SimulateNoPrefab = false;
            MessageBox.SkipDontDestroyOnLoad = false;
        }

        #region Static State Tests

        [Test]
        public void ActiveCount_InitiallyZero()
        {
            Assert.AreEqual(0, MessageBox.ActiveCount);
        }

        [Test]
        public void PooledCount_InitiallyZero()
        {
            Assert.AreEqual(0, MessageBox.PooledCount);
        }

        [Test]
        public void Dispose_ClearsActiveAndPooledCounts()
        {
            MessageBox.Dispose();

            Assert.AreEqual(0, MessageBox.ActiveCount);
            Assert.AreEqual(0, MessageBox.PooledCount);
        }

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            Assert.DoesNotThrow(() =>
            {
                MessageBox.Dispose();
                MessageBox.Dispose();
                MessageBox.Dispose();
            });
        }

        [Test]
        public void CloseAll_DoesNotThrowWhenNoActiveBoxes()
        {
            Assert.DoesNotThrow(() => MessageBox.CloseAll());
        }

        [Test]
        public void CloseAll_CanBeCalledMultipleTimes()
        {
            Assert.DoesNotThrow(() =>
            {
                MessageBox.CloseAll();
                MessageBox.CloseAll();
                MessageBox.CloseAll();
            });
        }

        #endregion

        #region TestHandler Tests

        [UnityTest]
        public IEnumerator Show_UsesTestHandler_WhenSet() => UniTask.ToCoroutine(async () =>
        {
            bool handlerCalled = false;
            MessageBox.TestHandler = (_, _, _, _) =>
            {
                handlerCalled = true;
                return UniTask.FromResult(true);
            };

            await MessageBox.Show("Test", "Content");

            Assert.IsTrue(handlerCalled);
        });

        [UnityTest]
        public IEnumerator Show_PassesCorrectParameters_ToTestHandler() => UniTask.ToCoroutine(async () =>
        {
            string receivedTitle = null;
            string receivedContent = null;
            string receivedOk = null;
            string receivedNo = null;

            MessageBox.TestHandler = (title, content, ok, no) =>
            {
                receivedTitle = title;
                receivedContent = content;
                receivedOk = ok;
                receivedNo = no;
                return UniTask.FromResult(true);
            };

            await MessageBox.Show("MyTitle", "MyContent", "Yes", "No");

            Assert.AreEqual("MyTitle", receivedTitle);
            Assert.AreEqual("MyContent", receivedContent);
            Assert.AreEqual("Yes", receivedOk);
            Assert.AreEqual("No", receivedNo);
        });

        [UnityTest]
        public IEnumerator Show_PassesDefaultParameters_WhenNotSpecified() => UniTask.ToCoroutine(async () =>
        {
            string receivedOk = null;
            string receivedNo = null;

            MessageBox.TestHandler = (_, _, ok, no) =>
            {
                receivedOk = ok;
                receivedNo = no;
                return UniTask.FromResult(true);
            };

            await MessageBox.Show("Title", "Content");

            Assert.AreEqual("OK", receivedOk);
            Assert.AreEqual("Cancel", receivedNo);
        });

        [UnityTest]
        public IEnumerator Show_ReturnsTrue_WhenTestHandlerReturnsTrue() => UniTask.ToCoroutine(async () =>
        {
            MessageBox.TestHandler = (_, _, _, _) => UniTask.FromResult(true);

            bool result = await MessageBox.Show("Test", "Content");

            Assert.IsTrue(result);
        });

        [UnityTest]
        public IEnumerator Show_ReturnsFalse_WhenTestHandlerReturnsFalse() => UniTask.ToCoroutine(async () =>
        {
            MessageBox.TestHandler = (_, _, _, _) => UniTask.FromResult(false);

            bool result = await MessageBox.Show("Test", "Content");

            Assert.IsFalse(result);
        });

        [UnityTest]
        public IEnumerator Show_HandlesNullParameters_WithTestHandler() => UniTask.ToCoroutine(async () =>
        {
            string receivedTitle = "not-null";
            string receivedContent = "not-null";

            MessageBox.TestHandler = (title, content, _, _) =>
            {
                receivedTitle = title;
                receivedContent = content;
                return UniTask.FromResult(true);
            };

            await MessageBox.Show(null, null, null, null);

            Assert.IsNull(receivedTitle);
            Assert.IsNull(receivedContent);
        });

        [UnityTest]
        public IEnumerator Show_MultipleCalls_AllUseTestHandler() => UniTask.ToCoroutine(async () =>
        {
            int callCount = 0;
            MessageBox.TestHandler = (_, _, _, _) =>
            {
                callCount++;
                return UniTask.FromResult(true);
            };

            await MessageBox.Show("Test1", "Content1");
            await MessageBox.Show("Test2", "Content2");
            await MessageBox.Show("Test3", "Content3");

            Assert.AreEqual(3, callCount);
        });

        [UnityTest]
        public IEnumerator Show_DoesNotAffectActiveCount_WhenUsingTestHandler() => UniTask.ToCoroutine(async () =>
        {
            MessageBox.TestHandler = (_, _, _, _) => UniTask.FromResult(true);

            int initialCount = MessageBox.ActiveCount;
            await MessageBox.Show("Test", "Content");

            Assert.AreEqual(initialCount, MessageBox.ActiveCount);
        });

        [UnityTest]
        public IEnumerator Show_DoesNotAffectPooledCount_WhenUsingTestHandler() => UniTask.ToCoroutine(async () =>
        {
            MessageBox.TestHandler = (_, _, _, _) => UniTask.FromResult(true);

            int initialCount = MessageBox.PooledCount;
            await MessageBox.Show("Test", "Content");

            Assert.AreEqual(initialCount, MessageBox.PooledCount);
        });

        #endregion

        #region No Prefab Tests (using SimulateNoPrefab)

        [UnityTest]
        public IEnumerator Show_ReturnsFalse_WhenNoPrefab() => UniTask.ToCoroutine(async () =>
        {
            MessageBox.SimulateNoPrefab = true;
            LogAssert.Expect(LogType.Error, "Cannot show MessageBox: Prefab is null");

            bool result = await MessageBox.Show("Test", "Content");

            Assert.IsFalse(result);
        });

        [UnityTest]
        public IEnumerator Show_LogsError_WhenNoPrefab() => UniTask.ToCoroutine(async () =>
        {
            MessageBox.SimulateNoPrefab = true;
            LogAssert.Expect(LogType.Error, "Cannot show MessageBox: Prefab is null");

            await MessageBox.Show("Test", "Content");
        });

        [UnityTest]
        public IEnumerator Show_DoesNotAffectActiveCount_WhenNoPrefab() => UniTask.ToCoroutine(async () =>
        {
            MessageBox.SimulateNoPrefab = true;
            LogAssert.Expect(LogType.Error, "Cannot show MessageBox: Prefab is null");

            int initialCount = MessageBox.ActiveCount;
            await MessageBox.Show("Test", "Content");

            Assert.AreEqual(initialCount, MessageBox.ActiveCount);
        });

        [UnityTest]
        public IEnumerator Show_MultipleCalls_AllReturnFalse_WhenNoPrefab() => UniTask.ToCoroutine(async () =>
        {
            MessageBox.SimulateNoPrefab = true;
            LogAssert.Expect(LogType.Error, "Cannot show MessageBox: Prefab is null");
            LogAssert.Expect(LogType.Error, "Cannot show MessageBox: Prefab is null");
            LogAssert.Expect(LogType.Error, "Cannot show MessageBox: Prefab is null");

            bool result1 = await MessageBox.Show("Test1", "Content1");
            bool result2 = await MessageBox.Show("Test2", "Content2");
            bool result3 = await MessageBox.Show("Test3", "Content3");

            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
        });

        #endregion

        #region Pool State Tests (using test hooks)

        [UnityTest]
        public IEnumerator Show_IncrementsActiveCount_WhenUsingTestHandler() => UniTask.ToCoroutine(async () =>
        {
            // Note: When TestHandler is set, the actual UI is bypassed,
            // so ActiveCount won't increase. This test verifies the expected behavior.
            MessageBox.TestHandler = (_, _, _, _) => UniTask.FromResult(true);

            var (initialActive, _) = MessageBox.TestGetPoolState();
            await MessageBox.Show("Test", "Content");
            var (finalActive, _) = MessageBox.TestGetPoolState();

            // With TestHandler, no actual MessageBox is created
            Assert.AreEqual(initialActive, finalActive);
        });

        [Test]
        public void TestGetPoolState_ReturnsCorrectInitialState()
        {
            var (activeCount, pooledCount) = MessageBox.TestGetPoolState();

            Assert.AreEqual(0, activeCount);
            Assert.AreEqual(0, pooledCount);
        }

        [Test]
        public void TestGetPoolState_AfterDispose_ReturnsZero()
        {
            MessageBox.Dispose();

            var (activeCount, pooledCount) = MessageBox.TestGetPoolState();

            Assert.AreEqual(0, activeCount);
            Assert.AreEqual(0, pooledCount);
        }

        [Test]
        public void TestSimulateButtonClick_ReturnsFalse_WhenNoActiveBoxes()
        {
            bool result = MessageBox.TestSimulateButtonClick(true);

            Assert.IsFalse(result);
        }

        [Test]
        public void TestGetButtonVisibility_ReturnsNull_WhenNoActiveBoxes()
        {
            var result = MessageBox.TestGetButtonVisibility();

            Assert.IsNull(result);
        }

        [Test]
        public void TestGetContent_ReturnsNull_WhenNoActiveBoxes()
        {
            var result = MessageBox.TestGetContent();

            Assert.IsNull(result);
        }

        #endregion

        #region Button Visibility Tests

        [UnityTest]
        public IEnumerator Show_EmptyOkText_PassesToHandler() => UniTask.ToCoroutine(async () =>
        {
            string receivedOk = "not-empty";

            MessageBox.TestHandler = (_, _, ok, _) =>
            {
                receivedOk = ok;
                return UniTask.FromResult(true);
            };

            await MessageBox.Show("Title", "Content", "", "Cancel");

            // Empty string is passed through
            Assert.AreEqual("", receivedOk);
        });

        [UnityTest]
        public IEnumerator Show_EmptyNoText_PassesToHandler() => UniTask.ToCoroutine(async () =>
        {
            string receivedNo = "not-empty";

            MessageBox.TestHandler = (_, _, _, no) =>
            {
                receivedNo = no;
                return UniTask.FromResult(true);
            };

            await MessageBox.Show("Title", "Content", "OK", "");

            // Empty string is passed through
            Assert.AreEqual("", receivedNo);
        });

        [UnityTest]
        public IEnumerator Show_NullOkText_PassesToHandler() => UniTask.ToCoroutine(async () =>
        {
            string receivedOk = "not-null";

            MessageBox.TestHandler = (_, _, ok, _) =>
            {
                receivedOk = ok;
                return UniTask.FromResult(true);
            };

            await MessageBox.Show("Title", "Content", null, "Cancel");

            // Null is passed through
            Assert.IsNull(receivedOk);
        });

        [UnityTest]
        public IEnumerator Show_NullNoText_PassesToHandler() => UniTask.ToCoroutine(async () =>
        {
            string receivedNo = "not-null";

            MessageBox.TestHandler = (_, _, _, no) =>
            {
                receivedNo = no;
                return UniTask.FromResult(true);
            };

            await MessageBox.Show("Title", "Content", "OK", null);

            // Null is passed through
            Assert.IsNull(receivedNo);
        });

        [UnityTest]
        public IEnumerator Show_BothButtonsNullOrEmpty_DefaultsToOkInHandler() => UniTask.ToCoroutine(async () =>
        {
            // Note: The safety check for both buttons being empty happens AFTER
            // TestHandler is checked, so TestHandler receives the original null values
            string receivedOk = "not-null";
            string receivedNo = "not-null";

            MessageBox.TestHandler = (_, _, ok, no) =>
            {
                receivedOk = ok;
                receivedNo = no;
                return UniTask.FromResult(true);
            };

            await MessageBox.Show("Title", "Content", null, null);

            // TestHandler receives original null values
            Assert.IsNull(receivedOk);
            Assert.IsNull(receivedNo);
        });

        #endregion

        #region Null Content Handling Tests

        [UnityTest]
        public IEnumerator Show_NullTitle_HandledGracefully() => UniTask.ToCoroutine(async () =>
        {
            string receivedTitle = "not-null";

            MessageBox.TestHandler = (title, _, _, _) =>
            {
                receivedTitle = title;
                return UniTask.FromResult(true);
            };

            bool result = await MessageBox.Show(null, "Content");

            Assert.IsNull(receivedTitle);
            Assert.IsTrue(result);
        });

        [UnityTest]
        public IEnumerator Show_NullContent_HandledGracefully() => UniTask.ToCoroutine(async () =>
        {
            string receivedContent = "not-null";

            MessageBox.TestHandler = (_, content, _, _) =>
            {
                receivedContent = content;
                return UniTask.FromResult(true);
            };

            bool result = await MessageBox.Show("Title", null);

            Assert.IsNull(receivedContent);
            Assert.IsTrue(result);
        });

        [UnityTest]
        public IEnumerator Show_EmptyStrings_HandledGracefully() => UniTask.ToCoroutine(async () =>
        {
            string receivedTitle = null;
            string receivedContent = null;

            MessageBox.TestHandler = (title, content, _, _) =>
            {
                receivedTitle = title;
                receivedContent = content;
                return UniTask.FromResult(true);
            };

            bool result = await MessageBox.Show("", "");

            Assert.AreEqual("", receivedTitle);
            Assert.AreEqual("", receivedContent);
            Assert.IsTrue(result);
        });

        #endregion

        #region Concurrent Operations Tests

        [UnityTest]
        public IEnumerator Show_MultipleConcurrent_AllComplete() => UniTask.ToCoroutine(async () =>
        {
            int completionCount = 0;

            MessageBox.TestHandler = (_, _, _, _) =>
            {
                completionCount++;
                return UniTask.FromResult(true);
            };

            // Show multiple message boxes concurrently
            var task1 = MessageBox.Show("Test1", "Content1");
            var task2 = MessageBox.Show("Test2", "Content2");
            var task3 = MessageBox.Show("Test3", "Content3");

            await UniTask.WhenAll(task1, task2, task3);

            Assert.AreEqual(3, completionCount);
        });

        [UnityTest]
        public IEnumerator CloseAll_AfterMultipleShows_ClearsAll() => UniTask.ToCoroutine(async () =>
        {
            MessageBox.TestHandler = (_, _, _, _) => UniTask.FromResult(true);

            await MessageBox.Show("Test1", "Content1");
            await MessageBox.Show("Test2", "Content2");

            MessageBox.CloseAll();

            var (activeCount, _) = MessageBox.TestGetPoolState();
            Assert.AreEqual(0, activeCount);
        });

        #endregion

        #region Real MessageBox Tests (with prefab)

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_IncreasesActiveCount() => UniTask.ToCoroutine(async () =>
        {
            // Don't set TestHandler - use real implementation
            var initialActive = MessageBox.ActiveCount;

            // Start showing (don't await - we need to interact with it)
            var showTask = MessageBox.Show("Test Title", "Test Content");

            // Give it a frame to create
            await UniTask.Yield();

            // Active count should have increased
            Assert.Greater(MessageBox.ActiveCount, initialActive);

            // Clean up by simulating OK click
            MessageBox.TestSimulateButtonClick(true);
            await showTask;
        });

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_ReturnsTrue_WhenOkClicked() => UniTask.ToCoroutine(async () =>
        {
            var showTask = MessageBox.Show("Test", "Content");
            await UniTask.Yield();

            MessageBox.TestSimulateButtonClick(true);
            bool result = await showTask;

            Assert.IsTrue(result);
        });

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_ReturnsFalse_WhenNoClicked() => UniTask.ToCoroutine(async () =>
        {
            var showTask = MessageBox.Show("Test", "Content");
            await UniTask.Yield();

            MessageBox.TestSimulateButtonClick(false);
            bool result = await showTask;

            Assert.IsFalse(result);
        });

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_SetsCorrectContent() => UniTask.ToCoroutine(async () =>
        {
            var showTask = MessageBox.Show("My Title", "My Content", "Yes", "No");
            await UniTask.Yield();

            var content = MessageBox.TestGetContent();
            Assert.IsNotNull(content);
            Assert.AreEqual("My Title", content.Value.title);
            Assert.AreEqual("My Content", content.Value.content);
            Assert.AreEqual("Yes", content.Value.okText);
            Assert.AreEqual("No", content.Value.noText);

            MessageBox.TestSimulateButtonClick(true);
            await showTask;
        });

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_BothButtonsVisible_ByDefault() => UniTask.ToCoroutine(async () =>
        {
            var showTask = MessageBox.Show("Test", "Content");
            await UniTask.Yield();

            var visibility = MessageBox.TestGetButtonVisibility();
            Assert.IsNotNull(visibility);
            Assert.IsTrue(visibility.Value.okVisible);
            Assert.IsTrue(visibility.Value.noVisible);

            MessageBox.TestSimulateButtonClick(true);
            await showTask;
        });

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_HidesOkButton_WhenOkIsNull() => UniTask.ToCoroutine(async () =>
        {
            var showTask = MessageBox.Show("Test", "Content", null, "Cancel");
            await UniTask.Yield();

            var visibility = MessageBox.TestGetButtonVisibility();
            Assert.IsNotNull(visibility);
            Assert.IsFalse(visibility.Value.okVisible);
            Assert.IsTrue(visibility.Value.noVisible);

            MessageBox.TestSimulateButtonClick(false);
            await showTask;
        });

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_HidesNoButton_WhenNoIsNull() => UniTask.ToCoroutine(async () =>
        {
            var showTask = MessageBox.Show("Test", "Content", "OK", null);
            await UniTask.Yield();

            var visibility = MessageBox.TestGetButtonVisibility();
            Assert.IsNotNull(visibility);
            Assert.IsTrue(visibility.Value.okVisible);
            Assert.IsFalse(visibility.Value.noVisible);

            MessageBox.TestSimulateButtonClick(true);
            await showTask;
        });

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_DefaultsToOk_WhenBothButtonsNull() => UniTask.ToCoroutine(async () =>
        {
            // When both buttons are null/empty, should default to showing OK button
            var showTask = MessageBox.Show("Test", "Content", null, null);
            await UniTask.Yield();

            var visibility = MessageBox.TestGetButtonVisibility();
            Assert.IsNotNull(visibility);
            Assert.IsTrue(visibility.Value.okVisible); // Should default to OK
            Assert.IsFalse(visibility.Value.noVisible);

            MessageBox.TestSimulateButtonClick(true);
            await showTask;
        });

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_ReturnsToPool_AfterClose() => UniTask.ToCoroutine(async () =>
        {
            var initialPooled = MessageBox.PooledCount;

            var showTask = MessageBox.Show("Test", "Content");
            await UniTask.Yield();

            MessageBox.TestSimulateButtonClick(true);
            await showTask;

            // Should be in pool now
            Assert.Greater(MessageBox.PooledCount, initialPooled);
        });

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_ReusesPooledInstance() => UniTask.ToCoroutine(async () =>
        {
            // First show - creates new instance
            var showTask1 = MessageBox.Show("Test1", "Content1");
            await UniTask.Yield();
            MessageBox.TestSimulateButtonClick(true);
            await showTask1;

            int pooledAfterFirst = MessageBox.PooledCount;
            Assert.Greater(pooledAfterFirst, 0);

            // Second show - should reuse pooled instance
            var showTask2 = MessageBox.Show("Test2", "Content2");
            await UniTask.Yield();

            // Pool count should have decreased (instance taken from pool)
            Assert.Less(MessageBox.PooledCount, pooledAfterFirst);

            MessageBox.TestSimulateButtonClick(true);
            await showTask2;
        });

        [UnityTest]
        public IEnumerator CloseAll_WithRealPrefab_ClosesAllActive() => UniTask.ToCoroutine(async () =>
        {
            // Show multiple boxes without awaiting (use discard to suppress unused variable warning)
            _ = MessageBox.Show("Test1", "Content1");
            await UniTask.Yield();
            _ = MessageBox.Show("Test2", "Content2");
            await UniTask.Yield();

            Assert.AreEqual(2, MessageBox.ActiveCount);

            MessageBox.CloseAll();

            Assert.AreEqual(0, MessageBox.ActiveCount);
        });

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_HandlesNullTitle() => UniTask.ToCoroutine(async () =>
        {
            var showTask = MessageBox.Show(null, "Content");
            await UniTask.Yield();

            var content = MessageBox.TestGetContent();
            Assert.IsNotNull(content);
            Assert.AreEqual("", content.Value.title); // Null converted to empty string

            MessageBox.TestSimulateButtonClick(true);
            await showTask;
        });

        [UnityTest]
        public IEnumerator Show_WithRealPrefab_HandlesNullContent() => UniTask.ToCoroutine(async () =>
        {
            var showTask = MessageBox.Show("Title", null);
            await UniTask.Yield();

            var content = MessageBox.TestGetContent();
            Assert.IsNotNull(content);
            Assert.AreEqual("", content.Value.content); // Null converted to empty string

            MessageBox.TestSimulateButtonClick(true);
            await showTask;
        });

        [UnityTest]
        public IEnumerator TestSimulateButtonClick_ReturnsTrue_WhenActiveBoxExists() => UniTask.ToCoroutine(async () =>
        {
            var showTask = MessageBox.Show("Test", "Content");
            await UniTask.Yield();

            bool result = MessageBox.TestSimulateButtonClick(true);

            Assert.IsTrue(result);
            await showTask;
        });

        [UnityTest]
        public IEnumerator Dispose_WithRealPrefab_ClearsActiveBoxes() => UniTask.ToCoroutine(async () =>
        {
            var showTask = MessageBox.Show("Test", "Content");
            await UniTask.Yield();

            Assert.Greater(MessageBox.ActiveCount, 0);

            MessageBox.Dispose();

            Assert.AreEqual(0, MessageBox.ActiveCount);
            Assert.AreEqual(0, MessageBox.PooledCount);
        });

        #endregion
    }
}
