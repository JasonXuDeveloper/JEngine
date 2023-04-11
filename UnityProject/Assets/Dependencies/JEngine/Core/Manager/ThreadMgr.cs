using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace JEngine.Core
{
    public static class ThreadMgr
    {
        /// <summary>
        /// Init loom
        /// </summary>
        public static void Initialize()
        {
            //注册Update到LifeCycleMgr
            _updateTaskId = LifeCycleMgr.Instance.AddUpdateTask(Update, () => _active);
            //默认运行
            Activate();
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
        /// Activate loom to execute loop
        /// </summary>
        public static void Activate()
        {
            _active = true;
        }

        /// <summary>
        /// Deactivate loom to stop loop
        /// </summary>
        public static void Deactivate()
        {
            _active = false;
        }

        /// <summary>
        /// Stop the current loom, requires re-initialize to rerun
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
        public static void QueueOnMainThread(Action<object> action, object p)
        {
            QueueOnMainThread(action, p, 0f);
        }

        /// <summary>
        /// Queue an action with param on main thread to run after specific seconds
        /// </summary>
        /// <param name="action"></param>
        /// <param name="p"></param>
        /// <param name="time"></param>
        public static void QueueOnMainThread<T>(Action<T> action, T p, float time = 0)
        {
            QueueOnMainThread(() => action(p), time);
        }

        /// <summary>
        /// Queue an action on main thread to run after specific seconds
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        public static void QueueOnMainThread(Action action, float time = 0f)
        {
            Delayed.Enqueue(new DelayedQueueItem { Time = _curTime + time, Action = action, MainThread = true});
        }
        
        /// <summary>
        /// Queue an action on other thread to run after specific seconds
        /// </summary>
        /// <param name="action"></param>
        /// <param name="p"></param>
        /// <param name="time"></param>
        /// <typeparam name="T"></typeparam>
        public static void QueueOnOtherThread<T>(Action<T> action, T p, float time = 0f)
        {
            QueueOnOtherThread(() => action(p), time);
        }

        /// <summary>
        /// Queue an action on other thread to run after specific seconds
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        public static void QueueOnOtherThread(Action action, float time = 0f)
        {
            Delayed.Enqueue(new DelayedQueueItem { Time = _curTime + time, Action = action , MainThread = false});
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
            _curTime = UnityEngine.Time.time;
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

            foreach (var (main,act) in CurActions)
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
                            UnityEngine.Debug.LogException(e);
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
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }

            CurActions.Clear();
        }
    }
}