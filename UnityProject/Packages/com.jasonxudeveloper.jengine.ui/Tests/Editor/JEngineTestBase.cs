// JEngineTestBase.cs
// Base class for all JEngine UI tests

using NUnit.Framework;
using UnityEngine.TestTools;

namespace JEngine.UI.Tests
{
    /// <summary>
    /// Base class for all JEngine UI tests.
    /// Provides common setup that suppresses Unity internal errors
    /// that may occur during test operations.
    /// </summary>
    /// <remarks>
    /// Test classes should inherit from this base class and override
    /// <see cref="BaseSetUp"/> and <see cref="BaseTearDown"/> methods.
    /// Always call <c>base.BaseSetUp()</c> at the start of the override
    /// and <c>base.BaseTearDown()</c> at the end of the override.
    /// </remarks>
    public abstract class JEngineTestBase
    {
        private bool _previousIgnoreFailingMessages;

        /// <summary>
        /// Setup method called before each test.
        /// Suppresses Unity internal error logs that may occur during test operations.
        /// Override this method and call <c>base.BaseSetUp()</c> first.
        /// </summary>
        [SetUp]
        public virtual void BaseSetUp()
        {
            // Suppress Unity internal errors that occur during test operations.
            // This prevents tests from failing due to unrelated Unity internal issues.
            _previousIgnoreFailingMessages = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
        }

        /// <summary>
        /// Teardown method called after each test.
        /// Restores the previous <see cref="LogAssert.ignoreFailingMessages"/> state.
        /// Override this method and call <c>base.BaseTearDown()</c> at the end.
        /// </summary>
        [TearDown]
        public virtual void BaseTearDown()
        {
            // Restore previous ignoreFailingMessages state to avoid leaking
            // global side effects into other tests.
            LogAssert.ignoreFailingMessages = _previousIgnoreFailingMessages;
        }
    }
}
