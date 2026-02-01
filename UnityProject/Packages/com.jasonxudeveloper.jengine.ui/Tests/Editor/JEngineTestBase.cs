// JEngineTestBase.cs
// Base class for all JEngine UI tests

using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;

namespace JEngine.UI.Tests
{
    /// <summary>
    /// Base class for all JEngine UI tests.
    /// Provides common setup that suppresses Unity internal errors
    /// (like "Bake Ambient Probe") that occur during build operations.
    /// </summary>
    public abstract class JEngineTestBase
    {
        [SetUp]
        public virtual void BaseSetUp()
        {
            // Suppress Unity internal errors that occur during build operations.
            // These errors come from Unity's Enlighten lighting system and BuildPipeline,
            // and cannot be prevented at the source. They don't indicate test failures.
            LogAssert.ignoreFailingMessages = true;
        }

        [TearDown]
        public virtual void BaseTearDown()
        {
            // Keep ignoreFailingMessages true to handle async errors that
            // might appear after the test completes
        }
    }
}
