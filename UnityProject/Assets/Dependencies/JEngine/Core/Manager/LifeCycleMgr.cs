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
using System.Linq;
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
            public readonly MethodInfo Method;

            public LifeCycleItem(object itemInstance, MethodInfo method)
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
        private List<object> _instances = new List<object>();

        /// <summary>
        /// All awake methods
        /// </summary>
        private readonly HashSet<LifeCycleItem> _awakeItems = new HashSet<LifeCycleItem>();

        /// <summary>
        /// All on enable methods
        /// </summary>
        private readonly HashSet<LifeCycleItem> _enableItems = new HashSet<LifeCycleItem>();

        /// <summary>
        /// All start methods
        /// </summary>
        private readonly HashSet<LifeCycleItem> _startItems = new HashSet<LifeCycleItem>();

        /// <summary>
        /// All fixed update methods
        /// </summary>
        private readonly HashSet<LifeCycleItem> _fixedUpdateItems = new HashSet<LifeCycleItem>();

        /// <summary>
        /// All update methods
        /// </summary>
        private readonly HashSet<LifeCycleItem> _updateItems = new HashSet<LifeCycleItem>();

        /// <summary>
        /// All late update methods
        /// </summary>
        private readonly HashSet<LifeCycleItem> _lateUpdateItems = new HashSet<LifeCycleItem>();
        
        /// <summary>
        /// Add awake task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddAwakeItem(object instance, MethodInfo method)
        {
            _awakeItems.Add(new LifeCycleItem(instance, method));
        }

        /// <summary>
        /// Add enable task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddOnEnableItem(object instance, MethodInfo method)
        {
            _enableItems.Add(new LifeCycleItem(instance, method));
        }

        /// <summary>
        /// Add start task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddStartItem(object instance, MethodInfo method)
        {
            _startItems.Add(new LifeCycleItem(instance, method));
        }

        /// <summary>
        /// Add update task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddUpdateItem(object instance, MethodInfo method)
        {
            _updateItems.Add(new LifeCycleItem(instance, method));
        }

        /// <summary>
        /// Remove update task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveUpdateItem(object instance)
        {
            _updateItems.RemoveWhere(i => i.ItemInstance == instance);
        }

        /// <summary>
        /// Add lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddLateUpdateItem(object instance, MethodInfo method)
        {
            _lateUpdateItems.Add(new LifeCycleItem(instance, method));
        }

        /// <summary>
        /// Remove lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveLateUpdateItem(object instance)
        {
            _lateUpdateItems.RemoveWhere(i => i.ItemInstance == instance);
        }

        /// <summary>
        /// Add fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddFixedUpdateItem(object instance, MethodInfo method)
        {
            _fixedUpdateItems.Add(new LifeCycleItem(instance, method));
        }

        /// <summary>
        /// Remove fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveFixedUpdateItem(object instance)
        {
            _fixedUpdateItems.RemoveWhere(i => i.ItemInstance == instance);
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
        private void ExecuteItems(HashSet<LifeCycleItem> items, bool removeAfterInvoke = true,
            Func<object, bool> ignoreCondition = null)
        {
            //执行后被移除的item
            HashSet<LifeCycleItem> itemToRemove = new HashSet<LifeCycleItem>();
            var cloned = items.ToList();
            //遍历
            foreach (var item in cloned)
            {
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

                itemToRemove.Add(item);
            }


            //如果需要remove after就记录
            if (removeAfterInvoke)
            {
                items.RemoveWhere(itemToRemove.Contains);
            }
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
        }

        /// <summary>
        /// unity周期
        /// </summary>
        private void FixedUpdate()
        {
            //处理fixed update
            //确保本帧没处理过这些对象
            if (_instances != null && _instances.Count > 0)
            {
                //调用fixed update
                ExecuteItems(_fixedUpdateItems, false, obj =>
                {
                    return _instances.Contains(obj) || _awakeItems.Any(i => i.ItemInstance == obj)
                                                    || _startItems.Any(i => i.ItemInstance == obj)
                                                    || _enableItems.Any(i => i.ItemInstance == obj);
                });
            }
            else
            {
                //调用fixed update
                ExecuteItems(_fixedUpdateItems, false, obj =>
                {
                    return _awakeItems.Any(i => i.ItemInstance == obj)
                           || _startItems.Any(i => i.ItemInstance == obj)
                           || _enableItems.Any(i => i.ItemInstance == obj);
                });
            }
        }

        /// <summary>
        /// unity周期
        /// </summary>
        private void Update()
        {
            _instances?.Clear();

            //处理update
            //确保本帧没处理过这些对象
            if (_instances != null && _instances.Count > 0)
            {
                //调用update
                ExecuteItems(_updateItems, false, obj =>
                {
                    return _instances.Contains(obj) || _awakeItems.Any(i => i.ItemInstance == obj)
                                                    || _startItems.Any(i => i.ItemInstance == obj)
                                                    || _enableItems.Any(i => i.ItemInstance == obj);
                });
            }
            else
            {
                //调用update
                ExecuteItems(_updateItems, false, obj =>
                {
                    return _awakeItems.Any(i => i.ItemInstance == obj)
                           || _startItems.Any(i => i.ItemInstance == obj)
                           || _enableItems.Any(i => i.ItemInstance == obj);
                });
            }
        }

        /// <summary>
        /// unity周期
        /// </summary>
        private void LateUpdate()
        {
            //如果有awake
            if (_awakeItems.Count > 0)
            {
                //调用awake，并记录本帧处理的对象
                if (_instances == null)
                {
                    _instances = new List<object>();
                }

                _instances.AddRange(_awakeItems.Select(i => i.ItemInstance));
                ExecuteItems(_awakeItems);
                
                //如果有enable，那么在awake的同一帧也该调用enable
                if (_enableItems.Count > 0)
                {
                    //确保本帧没处理过这些对象
                    if (_instances != null && _instances.Count > 0)
                    {
                        //调用执行过awake的对象的enable
                        ExecuteItems(_enableItems, true, s=>!_instances.Contains(s));
                    }
                }
            }

            //如果有enable（这些是没awake的enable）
            if (_enableItems.Count > 0)
            {
                //确保本帧没处理过这些对象
                if (_instances != null && _instances.Count > 0)
                {
                    //调用enable，并记录本帧处理的对象
                    _instances.AddRange(_enableItems.ToList().FindAll(i => !_instances.Contains(i.ItemInstance))
                        .Select(i => i.ItemInstance));
                    ExecuteItems(_enableItems, true, _instances.Contains);
                }
                else
                {
                    //调用enable，并记录本帧处理的对象
                    if (_instances == null)
                    {
                        _instances = new List<object>();
                    }

                    _instances.AddRange(_enableItems.Select(i => i.ItemInstance));
                    ExecuteItems(_enableItems);
                }
            }

            //如果有start
            if (_startItems.Count > 0)
            {
                //确保本帧没处理过这些对象
                if (_instances != null && _instances.Count > 0)
                {
                    //调用start，并记录本帧处理的对象
                    _instances.AddRange(_startItems.ToList().FindAll(i => !_instances.Contains(i.ItemInstance))
                        .Select(i => i.ItemInstance));
                    ExecuteItems(_startItems, true, _instances.Contains);
                }
                else
                {
                    //调用start，并记录本帧处理的对象
                    if (_instances == null)
                    {
                        _instances = new List<object>();
                    }

                    _instances.AddRange(_startItems.Select(i => i.ItemInstance));
                    ExecuteItems(_startItems);
                }
            }
            
            //处理late update
            //确保本帧没处理过这些对象
            if (_instances != null && _instances.Count > 0)
            {
                //调用late update
                ExecuteItems(_lateUpdateItems, false, obj =>
                {
                    return _instances.Contains(obj) || _awakeItems.Any(i => i.ItemInstance == obj)
                                                    || _startItems.Any(i => i.ItemInstance == obj)
                                                    || _enableItems.Any(i => i.ItemInstance == obj);
                });
            }
            else
            {
                //调用late update
                ExecuteItems(_lateUpdateItems, false, obj =>
                {
                    return _awakeItems.Any(i => i.ItemInstance == obj)
                           || _startItems.Any(i => i.ItemInstance == obj)
                           || _enableItems.Any(i => i.ItemInstance == obj);
                });
            }
        }
    }
}