using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ET;
using UnityEngine.Networking;

namespace BM
{
    public static partial class AssetComponent
    {
        /// <summary>
        /// 检查分包是否需要更新
        /// </summary>
        /// <param name="bundlePackageNames">所有分包的名称以及是否验证文件CRC</param>
        public static async ETTask<UpdateBundleDataInfo> CheckAllBundlePackageUpdate(Dictionary<string, bool> bundlePackageNames)
        {
            UpdateBundleDataInfo updateBundleDataInfo = new UpdateBundleDataInfo();
            if (AssetComponentConfig.AssetLoadMode != AssetLoadMode.Build)
            {
                updateBundleDataInfo.NeedUpdate = false;
                if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Local)
                {
                    AssetComponentConfig.HotfixPath = AssetComponentConfig.LocalBundlePath;
                }
                else
                {
#if !UNITY_EDITOR
                    AssetLogHelper.LogError("AssetLoadMode = AssetLoadMode.Develop 只能在编辑器下运行");
#endif
                }
                return updateBundleDataInfo;
            }
            updateBundleDataInfo.UpdateTime = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            //开始处理每个分包的更新信息
            foreach (var bundlePackageInfo in bundlePackageNames)
            {
                string bundlePackageName = bundlePackageInfo.Key;
                string remoteVersionLog = await GetRemoteBundlePackageVersionLog(bundlePackageName);
                if (remoteVersionLog == null)
                {
                    continue;
                }
                //创建各个分包对应的文件夹
                if (!Directory.Exists(Path.Combine(AssetComponentConfig.HotfixPath, bundlePackageName)))
                {
                    Directory.CreateDirectory(Path.Combine(AssetComponentConfig.HotfixPath, bundlePackageName));
                }
                //获取CRCLog
                string crcLogPath = Path.Combine(AssetComponentConfig.HotfixPath, bundlePackageName, "CRCLog.txt");
                updateBundleDataInfo.PackageCRCDictionary.Add(bundlePackageName, new Dictionary<string, uint>());
                if (File.Exists(crcLogPath))
                {
                    using (StringReader stringReader = new StringReader(crcLogPath))
                    {
                        string crcLog = await stringReader.ReadToEndAsync();
                        string[] crcLogData = crcLog.Split('\n');
                        for (int j = 0; j < crcLogData.Length; j++)
                        {
                            string crcLine = crcLogData[j];
                            if (string.IsNullOrWhiteSpace(crcLine))
                            {
                                continue;
                            }
                            string[] info = crcLine.Split('|');
                            if (info.Length != 3)
                            {
                                continue;
                            }
                            if (!uint.TryParse(info[1], out uint crc))
                            {
                                continue;
                            }
                            //如果存在重复就覆盖
                            if (updateBundleDataInfo.PackageCRCDictionary[bundlePackageName].ContainsKey(info[0]))
                            {
                                updateBundleDataInfo.PackageCRCDictionary[bundlePackageName][info[0]] = crc;
                            }
                            else
                            {
                                updateBundleDataInfo.PackageCRCDictionary[bundlePackageName].Add(info[0], crc);
                            }
                            
                        }
                    }
                }
                else
                {
                    CreateUpdateLogFile(crcLogPath, null);
                }
                updateBundleDataInfo.PackageCRCFile.Add(bundlePackageName, new StreamWriter(crcLogPath, true));
                //获取本地的VersionLog
                string localVersionLogExistPath = BundleFileExistPath(bundlePackageName, "VersionLogs.txt");
                ETTask logTcs = ETTask.Create();
                string localVersionLog;
                using (UnityWebRequest webRequest = UnityWebRequest.Get(localVersionLogExistPath))
                {
                    UnityWebRequestAsyncOperation weq = webRequest.SendWebRequest();
                    weq.completed += (o) =>
                    {
                        logTcs.SetResult();
                    };
                    await logTcs;
#if UNITY_2020_1_OR_NEWER
                    if (webRequest.result != UnityWebRequest.Result.Success)
#else
                    if (!string.IsNullOrEmpty(webRequest.error))
#endif
                    {
                        localVersionLog = "INIT|0";
                    }
                    else
                    {
                        localVersionLog = webRequest.downloadHandler.text;
                    }
                }

                if (bundlePackageInfo.Value)
                {
                    await CalcNeedUpdateBundleFileCRC(updateBundleDataInfo, bundlePackageName, remoteVersionLog, localVersionLog);
                }
                else
                {
                    await CalcNeedUpdateBundle(updateBundleDataInfo, bundlePackageName, remoteVersionLog, localVersionLog);
                }
            }
            if (updateBundleDataInfo.NeedUpdateSize > 0)
            {
                updateBundleDataInfo.NeedUpdate = true;
            }
            else
            {
                foreach (StreamWriter sw in updateBundleDataInfo.PackageCRCFile.Values)
                {
                    sw.Close();
                    sw.Dispose();
                }
                updateBundleDataInfo.PackageCRCFile.Clear();
            }
            return updateBundleDataInfo;
        }
        
