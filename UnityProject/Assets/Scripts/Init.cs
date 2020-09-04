using System;
using System.IO;
using ILRuntime.Mono.Cecil.Pdb;
using JEngine.Core;
using libx;
using UnityEditor;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class Init : MonoBehaviour
{
    public static Init Instance;
    AppDomain appdomain;
    MemoryStream fs;
    MemoryStream pdb;
    private const string dllPath = "Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.dll";
    private const string pdbPath = "Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.pdb";

    [SerializeField]private string Key;
    [SerializeField] private bool UsePdb = true;

    void Start()
    {
        Instance = this;
        LoadHotFixAssembly();
        Application.targetFrameRate = 30;
    }

    void LoadHotFixAssembly()
    {
        appdomain = new AppDomain();

        pdb = null;
        
        //编译模式
        if (!Assets.runtimeMode)
        {
            if (File.Exists(dllPath))
            {
                fs = new MemoryStream(DLLMgr.FileToByte(dllPath));
            }
            else
            {
                Log.PrintWarning("DLL文件不存在");
                return;
            }
                
            //查看是否有PDB文件
            if (File.Exists(pdbPath) && UsePdb)
            {
                var dllTime = File.GetLastAccessTime(dllPath);
                var pdbTime = File.GetLastAccessTime(pdbPath);
                if (pdbTime > dllTime.AddSeconds(10))
                {
                    Log.PrintWarning("PDB于DLL版本不匹配，已跳过使用PDB");
                }
                else
                {
                    pdb = new MemoryStream(DLLMgr.FileToByte(pdbPath));
                }
            }
        }
        else//解密加载
        {
            var dllAsset = Assets.LoadAsset("HotUpdateScripts.bytes", typeof(TextAsset));
            if (dllAsset.error != null)
            {
                Log.PrintError(dllAsset.error);
                return;
            }
            var dll = (TextAsset)dllAsset.asset;
            try
            {
                var original = CryptoHelper.AesDecrypt(dll.bytes, Key);
                fs = new MemoryStream(original);
            }
            catch(Exception ex)
            {
                Log.PrintError("加载热更DLL失败，可能是解密密码不正确");
                Log.PrintError("加载热更DLL错误：\n" + ex.Message);
                return;
            }
        }
        try
        {
            appdomain.LoadAssembly(fs, pdb, new PdbReaderProvider());
        }
        catch(Exception e)
        {
            if (!UsePdb)
            {
                Log.PrintError("加载热更DLL失败，请确保HotUpdateResources/Dll里面有HotUpdateScripts.bytes文件，并且Build Bundle后将DLC传入服务器");   
            }
            else
            {
                Log.PrintError("加载热更DLL失败");
            }
            Log.PrintError("加载热更DLL错误：\n" + e.Message);
            return;
        }
            
            
        InitILrt.InitializeILRuntime(appdomain);
        OnHotFixLoaded();
    }

    

    void OnHotFixLoaded()
    {
        appdomain.Invoke("HotUpdateScripts.Program", "RunGame", null, null);
    }
    
    private void OnDestroy()
    {
        if (fs != null)
            fs.Close();
        if (pdb != null)
            pdb.Close();
        fs = null;
        pdb = null;
    }
}
