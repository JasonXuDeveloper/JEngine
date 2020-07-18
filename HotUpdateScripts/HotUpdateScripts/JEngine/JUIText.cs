//
// JUIText.cs
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
using UnityEngine.UI;
using JEngine.LifeCycle;
using System;
using JEngine.Core;

namespace JEngine.UI
{
    public abstract class JUIText : JUIBehaviour
    {
        public Text Text;

        public JUIText onInit(Action<JUIText> init)
        {
            _init = init ?? new Action<JUIText>(t => { });
            return this;
        }

        public JUIText onRun(Action<JUIText> run)
        {
            _run = run ?? new Action<JUIText>(t => { });
            return this;
        }

        public JUIText onLoop(Action<JUIText> loop)
        {
            _loop = loop ?? new Action<JUIText>(t => { });
            return this;
        }

        public JUIText onEnd(Action<JUIText> end)
        {
            _end = end ?? new Action<JUIText>(t => { });
            return this;
        }

        public JUIText Activate()
        {
            Activated = true;
            return this;
        }

        private JUIText()
        {
            _init = new Action<JUIText>(t => { });
            _run = new Action<JUIText>(t => { });
            _loop = new Action<JUIText>(t => { });
            _end = new Action<JUIText>(t => { });
        }

        private Action<JUIText> _init;
        public sealed override void Init()
        {
            if (this.gameObject.GetComponent<Text>() == null)
            {
                Text = this.gameObject.AddComponent<Text>();
            }
            else
            {
                Text = this.gameObject.GetComponent<Text>();
            }
            _init?.Invoke(this);

            base.Init();
        }

        private Action<JUIText> _run;
        public sealed override void Run()
        {
            _run?.Invoke(this);
            base.Run();
        }

        private Action<JUIText> _loop;
        public sealed override void Loop()
        {
            _loop?.Invoke(this);
        }

        private Action<JUIText> _end;
        public sealed override void End()
        {
            _end?.Invoke(this);
        }
    }
}
