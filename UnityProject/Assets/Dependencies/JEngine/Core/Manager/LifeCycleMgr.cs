//
// LifeCycleMgr.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 JEngine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace JEngine.Core
{
    public partial class LifeCycleMgr : MonoBehaviour
    {
        private class LifeCycleItem
        {
            public readonly object ItemInstance;
            public readonly Action Action;
            public readonly Func<bool> ExecuteCondition;

            public LifeCycleItem(object itemInstance, Action action, Func<bool> cond)
            {
                ItemInstance = itemInstance;
                Action = action;
                ExecuteCondition = cond;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            if (_instance != null)
            {
                return;
            }

            _instance = new GameObject("LifeCycleMgr").AddComponent<LifeCycleMgr>();
            DontDestroyOnLoad(_instance);
        }

        /// <summary>
        /// 单例
        /// </summary>
        private static LifeCycleMgr _instance;

        /// <summary>
        /// 单例
        /// </summary>
        public static LifeCycleMgr Instance => _instance;
        
        
        /// <summary>
        /// unity周期
        /// </summary>
        private void Awake()
        {
            //只允许自己存在
            if (Instance != null)
            {
                DestroyImmediate(this);
            }
            _ignoreWithoutInInstancesFunc = IgnoreWithoutInInstances;
            _ignoreWithInInstancesFunc = IgnoreWithInInstances;
        }

        /// <summary>
        /// 单帧处理过的对象
        /// </summary>
        private readonly List<object> _instances = new List<object>(100);

        /// <summary>
        /// All awake methods
        /// </summary>
        private readonly List<LifeCycleItem> _awakeItems = new List<LifeCycleItem>(100);

        /// <summary>
        /// All start methods
        /// </summary>
        private readonly List<LifeCycleItem> _startItems = new List<LifeCycleItem>(100);

        /// <summary>
        /// All fixed update methods
        /// </summary>
        private readonly List<LifeCycleItem> _fixedUpdateItems = new List<LifeCycleItem>(100);

        /// <summary>
        /// All update methods
        /// </summary>
        private readonly List<LifeCycleItem> _updateItems = new List<LifeCycleItem>(100);

        /// <summary>
        /// All late update methods
        /// </summary>
        private readonly List<LifeCycleItem> _lateUpdateItems = new List<LifeCycleItem>(100);

        /// <summary>
        /// no gc search for awake objs
        /// </summary>
        private readonly HashSet<object> _awakeObjs = new HashSet<object>();
        
        /// <summary>
        /// no gc search for start objs
        /// </summary>
        private readonly HashSet<object> _startObjs = new HashSet<object>();

        /// <summary>
        /// Add awake task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddAwakeItem(object instance, MethodInfo method)
        {
            _awakeItems.Add(
                new LifeCycleItem(instance, () => method?.Invoke(instance, ConstMgr.NullObjects), () => true));
            _awakeObjs.Add(instance);
        }

        /// <summary>
        /// Add start task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddStartItem(object instance, MethodInfo method)
        {
            _startItems.Add(new LifeCycleItem(instance, () => method?.Invoke(instance, ConstMgr.NullObjects), () => true));
            _startObjs.Add(instance);
        }

        /// <summary>
        /// Add update task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        [Obsolete("Please provide a gameObject that holds this instance to be able to monitor whether or not it should update")]
        public void AddUpdateItem(object instance, MethodInfo method)
        {
            _updateItems.Add(new LifeCycleItem(instance, () => method?.Invoke(instance, ConstMgr.NullObjects), () => (instance.GetGameObject().gameObject.activeInHierarchy)));
        }

        /// <summary>
        /// Add update task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="parent"></param>
        public void AddUpdateItem(object instance, MethodInfo method, GameObject parent)
        {
            _updateItems.Add(new LifeCycleItem(instance, () => method?.Invoke(instance, ConstMgr.NullObjects), () => parent.activeInHierarchy));
        }

        /// <summary>
        /// Add a task that will always update each frame in the main thread
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Guid AddUpdateTask(Action action)
        {
            Guid guid = Guid.NewGuid();
            _updateItems.Add(new LifeCycleItem(guid, action, () => true));
            return guid;
        }

        /// <summary>
        /// Add a task that will update each frame in the main thread when condition is true
        /// </summary>
        /// <param name="action"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public Guid AddUpdateTask(Action action, Func<bool> condition)
        {
            Guid guid = Guid.NewGuid();
            while (_updateItems.Exists(i => (Guid)i.ItemInstance == guid))
            {
                guid = Guid.NewGuid();
            }
            _updateItems.Add(new LifeCycleItem(guid, action, condition));
            return guid;
        }

        /// <summary>
        /// Remove update task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveUpdateItem(object instance)
        {
            _updateItems.RemoveAll(i => i.ItemInstance == instance);
        }

        /// <summary>
        /// Add lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        [Obsolete("Please provide a gameObject that holds this instance to be able to monitor whether or not it should lateUpdate")]
        public void AddLateUpdateItem(object instance, MethodInfo method)
        {
            _lateUpdateItems.Add(new LifeCycleItem(instance, () => method?.Invoke(instance, ConstMgr.NullObjects), () => (instance.GetGameObject().gameObject.activeInHierarchy)));
        }

        /// <summary>
        /// Add lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="parent"></param>
        public void AddLateUpdateItem(object instance, MethodInfo method, GameObject parent)
        {
            _lateUpdateItems.Add(new LifeCycleItem(instance, () => method?.Invoke(instance, ConstMgr.NullObjects), () => parent.activeInHierarchy));
        }

        /// <summary>
        /// Remove lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveLateUpdateItem(object instance)
        {
            _lateUpdateItems.RemoveAll(i => i.ItemInstance == instance);
        }

        /// <summary>
        /// Add fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        [Obsolete("Please provide a gameObject that holds this instance to be able to monitor whether or not it should fixedUpdate")]
        public void AddFixedUpdateItem(object instance, MethodInfo method)
        {
            _fixedUpdateItems.Add(new LifeCycleItem(instance, () => method?.Invoke(instance, ConstMgr.NullObjects), () => (instance.GetGameObject().gameObject.activeInHierarchy)));
        }

        /// <summary>
        /// Add fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="parent"></param>
        public void AddFixedUpdateItem(object instance, MethodInfo method, GameObject parent)
        {
            _fixedUpdateItems.Add(new LifeCycleItem(instance, () => method?.Invoke(instance, ConstMgr.NullObjects), () => parent.activeInHierarchy));
        }

        /// <summary>
        /// Remove fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveFixedUpdateItem(object instance)
        {
            _fixedUpdateItems.RemoveAll(i => i.ItemInstance == instance);
        }

        /// <summary>
        /// 执行Item
        /// </summary>
        /// <param name="items"></param>
        /// <param name="removeAfterInvoke"></param>
        /// <param name="ignoreCondition"></param>
        private void ExecuteItems(List<LifeCycleItem> items, bool removeAfterInvoke = true,
            Func<object, bool> ignoreCondition = null)
        {
            lock (items)
            {
                int count = items.Count;
                //遍历
                for (int i = 0; i < count; i++)
                {
                    var item = items[i];
                    //忽略
                    if (ignoreCondition != null && ignoreCondition(item.ItemInstance)|| !item.ExecuteCondition())
                    {
                        continue;
                    }

                    //执行
                    if (item.ItemInstance != null)
                    {
                        try
                        {
                            item.Action?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }

                    //删了这个
                    if (removeAfterInvoke)
                    {
                        items.RemoveAt(i);
                        i--;
                        count--;
                    }
                } 
            }
        }

        /// <summary>
        /// IgnoreWithoutInInstances func obj
        /// </summary>
        private static Func<object, bool> _ignoreWithoutInInstancesFunc;

        /// <summary>
        /// IgnoreWithInInstances func obj
        /// </summary>
        private static Func<object, bool> _ignoreWithInInstancesFunc;
        
        /// <summary>
        /// whether or not ignore this obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool IgnoreWithoutInInstances(object obj)
        {
            return _instances.Contains(obj) || _awakeObjs.Contains(obj)
                                            || _startObjs.Contains(obj);
        }

        /// <summary>
        /// whether or not ignore this obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool IgnoreWithInInstances(object obj)
        {
            return _awakeObjs.Contains(obj)
                   || _startObjs.Contains(obj);
        }

        /// <summary>
        /// unity周期
        /// </summary>
        private void FixedUpdate()
        {
            //处理fixed update
            //确保本帧没处理过这些对象
            if (_instances.Count > 0)
            {
                //调用fixed update
                ExecuteItems(_fixedUpdateItems, false, _ignoreWithoutInInstancesFunc);
            }
            else
            {
                //调用fixed update
                ExecuteItems(_fixedUpdateItems, false, _ignoreWithInInstancesFunc);
            }
        }
        
        /// <summary>
        /// unity周期
        /// </summary>
        private void Update()
        {
            //处理update
            //确保本帧没处理过这些对象
            if (_instances.Count > 0)
            {
                //调用update
                ExecuteItems(_updateItems, false, _ignoreWithoutInInstancesFunc);
            }
            else
            {
                //调用update
                ExecuteItems(_updateItems, false, _ignoreWithInInstancesFunc);
            }
            _instances.Clear();
        }

        /// <summary>
        /// unity周期
        /// </summary>
        private void LateUpdate()
        {
            //temp val
            int cnt, i;
            LifeCycleItem item;
            
            //如果有awake
            if (_awakeItems.Count > 0)
            {
                //调用awake，并记录本帧处理的对象
                cnt = _awakeItems.Count;
                for (i = 0; i < cnt; i++)
                {
                    item = _awakeItems[i];
                    _instances.Add(item.ItemInstance);
                }

                ExecuteItems(_awakeItems);
                
                //清理
                lock (_awakeObjs)
                {
                    _awakeObjs.RemoveWhere(_instances.Contains);
                }
            }

            //如果有start
            if (_startItems.Count > 0)
            {
                //确保本帧没处理过这些对象
                if (_instances.Count > 0)
                {
                    //调用start，并记录本帧处理的对象
                    ExecuteItems(_startItems, true, _instances.Contains);

                    cnt = _startItems.Count;
                    for (i = 0; i < cnt; i++)
                    {
                        item = _startItems[i];
                        if (!_instances.Contains(item.ItemInstance))
                        {
                            _instances.Add(item.ItemInstance);
                        }
                    }
                }
                else
                {
                    ExecuteItems(_startItems);
                    
                    //调用start，并记录本帧处理的对象
                    cnt = _startItems.Count;
                    for (i = 0; i < cnt; i++)
                    {
                        item = _startItems[i];
                        _instances.Add(item.ItemInstance);
                    }
                }
                //清理
                lock (_startObjs)
                {
                    _startObjs.RemoveWhere(_instances.Contains);
                }
            }

            //处理late update
            //确保本帧没处理过这些对象
            if (_instances.Count > 0)
            {
                //调用late update
                ExecuteItems(_lateUpdateItems, false, _ignoreWithoutInInstancesFunc);
            }
            else
            {
                //调用late update
                ExecuteItems(_lateUpdateItems, false, _ignoreWithInInstancesFunc);
            }
        }
    }
}