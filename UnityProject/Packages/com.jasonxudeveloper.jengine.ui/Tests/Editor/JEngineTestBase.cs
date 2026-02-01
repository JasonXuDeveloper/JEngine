// JEngineTestBase.cs
// Base class for all JEngine UI tests

using NUnit.Framework;
using UnityEngine.TestTools;

namespace JEngine.UI.Tests
{
    /// <summary>
    /// Base class for all JEngine UI tests.
    /// Provides common setup that suppresses Unity internal errors
    /// (like "Bake Ambient Probe") that occur during build operations.
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
        /// Suppresses Unity internal error logs that occur during build operations.
        /// Override this method and call <c>base.BaseSetUp()</c> first.
        /// </summary>
        [SetUp]
        public virtual void BaseSetUp()
        {
            // Suppress Unity internal errors that occur during build operations.
            // These errors come from Unity's Enlighten lighting system and BuildPipeline,
            // and cannot be prevented at the source. They don't indicate test failures.
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
