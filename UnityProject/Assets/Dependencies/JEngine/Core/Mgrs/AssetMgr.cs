using libx;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace JEngine.Core
{
    public static class AssetMgr
    {
        private static readonly Dictionary<string, AssetRequest> AssetCache = new Dictionary<string, AssetRequest>();
        private static readonly Dictionary<string, BundleRequest> BundleCache = new Dictionary<string, BundleRequest>();

        public static bool RuntimeMode => Assets.runtimeMode;

        public static bool Loggable
        {
            get => Assets.loggable;
            set => Assets.loggable = value;
        }

        public static string Error(string path)
        {
            return AssetCache.ContainsKey(path) ? AssetCache[path].error : "";
        }

        public static LoadState State(string path)
        {
            return AssetCache.ContainsKey(path) ? AssetCache[path].loadState : LoadState.Init;
        }

        public static float Progress(string path)
        {
            return AssetCache.ContainsKey(path) ? AssetCache[path].progress : 0;
        }
        
        public static Object Load(string path,Type type = null)
        {
            var res = GetAssetFromCache(path);
            if (res != null)
            {
                return res;
            }
            type = CheckType(type);
            var req = Assets.LoadAsset(path, type);
            CheckError(path, req);
            AssetCache[path] = req;
            return req.asset;
        }
        
        public static Task<Object> LoadAsync(string path,Type type = null)
        {
            var res = GetAssetFromCache(path);
            var tcs = new TaskCompletionSource<Object>();
            if (res != null)
            {
                tcs.SetResult(res);
                return tcs.Task;
            }
            type = CheckType(type);
            var req = Assets.LoadAssetAsync(path, type);
            req.completed += ar =>
            {
                CheckError(path, req);
                AssetCache[path] = ar;
                tcs.SetResult(ar.asset);
            };
            return tcs.Task;
        }

        public static void Unload(string path, bool ignore = false)
        {
            if (AssetCache.TryGetValue(path, out var req))
            {
                ReleaseAsset(req);
            }
            else if (!ignore)
            {
                Log.PrintError($"Resource '{path}' has not loaded yet");
            }
        }

        public static async void LoadSceneAsync(string path, bool additive, Action<float> loadingCallback = null,
            Action<bool> finishedCallback = null)
        {
            var req = Assets.LoadSceneAsync(path, additive);
            while (!req.isDone)
            {
                loadingCallback?.Invoke(req.progress);
                await Task.Delay(10);
            }
            CheckError(path, req);
            finishedCallback?.Invoke(string.IsNullOrEmpty(req.error));
        }

        public static AssetBundle LoadBundle(string path)
        {
            var res = GetBundleFromCache(path);
            if (res != null)
            {
                return res;
            }
            var req = Assets.LoadBundle(path);
            CheckError(path, req);
            BundleCache[path] = req;
            return req.assetBundle;
        }
        
        public static Task<AssetBundle> LoadBundleAsync(string path)
        {
            var res = GetBundleFromCache(path);
            var tcs = new TaskCompletionSource<AssetBundle>();
            if (res != null)
            {
                tcs.SetResult(res);
                return tcs.Task;
            }
            var req = Assets.LoadBundleAsync(path);
            req.completed += ar =>
            {
                CheckError(path, req);
                AssetCache[path] = ar;
                tcs.SetResult(((BundleRequest) ar).assetBundle);
            };
            return tcs.Task;
        }

        public static void UnloadBundle(string path, bool ignore = false)
        {
            if (BundleCache.TryGetValue(path, out var req))
            {
                ReleaseAsset(req);
            }
            else if (!ignore)
            {
                Log.PrintError($"Bundle '{path}' has not loaded yet");
            }
        }

        public static void RemoveUnusedAssets()
        {
            Assets.RemoveUnusedAssets();
        }
        
        private static Object GetAssetFromCache(string path)
        {
            if (AssetCache.TryGetValue(path, out var v))
            {
                return v.asset;
            }

            return null;
        }

        private static AssetBundle GetBundleFromCache(string path)
        {
            if (BundleCache.TryGetValue(path, out var v))
            {
                return v.assetBundle;
            }

            return null;
        }

        private static Type CheckType(Type t)
        {
            if (t == null) return typeof(Object);
            return t;
        }

        private static void CheckError(string path, AssetRequest req)
        {
            if (req.isDone && !string.IsNullOrEmpty(req.error))
            {
                Log.PrintError($"Error when loading '{path}': {req.error}");
            }
        }

        private static void ReleaseAsset(AssetRequest req)
        {
            req.Release();
            req.Unload();
        }
    }
}