        /// <summary>
        /// 获取所哟需要更新的Bundle的文件(不检查文件CRC)
        /// </summary>
        private static async ETTask CalcNeedUpdateBundle(UpdateBundleDataInfo updateBundleDataInfo, string bundlePackageName, string remoteVersionLog, string localVersionLog)
        {
            string[] remoteVersionData = remoteVersionLog.Split('\n');
            string[] localVersionData = localVersionLog.Split('\n');
            int remoteVersion = int.Parse(remoteVersionData[0].Split('|')[1]);
            int localVersion = int.Parse(localVersionData[0].Split('|')[1]);
            updateBundleDataInfo.PackageToVersion.Add(bundlePackageName, new int[2]{localVersion, remoteVersion});
            if (localVersion > remoteVersion)
            {
                AssetLogHelper.LogError("本地版本号优先与远程版本号 " + localVersion + ">" + remoteVersion + "\n"
                + "localBundleTime: " + localVersionData[0].Split('|')[0] + "\n"
                + "remoteBundleTime: " + remoteVersionData[0].Split('|')[0] + "\n"
                + "Note: 发送了版本回退或者忘了累进版本号");
            }
            //判断StreamingAsset下资源
            string streamingAssetLogPath = Path.Combine(AssetComponentConfig.LocalBundlePath, bundlePackageName, "VersionLogs.txt");
#if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            streamingAssetLogPath = "file://" + streamingAssetLogPath;
#endif
            ETTask streamingLogTcs = ETTask.Create();
            string streamingLog;
            using (UnityWebRequest webRequest = UnityWebRequest.Get(streamingAssetLogPath))
            {
                UnityWebRequestAsyncOperation weq = webRequest.SendWebRequest();
                weq.completed += (o) =>
                {
                    streamingLogTcs.SetResult();
                };
                await streamingLogTcs;
#if UNITY_2020_1_OR_NEWER
                if (webRequest.result != UnityWebRequest.Result.Success)
#else
                    if (!string.IsNullOrEmpty(webRequest.error))
#endif
                {
                    streamingLog = "INIT|0";
                }
                else
                {
                    streamingLog = webRequest.downloadHandler.text;
                }
            }
            //添加streaming文件CRC信息
            Dictionary<string, uint> streamingCRC = new Dictionary<string, uint>();
            string[] streamingLogData = streamingLog.Split('\n');
            for (int i = 1; i < streamingLogData.Length; i++)
            {
                string streamingDataLine = streamingLogData[i];
                if (!string.IsNullOrWhiteSpace(streamingDataLine))
                {
                    string[] info = streamingDataLine.Split('|');
                    uint crc = uint.Parse(info[2]);
                    if (!updateBundleDataInfo.PackageCRCDictionary[bundlePackageName].ContainsKey(info[0]))
                    {
                        updateBundleDataInfo.PackageCRCDictionary[bundlePackageName].Add(info[0], crc);
                    }
                    streamingCRC.Add(info[0], crc);
                }
            }
            //添加本地文件CRC信息
            for (int i = 1; i < localVersionData.Length; i++)
            {
                string localVersionDataLine = localVersionData[i];
                if (!string.IsNullOrWhiteSpace(localVersionDataLine))
                {
                    string[] info = localVersionDataLine.Split('|');
                    if (updateBundleDataInfo.PackageCRCDictionary[bundlePackageName].ContainsKey(info[0]))
                    {
                        updateBundleDataInfo.PackageCRCDictionary[bundlePackageName][info[0]] = uint.Parse(info[2]);
                    }
                    else
                    {
                        updateBundleDataInfo.PackageCRCDictionary[bundlePackageName].Add(info[0], uint.Parse(info[2]));
                    }
                }
            }
            //创建最后需要返回的数据
            Dictionary<string, long> needUpdateBundles = new Dictionary<string, long>();
            for (int i = 1; i < remoteVersionData.Length; i++)
            {
                string remoteVersionDataLine = remoteVersionData[i];
                if (!string.IsNullOrWhiteSpace(remoteVersionDataLine))
                {
                    string[] info = remoteVersionDataLine.Split('|');
                    if (updateBundleDataInfo.PackageCRCDictionary[bundlePackageName].TryGetValue(info[0], out uint crc))
                    {
                        if (crc == uint.Parse(info[2]))
                        {
                            if (streamingCRC.TryGetValue(info[0], out uint streamingCrc))
                            {
                                if (crc != streamingCrc)
                                {
                                    if (!File.Exists(Path.Combine(AssetComponentConfig.HotfixPath, bundlePackageName, info[0])))
                                    {
                                        needUpdateBundles.Add(info[0], long.Parse(info[1]));
                                    }
                                }
                            }
                            else
                            {
                                if (!File.Exists(Path.Combine(AssetComponentConfig.HotfixPath, bundlePackageName, info[0])))
                                {
                                    needUpdateBundles.Add(info[0], long.Parse(info[1]));
                                }
                            }
                            continue;
                        }
                    }
                    needUpdateBundles.Add(info[0], long.Parse(info[1]));
                }
            }
            updateBundleDataInfo.PackageNeedUpdateBundlesInfos.Add(bundlePackageName, needUpdateBundles);
            foreach (long needUpdateBundleSize in needUpdateBundles.Values)
            {
                updateBundleDataInfo.NeedUpdateSize += needUpdateBundleSize;
                updateBundleDataInfo.NeedDownLoadBundleCount++;
            }
        }
        
