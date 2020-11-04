using System;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HotUpdateScripts
{   
    /// <summary>
    /// 新资源加载
    /// </summary>
    public class LoadResMgr : Singleton<LoadResMgr>
    {
        GameResMgr _resMgr = null;

        List<AssetTaskNodeBase> _listTaskObj = new List<AssetTaskNodeBase>();

        public bool IsLoadCompleted { get { return TaskCount == 0; } }

        public int TaskCount { get { return _listTaskObj.Count; } }

        public LoadResMgr()
        {
            _resMgr = new GameResMgr();
        }

        #region 预加载资源
        /// <summary>
        /// 预加载UI
        /// </summary>
        public void PreLoadGroupUIRes(APanelBase panel)
        {
            if (panel == null) return;
            PreLoadGroupRes(panel.GetReferList);
        }

        public void PreLoadGroupRes(List<ReferInfo> lists)
        {
            AddGroupTask(lists, null, false);
        }

        /// <summary>
        /// 预加载单个资源到内存
        /// </summary>
        public void PreLoadRes<T>(ResType loadType, string name, AssetType assetType)
        {
            AddSingleTask(ReferInfo.Create(loadType, name, assetType));
        }

        //执行加载单个资源
        private void AddSingleTask(ReferInfo info, Action callBack = null, bool isShowWaitcircle = true)
        {
            AssetTaskNodeBase curNode = AssetTaskNodeBase.CreatLoadNode(_resMgr, info.m_isSingleAsset);
            curNode.AddNode(info, isShowWaitcircle, callBack);
            _listTaskObj.Add(curNode);
        }

        public void AddGroupTask(List<ReferInfo> lists, Action callBack = null, bool isShowWaitcircle = true)
        {
            AssetGroupTaskNode curNode = new AssetGroupTaskNode(_resMgr);
            curNode.AddNode(lists, isShowWaitcircle, callBack);
            _listTaskObj.Add(curNode);
        }
        #endregion

        #region 资源加载接口
        /// <summary>
        /// 异步加载Prefab并且实例化
        /// </summary>
        public void LoadPrefabAsyncAndInstance(ResType type, string name, AssetType assetType, Action<GameObject> CallBack)
        {
            _resMgr.LoadAssetAsync<GameObject>(type, name, assetType, (obj) =>
            {
                CallBack?.Invoke(GetObject<GameObject>(obj));
            });
        }

        /// <summary>
        /// 同步加载Prefab资源，并实例化
        /// </summary>
        /// <returns></returns>
        public GameObject LoadPrefabAndInstance(ResType type, string name, AssetType assetType)
        {
            return GetObject<GameObject>(_resMgr.LoadAsset<GameObject>(type, ResPath.Instance.GetPath(name, assetType)));
        }

        /// <summary>
        /// 异步加载资源，不实例化
        /// </summary>
        public void LoadAssetAsync<T>(ResType type, string name, AssetType assetType, Action<T> CallBack) where T : Object
        {
            _resMgr.LoadAssetAsync<T>(type, name, assetType, (obj) =>
            {
                CallBack?.Invoke(obj as T);
            });
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <returns></returns>
        public T LoadAsset<T>(ResType type, string name, AssetType assetType) where T : Object
        {
            return _resMgr.LoadAsset<T>(type, ResPath.Instance.GetPath(name, assetType));
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <returns></returns>
        public void LoadSceneAsync(string name, Action onComplete = null, bool additive = false)
        {
            _resMgr.LoadSceneAsync(ResPath.Instance.GetPath(name, AssetType.Scene), onComplete, additive);
        }
        #endregion


        #region 获取预加载资源
        public T GetObject<T>(ResType type, string name) where T : Object
        {
            Object obj = _resMgr.GetObject(type, name);
            if (obj == null)
            {
                Log.PrintError($"当前预件未加载！Type = {type} Name = {name}");
                return null;
            }

            return GetObject<T>(obj);
        }

        private T GetObject<T>(Object obj)
        {
            if (obj == null) return default(T);
            if (typeof(T) == typeof(GameObject))
                return (T)Convert.ChangeType(GameObject.Instantiate(obj), typeof(T));
            else return (T)Convert.ChangeType(obj, typeof(T));
        }
        #endregion


        #region 释放资源策略
        /// <summary>
        /// 释放资源
        /// </summary>
        /// 
        public void UnloadAsset(ResType type)
        {
            _resMgr.UnloadAsset(type);
        }

        public void UnloadAsset(ResType type, string name)
        {
            _resMgr.UnloadAsset(type, name);
        }

        public void UnloadAssets(ResType type, List<string> names)
        {
            _resMgr.UnloadAsset(type, names);
        }

        /// <summary>
        /// 释放没有用到的资源
        /// </summary>
        public void RemoveUnusedAssets()
        {
            _resMgr.RemoveUnusedAssets();
        }

        #endregion
        

        public void Update()
        {
            if (_listTaskObj.Count != 0)
            {
                for (int i = _listTaskObj.Count - 1; i >= 0; i--)
                {
                    _listTaskObj[i].Update();

                    if (_listTaskObj[i].m_IsLoadCompleted)
                    {
                        _listTaskObj[i].m_CallBack?.Invoke();
                        _listTaskObj.RemoveAt(i);
                    }
                }
            }
        }
    }
}
