using System.Collections.Generic;
using UnityEngine;
using ET;

namespace BM
{
    public class LoadSceneHandler : LoadHandlerBase
    {
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
        
        public LoadSceneHandler(string scenePath, string bundlePackageName)
        {
            AssetPath = scenePath;
            UniqueId = HandlerIdHelper.GetUniqueId();
            BundlePackageName = bundlePackageName;
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                //Develop模式直接返回
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
        /// 同步加载场景资源所需的的AssetBundle包
        /// </summary>
        public void LoadSceneBundle()
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
        }
        
        /// <summary>
        /// 异步加载场景的Bundle
        /// </summary>
        public async ETTask LoadSceneBundleAsync(ETTask finishTask)
        {
            //计算出所有需要加载的Bundle包的总数
            RefLoadFinishCount = _loadDepends.Count + _loadDependFiles.Count + 1;
            _loadFile.OpenProgress();
            LoadAsyncLoader(_loadFile, finishTask).Coroutine();
            for (int i = 0; i < _loadDepends.Count; i++)
            {
                _loadDepends[i].OpenProgress();
                LoadAsyncLoader(_loadDepends[i], finishTask).Coroutine();
            }
            for (int i = 0; i < _loadDependFiles.Count; i++)
            {
                _loadDependFiles[i].OpenProgress();
                LoadAsyncLoader(_loadDependFiles[i], finishTask).Coroutine();
            }
            await finishTask;
            FileAssetBundle = _loadFile.AssetBundle;
        }

        /// <summary>
        /// 获取场景AssetBundle加载的进度
        /// </summary>
        public float GetProgress()
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                //Develop模式无法异步加载
                return 100;
            }
            if (UnloadFinish)
            {
                return 0;
            }
            float progress = 0;
            int loadCount = 1;
            progress += _loadFile.GetProgress();
            for (int i = 0; i < _loadDepends.Count; i++)
            {
                progress += _loadDepends[i].GetProgress();
                loadCount++;
            }
            for (int i = 0; i < _loadDependFiles.Count; i++)
            {
                progress += _loadDependFiles[i].GetProgress();
                loadCount++;
            }
            return progress / loadCount;
        }
        
        /// <summary>
        /// 卸载场景的AssetBundle包
        /// </summary>
        protected override void ClearAsset()
        {
            FileAssetBundle = null;
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
        }
    }
}