using System;
using System.Threading.Tasks;
using UnityEditor;

namespace JEngine.Editor
{
    internal static class XAsset
    {
        public static bool hasAdded;
        
        private static bool _delaying;
        private static bool _verifying;
        private static float _frequency = 3600;

        public static async void Update()
        {
            hasAdded = true;
            
            if (!Setting.XAssetLoggedIn || _delaying || _verifying || XAssetHelper.loggingXAsset) return;

            //验证
            _verifying = true;
            var result = await XAssetHelper.LoginXAsset();
            _verifying = false;
            
            if (!result)
            {
                XAssetHelper.LogOutXAsset();
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