        /// <summary>
        /// 获取所有需要更新的Bundle的文件(计算文件CRC)
        /// </summary>
        private static async ETTask CalcNeedUpdateBundleFileCRC(UpdateBundleDataInfo updateBundleDataInfo, string bundlePackageName, string remoteVersionLog, string localVersionLog)
        {
            string[] remoteVersionData = remoteVersionLog.Split('\n');
            string[] localVersionData = localVersionLog.Split('\n');
            int remoteVersion = int.Parse(remoteVersionData[0].Split('|')[1]);
            int localVersion = int.Parse(localVersionData[0].Split('|')[1]);
            updateBundleDataInfo.PackageToVersion.Add(bundlePackageName, new int[2]{localVersion, remoteVersion});
            if (localVersion > remoteVersion)
            {
                AssetLogHelper.LogError("本地版本号优先与远程版本号 " + localVersion + ">" + remoteVersion + "\n"
                + "localBundleTime: " + localVersionData[0].Split('|')[0] + "\n"
                + "remoteBundleTime: " + remoteVersionData[0].Split('|')[0] + "\n"
                + "Note: 发送了版本回退或者忘了累进版本号");
            }
            //创建最后需要返回的数据
            Dictionary<string, long> needUpdateBundles = new Dictionary<string, long>();
            _checkCount = remoteVersionData.Length - 1;
            Queue<int> loopQueue = new Queue<int>();
            //loopSize依据资源大小调节, 如果资源太大loopSize也调很大, 会撑爆内存, 调大只会节约一点点时间(目的是如果有进度条每个循环更新一下进度条)
            int loopSize = 5;
            for (int i = 0; i < _checkCount / loopSize; i++)
            {
                loopQueue.Enqueue(loopSize);
            }
            if (_checkCount % loopSize > 0)
            {
                loopQueue.Enqueue(_checkCount % loopSize);
            }
            int loop = loopQueue.Count;
            for (int i = 0; i < loop; i++)
            {
                _checkCount = loopQueue.Dequeue();
                int count = _checkCount;
                ETTask finishTcs = ETTask.Create();
                for (int j = 0; j < count; j++)
                {
                    CheckFileCRC(remoteVersionData[i * loopSize + j + 1], bundlePackageName, needUpdateBundles, finishTcs).Coroutine();
                }
                await finishTcs;
                _checkCount = 0;
            }
            updateBundleDataInfo.PackageNeedUpdateBundlesInfos.Add(bundlePackageName, needUpdateBundles);
            foreach (long needUpdateBundleSize in needUpdateBundles.Values)
            {
                updateBundleDataInfo.NeedUpdateSize += needUpdateBundleSize;
                updateBundleDataInfo.NeedDownLoadBundleCount++;
            }
        }

