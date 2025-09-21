using System.IO;
using UnityEditor;
using UnityEngine;

namespace JEngine.Editor
{
    public class UpdateJEngine
    {
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