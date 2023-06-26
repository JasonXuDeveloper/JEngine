using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace JEngine.Core
{
    public static unsafe class ThreadMgr
    {
        public struct ThreadTaskAwaiter : INotifyCompletion
        {
            public int Index;

            public void GetResult()
            {
            }

            public bool IsCompleted
                => false;

            public void OnCompleted(Action continuation)
            {
                Actions[Index] = continuation;
            }

            public ThreadTaskAwaiter GetAwaiter()
            {
                return this;
            }
        }

        private static int GetIndex()
        {
            bool gotLock = false;
            try
            {
                _createLock.Enter(ref gotLock);
                byte* ptr = UsageList;
                byte* max = ptr + MaxSize;
                while (ptr < max)
                {
                    if (*ptr == 0)
                    {
                        *ptr = 1;
                        break;
                    }

                    ptr++;
                }

                if (ptr == max)
                    throw new Exception("ThreadMgr: ThreadTaskAwaiter is full!");

                return (int)(ptr - UsageList);
            }
            finally
            {
                if (gotLock) _createLock.Exit();
            }
        }

        private static void SetCompleted(int index)
        {
            if (UsageList[index] == 0 || index < 0 || index >= MaxSize)
                return;
            Action act = Actions[index];
            try
            {
                UsageList[index] = 0;
                act?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 待执行任务
        /// </summary>
        private static readonly Action[] Actions = new Action[MaxSize];

        /// <summary>
        /// 使用列表
        /// </summary>
        private static readonly byte* UsageList = (byte*)UnsafeUtility.Malloc(MaxSize, 4, Allocator.Persistent);

        /// <summary>
        /// 最大数量
        /// </summary>
        private const int MaxSize = 10000;

        /// <summary>
        /// 锁
        /// </summary>
        private static SpinLock _createLock;


        /// <summary>
        /// Init ThreadMgr
        /// </summary>
        public static void Initialize()
        {
            //注册Update到LifeCycleMgr
            _updateTaskId = LifeCycleMgr.Instance.AddUpdateTask(Update, () => _active);
            //默认运行
            Activate();
            GC.AddMemoryPressure(MaxSize);
        }

        /// <summary>
        /// Task id
        /// </summary>
        private static Guid _updateTaskId;

        /// <summary>
        /// status of activeness
        /// </summary>
        private static bool _active;

        /// <summary>
        /// Activate threadMgr to execute loop
        /// </summary>
        public static void Activate()
        {
            _active = true;
        }

        /// <summary>
        /// Deactivate threadMgr to stop loop
        /// </summary>
        public static void Deactivate()
        {
            _active = false;
        }

        /// <summary>
        /// Stop the current threadMgr, requires re-initialize to rerun
        /// </summary>
        public static void Stop()
        {
            LifeCycleMgr.Instance.RemoveUpdateItem(_updateTaskId);
        }

        /// <summary>
        /// Item to execute
        /// </summary>
        private struct DelayedQueueItem
        {
            public float Time;
            public Action Action;
            public bool MainThread;
        }

        /// <summary>
        /// Actions Queue
        /// </summary>
        private static readonly ConcurrentQueue<DelayedQueueItem> Delayed = new ConcurrentQueue<DelayedQueueItem>();

        /// <summary>
        /// Queue an action with param on main thread to run
        /// </summary>
        /// <param name="action"></param>
        /// <param name="p"></param>
        [Obsolete("Use QueueOnMainThread<T> instead")]
        public static ThreadTaskAwaiter QueueOnMainThread(Action<object> action, object p)
            => QueueOnMainThread(action, p, 0f);

        /// <summary>
        /// Queue an action with param on main thread to run after specific seconds
        /// </summary>
        /// <param name="action"></param>
        /// <param name="p"></param>
        /// <param name="time"></param>
        public static ThreadTaskAwaiter QueueOnMainThread<T>(Action<T> action, T p, float time = 0)
        {
            var ret = new ThreadTaskAwaiter();
            ret.Index = GetIndex();
            int index = ret.Index;
            var act = new Action(() =>
            {
                action(p);
                SetCompleted(index);
            });
            Delayed.Enqueue(new DelayedQueueItem { Time = _curTime + time, Action = act, MainThread = true });
            return ret;
        }

        /// <summary>
        /// Queue an action on main thread to run after specific seconds
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        public static ThreadTaskAwaiter QueueOnMainThread(Action action, float time = 0f)
        {
            var ret = new ThreadTaskAwaiter();
            int index = GetIndex();
            ret.Index = index;
            var act = new Action(() =>
            {
                action();
                SetCompleted(index);
            });
            Delayed.Enqueue(new DelayedQueueItem { Time = _curTime + time, Action = act, MainThread = true });
            return ret;
        }

        /// <summary>
        /// Queue an action on other thread to run after specific seconds
        /// </summary>
        /// <param name="action"></param>
        /// <param name="p"></param>
        /// <param name="time"></param>
        /// <typeparam name="T"></typeparam>
        public static ThreadTaskAwaiter QueueOnOtherThread<T>(Action<T> action, T p, float time = 0f)
            => QueueOnMainThread(action, p, time);

        /// <summary>
        /// Queue an action on other thread to run after specific seconds
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        public static ThreadTaskAwaiter QueueOnOtherThread(Action action, float time = 0f)
        {
            var ret = new ThreadTaskAwaiter();
            int index = GetIndex();
            ret.Index = index;
            var act = new Action(() =>
            {
                action();
                SetCompleted(index);
            });
            Delayed.Enqueue(new DelayedQueueItem { Time = _curTime + time, Action = act, MainThread = false });
            return ret;
        }

        /// <summary>
        /// Current actions to process
        /// </summary>
        private static readonly List<(bool main, Action action)> CurActions = new List<(bool, Action)>(100);

        /// <summary>
        /// Current time
        /// </summary>
        private static float _curTime;

        /// <summary>
        /// Update loop on main thread
        /// </summary>
        static void Update()
        {
            _curTime = Time.time;
            var i = Delayed.Count;
            while (i-- > 0)
            {
                if (!Delayed.TryDequeue(out var item)) continue;
                if (item.Time <= _curTime)
                {
                    CurActions.Add((item.MainThread, item.Action));
                }
                else
                {
                    Delayed.Enqueue(item);
                }
            }

            foreach (var (main, act) in CurActions)
            {
                if (!main)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            act?.Invoke();
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    });
                }
                else
                {
                    try
                    {
                        act?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            CurActions.Clear();
        }
    }
}