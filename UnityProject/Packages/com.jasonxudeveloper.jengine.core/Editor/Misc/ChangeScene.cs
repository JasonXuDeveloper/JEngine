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
        // Don't auto-change scenes during Play Mode tests
#if !UNITY_INCLUDE_TESTS
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void DoChange()
        {
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