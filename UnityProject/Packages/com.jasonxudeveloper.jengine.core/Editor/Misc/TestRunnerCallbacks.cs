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
        private const string SessionStateKey = "JEngine.TestRunnerCallbacks.IsRunningTests";

        private static volatile bool _isRunningTests;

        /// <summary>
        /// Returns true when tests are currently running
        /// </summary>
        public static bool IsRunningTests
        {
            get => _isRunningTests;
            private set
            {
                _isRunningTests = value;
                // Persist to SessionState to survive domain reloads during test execution
                SessionState.SetBool(SessionStateKey, value);
            }
        }

        private static readonly TestRunnerCallbacks _instance = new TestRunnerCallbacks();
        private static TestRunnerApi _api;

        static TestRunnerCallbacks()
        {
            // Restore state after domain reload (e.g., recompilation during test run)
            _isRunningTests = SessionState.GetBool(SessionStateKey, false);

            _api = ScriptableObject.CreateInstance<TestRunnerApi>();
            _api.RegisterCallbacks(_instance);
            EditorApplication.quitting += OnEditorQuitting;
        }

        private static void OnEditorQuitting()
        {
            EditorApplication.quitting -= OnEditorQuitting;

            if (_api != null)
            {
                _api.UnregisterCallbacks(_instance);
                Object.Destroy(_api);
                _api = null;
            }

            // Clear session state on quit
            SessionState.EraseBool(SessionStateKey);
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            IsRunningTests = true;
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            IsRunningTests = false;
        }

        // Required by ICallbacks interface but not needed - we only track overall run state
        public void TestStarted(ITestAdaptor test) { }

        // Required by ICallbacks interface but not needed - we only track overall run state
        public void TestFinished(ITestResultAdaptor result) { }
    }
}
