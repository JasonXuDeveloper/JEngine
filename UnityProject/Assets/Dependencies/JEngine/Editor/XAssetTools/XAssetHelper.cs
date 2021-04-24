using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ILRuntime.Runtime;
using UnityEditor;
using UnityEngine;

namespace JEngine.Editor
{
    public static class XAssetHelper
    {
        public static void SignUpXAsset()
        {
            Application.OpenURL("https://game4d.cn/User/register");
        }

        public static void RechargeXAsset()
        {
            Application.OpenURL("https://game4d.cn");
        }

        public static void LogOutXAsset()
        {
            Setting.XAssetLoggedIn = false;
        }

        public static bool loggingXAsset;

        public static async Task<bool> LoginXAsset(bool showDialog = false)
        {
            if (string.IsNullOrEmpty(Setting.XAssetAccount) || string.IsNullOrEmpty(Setting.XAssetPassword))
            {
                EditorUtility.DisplayDialog("Login Result", "请填入账号密码！\n" +
                                                            "Please fill in account and password!", "OK");
                return false;
            }

            var url = "https://api.uoyou.com/v1/xasset.php";
            Dictionary<string, string> postParams = new Dictionary<string, string>
            {
                {"account", Setting.XAssetAccount},
                {"password", Setting.XAssetPassword}
            };

            loggingXAsset = true;
            Setting.Refresh();

            string result = await Tools.Post(url, postParams);
            string msg = result;
            
            loggingXAsset = false;
            Setting.Refresh();

            JSONObject json = new JSONObject(result);

            if (json.type != JSONObject.Type.NULL)
            {
                int code = json["code"].ToString().ToInt32();
                msg = json["msg"].str;
                
                if (code == 200)
                {
                    int remain = json["remain"].ToString().ToInt32();
                    Setting.XAssetLoggedIn = true;
                    Setting.XAssetRemainTime = remain;
                    Setting.Refresh();
                }
            }
            
            if (showDialog)
                EditorUtility.DisplayDialog("Login Result", msg, "OK");

            return json.type != JSONObject.Type.NULL;
        }

        public static async void GetXAssetPro()
        {
            string url = "https://api.uoyou.com/v1/download.php";
            var acc = WebUtility.UrlEncode(Setting.XAssetAccount);
            var pwd = WebUtility.UrlEncode(Setting.XAssetPassword);
            url += $"?file=xasset&account={acc}&password={pwd}";
            string localPath = Application.dataPath + "/xasset-pro.unitypackage";

            installing = true;
            //下载
            if (!await Tools.Download(url, localPath))
            {
                return;
            }
            installing = false;

            //打开导入
            EditorUtility.OpenWithDefaultApp(localPath);
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success",
                "下载成功\n" +
                "请根据XAsset Pro升级步骤进行操作"
                , "OK");
        }


        public static bool installing;

        public static async void Update()
        {
            string localURL = "https://xgamedev.uoyou.com/custom/Core.unitypackage";
            string hotURL = "https://xgamedev.uoyou.com/custom/JEngine.zip";
            string localPath = Application.dataPath + "/Core.unitypackage";
            string hotPath = Application.dataPath + "/JEngine.zip";
            DirectoryInfo localDirectory = new DirectoryInfo(Setting.LocalPath);
            DirectoryInfo tempDirectory = new DirectoryInfo(Setting.HotPath + "/../Temp");
            DirectoryInfo hotDirectory = new DirectoryInfo(Setting.HotPath);

            //对比路径
            if (!localDirectory.Exists || !hotDirectory.Exists)
            {
                EditorUtility.DisplayDialog("Error",
                    !localDirectory.Exists
                        ? $"本地JEngine框架路径不存在：{localDirectory.FullName}"
                        : $"热更JEngine框架路径不存在：{hotDirectory.FullName}"
                    , "OK");
                return;
            }

            installing = true;
            //下载
            if (!await Tools.Download(localURL, localPath) || !await Tools.Download(hotURL, hotPath))
            {
                return;
            }
            installing = false;

            //打开导入
            EditorUtility.OpenWithDefaultApp(localPath);

            //删热更的
            hotDirectory.Delete(true);
            Tools.Unzip(hotPath, tempDirectory.FullName);
            File.Delete(hotPath);
            //重新导入
            Directory.Move(tempDirectory.FullName + "/JEngine", hotDirectory.FullName);
            tempDirectory.Delete(true);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success",
                "下载成功\n" +
                "请导入并删除Core.unitypackage\n" +
                "请在IDE打开热更工程并重新导入JEngine源码"
                , "OK");
        }
    }
}