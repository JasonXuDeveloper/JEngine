using System.Linq;
using System.Threading.Tasks;
using JEngine.Core;
using UnityEditor;
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
        private static async void DoChange()
        {
            var prefix = $"JEngine.Editor.Setting.{Application.productName}.{SetData.GetPrefix()}.";
            var jump = PlayerPrefs.GetString($"{prefix}.JumpStartUpScene", "1") == "1";
            if (!jump) return;
            var scene = SceneManager.GetActiveScene();
            var path = PlayerPrefs.GetString($"{prefix}.StartUpScenePath", "Assets/Init.unity");
            if (scene.path != path)
            {
                string name = path
                    .Substring(path.LastIndexOf('/') + 1)
                    .Replace(".unity", "");

                SceneManager.LoadScene(name);
                while (SceneManager.GetActiveScene().name != name)
                {
                    if (!Application.isPlaying) return;
                    await Task.Delay(10);
                }

                DynamicGI.UpdateEnvironment();
            }

            var comp = Object.FindFirstObjectByType<InitJEngine>();
            if (comp == null)
            {
                Debug.LogWarning("没有找到InitJEngine脚本，无法检验秘钥是否正确");
            }
            var key = comp.key;
            var k = PlayerPrefs.GetString($"{prefix}.EncryptPassword", "");
            if (string.IsNullOrEmpty(k))
            {
                PlayerPrefs.SetString($"{prefix}.EncryptPassword", key);
            }
            else
            {
                if (key != k)
                {
                    var res = EditorUtility.DisplayDialog(
                        string.Format(Setting.GetString(SettingString.MismatchDLLKeyTitle), k, key),
                        string.Format(Setting.GetString(SettingString.MismatchDLLKeyContext), k, key),
                        Setting.GetString(SettingString.Ok), Setting.GetString(SettingString.Ignore));
                    if (res)
                    {
                        comp.key = k;
                    }
                }
            }
        }
    }
}