using System;
using System.IO;
using ILRuntime.Mono.Cecil.Pdb;
using JEngine.Core;
using JEngine.Helper;
using libx;
using UnityEngine;
using UnityEngine.Serialization;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class InitJEngine : MonoBehaviour
{
    public static InitJEngine Instance;
    public static AppDomain Appdomain;
    public static bool Success;

    #if UNITY_EDITOR
    public static long EncryptedCounts => ((JStream) (Instance._fs)).EncryptedCounts;
    #endif
    
    private const string DLLPath = "Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.dll";
    private const string PdbPath = "Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.pdb";

    [FormerlySerializedAs("Key")] [SerializeField] public string key;
    [FormerlySerializedAs("UsePdb")] [SerializeField] public bool usePdb;
    [FormerlySerializedAs("Debug")] [SerializeField] public bool debug = true;
    
    private Stream _fs;
    private Stream _pdb;
    
    private readonly object[] _param0 = new object[0];

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        GameStats.Initialize();
    }

    public void Load()
    {
        Instance = this;
        GameStats.Debug = debug;
        GameStats.Initialize();
        LoadHotFixAssembly();
    }

    void LoadHotFixAssembly()
    {
        Appdomain = new AppDomain();
        _pdb = null;

        byte[] buffer;
        
        //开发模式
        #if XASSET_PRO
        if (Assets.development)
        #else
        if (!Assets.runtimeMode)
        #endif
        {
            if (File.Exists(DLLPath))//直接读DLL
            {
                buffer = DLLMgr.FileToByte(DLLPath);
                
                //模拟加密
                buffer = CryptoHelper.AesEncrypt(buffer, key);
            }
            else
            {
                Log.PrintError("DLL文件不存在");
                return;
            }
                
            //查看是否有PDB文件
            if (File.Exists(PdbPath) && usePdb && (File.GetLastWriteTime(DLLPath)-File.GetLastWriteTime(PdbPath)).Seconds < 30)
            {
                _pdb = new MemoryStream(DLLMgr.FileToByte(PdbPath));
            }
        }
        else//真机模式解密加载
        {
            var dllAsset = Assets.LoadAsset("HotUpdateScripts.bytes", typeof(TextAsset));
            if (dllAsset.error != null)
            {
                Log.PrintError(dllAsset.error);
                return;
            }
            var dll = (TextAsset)dllAsset.asset;
            buffer = new byte[dll.bytes.Length];
            Array.Copy(dll.bytes, buffer, dll.bytes.Length);
            dllAsset.Release();//释放掉不需要再用的dll
        }
        try
        {
            // var original = CryptoHelper.AesDecrypt(dll.bytes, Key);以前的用法，过时了
                
            _fs = new JStream(buffer, key);
                
            /*
             * 如果一定要先解密，可以这样：
             * var original = CryptoHelper.AesDecrypt(dll.bytes, Key);
             * fs = new JStream(original, Key);
             * fs.Encrypted = false;
             */
            
            Appdomain.LoadAssembly(_fs, _pdb, new PdbReaderProvider());
        }
        catch(Exception e)
        {
            Log.PrintError("加载热更DLL错误：\n" + e);
            if (!usePdb)
            {
                Log.PrintError("加载热更DLL失败，请确保HotUpdateResources/Dll里面有HotUpdateScripts.bytes文件，并且Build Bundle后将DLC传入服务器");
            }
            else
            {
                Log.PrintError("PDB不可用，可能是DLL和PDB版本不一致，可能DLL是Release，如果是Release出包，请取消UsePdb选项，本次已跳过使用PDB");
                usePdb = false;
                LoadHotFixAssembly();
            }
            return;
        }
        
        Success = true;
        LoadILRuntime.InitializeILRuntime(Appdomain);
    }
    
    public void OnHotFixLoaded()
    {
        Appdomain.Invoke("HotUpdateScripts.Program", "RunGame", _param0, _param0);
        HotFixLoadedHelper.Init(Appdomain);
    }
}
