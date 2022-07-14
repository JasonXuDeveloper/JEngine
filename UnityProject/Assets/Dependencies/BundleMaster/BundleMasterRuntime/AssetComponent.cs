using ET;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BM
{
    public static partial class AssetComponent
    {
        /// <summary>
        /// 同步加载(泛型)
        /// </summary>
        public static T Load<T>(string assetPath, string bundlePackageName = null) where T : UnityEngine.Object
        {
            T asset = null;
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
#if UNITY_EDITOR
                asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
                AssetLogHelper.LogError("加载资源: " + assetPath + " 失败(资源加载Develop模式只能在编辑器下运行)");
#endif
                return asset;
            }
            LoadHandler loadHandler = null;
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError(bundlePackageName + "分包没有初始化");
                return null;
            }
            
            if (!bundleRuntimeInfo.AllAssetLoadHandler.TryGetValue(assetPath, out loadHandler))
            {
                loadHandler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, true, true);
                bundleRuntimeInfo.AllAssetLoadHandler.Add(assetPath, loadHandler);
                bundleRuntimeInfo.UnLoadHandler.Add(loadHandler.UniqueId, loadHandler);
            }
            if (loadHandler.LoadState == LoadState.NoLoad)
            {
                loadHandler.Load();
                asset = loadHandler.FileAssetBundle.LoadAsset<T>(assetPath);
                loadHandler.Asset = asset;
                return asset;
            }
            else if (loadHandler.LoadState == LoadState.Loading)
            {
                loadHandler.ForceAsyncLoadFinish();
                asset = loadHandler.FileAssetBundle.LoadAsset<T>(assetPath);
                loadHandler.Asset = asset;
            }
            if (loadHandler.LoadState == LoadState.Finish)
            {
                asset = (T)loadHandler.Asset;
            }
            return asset;
        }
        
        /// <summary>
        /// 获取Handler 同步加载(泛型)
        /// </summary>
        public static T Load<T>(out LoadHandler handler, string assetPath, bool isPool = false, string bundlePackageName = null) where T : UnityEngine.Object
        {
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            handler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, false, isPool);
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
#if UNITY_EDITOR
                handler.Asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
                AssetLogHelper.LogError("加载资源: " + assetPath + " 失败(资源加载Develop模式只能在编辑器下运行)");
#endif
                handler.CompleteCallback?.Invoke(handler);
                handler.CompleteCallback = null;
                return (T)handler.Asset;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError(bundlePackageName + "分包没有初始化");
                handler = null;
                return null;
            }
            handler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, false, isPool);
            bundleRuntimeInfo.UnLoadHandler.Add(handler.UniqueId, handler);
            handler.Load();
            handler.Asset = handler.FileAssetBundle.LoadAsset<T>(assetPath);
            handler.CompleteCallback?.Invoke(handler);
            handler.CompleteCallback = null;
            return (T)handler.Asset;
        }
        public static T Load<T>(out LoadHandler handler, string assetPath, string bundlePackageName) where T : UnityEngine.Object => Load<T>(out handler, assetPath, false, bundlePackageName);
        
        /// <summary>
        /// 同步加载
        /// </summary>
        public static UnityEngine.Object Load(string assetPath, string bundlePackageName = null)
        {
           
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
#if UNITY_EDITOR
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
#else
                UnityEngine.Object asset = null;
                AssetLogHelper.LogError("加载资源: " + assetPath + " 失败(资源加载Develop模式只能在编辑器下运行)");
#endif
                return asset;
            }
            LoadHandler loadHandler = null;
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError(bundlePackageName + "分包没有初始化");
                return null;
            }
            if (!bundleRuntimeInfo.AllAssetLoadHandler.TryGetValue(assetPath, out loadHandler))
            {
                loadHandler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, true, true);
                bundleRuntimeInfo.AllAssetLoadHandler.Add(assetPath, loadHandler);
                bundleRuntimeInfo.UnLoadHandler.Add(loadHandler.UniqueId, loadHandler);
            }
            if (loadHandler.LoadState == LoadState.NoLoad)
            {
                loadHandler.Load();
                loadHandler.Asset = loadHandler.FileAssetBundle.LoadAsset(assetPath);
                return loadHandler.Asset;
            }
            else if (loadHandler.LoadState == LoadState.Loading)
            {
                loadHandler.ForceAsyncLoadFinish();
                loadHandler.Asset = loadHandler.FileAssetBundle.LoadAsset(assetPath);
            }
            return loadHandler.Asset;
        }
        
        /// <summary>
        /// 获取Handler 同步加载
        /// </summary>
        public static UnityEngine.Object Load(out LoadHandler handler, string assetPath, bool isPool = false, string bundlePackageName = null)
        {
           
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                handler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, false, isPool);
#if UNITY_EDITOR
                handler.Asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
