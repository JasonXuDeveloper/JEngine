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

namespace JEngine.Core
{
    public class JAction
    {
        public JAction()
        {
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

        private JAction _reset(JAction j)
        {
            j = new JAction();
            return j;
        }

        private bool _executing;
        private bool _parallel;
        private bool _cancel;

        private List<Action> _toDo;
        private Action _onCancel;

        private Dictionary<int, float> _delays;

        private Dictionary<int, Func<bool>> _waits;
        private Dictionary<int, float> _frequency;
        private Dictionary<int, float> _timeout;

        private Dictionary<int, Action> _whens;
        private Dictionary<int, Func<bool>> _whenCauses;
        private Dictionary<int, float> _whenFrequency;
        private Dictionary<int, float> _whenTimeout;

        private CancellationTokenSource _cancellationTokenSource;


        public JAction Delay(float time)
        {
            if (time > 0)
            {
                _delays.Add(_toDo.Count, time);
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
            for(int i = 0; i < counts; i++)
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

        public JAction Execute()
        {
            if (_executing == true)
            {
                Log.PrintError("JAction is currently executing, if you want to execute JAction multiple times at the same time, call Parallel() before calling Execute()");
            }
            else
            {
                _ = Do();
            }
            return this;
        }

        public async Task<JAction> ExecuteAsync()
        {
            if (_executing == true)
            {
                Log.PrintError("JAction is currently executing, if you want to execute JAction multiple times at the same time, call Parallel() before calling ExecuteAsync()");
            }
            else
            {
                await Do();
            }
            return this;
        }

        public JAction ExecuteAsyncParallel(Action callback = null)
        {
            if (_executing == true)
            {
                Log.PrintError("JAction is currently executing, if you want to execute JAction multiple times at the same time, call Parallel() before calling ExecuteAsyncParallel()");
            }
            else
            {
                _ = Task.Run(async () =>
                {
                    await ExecuteAsync();
                    callback?.Invoke();
                });
            }
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
            _onCancel?.Invoke();
            return this;
        }

        public JAction Reset(bool force = true)
        {
            if (force)
            {
                Cancel();
                _reset(this);
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
                    _reset(this);
                }
            }
            return this;
        }


        private async Task<JAction> Do()
        {
            if (!_parallel)
            {
                _executing = true;
            }
            _cancel = false;

            int index = 0;
            foreach (var td in _toDo)
            {
                if (_cancel) return this;

                //Delay
                if (_delays.ContainsKey(index))
                {
                    await Task.Delay((int)(_delays[index] * 1000));
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
                        await Task.Run(_action, _cancellationTokenSource.Token);
                        await Task.Delay((int)(_frequency * 1000));
                        _time += _frequency;
                    }
                }

                if (td != null)
                {
                    await Task.Run(td, _cancellationTokenSource.Token);
                }

                index++;
            }
            _executing = false;
            return this;
        }
    }
}
