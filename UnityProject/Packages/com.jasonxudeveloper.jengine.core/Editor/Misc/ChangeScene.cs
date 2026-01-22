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
            // Unity Test Framework creates scenes with names like "InitTestScene<timestamp>"
            // Skip scene change if we're in a test scene
            var currentScene = SceneManager.GetActiveScene();
            if (currentScene.name.StartsWith("InitTestScene"))
            {
                Debug.Log($"[JEngine] Skipping scene change - Play Mode test detected (scene: {currentScene.name})");
                return;
            }

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