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
using ILRuntime.Runtime.Intepreter;

namespace JEngine.Core
{
    public partial class LifeCycleMgr : MonoBehaviour
    {
        private class LifeCycleItem
        {
            public readonly ILTypeInstance ItemInstance;
            public readonly MethodInfo Method;

            public LifeCycleItem(ILTypeInstance itemInstance, MethodInfo method)
            {
                ItemInstance = itemInstance;
                Method = method;
            }
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
        /// 单帧处理过的对象
        /// </summary>
        private readonly List<object> _instances = new List<object>(1000);

        /// <summary>
        /// All awake methods
        /// </summary>
        private readonly List<LifeCycleItem> _awakeItems = new List<LifeCycleItem>(1000);

        /// <summary>
        /// All on enable methods
        /// </summary>
        private readonly List<LifeCycleItem> _enableItems = new List<LifeCycleItem>(1000);

        /// <summary>
        /// All start methods
        /// </summary>
        private readonly List<LifeCycleItem> _startItems = new List<LifeCycleItem>(1000);

        /// <summary>
        /// All fixed update methods
        /// </summary>
        private readonly List<LifeCycleItem> _fixedUpdateItems = new List<LifeCycleItem>(1000);

        /// <summary>
        /// All update methods
        /// </summary>
        private readonly List<LifeCycleItem> _updateItems = new List<LifeCycleItem>(1000);

        /// <summary>
        /// All late update methods
        /// </summary>
        private readonly List<LifeCycleItem> _lateUpdateItems = new List<LifeCycleItem>(1000);

        /// <summary>
        /// no gc search for awake objs
        /// </summary>
        private readonly HashSet<ILTypeInstance> _awakeObjs = new HashSet<ILTypeInstance>();
        
        /// <summary>
        /// no gc search for enable objs
        /// </summary>
        private readonly HashSet<ILTypeInstance> _enableObjs = new HashSet<ILTypeInstance>();
        
        /// <summary>
        /// no gc search for start objs
        /// </summary>
        private readonly HashSet<ILTypeInstance> _startObjs = new HashSet<ILTypeInstance>();

        /// <summary>
        /// Add awake task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddAwakeItem(ILTypeInstance instance, MethodInfo method)
        {
            _awakeItems.Add(new LifeCycleItem(instance, method));
            _awakeObjs.Add(instance);
        }

        /// <summary>
        /// Add enable task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddOnEnableItem(ILTypeInstance instance, MethodInfo method)
        {
            _enableItems.Add(new LifeCycleItem(instance, method));
            _enableObjs.Add(instance);
        }

        /// <summary>
        /// Add start task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddStartItem(ILTypeInstance instance, MethodInfo method)
        {
            _startItems.Add(new LifeCycleItem(instance, method));
            _startObjs.Add(instance);
        }

        /// <summary>
        /// Add update task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddUpdateItem(ILTypeInstance instance, MethodInfo method)
        {
            _updateItems.Add(new LifeCycleItem(instance, method));
        }

        /// <summary>
        /// Remove update task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveUpdateItem(ILTypeInstance instance)
        {
            _updateItems.RemoveAll(i => ReferenceEquals(i.ItemInstance, instance));
        }

        /// <summary>
        /// Add lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddLateUpdateItem(ILTypeInstance instance, MethodInfo method)
        {
            _lateUpdateItems.Add(new LifeCycleItem(instance, method));
        }

        /// <summary>
        /// Remove lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveLateUpdateItem(ILTypeInstance instance)
        {
            _lateUpdateItems.RemoveAll(i => ReferenceEquals(i.ItemInstance, instance));
        }

        /// <summary>
        /// Add fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddFixedUpdateItem(ILTypeInstance instance, MethodInfo method)
        {
            _fixedUpdateItems.Add(new LifeCycleItem(instance, method));
        }