#else
                AssetLogHelper.LogError("加载资源: " + assetPath + " 失败(资源加载Develop模式只能在编辑器下运行)");
#endif
                handler.CompleteCallback?.Invoke(handler);
                handler.CompleteCallback = null;
                return handler.Asset;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError(bundlePackageName + "分包没有初始化");
                handler = null;
                return null;
            }
            handler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, false, isPool);
            bundleRuntimeInfo.UnLoadHandler.Add(handler.UniqueId, handler);
            handler.Load();
            handler.Asset = handler.FileAssetBundle.LoadAsset(assetPath);
            handler.CompleteCallback?.Invoke(handler);
            handler.CompleteCallback = null;
            return handler.Asset;
        }
        public static UnityEngine.Object Load(out LoadHandler handler, string assetPath, string bundlePackageName) => Load(out handler, assetPath, false, bundlePackageName);
        
        /// <summary>
        /// 异步加载(泛型)
        /// </summary>
        public static async ETTask<T> LoadAsync<T>(string assetPath, string bundlePackageName = null) where T : UnityEngine.Object
        {
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            
            T asset = null;
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
#if UNITY_EDITOR
                asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
                AssetLogHelper.LogError("加载资源: " + assetPath + " 失败(资源加载Develop模式只能在编辑器下运行)");
#endif
                return asset;
            }
            LoadHandler loadHandler = null;
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError(bundlePackageName + "分包没有初始化");
                return null;
            }
            if (!bundleRuntimeInfo.AllAssetLoadHandler.TryGetValue(assetPath, out loadHandler))
            {
                loadHandler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, true, true);
                bundleRuntimeInfo.AllAssetLoadHandler.Add(assetPath, loadHandler);
                bundleRuntimeInfo.UnLoadHandler.Add(loadHandler.UniqueId, loadHandler);
            }
            if (loadHandler.LoadState == LoadState.NoLoad)
            {
                ETTask tcs = ETTask.Create(true);
                loadHandler.AwaitEtTasks.Add(tcs);
                await loadHandler.LoadAsync();
                AssetBundleRequest loadAssetAsync = loadHandler.FileAssetBundle.LoadAssetAsync<T>(assetPath);
                loadAssetAsync.completed += operation =>
                {
                    loadHandler.Asset = loadAssetAsync.asset;
                    asset = loadHandler.Asset as T;
                    for (int i = 0; i < loadHandler.AwaitEtTasks.Count; i++)
                    {
                        ETTask etTask = loadHandler.AwaitEtTasks[i];
                        etTask.SetResult();
                    }
                    loadHandler.AwaitEtTasks.Clear();
                };
                await tcs;
                return asset;
            }
            else if (loadHandler.LoadState == LoadState.Loading)
            {
                ETTask tcs = ETTask.Create(true);
                loadHandler.AwaitEtTasks.Add(tcs);
                await tcs;
                return (T)loadHandler.Asset;
            }
            else
            {
                return (T)loadHandler.Asset;
            }
        }
        
        /// <summary>
        /// 获取Handler 异步加载(泛型)
        /// </summary>
        public static ETTask<T> LoadAsync<T>(out LoadHandler handler, string assetPath, bool isPool = false, string bundlePackageName = null) where T : UnityEngine.Object
        {
            ETTask<T> tcs = ETTask<T>.Create(true);
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                handler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, false, isPool);
#if UNITY_EDITOR
                handler.Asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
                AssetLogHelper.LogError("加载资源: " + assetPath + " 失败(资源加载Develop模式只能在编辑器下运行)");
