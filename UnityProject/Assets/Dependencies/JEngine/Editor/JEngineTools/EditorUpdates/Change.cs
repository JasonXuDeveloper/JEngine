using UnityEngine;
using UnityEngine.SceneManagement;

namespace JEngine.Editor
{
    /// <summary>
    /// 跳转场景
    /// </summary>
    internal static class Change
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void DoChange()
        {
            if (!Setting.JumpStartUp) return;
            var scene = SceneManager.GetActiveScene();
            if (scene.path != Setting.StartUpScenePath)
            {
                string name = Setting.StartUpScenePath
                    .Substring(Setting.StartUpScenePath.LastIndexOf('/') + 1)
                    .Replace(".unity", "");

                SceneManager.LoadScene(name);
            }
        }
    }
}