        /// <summary>
        /// Remove fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveFixedUpdateItem(ILTypeInstance instance)
        {
            _fixedUpdateItems.RemoveAll(i => ReferenceEquals(i.ItemInstance, instance));
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
        /// 执行Item
        /// </summary>
        /// <param name="items"></param>
        /// <param name="removeAfterInvoke"></param>
        /// <param name="ignoreCondition"></param>
        private void ExecuteItems(List<LifeCycleItem> items, bool removeAfterInvoke = true,
            Func<ILTypeInstance, bool> ignoreCondition = null)
        {
            int count = items.Count;
            //遍历
            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                //忽略
                if (ignoreCondition != null && ignoreCondition(item.ItemInstance))
                {
                    continue;
                }

                //执行
                if (item.ItemInstance != null && item.Method != null)
                {
                    try
                    {
                        item.Method.Invoke(item.ItemInstance, ConstMgr.NullObjects);
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

        /// <summary>
        /// IgnoreWithoutInInstances func obj
        /// </summary>
        private Func<ILTypeInstance, bool> _ignoreWithoutInInstancesFunc;
        
        /// <summary>
        /// IgnoreWithInInstances func obj
        /// </summary>
        private Func<ILTypeInstance, bool> _ignoreWithInInstancesFunc;
        
        /// <summary>
        /// whether or not ignore this obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool IgnoreWithoutInInstances(ILTypeInstance obj)
        {
            return _instances.Contains(obj) || _awakeObjs.Contains(obj)
                                            || _startObjs.Contains(obj)
                                            || _enableObjs.Contains(obj);
        }

        /// <summary>
        /// whether or not ignore this obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool IgnoreWithInInstances(ILTypeInstance obj)
        {
            return _awakeObjs.Contains(obj)
                   || _startObjs.Contains(obj)
                   || _enableObjs.Contains(obj);
        }


        /// <summary>
        /// unity周期
        /// </summary>
        private void Awake()
        {
            //只允许自己存在
            if (FindObjectsOfType<LifeCycleMgr>().Length > 1)
            {
                DestroyImmediate(this);
            }
            //申明函数
            _ignoreWithoutInInstancesFunc = IgnoreWithoutInInstances;
            _ignoreWithInInstancesFunc = IgnoreWithInInstances;
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
            _instances.Clear();

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

                //如果有enable，那么在awake的同一帧也该调用enable
                if (_enableItems.Count > 0)
                {
                    //确保本帧没处理过这些对象
                    if (_instances.Count > 0)
                    {
                        //调用执行过awake的对象的enable
                        ExecuteItems(_enableItems, true, s => !_instances.Contains(s));
                    }
                }
                
                //清理
                _awakeObjs.RemoveWhere(_instances.Contains);
                _enableObjs.RemoveWhere(_instances.Contains);
            }

            //如果有enable（这些是没awake的enable）
            if (_enableItems.Count > 0)
            {
                //确保本帧没处理过这些对象
                if (_instances.Count > 0)
                {
                    //调用enable，并记录本帧处理的对象
                    cnt = _enableItems.Count;
                    for (i = 0; i < cnt; i++)
                    {
                        item = _enableItems[i];
                        if (!_instances.Contains(item.ItemInstance))
                        {
                            _instances.Add(item.ItemInstance);
                        }
                    }

                    ExecuteItems(_enableItems, true, _instances.Contains);
                }
                else
                {
                    //调用enable，并记录本帧处理的对象
                    cnt = _enableItems.Count;
                    for (i = 0; i < cnt; i++)
                    {
                        item = _enableItems[i];
                        _instances.Add(item.ItemInstance);
                    }

                    ExecuteItems(_enableItems);
                }
                //清理
                _enableObjs.RemoveWhere(_instances.Contains);
            }

            //如果有start
            if (_startItems.Count > 0)
            {
                //确保本帧没处理过这些对象
                if (_instances.Count > 0)
                {
                    //调用start，并记录本帧处理的对象
                    cnt = _startItems.Count;
                    for (i = 0; i < cnt; i++)
                    {
                        item = _startItems[i];
                        if (!_instances.Contains(item.ItemInstance))
                        {
                            _instances.Add(item.ItemInstance);
                        }
                    }

                    ExecuteItems(_startItems, true, _instances.Contains);
                }
                else
                {
                    //调用start，并记录本帧处理的对象
                    cnt = _startItems.Count;
                    for (i = 0; i < cnt; i++)
                    {
                        item = _startItems[i];
                        _instances.Add(item.ItemInstance);
                    }

                    ExecuteItems(_startItems);
                }
                //清理
                _startObjs.RemoveWhere(_instances.Contains);
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