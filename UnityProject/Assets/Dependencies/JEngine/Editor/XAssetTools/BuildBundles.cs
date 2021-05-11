using System.Diagnostics;
using JEngine.Core;
using libx;

namespace JEngine.Editor
{
    public class BuildBundles
    {
        [UnityEditor.MenuItem("JEngine/XAsset/Bundles/Build Bundles %#&B")]
        private static void BuildAssetBundles()
        {
            DLLMgr.Delete("Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
            // DLLMgr.Delete(Directory.GetParent(Application.dataPath)+"/Assets/XAsset/ScriptableObjects/Rules.asset");
            // DLLMgr.Delete(Directory.GetParent(Application.dataPath)+"/Assets/XAsset/ScriptableObjects/Manifest.asset");


            CryptoWindow.ShowWindow();
            CryptoWindow.Build= s =>
            {
                var watch = new Stopwatch();
                watch.Start();
                var bytes = DLLMgr.FileToByte(DLLMgr.DllPath);
                var result = DLLMgr.ByteToFile(CryptoHelper.AesEncrypt(bytes,s), "Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
                watch.Stop();
                Log.Print("Convert Dlls in: " + watch.ElapsedMilliseconds + " ms.");
                if (!result)
                {
                    Log.PrintError("DLL转Byte[]出错！");
                }
            
                watch = new Stopwatch();
                watch.Start();
                BuildScript.ApplyBuildRules();
                watch.Stop();
                Log.Print("ApplyBuildRules in: " + watch.ElapsedMilliseconds + " ms.");
            
                watch = new Stopwatch();
                watch.Start();
                BuildScript.BuildAssetBundles();
                watch.Stop();
                Log.Print("BuildAssetBundles in: " + watch.ElapsedMilliseconds + " ms."); 
            };
        }
    }
}