        private static int _checkCount = 0;

        private static async ETTask CheckFileCRC(string remoteVersionDataLine, string bundlePackageName, Dictionary<string, long> needUpdateBundles, ETTask finishTcs)
        {
            if (!string.IsNullOrWhiteSpace(remoteVersionDataLine))
            {
                string[] info = remoteVersionDataLine.Split('|');
                //如果文件不存在直接加入更新
                string filePath = BundleFileExistPath(bundlePackageName, info[0]);
                uint fileCRC32 = await VerifyHelper.GetFileCRC32(filePath);
                //判断是否和远程一样, 不一样直接加入更新
                if (uint.Parse(info[2]) != fileCRC32)
                {
                    needUpdateBundles.Add(info[0], long.Parse(info[1]));
                }
            }
            _checkCount--;
            if (_checkCount <= 0)
            {
                finishTcs.SetResult();
            }
        }
        
        /// <summary>
        /// 下载更新
        /// </summary>
        public static async ETTask DownLoadUpdate(UpdateBundleDataInfo updateBundleDataInfo)
        {
            if (AssetComponentConfig.AssetLoadMode != AssetLoadMode.Build)
            {
                AssetLogHelper.LogError("AssetLoadMode != AssetLoadMode.Build 不需要更新");
                return;
            }
            Dictionary<string, Queue<DownLoadTask>> packageDownLoadTask = new Dictionary<string, Queue<DownLoadTask>>();
            ETTask downLoading = ETTask.Create();
            //准备需要下载的内容的初始化信息
            foreach (var packageNeedUpdateBundlesInfo in updateBundleDataInfo.PackageNeedUpdateBundlesInfos)
            {
                Queue<DownLoadTask> downLoadTaskQueue = new Queue<DownLoadTask>();
                string packageName = packageNeedUpdateBundlesInfo.Key;
                packageDownLoadTask.Add(packageName, downLoadTaskQueue);
                string downLoadPackagePath = Path.Combine(AssetComponentConfig.HotfixPath, packageName);
                if (!Directory.Exists(downLoadPackagePath))
                {
                    Directory.CreateDirectory(downLoadPackagePath);
                }
                foreach (var updateBundlesInfo in packageNeedUpdateBundlesInfo.Value)
                {
                    DownLoadTask downLoadTask = new DownLoadTask();
                    downLoadTask.UpdateBundleDataInfo = updateBundleDataInfo;
                    downLoadTask.DownLoadingKey = downLoading;
                    downLoadTask.PackageDownLoadTask = packageDownLoadTask;
                    downLoadTask.PackegName = packageName;
                    downLoadTask.DownLoadPackagePath = downLoadPackagePath;
                    downLoadTask.FileName = updateBundlesInfo.Key;
                    downLoadTask.FileSize = updateBundlesInfo.Value;
                    downLoadTaskQueue.Enqueue(downLoadTask);
                }
            }
            //开启下载
            for (int i = 0; i < AssetComponentConfig.MaxDownLoadCount; i++)
            {
                foreach (Queue<DownLoadTask> downLoadTaskQueue in packageDownLoadTask.Values)
                {
                    if (downLoadTaskQueue.Count > 0)
                    {
                        downLoadTaskQueue.Dequeue().DownLoad().Coroutine();
                        break;
                    }
                }
            }
            await downLoading;
            //下载完成关闭CRCLog文件
            foreach (StreamWriter sw in updateBundleDataInfo.PackageCRCFile.Values)
            {
                sw.Close();
                sw.Dispose();
            }
            updateBundleDataInfo.PackageCRCFile.Clear();
            //所有分包都下载完成了就处理分包的Log文件
            foreach (string packageName in updateBundleDataInfo.PackageNeedUpdateBundlesInfos.Keys)
            {
                byte[] fileLogsData = await DownloadBundleHelper.DownloadDataByUrl(Path.Combine(AssetComponentConfig.BundleServerUrl, packageName, "FileLogs.txt"));
                byte[] dependLogsData = await DownloadBundleHelper.DownloadDataByUrl(Path.Combine(AssetComponentConfig.BundleServerUrl, packageName, "DependLogs.txt"));
                byte[] versionLogsData = await DownloadBundleHelper.DownloadDataByUrl(Path.Combine(AssetComponentConfig.BundleServerUrl, packageName, "VersionLogs.txt"));
                if (fileLogsData == null || dependLogsData == null || versionLogsData == null)
                {
                    AssetLogHelper.LogError("获取Log表失败, PackageName: " + packageName);
                    continue;
                }
                CreateUpdateLogFile(Path.Combine(AssetComponentConfig.HotfixPath, packageName, "FileLogs.txt"),
                    System.Text.Encoding.UTF8.GetString(fileLogsData));
                CreateUpdateLogFile(Path.Combine(AssetComponentConfig.HotfixPath, packageName, "DependLogs.txt"),
                    System.Text.Encoding.UTF8.GetString(dependLogsData));
                CreateUpdateLogFile(Path.Combine(AssetComponentConfig.HotfixPath, packageName, "VersionLogs.txt"),
                    System.Text.Encoding.UTF8.GetString(versionLogsData));
            }
            AssetLogHelper.LogError("下载完成");
        }
        
    }


    public class DownLoadTask
    {
        public UpdateBundleDataInfo UpdateBundleDataInfo;

        public ETTask DownLoadingKey;

        public Dictionary<string, Queue<DownLoadTask>> PackageDownLoadTask;
        
        /// <summary>
        /// 下载的资源分包名称
        /// </summary>
        public string PackegName;

        /// <summary>
        /// 分包所在路径
        /// </summary>
        public string DownLoadPackagePath;

        /// <summary>
        /// 下载的文件的名称
        /// </summary>
        public string FileName;

        /// <summary>
        /// 下载的文件的大小
        /// </summary>
        public long FileSize;

        public async ETTask DownLoad()
        {
            string url = Path.Combine(AssetComponentConfig.BundleServerUrl, PackegName, UnityWebRequest.EscapeURL(FileName));
            byte[] data = await DownloadBundleHelper.DownloadDataByUrl(url);
            using (FileStream fs = new FileStream(Path.Combine(DownLoadPackagePath, FileName), FileMode.Create))
            {
                //大于2M用异步
                if (data.Length > 2097152)
                {
                    await fs.WriteAsync(data, 0, data.Length);
                }
                else
                {
                    fs.Write(data, 0, data.Length);
                }
                fs.Close();
            }
            UpdateBundleDataInfo.AddCRCFileInfo(PackegName, FileName, VerifyHelper.GetCRC32(data));
            UpdateBundleDataInfo.FinishUpdateSize += data.Length;
            UpdateBundleDataInfo.FinishDownLoadBundleCount++;
            foreach (Queue<DownLoadTask> downLoadTaskQueue in PackageDownLoadTask.Values)
            {
                if (downLoadTaskQueue.Count > 0)
                {
                    downLoadTaskQueue.Dequeue().DownLoad().Coroutine();
                    return;
                }
            }
            //说明下载完成了
            if (UpdateBundleDataInfo.FinishDownLoadBundleCount < UpdateBundleDataInfo.NeedDownLoadBundleCount)
            {
                return;
            }
            UpdateBundleDataInfo.FinishUpdate = true;
            DownLoadingKey.SetResult();
        }
    }
}