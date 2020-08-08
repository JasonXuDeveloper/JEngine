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

    [SerializeField]private string Key;

    void Start()
    {
        Instance = this;
        LoadHotFixAssembly();
        Application.targetFrameRate = 30;
    }

    void LoadHotFixAssembly()
    {
        appdomain = new AppDomain();

        var dllAsset = Assets.LoadAsset("HotUpdateScripts.bytes", typeof(TextAsset));
        if (dllAsset.error != null)
        {
            Log.PrintError(dllAsset.error);
            return;
        }

        var dll = (TextAsset)dllAsset.asset;

        byte[] original = dll.bytes;
        try
        {
            if (!Assets.runtimeMode)
            {
                original = CryptoHelper.AesDecrypt(original, "DevelopmentMode.");
            }
            else
            {
                original = CryptoHelper.AesDecrypt(original, Key);
            }
        }
        catch(Exception ex)
        {
            Log.PrintError("加载热更DLL失败，可能是解密密码不正确");
            Log.PrintError("加载热更DLL错误：\n" + ex.Message);
            return;
        }
            
        fs = new MemoryStream(original);
        MemoryStream pdb = null;
        if (Application.isEditor)
        {
            try
            {
                pdb = new MemoryStream(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.pdb").bytes);
            }
            catch
            {
            }
        }
            
        try
        {
            appdomain.LoadAssembly(fs, pdb, new PdbReaderProvider());
        }
        catch(Exception e)
        {
            Log.PrintError("加载热更DLL失败，请确保HotUpdateResources/Dll里面有HotUpdateScripts.bytes文件，并且Build Bundle后将DLC传入服务器");
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
        fs = null;
    }
}
