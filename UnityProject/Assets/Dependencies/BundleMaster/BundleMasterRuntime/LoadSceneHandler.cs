using System.Collections.Generic;
using UnityEngine;
using ET;

namespace BM
{
    public class LoadSceneHandler : LoadHandlerBase
    {
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
            //说明是组里的资源
            string groupPath = GroupAssetHelper.IsGroupAsset(AssetPath, AssetComponent.BundleNameToRuntimeInfo[BundlePackageName].LoadGroupDicKey);
            if (groupPath != null)
            {
                //先找到对应加载的LoadGroup类
                if (!AssetComponent.BundleNameToRuntimeInfo[BundlePackageName].LoadGroupDic.TryGetValue(groupPath, out LoadGroup loadGroup))
                {
                    AssetLogHelper.LogError("没有找到资源组: " + groupPath);
                    return;
                }
                _loadBase = loadGroup;
                //需要记录loadGroup的依赖
                for (int i = 0; i < loadGroup.DependFileName.Count; i++)
                {
                    string dependFile = loadGroup.DependFileName[i];
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
                    if (AssetComponent.BundleNameToRuntimeInfo[BundlePackageName].LoadGroupDic.TryGetValue(dependFile, out LoadGroup loadDependGroup))
                    {
                        _loadDependGroups.Add(loadDependGroup);
                        continue;
                    }
                    AssetLogHelper.LogError("场景依赖的资源没有找到对应的类: " + dependFile);
                }
                return;
            }
            //先找到对应加载的LoadFile类
            if (!AssetComponent.BundleNameToRuntimeInfo[BundlePackageName].LoadFileDic.TryGetValue(AssetPath, out LoadFile loadFile))
            {
                AssetLogHelper.LogError("没有找到资源: " + AssetPath);
                return;
            }
            _loadBase = loadFile;
            //需要记录loadFile的依赖
            for (int i = 0; i < loadFile.DependFileName.Length; i++)
            {
                string dependFile = loadFile.DependFileName[i];
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
                if (AssetComponent.BundleNameToRuntimeInfo[BundlePackageName].LoadGroupDic.TryGetValue(dependFile, out LoadGroup loadDependGroup))
                {
                    _loadDependGroups.Add(loadDependGroup);
                    continue;
                }
                AssetLogHelper.LogError("场景依赖的资源没有找到对应的类: " + dependFile);
            }
            
        }

        /// <summary>
        /// 同步加载场景资源所需的的AssetBundle包
        /// </summary>
        public void LoadSceneBundle()
        {
            _loadBase.LoadAssetBundle(BundlePackageName);
            for (int i = 0; i < _loadDepends.Count; i++)
            {
                _loadDepends[i].LoadAssetBundle(BundlePackageName);
            }
            for (int i = 0; i < _loadDependFiles.Count; i++)
            {
                _loadDependFiles[i].LoadAssetBundle(BundlePackageName);
            }
            for (int i = 0; i < _loadDependGroups.Count; i++)
            {
                _loadDependGroups[i].LoadAssetBundle(BundlePackageName);
            }
            FileAssetBundle = _loadBase.AssetBundle;
        }
        
        /// <summary>
        /// 异步加载场景的Bundle
        /// </summary>
        public async ETTask LoadSceneBundleAsync(ETTask finishTask)
        {
            //计算出所有需要加载的Bundle包的总数
            RefLoadFinishCount = _loadDepends.Count + _loadDependFiles.Count + _loadDependGroups.Count + 1;
            _loadBase.OpenProgress();
            LoadAsyncLoader(_loadBase, finishTask).Coroutine();
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
            for (int i = 0; i < _loadDependGroups.Count; i++)
            {
                _loadDependGroups[i].OpenProgress();
                LoadAsyncLoader(_loadDependGroups[i], finishTask).Coroutine();
            }
            await finishTask;
            FileAssetBundle = _loadBase.AssetBundle;
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
            progress += _loadBase.GetProgress();
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
            for (int i = 0; i < _loadDependGroups.Count; i++)
            {
                progress += _loadDependGroups[i].GetProgress();
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
            foreach (LoadGroup loadDependGroups in _loadDependGroups)
            {
                loadDependGroups.SubRefCount();
            }
            _loadDependGroups.Clear();
            _loadBase.SubRefCount();
            _loadBase = null;
        }
    }
}