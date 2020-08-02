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

namespace JEngine.Core
{
    public class JAction
    {
        public JAction()
        {
            _toDo = new List<Action>();
            _delays = new Dictionary<int, float>();
        }

        private List<Action> _toDo;
        private Dictionary<int, float> _delays;

        public JAction Delay(float time)
        {
            _delays.Add(_toDo.Count, time);
            return this;
        }

        public JAction Do(Action action)
        {
            _toDo.Add(action);
            return this;
        }

        public JAction Excute()
        {
            _ = Do();
            return this;
        }

        private async Task<JAction> Do()
        {
            int index = 0;
            foreach (var td in _toDo)
            {
                if (_delays.ContainsKey(index))
                {
                    await Task.Delay((int)(_delays[index] * 1000));
                }
                await Task.Run(td);
                index++;
            }
            return this;
        }
    }
}
