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
        /// Returns true when tests are currently running
        /// </summary>
        public static bool IsRunningTests { get; private set; }

        private static readonly TestRunnerCallbacks _instance = new TestRunnerCallbacks();
        private static TestRunnerApi _api;

        static TestRunnerCallbacks()
        {
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
                ScriptableObject.DestroyImmediate(_api);
                _api = null;
            }
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
