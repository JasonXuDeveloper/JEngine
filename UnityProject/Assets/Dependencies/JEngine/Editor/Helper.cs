using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using JEngine.Core;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace JEngine.Editor
{
    public class Helper
    {
        public static async void Update()
        {
            string localURL = "https://xgamedev.uoyou.com/Core.unitypackage";
            string hotURL = "https://xgamedev.uoyou.com/JEngine.zip";
            string localPath = Application.dataPath + "/Core.unitypackage";
            string hotPath = Application.dataPath + "/JEngine.zip";
            DirectoryInfo localDirectory = new DirectoryInfo(Setting.LocalPath);
            DirectoryInfo tempDirectory = new DirectoryInfo(Setting.HotPath+"/../Temp");
            DirectoryInfo hotDirectory = new DirectoryInfo(Setting.HotPath);

            //对比路径
            if (!localDirectory.Exists||!hotDirectory.Exists)
            {
                EditorUtility.DisplayDialog("Error",
                    !localDirectory.Exists?
                        $"本地JEngine框架路径不存在：{localDirectory.FullName}"
                        :
                        $"热更JEngine框架路径不存在：{hotDirectory.FullName}"
                    ,"OK");
                return;
            }
            
            //下载
            if (!await Download(localURL, localPath) || !await Download(hotURL, hotPath))
            {
                return;
            }
            //打开导入
            AssetDatabase.Refresh();
            EditorUtility.OpenWithDefaultApp(localPath);
            
            //删热更的
            hotDirectory.Delete(true);
            Unzip(hotPath,tempDirectory.FullName);
            File.Delete(hotPath);
            //重新导入
            Directory.Move(tempDirectory.FullName+"/JEngine",hotDirectory.FullName);
            tempDirectory.Delete(true);

            EditorUtility.DisplayDialog("Success",
                $"下载成功\n" +
                $"请点击导入后删除Core.unitypackage\n" +
                $"请在IDE打开热更工程并重新导入JEngine源码"
                ,"OK");  
            
            //删本地的
            localDirectory.Delete(true);
        }

        public static void Unzip(string file, string path)
        {
            ZipFile.ExtractToDirectory(file, path);
        }

        public static async Task<bool> Download(string url,string path)
        {
            //下载文件  
            WebClient client = new WebClient();  
            try  
            {  
                Pop("Downloading","下载中");
                await client.DownloadFileTaskAsync(url, path);  
            }  
            catch(Exception ex)  
            {  
                EditorUtility.DisplayDialog("Error",$"下载文件失败：{ex.Message}","OK");  
                return false;  
            }
            client.Dispose();
            isPopping = false;
            return true;  
        }

        private static bool isPopping = false;

        private static async void Pop(string title,string info)
        {
            isPopping = true;
            while (isPopping)
            {
                EditorUtility.DisplayProgressBar(title,info,Random.Range(0f,1f));
                await Task.Delay(1);
            }
            EditorUtility.ClearProgressBar();
        }
    }
}