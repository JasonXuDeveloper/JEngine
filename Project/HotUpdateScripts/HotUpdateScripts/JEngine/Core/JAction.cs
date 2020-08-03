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
using System.Threading.Tasks;
using UnityEngine;

namespace JEngine.Core
{
    public class JAction
    {
        public JAction()
        {
            _excuting = false;
            _parallel = false;
            _toDo = new List<Action>();
            _delays = new Dictionary<int, float>();
            _waits = new Dictionary<int, Func<bool>>();
            _frequency = new Dictionary<int, float>();
            _timeout = new Dictionary<int, float>();
            _whens = new Dictionary<int, Action>();
            _whenCauses = new Dictionary<int, Func<bool>>();
            _whenFrequency = new Dictionary<int, float>();
            _whenTimeout = new Dictionary<int, float>();
        }

        private bool _excuting;
        private bool _parallel;

        private List<Action> _toDo;

        private Dictionary<int, float> _delays;

        private Dictionary<int, Func<bool>> _waits;
        private Dictionary<int, float> _frequency;
        private Dictionary<int, float> _timeout;

        private Dictionary<int, Action> _whens;
        private Dictionary<int, Func<bool>> _whenCauses;
        private Dictionary<int, float> _whenFrequency;
        private Dictionary<int, float> _whenTimeout;

        public JAction Delay(float time)
        {
            if (time > 0)
            {
                _delays.Add(_toDo.Count, time);
                _toDo.Add(null);
            }
            return this;
        }

        public JAction Until(Func<bool> condition, float frequency = 25, float timeout = -1)
        {
            _waits.Add(_toDo.Count, condition);
            _frequency.Add(_toDo.Count, frequency);
            _timeout.Add(_toDo.Count, timeout);
            _toDo.Add(null);
            return this;
        }

        public JAction RepeatWhen(Action action, Func<bool> condition, float frequency = 25, float timeout = -1)
        {
            _whens.Add(_toDo.Count, action);
            _whenCauses.Add(_toDo.Count, condition);
            _whenFrequency.Add(_toDo.Count, frequency);
            _whenTimeout.Add(_toDo.Count, timeout);
            _toDo.Add(null);
            return this;
        }

        public JAction RepeatUntil(Action action, Func<bool> condition, float frequency = 25, float timeout = -1)
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

        public JAction Excute()
        {
            if(_excuting == true)
            {
                Log.PrintError("JAction is currently excuting, if you want to excute JAction multiple times at the same time, call Parallel() before calling Excute()");
            }
            else
            {
                _ = Do();
            }
            return this;
        }

        private async Task<JAction> Do()
        {
            if (!_parallel)
            {
                _excuting = true;
            }
            int index = 0;
            foreach (var td in _toDo)
            {
                //Delay
                if (_delays.ContainsKey(index))
                {
                    await Task.Delay((int)(_delays[index] * 1000));
                }

                //Wait Until
                if (_waits.ContainsKey(index))
                {
                    float _time = 0;
                    while (!_waits[index]() && Application.isPlaying)
                    {
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
                    while (_condition() && Application.isPlaying)
                    {
                        if (_timeout > 0 && _time >= _timeout)
                        {
                            throw new TimeoutException();
                        }
                        await Task.Run(_action);
                        await Task.Delay((int)(_frequency * 1000));
                        _time += _frequency;
                    }
                }

                if (td != null)
                {
                    await Task.Run(td);
                }

                index++;
            }
            _excuting = false;
            return this;
        }
    }
}
