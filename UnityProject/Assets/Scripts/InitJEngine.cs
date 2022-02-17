using libx;
using System;
using System.IO;
using UnityEngine;
using JEngine.Core;
using JEngine.Helper;
using ILRuntime.Mono.Cecil.Pdb;
using UnityEngine.Serialization;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class InitJEngine : MonoBehaviour
{
    //单例
    public static InitJEngine Instance;

    //热更域
    public static AppDomain Appdomain;

    //是否成功加载
    public static bool Success;

    //编辑器下数据，用于统计分块解密，以及提供dll和pdb的位置
#if UNITY_EDITOR
    public static long EncryptedCounts => ((JStream)(Instance._fs)).EncryptedCounts;
    private const string DLLPath = "Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.dll";
    private const string PdbPath = "Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.pdb";
#endif

    //dll名字，入口函数名字，以及周期方法名
    private const string DllName = "HotUpdateScripts.bytes";
    private const string HotMainType = "HotUpdateScripts.Program";
    private const string RunGameMethod = "RunGame";
    private const string SetupGameMethod = "SetupGame";

    //加密密钥
    [Tooltip("加密密钥，需要16位")] [FormerlySerializedAs("Key")] [SerializeField]
    public string key;

    //寄存器模式
    [Tooltip("ILRuntime寄存器模式")] [SerializeField]
    private ILRuntimeJITFlag useJIT = ILRuntimeJITFlag.JITOnDemand;

    //是否使用pdb，真机禁止
    [Tooltip("是否使用pdb，仅编辑器生效")] [FormerlySerializedAs("UsePdb")] [SerializeField]
    public bool usePdb;

    //是否允许debug（会产生log）
    [Tooltip("是否允许debug，勾选后产生log")] [FormerlySerializedAs("Debug")] [SerializeField]
    public bool debug = true;

    //数据流
    private Stream _fs;
    private Stream _pdb;

    /// <summary>
    /// 初始化
    /// </summary>
    private void Awake()
    {
        //单例
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //初始化Debug
        GameStats.Initialize();
        GameStats.Debug = debug;
        AssetMgr.Loggable = debug;

        //进入热更后的回调
        Updater.OnAssetsInitialized = (gameScene, onProgress) =>
        {
            //短路径
            Assets.AddSearchPath("Assets/HotUpdateResources/Controller");
            Assets.AddSearchPath("Assets/HotUpdateResources/Dll");
            Assets.AddSearchPath("Assets/HotUpdateResources/Material");
            Assets.AddSearchPath("Assets/HotUpdateResources/Other");
            Assets.AddSearchPath("Assets/HotUpdateResources/Prefab");
            Assets.AddSearchPath("Assets/HotUpdateResources/Scene");
            Assets.AddSearchPath("Assets/HotUpdateResources/ScriptableObject");
            Assets.AddSearchPath("Assets/HotUpdateResources/TextAsset");
            Assets.AddSearchPath("Assets/HotUpdateResources/UI");

            //跳转场景并初始化热更代码
            AssetMgr.LoadSceneAsync(gameScene, false, onProgress, (b) =>
            {
                //先确保可以跳转场景
                if (!b) return;
                //加载热更DLL
                Instance.LoadHotFixAssembly();
                //调用SetupGame周期
                Instance.InvokeHotUpdate(SetupGameMethod);
                //初始化ClassBind
                ClassBindMgr.Instantiate();
                //调用RunGame周期
                Instance.InvokeHotUpdate(RunGameMethod);
                //调用在主工程的热更代码加载完毕后的周期
                HotUpdateLoadedHelper.Init(Appdomain);
            });
        };
    }

    /// <summary>
    /// 使用ILRuntime加载热更工程
    /// </summary>
    private void LoadHotFixAssembly()
    {
        //创建新对象实例
        Appdomain = new AppDomain((int)useJIT);
        _pdb = null;

        //dll的二进制
        byte[] dll;

#if UNITY_EDITOR
        //开发模式
        if (!AssetMgr.RuntimeMode)
        {
            //判断有没有dll
            if (File.Exists(DLLPath))
            {
                //直接读DLL
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
        else
#endif
        {
            //真机模式解密加载
            var dllFile = (TextAsset)AssetMgr.Load(DllName, typeof(TextAsset));
            if (dllFile == null)
            {
                return;
            }

            //对定义的对象赋值
            dll = dllFile.bytes;
        }

        //生成缓冲区，复制加密dll数据
        var buffer = new byte[dll.Length];
        Array.Copy(dll, buffer, dll.Length);
        //卸载dll资源
        AssetMgr.Unload(DllName, true);

        //尝试加载dll
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

            //加载dll
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
            else if (Application.isEditor && usePdb)
            {
                Log.PrintError("PDB不可用，可能是DLL和PDB版本不一致，可能DLL是Release，如果是Release出包，请取消UsePdb选项，本次已跳过使用PDB");
                usePdb = false;
                LoadHotFixAssembly();
            }

            return;
        }

        //成功加载热更dll
        Success = true;
        //初始化ILRuntime
        LoadILRuntime.InitializeILRuntime(Appdomain);
    }

    public void InvokeHotUpdate(string method)
    {
        Appdomain.Invoke(HotMainType, method, Tools.Param0, Tools.Param0);
    }

    [Serializable]
    private enum ILRuntimeJITFlag
    {
        None = 0,

        /// <summary>
        /// Method will be JIT when method is called multiple time
        /// </summary>
        JITOnDemand = 1,

        /// <summary>
        /// Method will be JIT immediately when called, instead of progressively warm up
        /// </summary>
        JITImmediately = 2,

        /// <summary>
        /// Method will not be JIT when called
        /// </summary>
        NoJIT = 4,

        /// <summary>
        /// Method will always be inlined when called
        /// </summary>
        ForceInline = 8
    }
}