#endif
                tcs.SetResult((T)handler.Asset);
                handler.CompleteCallback?.Invoke(handler);
                handler.CompleteCallback = null;
                return tcs;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError(bundlePackageName + "分包没有初始化");
                handler = null;
                tcs.SetResult(null);
                return tcs;
            }
            handler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, false, isPool);
            bundleRuntimeInfo.UnLoadHandler.Add(handler.UniqueId, handler);
            LoadAsyncTcs(handler, assetPath, tcs).Coroutine();
            return tcs;
        }
        public static ETTask<T> LoadAsync<T>(out LoadHandler handler, string assetPath, string bundlePackageName) where T : UnityEngine.Object => LoadAsync<T>(out handler, assetPath, false, bundlePackageName);
        
        /// <summary>
        /// 异步加载
        /// </summary>
        public static async ETTask<UnityEngine.Object> LoadAsync(string assetPath, string bundlePackageName = null)
        {
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
#if UNITY_EDITOR
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
#else
                UnityEngine.Object asset = null;
                AssetLogHelper.LogError("加载资源: " + assetPath + " 失败(资源加载Develop模式只能在编辑器下运行)");
#endif
                return asset;
            }
            LoadHandler loadHandler = null;
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError(bundlePackageName + "分包没有初始化");
                return null;
            }
            if (!bundleRuntimeInfo.AllAssetLoadHandler.TryGetValue(assetPath, out loadHandler))
            {
                loadHandler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, true, true);
                bundleRuntimeInfo.AllAssetLoadHandler.Add(assetPath, loadHandler);
                bundleRuntimeInfo.UnLoadHandler.Add(loadHandler.UniqueId, loadHandler);
            }
            if (loadHandler.LoadState == LoadState.NoLoad)
            {
                ETTask tcs = ETTask.Create(true);
                loadHandler.AwaitEtTasks.Add(tcs);
                await loadHandler.LoadAsync();
                AssetBundleRequest loadAssetAsync = loadHandler.FileAssetBundle.LoadAssetAsync(assetPath);
                loadAssetAsync.completed += operation =>
                {
                    loadHandler.Asset = loadAssetAsync.asset;
                    for (int i = 0; i < loadHandler.AwaitEtTasks.Count; i++)
                    {
                        ETTask etTask = loadHandler.AwaitEtTasks[i];
                        etTask.SetResult();
                    }
                    loadHandler.AwaitEtTasks.Clear();
                };
                await tcs;
                return loadHandler.Asset;
            }
            else if (loadHandler.LoadState == LoadState.Loading)
            {
                ETTask tcs = ETTask.Create(true);
                loadHandler.AwaitEtTasks.Add(tcs);
                await tcs;
                return loadHandler.Asset;
            }
            else
            {
                return loadHandler.Asset;
            }
        }
        
        /// <summary>
        /// 获取Handler 异步加载
        /// </summary>
        public static ETTask<UnityEngine.Object> LoadAsync(out LoadHandler handler, string assetPath, bool isPool = false, string bundlePackageName = null)
        {
            ETTask<UnityEngine.Object> tcs = ETTask<UnityEngine.Object>.Create(true);
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                handler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, false, isPool);
#if UNITY_EDITOR
                handler.Asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
#else
                AssetLogHelper.LogError("加载资源: " + assetPath + " 失败(资源加载Develop模式只能在编辑器下运行)");
#endif
                tcs.SetResult(handler.Asset);
                handler.CompleteCallback?.Invoke(handler);
                handler.CompleteCallback = null;
                return tcs;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError(bundlePackageName + "分包没有初始化");
                handler = null;
                tcs.SetResult(null);
                return tcs;
            }
            handler = LoadHandlerFactory.GetLoadHandler(assetPath, bundlePackageName, false, isPool);
            bundleRuntimeInfo.UnLoadHandler.Add(handler.UniqueId, handler);
            LoadAsyncTcs(handler, assetPath, tcs).Coroutine();
            return tcs;
        }
        public static ETTask<UnityEngine.Object> LoadAsync(out LoadHandler handler, string assetPath, string bundlePackageName) => LoadAsync(out handler, assetPath, false, bundlePackageName);
        
        private static async ETTask LoadAsyncTcs<T>(LoadHandler handlerRef, string assetPath, ETTask<T> finishTcs) where T : UnityEngine.Object
        {
            await handlerRef.LoadAsync();
            AssetBundleRequest loadAssetAsync = handlerRef.FileAssetBundle.LoadAssetAsync<T>(assetPath);
            loadAssetAsync.completed += operation =>
            {
                handlerRef.Asset = loadAssetAsync.asset;
                handlerRef.CompleteCallback?.Invoke(handlerRef);
                handlerRef.CompleteCallback = null;
                finishTcs.SetResult((T)loadAssetAsync.asset);
            };
        }
        private static async ETTask LoadAsyncTcs(LoadHandler handlerRef, string assetPath, ETTask<UnityEngine.Object> finishTcs)
        {
            await handlerRef.LoadAsync();
            AssetBundleRequest loadAssetAsync = handlerRef.FileAssetBundle.LoadAssetAsync<UnityEngine.Object>(assetPath);
            loadAssetAsync.completed += operation =>
            {
                handlerRef.Asset = loadAssetAsync.asset;
                handlerRef.CompleteCallback?.Invoke(handlerRef);
                handlerRef.CompleteCallback = null;
                finishTcs.SetResult(loadAssetAsync.asset);
            };
        }
        
        /// <summary>
        /// 同步加载场景的AssetBundle包
        /// </summary>
        public static LoadSceneHandler LoadScene(string scenePath, string bundlePackageName = null)
        {
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            LoadSceneHandler loadSceneHandler = new LoadSceneHandler(scenePath, bundlePackageName);
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                //Develop模式可以直接加载场景
                return loadSceneHandler;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError(bundlePackageName + "分包没有初始化");
                return null;
            }
            bundleRuntimeInfo.UnLoadHandler.Add(loadSceneHandler.UniqueId, loadSceneHandler);
            loadSceneHandler.LoadSceneBundle();
            return loadSceneHandler;
        }
        
        /// <summary>
        /// 异步加载场景的AssetBundle包
        /// </summary>
        public static async ETTask<LoadSceneHandler> LoadSceneAsync(string scenePath, string bundlePackageName = null)
        {
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            LoadSceneHandler loadSceneHandler = new LoadSceneHandler(scenePath, bundlePackageName);
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                //Develop模式不需要加载场景
                return loadSceneHandler;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError(bundlePackageName + "分包没有初始化");
                return null;
            }
            bundleRuntimeInfo.UnLoadHandler.Add(loadSceneHandler.UniqueId, loadSceneHandler);
            ETTask tcs = ETTask.Create();
            await loadSceneHandler.LoadSceneBundleAsync(tcs);
            return loadSceneHandler;
        }

        /// <summary>
        /// 异步加载场景的AssetBundle包 提前返回LoadSceneHandler计算进度用
        /// </summary>
        public static ETTask LoadSceneAsync(out LoadSceneHandler loadSceneHandler, string scenePath, string bundlePackageName = null)
        {
            ETTask tcs = ETTask.Create();
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            loadSceneHandler = new LoadSceneHandler(scenePath, bundlePackageName);
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                //Develop模式不需要加载场景
                tcs.SetResult();
                return tcs;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError(bundlePackageName + "分包没有初始化");
                return null;
            }
            bundleRuntimeInfo.UnLoadHandler.Add(loadSceneHandler.UniqueId, loadSceneHandler);
            loadSceneHandler.LoadSceneBundleAsync(tcs).Coroutine();
            return tcs;
        }
        
        /// <summary>
        /// 从一个分包里加载shader
        /// </summary>
        public static Shader LoadShader(string shaderPath, string bundlePackageName = null)
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
#if UNITY_EDITOR
                return AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
