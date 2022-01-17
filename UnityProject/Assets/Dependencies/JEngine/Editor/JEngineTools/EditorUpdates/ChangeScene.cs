using System.Threading.Tasks;
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
                while (SceneManager.GetActiveScene().name != name)
                {
                    if (!Application.isPlaying) return;
                    await Task.Delay(10);
                }
                DynamicGI.UpdateEnvironment();
            }
            var key = Object.FindObjectOfType<InitJEngine>().key;
            var k = PlayerPrefs.GetString($"JEngine.Editor.Setting.{Application.productName}.EncryptPassword", "");
            if (string.IsNullOrEmpty(k))
            {
                PlayerPrefs.SetString($"JEngine.Editor.Setting.{Application.productName}.EncryptPassword", key);
            }
            else
            {
                if (key != k)
                {
                    var res = EditorUtility.DisplayDialog("DLL解密秘钥异常",
                                    $"面板里配置的加密密码是：{k}\n" +
                                    $"游戏场景里配置的解密密码是：{key}\n" +
                                    $"点击确定使用面板配置的密码，点击忽略则继续使用当前密码'{key}'"
                                    , "确定","忽略");
                    if (res)
                    {
                        Object.FindObjectOfType<InitJEngine>().key = k;
                    }
                }
            }
        }
    }
}