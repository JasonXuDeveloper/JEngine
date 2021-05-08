using UnityEngine;
using UnityEngine.SceneManagement;

namespace JEngine.Editor
{
    /// <summary>
    /// 跳转场景
    /// </summary>
    internal static class ChangeScene
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void DoChange()
        {
            var prefix = $"JEngine.Editor.Setting.{Application.productName}";
            var jump = PlayerPrefs.GetString($"{prefix}.JumpStartUpScene", "1") == "1";
            if (!jump) return;
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