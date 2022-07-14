using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ET;

namespace BM
{
    public static partial class AssetComponent
    {
        /// <summary>
        /// 获取Bundle信息文件的路径
        /// </summary>
        internal static string BundleFileExistPath(string bundlePackageName, string fileName)
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Local)
            {
                string path = Path.Combine(AssetComponentConfig.LocalBundlePath, bundlePackageName, fileName);
#if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                path = "file://" + path;
#endif
                return path;
            }
            else
            {
                string path = Path.Combine(AssetComponentConfig.HotfixPath, bundlePackageName, fileName);
                if (!File.Exists(path))
                {
                    path = Path.Combine(AssetComponentConfig.LocalBundlePath, bundlePackageName, fileName);
#if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    path = "file://" + path;
#endif
                }
                else
                {
                    path = "file://" + path;
                }
                return path;
            }
        }

        /// <summary>
        /// 获取分包的更新索引列表
        /// </summary>
        private static async ETTask<string> GetRemoteBundlePackageVersionLog(string bundlePackageName)
        {
            byte[] data = await DownloadBundleHelper.DownloadDataByUrl(Path.Combine(AssetComponentConfig.BundleServerUrl, bundlePackageName, "VersionLogs.txt"));
            if (data == null)
            {
                AssetLogHelper.LogError(bundlePackageName + "获取更新索引列表失败");
                return null;
            }
            return System.Text.Encoding.UTF8.GetString(data);
        }
        
        /// <summary>
        /// 创建更新后的Log文件
        /// </summary>
        /// <param name="filePath">文件的全路径</param>
        /// <param name="fileData">文件的内容</param>
        private static void CreateUpdateLogFile(string filePath, string fileData)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(fileData);
                sw.WriteLine(sb.ToString());
            }
        }
        
    }

    public class UpdateBundleDataInfo
    {
        /// <summary>
        /// 是否需要更新
        /// </summary>
        public bool NeedUpdate = false;

        /// <summary>
        /// 需要更新的总大小
        /// </summary>
        public long NeedUpdateSize = 0;

        /// <summary>
        /// 是否取消
        /// </summary>
        internal bool Cancel = false;
        
        /// <summary>
        /// 需要更新的Bundle的信息
        /// </summary>
        internal readonly Dictionary<string, Dictionary<string, long>> PackageNeedUpdateBundlesInfos = new Dictionary<string, Dictionary<string, long>>();

        /// <summary>
        /// 分包对应的版本号    int[本地版本, 远程版本] 仅Build模式可用
        /// </summary>
        internal readonly Dictionary<string, int[]> PackageToVersion = new Dictionary<string, int[]>();
        
        /// <summary>
        /// 分包以及对于的类型
        /// </summary>
        internal readonly Dictionary<string, PackageType> PackageToType = new Dictionary<string, PackageType>();

        /// <summary>
        /// 客户端更新时间
        /// </summary>
        internal string UpdateTime = "";
        
        /// <summary>
        /// CRC信息字典
        /// </summary>
        internal readonly Dictionary<string, Dictionary<string, uint>> PackageCRCDictionary = new Dictionary<string, Dictionary<string, uint>>();
        
        /// <summary>
        /// CRC对应的写入流
        /// </summary>
        internal readonly Dictionary<string, StreamWriter> PackageCRCFile = new Dictionary<string, StreamWriter>();
        
        /// <summary>
        /// 更新完成的大小
        /// </summary>
        public long FinishUpdateSize = 0;

        /// <summary>
        /// 是否更新完成
        /// </summary>
        public bool FinishUpdate = false;

        /// <summary>
        /// 更新进度(1 - 100)
        /// </summary>
        public float Progress
        {
            get
            {
                float progress = ((float)FinishUpdateSize / NeedUpdateSize) * 100.0f;
                return progress;
            }
        }

        /// <summary>
        /// 总共需要下载的Bundle的数量
        /// </summary>
        public int NeedDownLoadBundleCount = 0;
        
        /// <summary>
        /// 下载完成的Bundle的数量
        /// </summary>
        public int FinishDownLoadBundleCount = 0;
        
        /// <summary>
        /// 平滑进度
        /// </summary>
        internal float SmoothProgress = 0.0f;
        private Action<float> progressCallback;
        /// <summary>
        /// 下载进度回调，每帧刷新，有插值
        /// </summary>
        public event Action<float> ProgressCallback
        {
            add => progressCallback += value;
            remove => this.progressCallback -= value;
        }

        internal void UpdateProgress(float deltaTime)
        {
            if (FinishUpdate)
            {
                SmoothProgress = 100;
            }
            else
            {
                SmoothProgress += (Progress - SmoothProgress) * ValueHelper.GetMinValue(deltaTime / 0.1f, 1.0f);
            }
            progressCallback?.Invoke(SmoothProgress);
        }
        
        internal Action FinishCallback;

        /// <summary>
        /// 下载更新完成回调
        /// </summary>
        public event Action DownLoadFinishCallback
        {
            add => FinishCallback += value;
            remove => this.FinishCallback -= value;
        }
        
        /// <summary>
        /// 下载速度平均个数
        /// </summary>
        private int speedAvgCount = 10;
        /// <summary>
        /// 下载速度缓存队列
        /// </summary>
        private readonly Queue<int> downLoadSpeedQueue = new Queue<int>();
        /// <summary>
        /// 下载速度
        /// </summary>
        public int DownLoadSpeed
        {
            get
            {
                int addSpeed = 0;
                int speedQueueCount = downLoadSpeedQueue.Count;
                if (speedQueueCount == 0)
                {
                    return 0;
                }
                for (int i = 0; i < speedQueueCount; i++)
                {
                    int tempSpeed = downLoadSpeedQueue.Dequeue();
                    addSpeed += tempSpeed;
                    downLoadSpeedQueue.Enqueue(tempSpeed);
                }
                return addSpeed / speedQueueCount * AssetComponentConfig.MaxDownLoadCount;
            }
        }

        private Action<int> downLoadSpeedCallback;
        
        /// <summary>
        /// 下载速度改变回调，非每帧都刷新 单位byte每秒
        /// </summary>
        public event Action<int> DownLoadSpeedCallback
        {
            add => downLoadSpeedCallback += value;
            remove => this.downLoadSpeedCallback -= value;
        }
        
        public void AddSpeedQueue(int speed)
        {
            if (downLoadSpeedQueue.Count >= speedAvgCount)
            {
                downLoadSpeedQueue.Dequeue();
                AddSpeedQueue(speed);
                return;
            }
            downLoadSpeedQueue.Enqueue(speed);
            downLoadSpeedCallback?.Invoke(DownLoadSpeed);
        }
        
        /// <summary>
        /// 获取更新的分包的版本索引    int[本地版本, 远程版本]
        /// </summary>
        public int[] GetVersion(string bundlePackageName)
        {
            if (AssetComponentConfig.AssetLoadMode != AssetLoadMode.Build)
            {
                AssetLogHelper.LogError("仅Build模式可用获取版本索引");
                return null;
            }
            if (!PackageToVersion.TryGetValue(bundlePackageName, out int[] versionData))
            {
                AssetLogHelper.LogError("获取索引号没有找到分包: " + bundlePackageName);
                return null;
            }
            return versionData;
        }

        /// <summary>
        /// 添加一个更新好的CRC文件信息
        /// </summary>
        internal void AddCRCFileInfo(string bundlePackageName, string fileName, uint crc)
        {
            if (AssetComponentConfig.AssetLoadMode != AssetLoadMode.Build)
            {
                AssetLogHelper.LogError("AssetLoadMode != Build 不涉及更新");
                return;
            }
            if (!PackageCRCDictionary.TryGetValue(bundlePackageName, out Dictionary<string, uint> crcDictionary))
            {
                AssetLogHelper.LogError("获取索引号没有找到分包: " + bundlePackageName);
                return;
            }
            if (Cancel)
            {
                return;
            }
            if (!crcDictionary.ContainsKey(fileName))
            {
                crcDictionary.Add(fileName, crc);
            }
            PackageCRCFile[bundlePackageName].WriteLine(fileName + "|" + crc.ToString() + "|" + UpdateTime);
            PackageCRCFile[bundlePackageName].Flush();
        }

        private Action errorCancelCallback;

        /// <summary>
        /// 下载失败回调
        /// </summary>
        public event Action ErrorCancelCallback
        {
            add => errorCancelCallback += value;
            remove => this.errorCancelCallback -= value;
        }
        
        /// <summary>
        /// 取消更新
        /// </summary>
        public void CancelUpdate()
        {
            if (Cancel)
            {
                return;
            }
            Cancel = true;
            errorCancelCallback?.Invoke();
            DestroySelf();
        }
        
        private void DestroySelf()
        {
            foreach (StreamWriter sw in PackageCRCFile.Values)
            {
                sw.Close();
                sw.Dispose();
            }
            PackageCRCFile.Clear();
            AssetComponent.DownLoadAction -= UpdateProgress;
            progressCallback = null;
            FinishCallback = null;
            downLoadSpeedCallback = null;
            errorCancelCallback = null;
        }

        /// <summary>
        /// 分包类型
        /// </summary>
        internal enum PackageType
        {
            /// <summary>
            /// 未加密
            /// </summary>
            Normal,
            /// <summary>
            /// 加密
            /// </summary>
            Encrypt,
            /// <summary>
            /// 原始资源
            /// </summary>
            Origin,
        }
    }
}