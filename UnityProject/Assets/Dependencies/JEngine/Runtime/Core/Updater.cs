using System;
using YooAsset;
using UnityEngine;
using JEngine.Core;
using System.Threading.Tasks;

public partial class Updater : MonoBehaviour
{
    [SerializeField] [Tooltip("热更资源下载地址")] private string resourceUrl = "http://127.0.0.1:7888/";
    [SerializeField] [Tooltip("热更资源下载备用地址")] private string fallbackUrl = "http://127.0.0.1:7888/";
    [SerializeField] private string gameScene = "Assets/HotUpdateResources/Main/Scene/Game.unity";
    [SerializeField] private string mainPackageName = "Main";
    [Tooltip("Simulate是开发模式，Standalone是离线模式，Remote是真机模式")] [SerializeField]
    private UpdateMode mode = UpdateMode.Simulate;

    /// <summary>
    /// 主包
    /// </summary>
    public static string MainPackageName => _instance.mainPackageName;
    
    /// <summary>
    /// 模式
    /// </summary>
    public static UpdateMode Mode => _instance.mode;
    
    /// <summary>
    /// 更新模式
    /// </summary>
    public enum UpdateMode: byte
    {
        Simulate = 0,
        Standalone = 1,
        Remote = 2,
    }

    /// <summary>
    /// 初始化资源包
    /// </summary>
    /// <param name="packageName"></param>
    public static async Task SetUpPackage(string packageName)
    {
        string resourceUrl = _instance.resourceUrl;
        string fallbackUrl = _instance.fallbackUrl;
        // 更新URL
        string end = $"/Bundles/{platform}/{packageName}";
        if(!resourceUrl.EndsWith(end)) resourceUrl = $"{resourceUrl}{end}";
        if(!fallbackUrl.EndsWith(end)) fallbackUrl = $"{fallbackUrl}{end}";
        // 创建默认的资源包
        var package = YooAssets.CreatePackage(packageName);
        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(package);
        // 初始化
        InitializeParameters initParameters = null;
        switch (_instance.mode)
        {
            case UpdateMode.Simulate:
                initParameters = new EditorSimulateModeParameters()
                {
                    SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName)
                };
                break;
            case UpdateMode.Standalone:
                initParameters = new OfflinePlayModeParameters();
                break;
            case UpdateMode.Remote:
                initParameters = new HostPlayModeParameters()
                {
                    QueryServices = new QueryStreamingAssetsFileServices(),
                    DefaultHostServer = resourceUrl,
                    FallbackHostServer = fallbackUrl
                };
                break;
        }
        await package.InitializeAsync(initParameters).Task;
    }
    
    /// <summary>
    /// 下载包
    /// </summary>
    /// <param name="packageName">包名</param>
    /// <param name="updater">回调事件</param>
    public static async Task UpdatePackage(string packageName, IUpdater updater)
    {
        await SetUpPackage(packageName);
        MessageBox.Dispose();
        string version = $"v{Application.version}";
        
        void Init()
        {
            updater?.OnProgress(1);
            updater?.OnMessage("下载完成");
            //直接调用热更完成
            updater?.OnUpdateFinish(true);
        }
        
        if (_instance.mode == UpdateMode.Remote)
        {
            var package = YooAssets.GetPackage(packageName);
            var updatePackageVersionOp = package.UpdatePackageVersionAsync();
            await updatePackageVersionOp.Task;
            bool updatedManifest = false;
            string err = null;

            if (updatePackageVersionOp.Status == EOperationStatus.Succeed)
            {
                version = updatePackageVersionOp.PackageVersion;
                var updatePackageManifestOp = package.UpdatePackageManifestAsync(version);
                await updatePackageManifestOp.Task;
                updatedManifest = updatePackageManifestOp.Status == EOperationStatus.Succeed;
                err = updatePackageManifestOp.Error;
            }
            if (updatePackageVersionOp.Status != EOperationStatus.Succeed || !updatedManifest)
            {
                if (err == null)
                {
                    err = updatePackageVersionOp.Error;
                }
                var mb = MessageBox.Show("错误", $"无法获取服务器资源信息：{err}", "返回", "退出");
                mb.onComplete = ok =>
                {
                    if (ok == MessageBox.EventId.Ok)
                    {
                        updater?.OnUpdateFinish(false);
                    }
                    else
                    {
                        Quit();
                    }
                };
                return;
            }
        }

        updater?.OnVersion($"资源版本号: {version}");
        

        if (_instance.mode == UpdateMode.Remote)
        {
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            int timeout = 60;
            var package = YooAssets.GetPackage(packageName);
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);
    
            //没有需要下载的资源
            if (downloader.TotalDownloadCount == 0)
            {
                Init();
            }
            else {
            
                //需要下载的文件总数和总大小
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;   
            
                updater?.OnMessage($"需要更新, 大小: {Tools.GetDisplaySize(totalDownloadBytes)}");
                var tips =
                    $"发现{totalDownloadCount}个资源有更新，总计需要下载 {Tools.GetDisplaySize(totalDownloadBytes)}";
                var mb = MessageBox.Show("提示", tips, "下载", "退出");
                mb.onComplete = async ok =>
                {
                    if (ok == MessageBox.EventId.Ok)
                    {
                        downloader.OnDownloadProgressCallback = (totalDownloadCount, currentDownloadCount, totalDownloadBytes,  currentDownloadBytes) =>
                        {
                            updater?.OnMessage(
                                $"正在下载第{currentDownloadCount}/{totalDownloadCount}个文件，总进度{Tools.GetDisplaySize(currentDownloadBytes)}/{Tools.GetDisplaySize(totalDownloadBytes)}");
                            updater?.OnProgress(
                                (float)Math.Round((double)currentDownloadBytes / totalDownloadBytes, 2));
                        };
                        downloader.OnDownloadErrorCallback = (file, err) =>
                        {
                            updater?.OnMessage($"下载{file}失败：{err}");
                            Log.PrintError($"下载{file}失败：{err}");
                        };
                        
                        //开启下载
                        downloader.BeginDownload();
                        await downloader.Task;

                        //检测下载结果
                        if (downloader.Status == EOperationStatus.Succeed)
                        {
                            //下载成功
                            Init();
                        }
                        else
                        {
                            //下载失败
                            MessageBox.Show("错误", $"下载失败：{downloader.Error}", "返回", "退出");
                            updater?.OnUpdateFinish(false);
                        }
                    }
                    else
                    {
                        Quit();
                    }
                }; 
            }
        }
        else
        {
            Init();
        }
    }

    /// <summary>
    /// 单例
    /// </summary>
    private static Updater _instance;

    /// <summary>
    /// 更新配置
    /// </summary>
    private void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        // 初始化资源系统
        YooAssets.Initialize();
    }
    
    /// <summary>
    /// 获取平台
    /// </summary>
    private static RuntimePlatform platform
    {
        get
        {
#if UNITY_ANDROID
            return RuntimePlatform.Android;
#elif UNITY_IOS 
             return RuntimePlatform.IPhonePlayer;
#elif UNITY_STANDALONE_OSX
              return RuntimePlatform.OSXPlayer;
#elif UNITY_STANDALONE_WIN
             return RuntimePlatform.WindowsPlayer;
#else
            return Application.platform;
#endif
        }
    }

    /// <summary>
    /// 给按钮拖拽赋值的点击事件，下载更新，用于初始化主包
    /// </summary>
    public void StartUpdate()
    {
        var updater = FindObjectOfType<UpdateScreen>();
        updater.sceneName = gameScene;
        _ = UpdatePackage(mainPackageName, updater);
    }

    private void OnDestroy()
    {
        MessageBox.Dispose();
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}