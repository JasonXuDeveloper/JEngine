using System;
using System.IO;
using UnityEngine;
using JEngine.Core;
using JEngine.Helper;
using ILRuntime.Mono.Cecil.Pdb;
using libx;
using UnityEngine.Serialization;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class InitJEngine : MonoBehaviour
{
    public static InitJEngine Instance;
    public static AppDomain Appdomain;
    public static bool Success;

#if UNITY_EDITOR
    public static long EncryptedCounts => ((JStream) (Instance._fs)).EncryptedCounts;
    private const string DLLPath = "Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.dll";
    private const string PdbPath = "Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.pdb";
#endif

    private const string DllName = "HotUpdateScripts.bytes";
    private const string HotMainType = "HotUpdateScripts.Program";
    private const string HotMainMethod = "RunGame";

    [FormerlySerializedAs("Key")] [SerializeField]
    public string key;

    [FormerlySerializedAs("UsePdb")] [SerializeField]
    public bool usePdb;

    [FormerlySerializedAs("Debug")] [SerializeField]
    public bool debug = true;

    private Stream _fs;
    private Stream _pdb;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        GameStats.Initialize();
        GameStats.Debug = debug;
        AssetMgr.Loggable = debug;

        Updater.OnAssetsInitialized = (gameScene,onProgress) =>
        {
            Assets.AddSearchPath("Assets/HotUpdateResources/Controller");
            Assets.AddSearchPath("Assets/HotUpdateResources/Dll");
            Assets.AddSearchPath("Assets/HotUpdateResources/Material");
            Assets.AddSearchPath("Assets/HotUpdateResources/Other");
            Assets.AddSearchPath("Assets/HotUpdateResources/Prefab");
            Assets.AddSearchPath("Assets/HotUpdateResources/Scene");
            Assets.AddSearchPath("Assets/HotUpdateResources/ScriptableObject");
            Assets.AddSearchPath("Assets/HotUpdateResources/TextAsset");
            Assets.AddSearchPath("Assets/HotUpdateResources/UI");

            AssetMgr.LoadSceneAsync(gameScene, false, onProgress, (b) =>
            {
                if (!b) return;
                Instance.Load();
                ClassBindMgr.Instantiate();
                Instance.OnHotFixLoaded();
            });
        };
    }

    public void Load()
    {
        LoadHotFixAssembly();
    }

    void LoadHotFixAssembly()
    {
        Appdomain = new AppDomain();
        _pdb = null;

        byte[] dll;

#if UNITY_EDITOR
        //开发模式
        if (!AssetMgr.RuntimeMode)
        {
            if (File.Exists(DLLPath)) //直接读DLL
            {
                dll = DLLMgr.FileToByte(DLLPath);

                //模拟加密
                dll = CryptoHelper.AesEncrypt(dll, key);
            }
            else
            {
                Log.PrintError("DLL文件不存在");
                return;
            }

            //查看是否有PDB文件
            if (File.Exists(PdbPath) && usePdb &&
                (File.GetLastWriteTime(DLLPath) - File.GetLastWriteTime(PdbPath)).Seconds < 30)
            {
                _pdb = new MemoryStream(DLLMgr.FileToByte(PdbPath));
            }
        }
        else //真机模式解密加载
#endif
        {
            var dllFile = (TextAsset) AssetMgr.Load(DllName, typeof(TextAsset));
            if (dllFile == null)
            {
                return;
            }

            dll = dllFile.bytes;
        }
        
        var buffer = new byte[dll.Length];
        Array.Copy(dll, buffer, dll.Length);
        AssetMgr.Unload(DllName, true);

        try
        {
            //这里默认用分块解密，JStream
            _fs = new JStream(buffer, key);

            /*
             * 如果一定要直接解密然后不进行分块解密加载Dll，可以这样：
             * var original = CryptoHelper.AesDecrypt(dll.bytes, Key);
             * _fs = new JStream(original, Key);
             * _fs.Encrypted = false;
             */

            Appdomain.LoadAssembly(_fs, _pdb, new PdbReaderProvider());
        }
        catch (Exception e)
        {
            Log.PrintError("加载热更DLL错误：\n" + e);
            if (!usePdb)
            {
                Log.PrintError(
                    "加载热更DLL失败，请确保HotUpdateResources/Dll里面有HotUpdateScripts.bytes文件，并且Build Bundle后将DLC传入服务器");
                Log.PrintError("也有可能是密码不匹配或密码包含特殊字符导致的");
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
        Appdomain.Invoke(HotMainType, HotMainMethod, Tools.Param0, Tools.Param0);
        HotFixLoadedHelper.Init(Appdomain);
    }
}