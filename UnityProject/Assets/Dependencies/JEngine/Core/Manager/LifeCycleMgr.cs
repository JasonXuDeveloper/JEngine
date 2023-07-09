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
using System.Threading;
using Unity.Collections;
using System.Reflection;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

namespace JEngine.Core
{
    public unsafe partial class LifeCycleMgr : MonoBehaviour
    {
        private struct LifeCycleItem
        {
            public IntPtr InstancePtr;
            private void* _actionPtr;
            private void* _condPtr;
            private ulong _instanceGCHandleAddress;
            private ulong _actionGCHandleAddress;
            private ulong _condGCHandleAddress;

            public bool IsObject => _instanceGCHandleAddress != 0;
            public Action Action => UnsafeMgr.Instance.FromPtr<Action>(_actionPtr);
            public Func<bool> ExecuteCondition => UnsafeMgr.Instance.FromPtr<Func<bool>>(_condPtr);

            public object InstanceObj => _instanceGCHandleAddress != 0
                ? UnsafeMgr.Instance.FromPtr<object>((void*)InstancePtr)
                : null;

            public static LifeCycleItem* Create(in void* instancePtr, in ulong gcAddr, Action action, Func<bool> cond)
            {
                byte* ptr = UsageList;
                LifeCycleItem* item = ItemList;
                byte* max = ptr + MaxSize;
                while (ptr < max)
                {
                    if (*ptr == 0)
                    {
                        *ptr = 1;
                        break;
                    }

                    ptr++;
                    item++;
                }

                if (ptr == max)
                    throw new Exception("LifeCycleMgr: LifeCycleItem is full!");

                item->InstancePtr = (IntPtr)instancePtr;
                item->_instanceGCHandleAddress = gcAddr;
                item->_actionPtr = UnsafeUtility.PinGCObjectAndGetAddress(action, out item->_actionGCHandleAddress);
                item->_condPtr = UnsafeUtility.PinGCObjectAndGetAddress(cond, out item->_condGCHandleAddress);
                return item;
            }

            public void Dispose()
            {
                if (IsObject)
                {
                    UnsafeUtility.ReleaseGCObject(_instanceGCHandleAddress);
                }

                UnsafeUtility.ReleaseGCObject(_actionGCHandleAddress);
                UnsafeUtility.ReleaseGCObject(_condGCHandleAddress);
                fixed (LifeCycleItem* ptr = &this)
                {
                    UsageList[ptr - ItemList] = 0;
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
        /// 非托管内存
        /// </summary>
        private static readonly LifeCycleItem* ItemList =
            (LifeCycleItem*)UnsafeUtility.Malloc(sizeof(LifeCycleItem) * MaxSize, 4, Allocator.Persistent);

        /// <summary>
        /// 使用列表
        /// </summary>
        private static readonly byte* UsageList = (byte*)UnsafeUtility.Malloc(MaxSize, 4, Allocator.Persistent);

        /// <summary>
        /// 最大数量
        /// </summary>
        private const int MaxSize = 30000;

        /// <summary>
        /// 锁
        /// </summary>
        private static SpinLock _createLock;

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
            GC.AddMemoryPressure(sizeof(LifeCycleItem) * MaxSize);
            GC.AddMemoryPressure(MaxSize);
        }

        /// <summary>
        /// 清理非托管
        /// </summary>
        private void OnDestroy()
        {
            UnsafeUtility.Free(ItemList, Allocator.Persistent);
            UnsafeUtility.Free(UsageList, Allocator.Persistent);
            GC.RemoveMemoryPressure(sizeof(LifeCycleItem) * MaxSize);
            GC.RemoveMemoryPressure(MaxSize);
        }

        /// <summary>
        /// 单帧处理过的对象
        /// </summary>
        private readonly List<IntPtr> _instances = new List<IntPtr>(100);

        /// <summary>
        /// All awake methods
        /// </summary>
        private readonly List<IntPtr> _awakeItems = new List<IntPtr>(100);

        /// <summary>
        /// All start methods
        /// </summary>
        private readonly List<IntPtr> _startItems = new List<IntPtr>(100);

        /// <summary>
        /// All fixed update methods
        /// </summary>
        private readonly List<IntPtr> _fixedUpdateItems = new List<IntPtr>(100);

        /// <summary>
        /// All update methods
        /// </summary>
        private readonly List<IntPtr> _updateItems = new List<IntPtr>(100);

        /// <summary>
        /// All late update methods
        /// </summary>
        private readonly List<IntPtr> _lateUpdateItems = new List<IntPtr>(100);

        /// <summary>
        /// All once task methods
        /// </summary>
        private readonly List<IntPtr> _onceTaskItems = new List<IntPtr>(100);

        /// <summary>
        /// no gc search for awake objs
        /// </summary>
        private readonly HashSet<IntPtr> _awakeObjs = new HashSet<IntPtr>();

        /// <summary>
        /// no gc search for start objs
        /// </summary>
        private readonly HashSet<IntPtr> _startObjs = new HashSet<IntPtr>();

        /// <summary>
        /// Create lifecycle item
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="gcAddr"></param>
        /// <param name="action"></param>
        /// <param name="cond"></param>
        private static IntPtr GetLifeCycleItem(in void* addr, in ulong gcAddr, Action action,
            Func<bool> cond)
        {
            bool gotLock = false;
            try
            {
                _createLock.Enter(ref gotLock);
                return
                    (IntPtr)LifeCycleItem.Create(in addr, in gcAddr, action, cond);
            }
            finally
            {
                if (gotLock) _createLock.Exit();
            }
        }

        /// <summary>
        /// Add awake task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddAwakeItem<T>(T instance, MethodInfo method) where T : class
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _awakeItems.Add(GetLifeCycleItem(in ptr, in address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => true));
            _awakeObjs.Add((IntPtr)ptr);
        }

