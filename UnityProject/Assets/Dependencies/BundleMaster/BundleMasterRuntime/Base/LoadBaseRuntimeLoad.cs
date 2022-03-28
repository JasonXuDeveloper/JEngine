using System.Collections.Generic;
using UnityEngine;
using ET;

namespace BM
{
    public partial class LoadBase
    {
        /// <summary>
        /// 引用计数
        /// </summary>
        private int _refCount = 0;

        /// <summary>
        /// AssetBundle加载的状态
        /// </summary>
        private LoadState _loadState = LoadState.NoLoad;

        /// <summary>
        /// 加载请求索引
        /// </summary>
        private AssetBundleCreateRequest _assetBundleCreateRequest;
    
        /// <summary>
        /// AssetBundle的引用
        /// </summary>
        public AssetBundle AssetBundle;

        /// <summary>
        /// 加载完成后需要执行的Task
        /// </summary>
        private List<ETTask> _loadFinishTasks = new List<ETTask>();

        /// <summary>
        /// 需要统计进度
        /// </summary>
        private WebLoadProgress _loadProgress = null;

        private void AddRefCount()
        {
            _refCount++;
            if (_refCount == 1 && _loadState == LoadState.Finish)
            {
                AssetComponent.SubPreUnLoadPool(this);
            }
        }

        internal void SubRefCount()
        {
            _refCount--;
            if (_loadState == LoadState.NoLoad)
            {
                AssetLogHelper.LogError("资源未被加载，引用不可能减少\n" + FilePath);
                return;
            }
            if (_loadState == LoadState.Loading)
            {
                AssetLogHelper.Log("资源加载中，等加载完成后再进入卸载逻辑\n" + FilePath);
                return;
            }
            if (_refCount <= 0)
            {
                //需要进入预卸载池等待卸载
                AssetComponent.AddPreUnLoadPool(this);
            }
        }

        internal void LoadAssetBundle(string bundlePackageName)
        {
            AddRefCount();
            if (_loadState == LoadState.Finish)
            {
                return;
            }
            if (_loadState == LoadState.Loading)
            {
                AssetLogHelper.LogError("同步加载了正在异步加载的资源, 打断异步加载资源会导致所有异步加载的资源都立刻同步加载出来。资源名: " + FilePath + 
                                        "\nAssetBundle包名: " + AssetBundleName);
                if (_assetBundleCreateRequest != null)
                {
                    AssetBundle = _assetBundleCreateRequest.assetBundle;
                    return;
                }
            }
            string assetBundlePath = AssetComponent.BundleFileExistPath(bundlePackageName, AssetBundleName);
            byte[] data;
            if (AssetComponent.BundleNameToRuntimeInfo[bundlePackageName].Encrypt)
            {
                data = VerifyHelper.GetDecryptData(assetBundlePath, AssetComponent.BundleNameToRuntimeInfo[bundlePackageName].SecretKey);
            }
            else
            {
                data = VerifyHelper.GetDecryptData(assetBundlePath);
            }
            AssetBundle = AssetBundle.LoadFromMemory(data);
            _loadState = LoadState.Finish;
            for (int i = 0; i < _loadFinishTasks.Count; i++)
            {
                _loadFinishTasks[i].SetResult();
            }
            _loadFinishTasks.Clear();
        }
    
        internal async ETTask LoadAssetBundleAsync(ETTask tcs, string bundlePackageName)
        {
            AddRefCount();
            if (_loadState == LoadState.Finish)
            {
                tcs.SetResult();
                return;
            }
            if (_loadState == LoadState.Loading)
            {
                _loadFinishTasks.Add(tcs);
                return;
            }
            _loadFinishTasks.Add(tcs);
            _loadState = LoadState.Loading;
            string assetBundlePath = AssetComponent.BundleFileExistPath(bundlePackageName, AssetBundleName);
            byte[] data;
            if (AssetComponent.BundleNameToRuntimeInfo[bundlePackageName].Encrypt)
            {
                data = await VerifyHelper.GetDecryptDataAsync(assetBundlePath, _loadProgress, AssetComponent.BundleNameToRuntimeInfo[bundlePackageName].SecretKey);
            }
            else
            {
                data = await VerifyHelper.GetDecryptDataAsync(assetBundlePath, _loadProgress);
            }
            LoadDataFinish(data);
        }

        /// <summary>
        /// Data加载完成后执行的
        /// </summary>
        private void LoadDataFinish(byte[] data)
        {
            if (_loadState == LoadState.Finish)
            {
                return;
            }
            _assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(data);
            _assetBundleCreateRequest.completed += operation =>
            {
                AssetBundle = _assetBundleCreateRequest.assetBundle;
                for (int i = 0; i < _loadFinishTasks.Count; i++)
                {
                    _loadFinishTasks[i].SetResult();
                }
                _loadFinishTasks.Clear();
                _loadState = LoadState.Finish;
                //判断是否还需要
                if (_refCount <= 0)
                {
                    AssetComponent.AddPreUnLoadPool(this);
                }
            };
        }
        
        /// <summary>
        /// 强制加载完成
        /// </summary>
        internal void ForceLoadFinish(string bundlePackageName)
        {
            if (_loadState == LoadState.Finish)
            {
                return;
            }
            if (_assetBundleCreateRequest != null)
            {
                AssetLogHelper.LogError("触发强制加载, 打断异步加载资源会导致所有异步加载的资源都立刻同步加载出来。资源名: " + FilePath + 
                                       "\nAssetBundle包名: " + AssetBundleName);
                AssetBundle = _assetBundleCreateRequest.assetBundle;
                return;
            }
            string assetBundlePath = AssetComponent.BundleFileExistPath(bundlePackageName, AssetBundleName);
            byte[] data;
            if (AssetComponent.BundleNameToRuntimeInfo[bundlePackageName].Encrypt)
            {
                data = VerifyHelper.GetDecryptData(assetBundlePath, AssetComponent.BundleNameToRuntimeInfo[bundlePackageName].SecretKey);
            }
            else
            {
                data = VerifyHelper.GetDecryptData(assetBundlePath);
            }
            AssetBundle = AssetBundle.LoadFromMemory(data);
            for (int i = 0; i < _loadFinishTasks.Count; i++)
            {
                _loadFinishTasks[i].SetResult();
            }
            _loadFinishTasks.Clear();
            _loadState = LoadState.Finish;
            //判断是否还需要
            if (_refCount <= 0)
            {
                AssetComponent.AddPreUnLoadPool(this);
            }
        }
        
        /// <summary>
        /// 打开进度统计
        /// </summary>
        internal void OpenProgress()
        {
            _loadProgress = new WebLoadProgress();
        }
        
        internal float GetProgress()
        {
            if (_loadProgress == null)
            {
                AssetLogHelper.LogError("未打开进度统计无法获取进度");
                return 0;
            }
            if (_loadState == LoadState.Finish)
            {
                return 1;
            }
            if (_loadState == LoadState.NoLoad)
            {
                return 0;
            }
            if (_assetBundleCreateRequest == null)
            {
                return _loadProgress.GetWebProgress() / 2;
            }
            else
            {
                return (_assetBundleCreateRequest.progress + 1.0f) / 2;
            }
        }
        
    }

    /// <summary>
    /// AssetBundle加载的状态
    /// </summary>
    internal enum LoadState
    {
        NoLoad = 0,
        Loading = 1,
        Finish = 2
    }
}
