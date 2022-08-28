//
// TimeMgr.cs
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

using ET;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Timer = System.Threading.Timer;

namespace JEngine.Core
{
    /// <summary>
    /// Time Manager
    /// </summary>
    public static partial class TimeMgr
    {
        /// <summary>
        /// 线程安全字典
        /// </summary>
        private static readonly ConcurrentDictionary<int, Stack<TimerWorker>> Timers =
            new ConcurrentDictionary<int, Stack<TimerWorker>>();

        /// <summary>
        /// 计时器任务对象
        /// </summary>
        private class TimerWorker
        {
            /// <summary>
            /// 计时器
            /// </summary>
            private readonly Timer _timer;
            /// <summary>
            /// 该任务使用哪个ETTask来设置回调
            /// </summary>
            private readonly int _index;
            /// <summary>
            /// 延迟
            /// </summary>
            private readonly int _ms;
            /// <summary>
            /// 上下文
            /// </summary>
            private SynchronizationContext _ctx;
            /// <summary>
            /// 是否开始了
            /// </summary>
            private bool _hasStart;

            /// <summary>
            /// 构造一个任务，延迟是delay毫秒
            /// </summary>
            /// <param name="delay"></param>
            /// <param name="ctx"></param>
            public TimerWorker(int delay, SynchronizationContext ctx)
            {
                _ms = delay;
                _index = Tasks.Count;
                _ctx = ctx;
                _hasStart = false;
                Tasks.Add(ETTask.Create(true));
                _timer = new Timer(__ =>
                    {
                        if (!_hasStart || Tasks[_index] == null) return;
                        _ctx?.Send(_ => { Tasks[_index]?.SetResult(); }, null);
                    }, null, _ms,
                    Timeout.Infinite);
            }

            /// <summary>
            /// 开始在一个指定上下文里等待
            /// </summary>
            /// <param name="context"></param>
            public void Start(SynchronizationContext context)
            {
                _ctx = context;
                if (Tasks[_index] == null)
                {
                    Tasks[_index] = ETTask.Create(true);
                }

                _hasStart = true;
                _timer.Change(_ms, Timeout.Infinite);
            }

            /// <summary>
            /// 等待任务完成
            /// </summary>
            public async ETTask WaitForFinish()
            {
                await Tasks[_index];
                Tasks[_index] = null;
                _hasStart = false;
            }
        }

        /// <summary>
        /// 全部ETTask，用于存等待的结果的
        /// </summary>
        private static readonly List<ETTask> Tasks = new List<ETTask>(10);

        /// <summary>
        /// Delay for delay ms
        /// </summary>
        /// <param name="delayInMs"></param>
        /// <returns></returns>
        public static async ETTask Delay(int delayInMs)
        {
            //当前线程的同步上下文
            SynchronizationContext cur = SynchronizationContext.Current;
            //获取任务
            TimerWorker worker = null;
            //先看有没有缓存过
            if (Timers.TryGetValue(delayInMs, out var timers))
            {
                //有的话有没有空的
                if (timers.Count > 0)
                {
                    //取
                    worker = timers.Pop();
                }
            }
            //创建
            else
            {
                Timers[delayInMs] = new Stack<TimerWorker>(10);
            }

            if (worker == null)
            {
                worker = new TimerWorker(delayInMs, cur);
            }

            //开始
            worker.Start(cur);
            //等
            await worker.WaitForFinish();
            //存
            Timers[delayInMs].Push(worker);
        }
    }
}