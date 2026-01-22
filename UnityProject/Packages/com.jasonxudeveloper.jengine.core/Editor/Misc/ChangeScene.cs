using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JEngine.Core.Editor.Misc
{
    /// <summary>
    /// Scene transition utilities
    /// </summary>
    internal static class ChangeScene
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void DoChange()
        {
#if UNITY_INCLUDE_TESTS
            // Skip scene change when running Play Mode tests
            if (TestRunnerCallbacks.IsRunningTests)
            {
                return;
            }
#endif

            var jump = Settings.Instance.jumpStartUp;
            if (!jump) return;
            var scene = SceneManager.GetActiveScene();
            var path = Settings.Instance.startUpScenePath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return;
            }

            if (scene.path == path) return;
            string name = Path.GetFileNameWithoutExtension(path);
            SceneManager.LoadScene(name);
            DynamicGI.UpdateEnvironment();
        }
    }
}