        /// <summary>
        /// Add start task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public void AddStartItem<T>(T instance, MethodInfo method) where T : class
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _startItems.Add(GetLifeCycleItem(in ptr, in address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => true));
            _startObjs.Add((IntPtr)ptr);
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
            _updateItems.Add(GetLifeCycleItem(in ptr, in address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => (instance.GetGameObject().gameObject.activeInHierarchy)));
        }

        /// <summary>
        /// Add update task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="parent"></param>
        /// <param name="cond"></param>
        public void AddUpdateItem<T>(T instance, MethodInfo method, GameObject parent, Func<bool> cond = null) where T : class
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _updateItems.Add(GetLifeCycleItem(in ptr, in address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => cond == null ? parent.activeInHierarchy : parent.activeInHierarchy && cond.Invoke()));
        }

        /// <summary>
        /// Add a task that will always update each frame in the main thread
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Guid AddUpdateTask(Action action) => AddUpdateTask(action, () => true);

        /// <summary>
        /// Add a task that will update each frame in the main thread when condition is true
        /// </summary>
        /// <param name="action"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public Guid AddUpdateTask(Action action, Func<bool> condition)
        {
            Guid guid = Guid.NewGuid();
            var guidIdent = new IntPtr(guid.GetHashCode());
            while (_updateItems.Exists(i => ((LifeCycleItem*)i)->InstancePtr == guidIdent))
            {
                guid = Guid.NewGuid();
                guidIdent = new IntPtr(guid.GetHashCode());
            }

            _updateItems.Add(GetLifeCycleItem((void*)guidIdent, 0, action, condition));
            return guid;
        }

        /// <summary>
        /// Remove update task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveUpdateItem<T>(T instance)
        {
            void* ptr = typeof(T).IsClass
                ? UnsafeMgr.Instance.GetPtr(instance)
                : (void*)new IntPtr(instance.GetHashCode());
            _updateItems.RemoveAll(i =>
            {
                LifeCycleItem* iPtr = (LifeCycleItem*)i;
                if (iPtr->InstancePtr != (IntPtr)ptr) return false;
                iPtr->Dispose();
                return true;
            });
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
            _lateUpdateItems.Add(GetLifeCycleItem(in ptr, in address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => (instance.GetGameObject().gameObject.activeInHierarchy)));
        }