#else
                AssetLogHelper.LogError("资源加载Develop模式只能在编辑器下运行");
                return null;
#endif
            }
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError("加载Shader没有此分包: " + bundlePackageName);
                return null;
            }
            return bundleRuntimeInfo.Shader.LoadAsset<Shader>(shaderPath);
        }
        
        /// <summary>
        /// 从一个分包里异步加载shader
        /// </summary>
        public static ETTask<Shader> LoadShaderAsync(string shaderPath, string bundlePackageName = null)
        {
            ETTask<Shader> tcs = ETTask<Shader>.Create();
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
#if UNITY_EDITOR
                tcs.SetResult(AssetDatabase.LoadAssetAtPath<Shader>(shaderPath));
#else
                AssetLogHelper.LogError("资源加载Develop模式只能在编辑器下运行");
#endif
                return tcs;

            }
            if (bundlePackageName == null)
            {
                bundlePackageName = AssetComponentConfig.DefaultBundlePackageName;
            }
            if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                AssetLogHelper.LogError("加载Shader没有此分包: " + bundlePackageName);
                return null;
            }
            
            AssetBundleRequest bundleRequest = bundleRuntimeInfo.Shader.LoadAssetAsync<Shader>(shaderPath);
            bundleRequest.completed += operation =>
            {
                tcs.SetResult(bundleRequest.asset as  Shader);
            };
            return tcs;
        }

        /// <summary>
        /// 获取一个已经初始化完成的分包的信息
        /// </summary>
        public static BundleRuntimeInfo GetBundleRuntimeInfo(string bundlePackageName)
        {
            
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
#if UNITY_EDITOR
                BundleRuntimeInfo devBundleRuntimeInfo;
                if (!BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out devBundleRuntimeInfo))
                {
                    devBundleRuntimeInfo = new BundleRuntimeInfo(bundlePackageName);
                    BundleNameToRuntimeInfo.Add(bundlePackageName, devBundleRuntimeInfo);
                }
                return devBundleRuntimeInfo;
#else
                AssetLogHelper.LogError("资源加载Develop模式只能在编辑器下运行");
#endif
               
            }
            if (BundleNameToRuntimeInfo.TryGetValue(bundlePackageName, out BundleRuntimeInfo bundleRuntimeInfo))
            {
                return bundleRuntimeInfo;
            }
            else
            {
                AssetLogHelper.LogError("初始化的分包里没有这个分包: " + bundlePackageName);
                return null;
            }
        }
    }

    public enum AssetLoadMode
    {
        /// <summary>
        /// 开发模式(无需打包，编辑器下AssetDatabase加载)
        /// </summary>
        Develop = 0,
        
        /// <summary>
        /// 本地调试模式(需要打包，直接加载最新Bundle，不走热更逻辑)
        /// </summary>
        Local = 1,
        
        /// <summary>
        /// 发布模式(需要打包，走版本对比更新流程)
        /// </summary>
        Build = 2,
    }
    
}


