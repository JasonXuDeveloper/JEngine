using System.Collections.Generic;
using UnityEngine;
using ET;

namespace BM
{
    public class LoadHandler : LoadHandlerBase
    {
        /// <summary>
        /// 是否进池
        /// </summary>
        private bool isPool;
        
        /// <summary>
        /// 加载出来的资源
        /// </summary>
        public UnityEngine.Object Asset = null;
        
        /// <summary>
        /// File文件AssetBundle的引用
        /// </summary>
        public AssetBundle FileAssetBundle;
    
        /// <summary>
        /// 资源所在的File包
        /// </summary>
        private LoadFile _loadFile = null;
    
        /// <summary>
        /// 依赖的Bundle包
        /// </summary>
        private List<LoadDepend> _loadDepends = new List<LoadDepend>();
    
        /// <summary>
        /// 依赖的其它File包
        /// </summary>
        private List<LoadFile> _loadDependFiles = new List<LoadFile>();

        /// <summary>
        /// 加载的状态
        /// </summary>
        internal LoadState LoadState = LoadState.NoLoad;
        
        /// <summary>
        /// 异步等待加载的Task
        /// </summary>
        internal List<ETTask> AwaitEtTasks = new List<ETTask>();


        internal LoadHandler(bool isPool)
        {
            this.isPool = isPool;
        }
        
        internal void Init(string assetPath, string bundlePackageName, bool haveHandler)
        {
            AssetPath = assetPath;
            UniqueId = HandlerIdHelper.GetUniqueId();
            BundlePackageName = bundlePackageName;
            LoadState = LoadState.NoLoad;
            UnloadFinish = false;
            HaveHandler = haveHandler;
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                //Develop模式直接返回就行
                return;
            }
            //先找到对应加载的LoadFile类
            if (!AssetComponent.BundleNameToRuntimeInfo[BundlePackageName].LoadFileDic.TryGetValue(AssetPath, out LoadFile loadFile))
            {
                AssetLogHelper.LogError("没有找到资源: " + AssetPath);
                return;
            }
            _loadFile = loadFile;
            //需要记录loadFile的依赖
            for (int i = 0; i < _loadFile.DependFileName.Length; i++)
            {
                string dependFile = _loadFile.DependFileName[i];
                if (AssetComponent.BundleNameToRuntimeInfo[BundlePackageName].LoadDependDic.TryGetValue(dependFile, out LoadDepend loadDepend))
                {
                    _loadDepends.Add(loadDepend);
                    continue;
                }
                if (AssetComponent.BundleNameToRuntimeInfo[BundlePackageName].LoadFileDic.TryGetValue(dependFile, out LoadFile loadDependFile))
                {
                    _loadDependFiles.Add(loadDependFile);
                    continue;
                }
                AssetLogHelper.LogError("依赖的资源没有找到对应的类: " + dependFile);
            }
        }
    
        /// <summary>
        /// 同步加载所有的Bundle
        /// </summary>
        internal void Load()
        {
            _loadFile.LoadAssetBundle(BundlePackageName);
            for (int i = 0; i < _loadDepends.Count; i++)
            {
                _loadDepends[i].LoadAssetBundle(BundlePackageName);
            }
            for (int i = 0; i < _loadDependFiles.Count; i++)
            {
                _loadDependFiles[i].LoadAssetBundle(BundlePackageName);
            }
            FileAssetBundle = _loadFile.AssetBundle;
            LoadState = LoadState.Finish;
        }

        /// <summary>
        /// 异步加载所有的Bundle
        /// </summary>
        internal async ETTask LoadAsync()
        {
            LoadState = LoadState.Loading;
            //计算出所有需要加载的Bundle包的总数
            RefLoadFinishCount = _loadDepends.Count + _loadDependFiles.Count + 1;
            ETTask tcs = ETTask.Create(true);
            LoadAsyncLoader(_loadFile, tcs).Coroutine();
            for (int i = 0; i < _loadDepends.Count; i++)
            {
                LoadAsyncLoader(_loadDepends[i], tcs).Coroutine();
            }
            for (int i = 0; i < _loadDependFiles.Count; i++)
            {
                LoadAsyncLoader(_loadDependFiles[i], tcs).Coroutine();
            }
            await tcs;
            if (LoadState != LoadState.Finish)
            {
                LoadState = LoadState.Finish;
                FileAssetBundle = _loadFile.AssetBundle;
            }
        }
        
        /// <summary>
        /// 强制异步加载完成
        /// </summary>
        internal void ForceAsyncLoadFinish()
        {
            _loadFile.ForceLoadFinish(BundlePackageName);
            for (int i = 0; i < _loadDepends.Count; i++)
            {
                _loadDepends[i].ForceLoadFinish(BundlePackageName);
            }
            for (int i = 0; i < _loadDependFiles.Count; i++)
            {
                _loadDependFiles[i].ForceLoadFinish(BundlePackageName);
            }
            FileAssetBundle = _loadFile.AssetBundle;
            LoadState = LoadState.Finish;
        }
        
        /// <summary>
        /// 清理引用
        /// </summary>
        protected override void ClearAsset()
        {
            Asset = null;
            FileAssetBundle = null;
            LoadState = LoadState.NoLoad;
            foreach (LoadDepend loadDepends in _loadDepends)
            {
                loadDepends.SubRefCount();
            }
            _loadDepends.Clear();
            foreach (LoadFile loadDependFiles in _loadDependFiles)
            {
                loadDependFiles.SubRefCount();
            }
            _loadDependFiles.Clear();
            _loadFile.SubRefCount();
            _loadFile = null;
            //从缓存里取出进池
            if (!AssetComponent.BundleNameToRuntimeInfo.TryGetValue(BundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError("没要找到分包的信息: " + BundlePackageName);
                return;
            }
            if (HaveHandler)
            {
                if (!bundleRuntimeInfo.AllAssetLoadHandler.ContainsKey(AssetPath))
                {
                    AssetLogHelper.LogError("没要找到缓存的LoadHandler: " + AssetPath);
                    return;
                }
                bundleRuntimeInfo.AllAssetLoadHandler.Remove(AssetPath);
            }
            if (isPool)
            {
                //进池
                LoadHandlerFactory.EnterPool(this);
            }
        }
        
    }
}