        /// <summary>
        /// Add lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="parent"></param>
        /// <param name="cond"></param>
        public void AddLateUpdateItem<T>(T instance, MethodInfo method, GameObject parent, Func<bool> cond = null) where T : class
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _lateUpdateItems.Add(GetLifeCycleItem(in ptr, in address,
                () => method?.Invoke(instance, ConstMgr.NullObjects), 
                () => cond == null ? parent.activeInHierarchy : parent.activeInHierarchy && cond.Invoke()));
        }

        /// <summary>
        /// Remove lateUpdate task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveLateUpdateItem<T>(T instance) where T : class
        {
            void* ptr = UnsafeMgr.Instance.GetPtr(instance);
            _lateUpdateItems.RemoveAll(i =>
            {
                LifeCycleItem* iPtr = (LifeCycleItem*)i;
                if (iPtr->InstancePtr != (IntPtr)ptr) return false;
                iPtr->Dispose();
                return true;
            });
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
            _fixedUpdateItems.Add(GetLifeCycleItem(in ptr, in address,
                () => method?.Invoke(instance, ConstMgr.NullObjects),
                () => (instance.GetGameObject().gameObject.activeInHierarchy)));
        }

        /// <summary>
        /// Add fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="parent"></param>
        /// <param name="cond"></param>
        public void AddFixedUpdateItem<T>(T instance, MethodInfo method, GameObject parent, Func<bool> cond = null) where T : class
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _fixedUpdateItems.Add(GetLifeCycleItem(in ptr, in address,
                () => method?.Invoke(instance, ConstMgr.NullObjects), 
                () => cond == null ? parent.activeInHierarchy : parent.activeInHierarchy && cond.Invoke()));
        }

        /// <summary>
        /// Remove fixedUpdate task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveFixedUpdateItem<T>(T instance) where T : class
        {
            void* ptr = UnsafeMgr.Instance.GetPtr(instance);
            _fixedUpdateItems.RemoveAll(i =>
            {
                LifeCycleItem* iPtr = (LifeCycleItem*)i;
                if (iPtr->InstancePtr != (IntPtr)ptr) return false;
                iPtr->Dispose();
                return true;
            });
        }

        /// <summary>
        /// Add a task that will call once in the main thread
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Guid AddTask(Action action) => AddTask(action, () => true);

        /// <summary>
        /// Add a task that will call once in the main thread when condition is true
        /// </summary>
        /// <param name="action"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public Guid AddTask(Action action, Func<bool> condition)
        {
            Guid guid = Guid.NewGuid();
            var guidIdent = new IntPtr(guid.GetHashCode());
            while (_onceTaskItems.Exists(i => ((LifeCycleItem*)i)->InstancePtr == guidIdent))
            {
                guid = Guid.NewGuid();
                guidIdent = new IntPtr(guid.GetHashCode());
            }

            _onceTaskItems.Add(GetLifeCycleItem((void*)guidIdent, 0, action, condition));
            return guid;
        }

        /// <summary>
        /// Add a task that will call once in the main thread
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public void AddTask<T>(T instance, Action action) => AddTask(action, () => true);

        /// <summary>
        /// Add a task that will call once in the main thread when condition is true
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="action"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public void AddTask<T>(T instance, Action action, Func<bool> condition) where T : class
        {
            void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(instance, out var address);
            _onceTaskItems.Add(GetLifeCycleItem(in ptr, in address, action, condition));
        }

        /// <summary>
        /// Remove a task that will call once in the main thread
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveTask(in Guid guid)
        {
            var guidIdent = new IntPtr(guid.GetHashCode());
            _onceTaskItems.RemoveAll(i =>
            {
                LifeCycleItem* iPtr = (LifeCycleItem*)i;
                if (iPtr->InstancePtr != guidIdent) return false;
                iPtr->Dispose();
                return true;
            });
        }

        /// <summary>
        /// Remove one time task
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveTask<T>(T instance) where T : class
        {
            void* ptr = UnsafeMgr.Instance.GetPtr(instance);
            _onceTaskItems.RemoveAll(i =>
            {
                LifeCycleItem* iPtr = (LifeCycleItem*)i;
                if (iPtr->InstancePtr != (IntPtr)ptr) return false;
                iPtr->Dispose();
                return true;
            });
        }

        /// <summary>
        /// 执行Item
        /// </summary>
        /// <param name="items"></param>
        /// <param name="removeAfterInvoke"></param>
        /// <param name="ignoreCondition"></param>
        /// <param name="iterate"></param>
        private void ExecuteItems(List<IntPtr> items, in bool removeAfterInvoke = true,
            IgnoreCondFunc ignoreCondition = null, Action<LifeCycleItem> iterate = null)
        {
            int count = items.Count;
            //遍历
            int i = 0;
            while (i < count)
            {
                count = items.Count;
                if (i >= count) break;
                var item = (LifeCycleItem*)items[i];
                bool earlyQuit = item->IsObject && item->InstanceObj == null;
                bool cond = false;
                if (!earlyQuit)
                {
                    try
                    {
                        cond = item->ExecuteCondition();
                    }
                    catch
                    {
                        //条件判断有报错-> 删除该任务
                        earlyQuit = true;
                    }
                }

                //检查是否存在
                if (earlyQuit)
                {
                    //删了这个
                    item->Dispose();
                    items.RemoveAt(i);
                    i--;
                    count--;
                }

                //忽略
                if ((ignoreCondition != null && ignoreCondition(item)) || !cond)
                {
                    i++;
                    continue;
                }

                //执行
                try
                {
                    item->Action?.Invoke();
                    iterate?.Invoke(*item);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                //删了这个
                if (!removeAfterInvoke)
                {
                    i++;
                    continue;
                }

                item->Dispose();
                items.RemoveAt(i);
                count--;
            }
        }

        /// <summary>
        /// a delegate that justifies whether to ignore the execution of the item
        /// </summary>
        private delegate bool IgnoreCondFunc(in LifeCycleItem* ptr);

        /// <summary>
        /// IgnoreWithoutInInstances func obj
        /// </summary>
        private static IgnoreCondFunc _ignoreWithoutInInstancesFunc;

        /// <summary>
        /// IgnoreWithInInstances func obj
        /// </summary>
        private static IgnoreCondFunc _ignoreWithInInstancesFunc;

        /// <summary>
        /// whether or not ignore this obj
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool IgnoreWithoutInInstances(in LifeCycleItem* item)
        {
            return InstancesContains(in item) || IgnoreWithInInstances(in item);
        }

        /// <summary>
        /// whether or not ignore this obj
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool InstancesContains(in LifeCycleItem* item)
        {
            return _instances.Contains(item->InstancePtr);
        }

        /// <summary>
        /// whether or not ignore this obj
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool IgnoreWithInInstances(in LifeCycleItem* item)
        {
            return _awakeObjs.Contains(item->InstancePtr)
                   || _startObjs.Contains(item->InstancePtr);
        }

        /// <summary>
        /// execute once task
        /// </summary>
        private bool _onceTaskExecuting;

        /// <summary>
        /// 处理只调用一次的任务
        /// </summary>
        public void ExecuteOnceTask()
        {
            if (_onceTaskExecuting) return;
            _onceTaskExecuting = true;
            ExecuteItems(_onceTaskItems);
            _onceTaskExecuting = false;
        }

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
            //处理只调用一次的任务
            ExecuteOnceTask();
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
            LifeCycleItem* item;

            //如果有awake
            if (_awakeItems.Count > 0)
            {
                //调用awake，并记录本帧处理的对象
                cnt = _awakeItems.Count;
                for (i = 0; i < cnt; i++)
                {
                    item = (LifeCycleItem*)_awakeItems[i];
                    _instances.Add(item->InstancePtr);
                }

                ExecuteItems(_awakeItems,
                    iterate: il => _awakeObjs.Remove(il.InstancePtr));
            }

            //如果有start
            if (_startItems.Count > 0)
            {
                //确保本帧没处理过这些对象
                if (_instances.Count > 0)
                {
                    cnt = _startItems.Count;
                    for (i = 0; i < cnt; i++)
                    {
                        item = (LifeCycleItem*)_startItems[i];
                        if (!_instances.Contains(item->InstancePtr))
                        {
                            _instances.Add(item->InstancePtr);
                        }
                    }
                    
                    //调用start，并记录本帧处理的对象
                    ExecuteItems(_startItems, true, InstancesContains,
                        iterate: il => _startObjs.Remove(il.InstancePtr));
                }
                else
                {
                    //调用start，并记录本帧处理的对象
                    cnt = _startItems.Count;
                    for (i = 0; i < cnt; i++)
                    {
                        item = (LifeCycleItem*)_startItems[i];
                        _instances.Add(item->InstancePtr);
                    }
                    
                    ExecuteItems(_startItems, iterate: il => _startObjs.Remove(il.InstancePtr));
                }
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