using System;
using UnityEngine;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace JEngine.Core
{
    public class Loom : MonoBehaviour
    {
        public static int maxThreads = 6;
        static int numThreads;

        private static Loom _current;
        private int _count;
        public static Loom Current => _current;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            maxThreads = Environment.ProcessorCount;
            Log.Print($"Loom ProcessorCount, {maxThreads}");

            var go = new GameObject();
            go.name = "Loom";
            _current = go.AddComponent<Loom>();
            DontDestroyOnLoad(go);
        }

        private List<Action> _actions = new List<Action>();

        public struct DelayedQueueItem
        {
            public float time;
            public Action action;
        }

        private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

        public static void QueueOnMainThread(Action action)
        {
            QueueOnMainThread(action, 0f);
        }

        public static void QueueOnMainThread(Action<object> action, object p)
        {
            QueueOnMainThread(action, p, 0f);
        }

        public static void QueueOnMainThread(Action action, float time)
        {
            if (time > 0f)
            {
                lock (Current._delayed)
                {
                    Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
                }
            }
            else
            {
                lock (Current._actions)
                {
                    Current._actions.Add(action);
                }
            }
        }

        public static void QueueOnMainThread(Action<object> action, object p, float time)
        {
            QueueOnMainThread(() => action(p), time);
        }

        public static Thread RunAsync(Action a)
        {
            while (numThreads >= maxThreads)
            {
                Thread.Sleep(1);
            }

            Interlocked.Increment(ref numThreads);
            ThreadPool.QueueUserWorkItem(RunAction, a);
            return null;
        }

        private static void RunAction(object action)
        {
            try
            {
                ((Action)action)();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Interlocked.Decrement(ref numThreads);
            }

        }

        private List<Action> curActions = new List<Action>();

        private List<DelayedQueueItem> curDelayeds = new List<DelayedQueueItem>();

        // Update is called once per frame
        void Update()
        {
            lock (_actions)
            {
                if (_actions.Count > 0)
                {
                    curActions.AddRange(_actions);
                    _actions.Clear();
                }
            }

            if (curActions.Count > 0)
            {
                foreach (var a in curActions)
                {
                    try
                    {
                        a();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                curActions.Clear();
            }

            lock (_delayed)
            {
                var i = _delayed.Count - 1;
                while (i >= 0)
                {
                    var item = _delayed[i];
                    if (item.time <= Time.time)
                    {
                        curDelayeds.Add(item);
                        _delayed.RemoveAt(i);
                    }

                    i--;
                }
            }

            if (curDelayeds.Count > 0)
            {
                foreach (var item in curDelayeds)
                {
                    try
                    {
                        item.action();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                curDelayeds.Clear();
            }
        }
    }
}