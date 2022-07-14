using System;
using System.Collections.Generic;
using UnityEngine;

namespace BM
{
    public static partial class AssetComponent
    {
        /// <summary>
        /// 计时
        /// </summary>
        private static float _timer = 0;

        /// <summary>
        /// 下载进度更新器
        /// </summary>
        internal static Action<float> DownLoadAction = null;

        /// <summary>
        /// 等待计时队列
        /// </summary>
        internal static Queue<TimerAwait> TimerAwaitQueue = new Queue<TimerAwait>();

        /// <summary>
        /// 卸载周期计时循环
        /// </summary>
        public static void Update()
        {
            float nowTime = Time.deltaTime;
            int awaitQueueCount = TimerAwaitQueue.Count;
            for (int i = 0; i < awaitQueueCount; i++)
            {
                TimerAwait timerAwait = TimerAwaitQueue.Dequeue();
                if (!timerAwait.CalcSubTime(nowTime))
                {
                    TimerAwaitQueue.Enqueue(timerAwait);
                }
            }
            _timer += nowTime;
            if (_timer >= _unLoadCirculateTime)
            {
                _timer = 0;
                AutoAddToTrueUnLoadPool();
            }
            DownLoadAction?.Invoke(nowTime);
        }
    }
}