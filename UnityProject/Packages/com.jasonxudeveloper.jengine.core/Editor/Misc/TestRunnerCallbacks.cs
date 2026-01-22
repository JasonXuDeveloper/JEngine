using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace JEngine.Core.Editor.Misc
{
    /// <summary>
    /// Tracks Unity Test Runner state to allow other systems to detect when tests are running
    /// </summary>
    [InitializeOnLoad]
    internal class TestRunnerCallbacks : ICallbacks
    {
        /// <summary>
        /// Returns true when Play Mode tests are currently running
        /// </summary>
        public static bool IsRunningTests { get; private set; }

        static TestRunnerCallbacks()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new TestRunnerCallbacks());
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            IsRunningTests = true;
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            IsRunningTests = false;
        }

        public void TestStarted(ITestAdaptor test) { }

        public void TestFinished(ITestResultAdaptor result) { }
    }
}
