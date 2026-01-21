using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace JEngine.Core.Editor.Misc
{
    /// <summary>
    /// Detects when Unity Test Runner is executing tests.
    /// Uses ICallbacks API to track test run state.
    /// </summary>
    [InitializeOnLoad]
    internal class TestRunDetector : ICallbacks
    {
        private const string SessionStateKey = "JEngine.TestRunDetector.IsTestRunning";

        /// <summary>
        /// Returns true if tests are currently running.
        /// </summary>
        public static bool IsTestRunning => SessionState.GetBool(SessionStateKey, false);

        static TestRunDetector()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new TestRunDetector());

            // Clear stale flag when entering edit mode (tests finished or cancelled)
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                    SessionState.SetBool(SessionStateKey, false);
            };
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            SessionState.SetBool(SessionStateKey, true);
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            SessionState.SetBool(SessionStateKey, false);
        }

        public void TestStarted(ITestAdaptor test) { }

        public void TestFinished(ITestResultAdaptor result) { }
    }
}
