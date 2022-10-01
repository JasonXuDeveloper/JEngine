using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace JEngine.Core
{
    public static class Loom
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
        public static void QueueOnMainThread(Action<object> action, object p, float time)
        {
            QueueOnMainThread(() => action(p), time);
        }

        /// <summary>
        /// Queue an action on main thread to run
        /// </summary>
        /// <param name="action"></param>
        public static void QueueOnMainThread(Action action)
        {
            QueueOnMainThread(action, 0f);
        }

        /// <summary>
        /// Queue an action on main thread to run after specific seconds
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        public static void QueueOnMainThread(Action action, float time)
        {
            Delayed.Enqueue(new DelayedQueueItem { Time = _curTime + time, Action = action });
        }

        /// <summary>
        /// Current actions to process
        /// </summary>
        private static readonly List<Action> CurActions = new List<Action>(100);
        
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
                if (Delayed.TryDequeue(out var item))
                {
                    if (item.Time <= _curTime)
                    {
                        CurActions.Add(item.Action);
                    }
                    else
                    {
                        Delayed.Enqueue(item); 
                    }
                }
            }

            foreach (var a in CurActions)
            {
                try
                {
                    a?.Invoke();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }

            CurActions.Clear();
        }
    }
}