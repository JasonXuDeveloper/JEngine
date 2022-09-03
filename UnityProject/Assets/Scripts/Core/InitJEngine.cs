using System;
using System.IO;
using UnityEngine;
using JEngine.Core;
using JEngine.Helper;
using ILRuntime.Mono.Cecil.Pdb;
using UnityEngine.Serialization;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public partial class InitJEngine : MonoBehaviour
{
    //单例
    public static InitJEngine Instance;

    //热更域
    public static AppDomain Appdomain;

    //是否成功加载
    public static bool Success;

    //dll名字，入口函数名字，以及周期方法名
    private const string DllName = "HotUpdateScripts";
    private const string HotMainType = "HotUpdateScripts.Program";
    private const string SetupGameMethod = "SetupGame";
    private const string RunGameMethod = "RunGame";

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
    }

    /// <summary>
    /// 加载热更
    /// </summary>
    public void LoadHotUpdateCallback()
    {
        //替换LogHandler，让UnityEngine.Debug.LogException能定位更精准的堆栈，同时利用插件精简堆栈信息
        Debug.unityLogger.logHandler = new JEngine.Core.Logger(Debug.unityLogger.logHandler);
        //给ETTask/ETVoid的报错定位到UnityEngine.Debug.LogException
        ET.ETTask.ExceptionHandler += Debug.LogException;
        //加载热更DLL
        Instance.LoadHotFixAssembly();
        //初始化LifeCycle
        LifeCycleMgr.Initialize();
        //调用SetupGame周期
        Tools.InvokeHotMethod(HotMainType, SetupGameMethod);
#if INIT_JE
        //初始化ClassBind
        ClassBindMgr.Instantiate();
#endif
        //调用RunGame周期
        Tools.InvokeHotMethod(HotMainType, RunGameMethod);
        //调用在主工程的热更代码加载完毕后的周期
        HotUpdateLoadedHelper.Init(Appdomain);
    }

    /// <summary>
    /// 使用ILRuntime加载热更工程
    /// </summary>
    private void LoadHotFixAssembly()
    {
        //创建新对象实例
        Appdomain = new AppDomain((int)useJIT);
        _pdb = null;

        bool isEditorMode = !AssetMgr.RuntimeMode;

        //dll的二进制
        byte[] dll = DllMgr.GetDllBytes(DllName, isEditorMode);
        //pdb默认不存在
        byte[] pdb = ConstMgr.NullBytes;
        //编辑器下模拟加密dll
        if (isEditorMode)
        {
            DllMgr.SimulateEncryption(ref dll, key);
        }
        if (usePdb)
        {
            try
            {
                var pdbFileBytes = DllMgr.GetPdbBytes(DllName, Application.isEditor && !AssetMgr.RuntimeMode);
                pdb = new byte[pdbFileBytes.Length];
                Array.Copy(pdbFileBytes, pdb, pdbFileBytes.Length);
            }
            catch
            {
                //ignore
            }
        }

        //尝试加载dll
        try
        {
            //这里默认用分块解密，JStream
            _fs = new JStream(dll, key);
            if (pdb.Length != 0)
            {
                _pdb = new MemoryStream(pdb);
            }

            //加载dll
            if (usePdb && pdb != null)
            {
                Appdomain.LoadAssembly(_fs, _pdb, new PdbReaderProvider());
            }
            else
            {
                Appdomain.LoadAssembly(_fs);
            }
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