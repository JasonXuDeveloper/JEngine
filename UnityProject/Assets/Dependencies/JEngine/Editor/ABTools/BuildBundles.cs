using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BM;
using JEngine.Core;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace JEngine.Editor
{
    public class BuildBundles
    {
        [MenuItem("Tools/BuildAsset/构建AssetBundle %#&B")]
        private static void BuildAssetBundles()
        {
            FileMgr.Delete(DllMgr.GetDllInRuntimePath(ConstMgr.MainHotDLLName));
            FileMgr.Delete(DllMgr.GetPdbInRuntimePath(ConstMgr.MainHotDLLName));

            Action<string> buildAct = async s =>
            {
                var watch = new Stopwatch();
                watch.Start();
                string dllPath = DllMgr.GetDllInEditorPath(ConstMgr.MainHotDLLName);
                var bytes = FileMgr.FileToByte(dllPath);
                var result = FileMgr.ByteToFile(CryptoMgr.AesEncrypt(bytes, s),
                    DllMgr.GetDllInRuntimePath(ConstMgr.MainHotDLLName));
                watch.Stop();
                Log.Print("Convert Dlls in: " + watch.ElapsedMilliseconds + " ms.");
                if (!result)
                {
                    Log.PrintError("DLL转Byte[]出错！");
                    return;
                }

                watch.Reset();
                watch.Start();
                string pdbPath = DllMgr.GetPdbInEditorPath(ConstMgr.MainHotDLLName);
                if (File.Exists(pdbPath))
                {
                    bytes = FileMgr.FileToByte(pdbPath);
                    result = FileMgr.ByteToFile(bytes,
                        DllMgr.GetPdbInRuntimePath(ConstMgr.MainHotDLLName));
                    watch.Stop();
                    Log.Print("Convert PDBs in: " + watch.ElapsedMilliseconds + " ms.");
                    if (!result)
                    {
                        Log.PrintError("PDB转Byte[]出错！");
                        return;
                    }   
                }

                Setting.EncryptPassword = s;

                await TimeMgr.Delay(3);
                AssetDatabase.Refresh();

                watch = new Stopwatch();
                watch.Start();
                BuildAssets.BuildAllBundle();
                watch.Stop();
                Log.Print("BuildAssetBundles in: " + watch.ElapsedMilliseconds + " ms.");
            };

            if (string.IsNullOrEmpty(Setting.EncryptPassword))
            {
                CryptoWindow.ShowWindow();
                CryptoWindow.Build = buildAct;
            }
            else
            {
                buildAct.Invoke(Setting.EncryptPassword);
            }
        }

        private const string KViewCachePath = "Tools/BuildAsset/View/Caches";
        private const string KViewDataPath = "Tools/BuildAsset/View/Built Bundles";
        
        [MenuItem(KViewDataPath)]
        private static void ViewDataPath()
        {
            if(Directory.Exists(Directory.GetParent(Application.dataPath).FullName + "/DLC"))
            {
                EditorUtility.OpenWithDefaultApp(Directory.GetParent(Application.dataPath).FullName + "/DLC");
            }
            else 
            {
                Log.PrintError("Unable to View Bundles: Please Build Bundles First");
            }
        }
        
        [MenuItem(KViewCachePath)]
        private static void ViewCachePath()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }
    }
}