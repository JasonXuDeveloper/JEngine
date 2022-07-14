using System.Collections.Generic;
using System.Linq;

namespace BM
{
    public static partial class AssetComponent
    {
        /// <summary>
        /// 一个卸载周期的循环时间
        /// </summary>
        private static float _unLoadCirculateTime = 5.0f;
        
        /// <summary>
        /// 预卸载池
        /// </summary>
        private static readonly Dictionary<string, LoadBase> PreUnLoadPool = new Dictionary<string, LoadBase>();
        
        /// <summary>
        /// 真卸载池
        /// </summary>
        private static readonly Dictionary<string, LoadBase> TrueUnLoadPool = new Dictionary<string, LoadBase>();
        
        /// <summary>
        /// 通过路径卸载(场景资源不可以通过路径卸载)
        /// </summary>
        public static void UnLoadByPath(string assetPath, string bundlePackageName = null)
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                AssetLogHelper.Log("AssetLoadMode = Develop 不需要卸载");
                return;
            }
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError("没有找到这个分包: " + bundlePackageName);
                return;
            }
            if (!bundleRuntimeInfo.AllAssetLoadHandler.TryGetValue(assetPath, out LoadHandler loadHandler))
            {
                AssetLogHelper.LogError("卸载没有找到这个资源的Handler: " + assetPath);
                return;
            }
            loadHandler.UnLoad();
        }
        
        public static void UnLoad(LoadHandlerBase loadHandler)
        {
            loadHandler.UnLoad();
        }
        
        /// <summary>
        /// 卸载所有没卸载的资源
        /// </summary>
        public static void UnLoadAllAssets()
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                AssetLogHelper.Log("AssetLoadMode = Develop 不需要卸载");
                return;
            }
            BundleRuntimeInfo[] bundleRuntimeInfos = BundleNameToRuntimeInfo.Values.ToArray();
            for (int i = 0; i < bundleRuntimeInfos.Length; i++)
            {
                LoadHandlerBase[] loadHandlers = bundleRuntimeInfos[i].UnLoadHandler.Values.ToArray();
                for (int j = 0; j < loadHandlers.Length; j++)
                {
                    loadHandlers[j].UnLoad();
                }
                bundleRuntimeInfos[i].AllAssetLoadHandler.Clear();
            }
        }

        /// <summary>
        /// 卸载一个分包内没卸载的资源
        /// </summary>
        public static void UnLoadPackageAssets(string bundlePackageNames)
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                AssetLogHelper.Log("AssetLoadMode = Develop 不需要卸载");
                return;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageNames, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.Log("没有找到这个初始化的分包: " + bundlePackageNames);
                return;
            }
            LoadHandlerBase[] loadHandlers = bundleRuntimeInfo.UnLoadHandler.Values.ToArray();
            for (int i = 0; i < loadHandlers.Length; i++)
            {
                loadHandlers[i].UnLoad();
            }
            bundleRuntimeInfo.AllAssetLoadHandler.Clear();
        }
        
        /// <summary>
        /// 卸载所有分包内没有返回Handler但是未卸载的资源(场景资源一定有Handler)
        /// </summary>
        public static void UnLoadAllNoHandlerAssets()
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                AssetLogHelper.Log("AssetLoadMode = Develop 不需要卸载");
                return;
            }
            BundleRuntimeInfo[] bundleRuntimeInfos = BundleNameToRuntimeInfo.Values.ToArray();
            for (int i = 0; i < bundleRuntimeInfos.Length; i++)
            {
                LoadHandler[] loadHandlers = bundleRuntimeInfos[i].AllAssetLoadHandler.Values.ToArray();
                for (int j = 0; j < loadHandlers.Length; j++)
                {
                    loadHandlers[j].UnLoad();
                }
                bundleRuntimeInfos[i].AllAssetLoadHandler.Clear();
            }
        }
        
        /// <summary>
        /// 卸载一个分包内没有返回Handler但是未卸载的资源(场景资源一定有Handler)
        /// </summary>
        public static void UnLoadPackageNoHandlerAssets(string bundlePackageNames)
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                AssetLogHelper.Log("AssetLoadMode = Develop 不需要卸载");
                return;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageNames, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.Log("没有找到这个初始化的分包: " + bundlePackageNames);
                return;
            }
            LoadHandler[] loadHandlers = bundleRuntimeInfo.AllAssetLoadHandler.Values.ToArray();
            for (int i = 0; i < loadHandlers.Length; i++)
            {
                loadHandlers[i].UnLoad();
            }
            bundleRuntimeInfo.AllAssetLoadHandler.Clear();
        }
        
        /// <summary>
        /// 卸载所有分包内返回了Handler但是没有卸载的资源
        /// </summary>
        public static void UnLoadAllHaveHandlerAssets()
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                AssetLogHelper.Log("AssetLoadMode = Develop 不需要卸载");
                return;
            }
            BundleRuntimeInfo[] bundleRuntimeInfos = BundleNameToRuntimeInfo.Values.ToArray();
            for (int i = 0; i < bundleRuntimeInfos.Length; i++)
            {
                LoadHandlerBase[] loadHandlers = bundleRuntimeInfos[i].UnLoadHandler.Values.ToArray();
                for (int j = 0; j < loadHandlers.Length; j++)
                {
                    if (!loadHandlers[j].HaveHandler)
                    {
                        loadHandlers[j].UnLoad();
                    }
                }
            }
        }
        
        /// <summary>
        /// 卸载一个分包内返回了Handler但是没有卸载的资源
        /// </summary>
        public static void UnLoadPackageHaveHandlerAssets(string bundlePackageNames)
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                AssetLogHelper.Log("AssetLoadMode = Develop 不需要卸载");
                return;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageNames, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.Log("没有找到这个初始化的分包: " + bundlePackageNames);
                return;
            }
            LoadHandlerBase[] loadHandlers = bundleRuntimeInfo.UnLoadHandler.Values.ToArray();
            for (int i = 0; i < loadHandlers.Length; i++)
            {
                if (!loadHandlers[i].HaveHandler)
                {
                    loadHandlers[i].UnLoad();
                }
            }
        }
        
        /// <summary>
        /// 添加进预卸载池
        /// </summary>
        internal static void AddPreUnLoadPool(LoadBase loadBase)
        {
            PreUnLoadPool.Add(loadBase.AssetBundleName, loadBase);
        }

        /// <summary>
        /// 从预卸载池里面取出
        /// </summary>
        internal static void SubPreUnLoadPool(LoadBase loadBase)
        {
            if (PreUnLoadPool.ContainsKey(loadBase.AssetBundleName))
            {
                PreUnLoadPool.Remove(loadBase.AssetBundleName);
            }
            if (TrueUnLoadPool.ContainsKey(loadBase.AssetBundleName))
            {
                TrueUnLoadPool.Remove(loadBase.AssetBundleName);
            }
        }
        
        /// <summary>
        /// 强制卸载所有待卸载的资源，注意是待卸载
        /// </summary>
        public static void ForceUnLoadAll()
        {
            TrueUnLoadPool.Clear();
            foreach (var loadBase in PreUnLoadPool)
            {
                loadBase.Value.UnLoad();
            }
            PreUnLoadPool.Clear();
        }
        
        /// <summary>
        /// 自动添加到真卸载池子
        /// </summary>
        private static void AutoAddToTrueUnLoadPool()
        {
            //卸载之前就已经添加到真卸载池的资源
            foreach (var loadBase in TrueUnLoadPool)
            {
                PreUnLoadPool.Remove(loadBase.Key);
                loadBase.Value.UnLoad();
            }
            TrueUnLoadPool.Clear();
            foreach (var loadBase in PreUnLoadPool)
            {
                TrueUnLoadPool.Add(loadBase.Key, loadBase.Value);
            }
        }

        /// <summary>
        /// 取消一个分包的初始化
        /// </summary>
        public static void UnInitialize(string bundlePackageName)
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                AssetLogHelper.Log("AssetLoadMode = Develop 无法取消初始化分包");
                return;
            }
            UnInitializePackage(bundlePackageName);
            ForceUnLoadAll();
        }
        
        /// <summary>
        /// 取消所有分包的初始化
        /// </summary>
        public static void UnInitializeAll()
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                AssetLogHelper.Log("AssetLoadMode = Develop 无法取消初始化分包");
                return;
            }
            string[] bundlePackageNames = BundleNameToRuntimeInfo.Keys.ToArray();
            for (int i = 0; i < bundlePackageNames.Length; i++)
            {
                UnInitializePackage(bundlePackageNames[i]);
            }
            ForceUnLoadAll();
        }

        private static void UnInitializePackage(string bundlePackageName)
        {
            if (!BundleNameToRuntimeInfo.ContainsKey(bundlePackageName))
            {
                AssetLogHelper.LogError("找不到要取消初始化的分包: " + bundlePackageName);
                return;
            }
            BundleRuntimeInfo bundleRuntimeInfo = BundleNameToRuntimeInfo[bundlePackageName];
            LoadHandlerBase[] loadHandlers = bundleRuntimeInfo.UnLoadHandler.Values.ToArray();
            for (int i = 0; i < loadHandlers.Length; i++)
            {
                loadHandlers[i].UnLoad();
            }
            bundleRuntimeInfo.AllAssetLoadHandler.Clear();
            if (bundleRuntimeInfo.Shader != null)
            {
                bundleRuntimeInfo.Shader.Unload(true);
            }
            BundleNameToRuntimeInfo.Remove(bundlePackageName);
        }
        
    }
}