using System;
using System.Threading.Tasks;
using UnityEditor;

namespace JEngine.Editor
{
    internal static class XAsset
    {
        private static bool _delaying;
        private static bool _verifying;
        private static float _frequency = 3600;

        public static async void Update()
        {
            if (!Setting.XAssetLoggedIn || _delaying || _verifying || Helpers.loggingXAsset) return;

            bool result = false;
            //验证
            _verifying = true;
            result = await Helpers.LoginXAsset();
            _verifying = false;
            
            if (!result)
            {
                Helpers.LogOutXAsset();
                EditorUtility.DisplayDialog("XAsset", "登入状态异常，请重新登入\n" +
                                                      "An error just occured, please log in again", "OK");
                Setting.Refresh();
            }
            _delaying = true;
            await Task.Delay(TimeSpan.FromSeconds(_frequency));
            _delaying = false;
        }
    }
}