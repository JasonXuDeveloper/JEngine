//
// JAction.cs
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace JEngine.Core
{
    public class JAction : IDisposable
    {
        private static List<JAction> JActions = new List<JAction>();
        public JAction()
        {
            Reset();
            JActions.Add(this);
            Loom.Initialize();
        }

        private int _index;

        private bool _executing = false;
        private bool _parallel = false;
        private bool _cancel = false;

        private List<Action> _toDo = new List<Action>();
        private Action _onCancel = new Action(() => { });

        private Dictionary<int, float> _delays = new Dictionary<int, float>();
        private Dictionary<int, int> _delayFrames = new Dictionary<int, int>();

        private Dictionary<int, Func<bool>> _waits = new Dictionary<int, Func<bool>>();
        private Dictionary<int, float> _frequency = new Dictionary<int, float>();
        private Dictionary<int, float> _timeout = new Dictionary<int, float>();

        private Dictionary<int, Action> _whens = new Dictionary<int, Action>();
        private Dictionary<int, Func<bool>> _whenCauses = new Dictionary<int, Func<bool>>();
        private Dictionary<int, float> _whenFrequency = new Dictionary<int, float>();
        private Dictionary<int, float> _whenTimeout = new Dictionary<int, float>();

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();


        public JAction Delay(float time)
        {
            if (time > 0)
            {
                _delays.Add(_toDo.Count, time);
                _toDo.Add(null);
            }
            return this;
        }

        public JAction DelayFrame(int frame)
        {
            if (frame > 0)
            {
                _delayFrames.Add(_toDo.Count, frame);
                _toDo.Add(null);
            }
            return this;
        }

        public JAction Until(Func<bool> condition, float frequency = 0.5f, float timeout = -1)
        {
            _waits.Add(_toDo.Count, condition);
            _frequency.Add(_toDo.Count, frequency);
            _timeout.Add(_toDo.Count, timeout);
            _toDo.Add(null);
            return this;
        }

        public JAction RepeatWhen(Action action, Func<bool> condition, float frequency = 0.5f, float timeout = -1)
        {
            _whens.Add(_toDo.Count, action);
            _whenCauses.Add(_toDo.Count, condition);
            _whenFrequency.Add(_toDo.Count, frequency);
            _whenTimeout.Add(_toDo.Count, timeout);
            _toDo.Add(null);
            return this;
        }

        public JAction RepeatUntil(Action action, Func<bool> condition, float frequency = 0.5f, float timeout = -1)
        {
            Func<bool> _condition = new Func<bool>(() => !condition());
            RepeatWhen(action, _condition, frequency, timeout);
            return this;
        }

        public JAction Repeat(Action action, int counts, float duration = 0)
        {
            for (int i = 0; i < counts; i++)
            {
                Do(action);
                Delay(duration);
            }
            return this;
        }

        public JAction Do(Action action)
        {
            _toDo.Add(action);
            return this;
        }

        public JAction Parallel()
        {
            _parallel = true;
            return this;
        }

        public JAction Execute(bool onMainThread = false)
        {
            _ = Do(onMainThread);
            return this;
        }

        public JAction ExecuteAsyncParallel(Action callback = null, bool onMainThread = false)
        {
            _ = _ExecuteAsync(callback, onMainThread);
            return this;
        }

        public async Task<JAction> ExecuteAsync(bool onMainThread = false)
        {
            return await Do(onMainThread);
        }

        private async Task<JAction> _ExecuteAsync(Action callback, bool onMainThread)
        {
            await Do(onMainThread);
            callback?.Invoke();
            return this;
        }

        public JAction OnCancel(Action action)
        {
            _onCancel = action;
            return this;
        }


        public JAction Cancel()
        {
            _cancel = true;
            _cancellationTokenSource.Cancel();
            if (Application.isPlaying)
            {
                _onCancel?.Invoke();
            }
            return this;
        }

        public JAction Reset(bool force = true)
        {
            if (force)
            {
                Cancel();
                _reset();
            }
            else
            {
                if (_executing)
                {
                    Log.PrintError("JAction is currently executing, if you want to force reset, call Reset(true)");
                }
                else
                {
                    Cancel();
                    _reset();
                }
            }
            return this;
        }

        private void _reset()
        {
            _index = JActions.Count;
            _executing = false;
            _parallel = false;
            _cancel = false;
            _toDo = new List<Action>();
            _onCancel = new Action(() => { });
            _delays = new Dictionary<int, float>();
            _waits = new Dictionary<int, Func<bool>>();
            _frequency = new Dictionary<int, float>();
            _timeout = new Dictionary<int, float>();
            _whens = new Dictionary<int, Action>();
            _whenCauses = new Dictionary<int, Func<bool>>();
            _whenFrequency = new Dictionary<int, float>();
            _whenTimeout = new Dictionary<int, float>();
            _cancellationTokenSource = new CancellationTokenSource();
        }


        private async Task<JAction> Do(bool onMainThread)
        {
            //这边不log的时候，会有概率跳过，迫不得已加了个Log
            if (onMainThread)
            {
                Log.Print($"正在往主线程增加JAction[{_index}]的任务");
            }

            if (_executing == true && !_parallel)
            {
                Log.PrintError("JAction is currently executing, if you want to execute JAction multiple times at the same time, call Parallel() before calling Execute()");
                return this;
            }
            if (!_parallel)
            {
                _executing = true;
            }
            _cancel = false;

            for (int i = 0; i < _toDo.Count; i++)
            {
                int index = i;

                if (_cancel || !Application.isPlaying) break;

                //Delay
                if (_delays.ContainsKey(index))
                {
                    await Task.Delay((int)(_delays[index] * 1000));
                    continue;
                }

                //DelayFrames
                if (_delayFrames.ContainsKey(index))
                {
                    //计算1帧时间(ms)
                    var durationPerFrame = 1000 / (int)(Application.targetFrameRate <= 0 ? GameStats.FPS : Application.targetFrameRate);
                    var duration = durationPerFrame * _delayFrames[index];
                    await Task.Delay(duration);
                    continue;
                }


                //Wait Until
                if (_waits.ContainsKey(index))
                {
                    float _time = 0;
                    while (!_waits[index]())
                    {
                        if (_cancel) return this;

                        if (_timeout[index] > 0 && _time >= _timeout[index])
                        {
                            throw new TimeoutException();
                        }

                        await Task.Delay((int)(_frequency[index] * 1000));
                        _time += _frequency[index];
                    }
                    continue;
                }

                //Repeat When
                if (_whens.ContainsKey(index))
                {
                    float _time = 0;
                    Func<bool> _condition = _whenCauses[index];
                    float _frequency = _whenFrequency[index];
                    float _timeout = _whenTimeout[index];
                    Action _action = _whens[index];
                    while (_condition())
                    {
                        if (_cancel) return this;

                        if (_timeout > 0 && _time >= _timeout)
                        {
                            throw new TimeoutException();
                        }

                        if (onMainThread)
                        {
                            await Task.Run(() =>
                            {
                                Loom.QueueOnMainThread(p =>
                                {
                                    _action();
                                }, null);
                            }, _cancellationTokenSource.Token);
                        }
                        else
                        {
                            await Task.Run(_action, _cancellationTokenSource.Token);
                        }


                        await Task.Delay((int)(_frequency * 1000));
                        _time += _frequency;
                    }
                    continue;
                }

                //Do
                await Task.Run(() =>
                {
                    void action(object p)
                    {
                        try
                        {
                            _toDo[index]?.Invoke();
                        }
                        catch (Exception e)
                        {
                            Log.PrintError($"JAction错误: {e.Message}, {e.Data["StackTrace"]}，已跳过");
                        }
                    }
                    if (onMainThread)
                    {
                        Loom.QueueOnMainThread(action, null);
                    }
                    else
                    {
                        action(null);
                    }
                }, _cancellationTokenSource.Token);
            }
            _executing = false;
            return this;
        }

        ~JAction()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            _toDo = null;
            _onCancel = null;
            _delays = null;
            _waits = null;
            _frequency = null;
            _timeout = null;
            _whens = null;
            _whenCauses = null;
            _whenFrequency = null;
            _whenTimeout = null;
            _cancellationTokenSource = null;
            JActions.Remove(this);
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
