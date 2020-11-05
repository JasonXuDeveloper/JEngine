using System;
using System.Collections.Generic;
using JEngine.Core;
using libx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HotUpdateScripts
{
    public class GameResMgr
    {
        /// <summary> 资源分类策略，分类型存储,key = 加载类型,value = 资源 </summary>
        Dictionary<ResType, Dictionary<string, AssetRequest>> _gameDic = null;

        Dictionary<string, Action<Object>> _gameActionDic = null;

        public GameResMgr()
        {
            _gameDic = new Dictionary<ResType, Dictionary<string, AssetRequest>>();
            foreach (ResType type in Enum.GetValues(typeof(ResType)))
                _gameDic.Add(type, new Dictionary<string, AssetRequest>());

            _gameActionDic = new Dictionary<string, Action<Object>>();
        }


        //加载场景的进度
        public static float LoadSceneProgress = 0;

        public bool IsExistRes(ResType type, string name) { return _gameDic[type].ContainsKey(name); }

        public void AddAssetRequest(AssetRequest req, string name, ResType loadType)
        {
            if (!_gameDic.ContainsKey(loadType))
                _gameDic.Add(loadType, new Dictionary<string, AssetRequest>());
            if (!_gameDic[loadType].ContainsKey(name))
                _gameDic[loadType].Add(name, req);
        }

        public AssetRequest GetAssetRequest(string resName, ResType loadType)
        {
            if (string.IsNullOrEmpty(resName)) return null;
            AssetRequest req = null;
            Dictionary<string, AssetRequest> _dic = null;
            _gameDic.TryGetValue(loadType, out _dic);
            if (_dic == null || _dic.Count == 0) return req;
            _dic.TryGetValue(resName, out req);
            return req;
        }

        public Object GetObject(ResType type, string name)
        {
            AssetRequest req = GetAssetRequest(name, type);
            return req != null ? req.asset : null;
        }

        #region 资源加载策略
        /// <summary>
        /// 同步加载资源
        /// </summary>
        public T LoadAsset<T>(ResType loadType, string name) where T : UnityEngine.Object
        {
            AssetRequest res = GetAssetRequest(name, loadType);
            if (res == null)
            {
                AddAssetRequest(res, name, loadType);
                res = Assets.LoadAsset(name, typeof(T));
            }
            return res.asset as T;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public void LoadAssetAsync<T>(ResType loadType, string name, AssetType assetType, Action<Object> callback) where T : UnityEngine.Object
        {
            AssetRequest res = GetAssetRequest(name, loadType);
            if (res != null)
            {
                callback?.Invoke(res.asset as T);
                return;
            }

            if (_gameActionDic.ContainsKey(name))
            {
                _gameActionDic[name] += callback;
            }
            else
            {               
                _gameActionDic.Add(name, callback);
                res = Assets.LoadAssetAsync(ResPath.Instance.GetPath(name, assetType), typeof(T));
                res.completed += delegate (AssetRequest resource)
                {
                    AddAssetRequest(res, name, loadType);
                    //callback?.Invoke(resource.asset as T);
                    if (_gameActionDic.ContainsKey(name))
                    {
                        _gameActionDic[name]?.Invoke(resource.asset as T);
                        _gameActionDic.Remove(name);
                    }
                };
            }
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path">场景名称</param>
        /// <param name="callback">完成回调</param>
        /// <param name="additive">是否叠加在现有场景上</param>
        public async void LoadSceneAsync(string path, Action callback = null, bool additive = false)
        {
            var req = Assets.LoadSceneAsync(path, additive);
            req.completed += delegate
            {
                callback?.Invoke();
                LoadSceneProgress = 1;
            };
            while (!req.isDone)
            {
                LoadSceneProgress = req.progress;
                await System.Threading.Tasks.Task.Delay(1);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void UnloadAsset(ResType type, string name)
        {
            if (_gameDic[type].ContainsKey(name))
            {
                Assets.UnloadAsset(_gameDic[type][name]);
                _gameDic[type].Remove(name);
            }
        }

        /// <summary>
        /// 释放整个类别的资源
        /// </summary>
        public void UnloadAsset(ResType type)
        {
            foreach (AssetRequest req in _gameDic[type].Values)
            {
                Assets.UnloadAsset(req);
            }
            _gameDic[type].Clear();
        }

        /// <summary>
        /// 释放一组资源
        /// </summary>
        public void UnloadAsset(ResType type, List<string> names)
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (_gameDic[type].ContainsKey(names[i]))
                {
                    Assets.UnloadAsset(_gameDic[type][names[i]]);
                    _gameDic[type].Remove(names[i]);
                }
            }
        }

        /// <summary>
        /// 卸载所有未引用到的资源
        /// </summary>
        public void RemoveUnusedAssets()
        {
            Assets.RemoveUnusedAssets();
        }
        #endregion
    }
}
