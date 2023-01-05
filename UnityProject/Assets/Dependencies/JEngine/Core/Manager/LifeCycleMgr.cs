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
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace JEngine.Core
{
    public unsafe partial class LifeCycleMgr : MonoBehaviour
    {
        private readonly struct LifeCycleItem : IDisposable
        {
            public readonly IntPtr InstancePtr;
            public readonly object InstanceObj;
            public readonly Action Action;
            public readonly Func<bool> ExecuteCondition;
            private readonly bool _hasGCHandle;
            private readonly ulong _gcHandleAddress;

            public bool IsObject => _hasGCHandle;

            public LifeCycleItem(void* instancePtr, object instanceObj, ulong gcAddr, Action action, Func<bool> cond)
            {
                InstancePtr = (IntPtr)instancePtr;
                InstanceObj = instanceObj;
                _hasGCHandle = gcAddr != 0;
                _gcHandleAddress = gcAddr;
                Action = action;
                ExecuteCondition = cond;
            }

            public void Dispose()
            {
                if (_hasGCHandle)
                {
                    UnsafeUtility.ReleaseGCObject(_gcHandleAddress);
                }
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
        private readonly List<IntPtr> _instances = new List<IntPtr>(100);

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
        /// Create lifecycle item
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="instance"></param>
        /// <param name="gcAddr"></param>
        /// <param name="action"></param>
        /// <param name="cond"></param>
        private static LifeCycleItem GetLifeCycleItem(void* addr, object instance, ulong gcAddr, Action action,
            Func<bool> cond) => new LifeCycleItem(addr, instance, gcAddr, action, cond);

        /// <summary>
        /// Add awake task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddAwakeItem<T>(T instance, MethodInfo method) where T : class
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _awakeItems.Add(GetLifeCycleItem(ptr, instance, address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => true));
            _awakeObjs.Add(instance);
        }

        /// <summary>
        /// Add start task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddStartItem<T>(T instance, MethodInfo method) where T : class
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _startItems.Add(GetLifeCycleItem(ptr, instance, address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => true));
            _startObjs.Add(instance);
        }

        /// <summary>
        /// Add update task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        [Obsolete(
            "Please provide a gameObject that holds this instance to be able to monitor whether or not it should update")]
        public void AddUpdateItem(object instance, MethodInfo method)
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _updateItems.Add(GetLifeCycleItem(ptr, instance, address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => (instance.GetGameObject().gameObject.activeInHierarchy)));
        }

        /// <summary>
        /// Add update task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="parent"></param>
        public void AddUpdateItem(object instance, MethodInfo method, GameObject parent)
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _updateItems.Add(GetLifeCycleItem(ptr, instance, address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => parent.activeInHierarchy));
        }

        /// <summary>
        /// Add a task that will always update each frame in the main thread
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Guid AddUpdateTask(Action action)
        {
            Guid guid = Guid.NewGuid();
            _updateItems.Add(GetLifeCycleItem(&guid, null, 0, action, () => true));
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
            var guidPtr = &guid;
            while (_updateItems.Exists(i => i.InstancePtr == (IntPtr)guidPtr))
            {
                guid = Guid.NewGuid();
            }

            _updateItems.Add(GetLifeCycleItem(&guid, null, 0, action, condition));
            return guid;
        }

        /// <summary>
        /// Remove update task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveUpdateItem<T>(T instance)
        {
            if (typeof(T).IsClass)
            {
                void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
                _updateItems.RemoveAll(i =>
                {
                    if (i.InstancePtr != (IntPtr)ptr) return false;
                    i.Dispose();
                    return true;
                });
                UnsafeUtility.ReleaseGCObject(address);
            }
            else
            {
                _updateItems.RemoveAll(i => i.InstancePtr == (IntPtr)Unsafe.AsPointer(ref instance));
            }
        }

        /// <summary>
        /// Add lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        [Obsolete(
            "Please provide a gameObject that holds this instance to be able to monitor whether or not it should lateUpdate")]
        public void AddLateUpdateItem(object instance, MethodInfo method)
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _lateUpdateItems.Add(GetLifeCycleItem(ptr, instance, address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => (instance.GetGameObject().gameObject.activeInHierarchy)));
        }

        /// <summary>
        /// Add lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="parent"></param>
        public void AddLateUpdateItem(object instance, MethodInfo method, GameObject parent)
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _lateUpdateItems.Add(GetLifeCycleItem(ptr, instance, address,
                () => method?.Invoke(instance, ConstMgr.NullObjects), () => parent.activeInHierarchy));
        }

        /// <summary>
        /// Remove lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveLateUpdateItem(object instance)
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _lateUpdateItems.RemoveAll(i =>
            {
                if (i.InstancePtr != (IntPtr)ptr) return false;
                i.Dispose();
                return true;
            });
            UnsafeUtility.ReleaseGCObject(address);
        }

        /// <summary>
        /// Add fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        [Obsolete(
            "Please provide a gameObject that holds this instance to be able to monitor whether or not it should fixedUpdate")]
        public void AddFixedUpdateItem(object instance, MethodInfo method)
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _fixedUpdateItems.Add(GetLifeCycleItem(ptr, instance, address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => (instance.GetGameObject().gameObject.activeInHierarchy)));
        }

        /// <summary>
        /// Add fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="parent"></param>
        public void AddFixedUpdateItem(object instance, MethodInfo method, GameObject parent)
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _fixedUpdateItems.Add(GetLifeCycleItem(ptr, instance, address,
                () => method?.Invoke(instance, ConstMgr.NullObjects), () => parent.activeInHierarchy));
        }

        /// <summary>
        /// Remove fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveFixedUpdateItem(object instance)
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _fixedUpdateItems.RemoveAll(i =>
            {
                if (i.InstancePtr != (IntPtr)ptr) return false;
                i.Dispose();
                return true;
            });
            UnsafeUtility.ReleaseGCObject(address);
        }

        /// <summary>
        /// 执行Item
        /// </summary>
        /// <param name="items"></param>
        /// <param name="removeAfterInvoke"></param>
        /// <param name="ignoreCondition"></param>
        private void ExecuteItems(List<LifeCycleItem> items, in bool removeAfterInvoke = true,
            Func<LifeCycleItem, bool> ignoreCondition = null)
        {
            int count = items.Count;
            //遍历
            for (int i = 0; i < count; i++)
            {
                var item = items[i];

                //检查是否存在
                if (item.IsObject && item.InstanceObj == null)
                {
                    //删了这个
                    item.Dispose();
                    items.RemoveAt(i);
                    i--;
                    count--;
                }

                //忽略
                if (ignoreCondition != null && ignoreCondition(item) ||
                    !item.ExecuteCondition())
                {
                    continue;
                }

                //执行
                try
                {
                    item.Action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                //删了这个
                if (!removeAfterInvoke) continue;
                item.Dispose();
                items.RemoveAt(i);
                i--;
                count--;
            }
        }

        /// <summary>
        /// IgnoreWithoutInInstances func obj
        /// </summary>
        private static Func<LifeCycleItem, bool> _ignoreWithoutInInstancesFunc;

        /// <summary>
        /// IgnoreWithInInstances func obj
        /// </summary>
        private static Func<LifeCycleItem, bool> _ignoreWithInInstancesFunc;

        /// <summary>
        /// whether or not ignore this obj
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool IgnoreWithoutInInstances(LifeCycleItem item)
        {
            return _instances.Contains(item.InstancePtr) || _awakeObjs.Contains(item.InstanceObj)
                                            || _startObjs.Contains(item.InstanceObj);
        }

        /// <summary>
        /// whether or not ignore this obj
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool IgnoreWithInInstances(LifeCycleItem item)
        {
            return _awakeObjs.Contains(item.InstanceObj)
                   || _startObjs.Contains(item.InstanceObj);
        }

        /// <summary>
        /// remove obj from instances
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool RemoveInstanceIfContains(object obj)
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(obj, out var address);
            var ret = _instances.Contains((IntPtr)ptr);
            UnsafeUtility.ReleaseGCObject(address);
            return ret;
        }

        /// <summary>
        /// remove obj from instances
        /// </summary>
        private Predicate<object> RemoveInstanceIfContainsPredicate => RemoveInstanceIfContains;

        /// <summary>
        /// unity周期
        /// </summary>
        private void FixedUpdate()
        {
            //处理fixed update
            //确保本帧没处理过这些对象
            //调用fixed update
            ExecuteItems(_fixedUpdateItems, false,
                //调用fixed update
                _instances.Count > 0 ? _ignoreWithoutInInstancesFunc : _ignoreWithInInstancesFunc);
        }

        /// <summary>
        /// unity周期
        /// </summary>
        private void Update()
        {
            //处理update
            //确保本帧没处理过这些对象
            //调用update
            ExecuteItems(_updateItems, false,
                //调用update
                _instances.Count > 0 ? _ignoreWithoutInInstancesFunc : _ignoreWithInInstancesFunc);

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
                    _instances.Add(item.InstancePtr);
                }

                ExecuteItems(_awakeItems);

                //清理
                _awakeObjs.RemoveWhere(RemoveInstanceIfContainsPredicate);
            }

            //如果有start
            if (_startItems.Count > 0)
            {
                //确保本帧没处理过这些对象
                if (_instances.Count > 0)
                {
                    //调用start，并记录本帧处理的对象
                    ExecuteItems(_startItems, true, lci => _instances.Contains(lci.InstancePtr));

                    cnt = _startItems.Count;
                    for (i = 0; i < cnt; i++)
                    {
                        item = _startItems[i];
                        if (!_instances.Contains(item.InstancePtr))
                        {
                            _instances.Add(item.InstancePtr);
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
                        _instances.Add(item.InstancePtr);
                    }
                }

                //清理
                _startObjs.RemoveWhere(RemoveInstanceIfContainsPredicate);
            }

            //处理late update
            //确保本帧没处理过这些对象
            //调用late update
            ExecuteItems(_lateUpdateItems, false,
                //调用late update
                _instances.Count > 0 ? _ignoreWithoutInInstancesFunc : _ignoreWithInInstancesFunc);
        }
    }
}