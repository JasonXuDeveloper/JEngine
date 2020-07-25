//
// JUI.cs
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
using JEngine.Core;
using JEngine.LifeCycle;
using UnityEngine.EventSystems;

namespace JEngine.UI
{
    public abstract class JUI : JBehaviour
    {
        /// <summary>
        /// Get Generic of UnityEngine.UI
        /// 获取UnityEngine.UI的泛型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Element<T>() where T : UIBehaviour
        {
            if (_elements != null)
            {
                //Get from storage
                if (_elements.ContainsKey(typeof(T)))
                {
                    return (T)_elements[typeof(T)];
                }
            }
            else
            {
                _elements = new Dictionary<Type, UIBehaviour>(0);
            }

            T result;
            //Add
            if (!this.GetComponent<T>())
            {
                result = this.gameObject.AddComponent<T>();
            }
            //Get
            else
            {
                result = this.GetComponent<T>();
            }

            //Store
            _elements.Add(typeof(T), result);
            return result;
        }

        /// <summary>
        /// Dictionary which stores elements
        /// 通过字典缓存一些UI控件，提高Element效率
        /// </summary>
        private Dictionary<Type, UIBehaviour> _elements = new Dictionary<Type, UIBehaviour>(0);

        /// <summary>
        /// Binds a data
        /// 绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public JUI Bind<T>(BindableProperty<T> val)
        {
            _bind = true;
            val.OnChange = new Action (Message);
            return this;
        }

        /// <summary>
        /// Whether has bind or not
        /// </summary>
        private bool _bind;

        /// <summary>
        /// Calls when UI has been inited
        /// UI被初始化的时候调用
        /// </summary>
        /// <param name="init"></param>
        /// <returns></returns>
        public JUI onInit(Action<JUI> init)
        {
            _init = init ?? new Action<JUI>(t => { });
            return this;
        }

        /// <summary>
        /// Calls when UI runs at the first time
        /// UI第一次被使用的时候调用
        /// </summary>
        /// <param name="run"></param>
        /// <returns></returns>
        public JUI onRun(Action<JUI> run)
        {
            _run = run ?? new Action<JUI>(t => { });
            return this;
        }

        /// <summary>
        /// Calls on loop
        /// 循环的时候被调用
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public JUI onLoop(Action<JUI> loop)
        {
            _loop = loop ?? new Action<JUI>(t => { });
            return this;
        }

        /// <summary>
        /// Calls on destruction
        /// 销毁时调用
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public JUI onEnd(Action<JUI> end)
        {
            _end = end ?? new Action<JUI>(t => { });
            return this;
        }

        /// <summary>
        /// Calls on message
        /// 通知时调用
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public JUI onMessage(Action<JUI> message)
        {
            _message = message ?? new Action<JUI>(t => { });
            return this;
        }

        /// <summary>
        /// Activates an UI
        /// 激活UI
        /// </summary>
        /// <returns></returns>
        public JUI Activate()
        {
            Pause = false;

            if (_bind)//Call message() once to init UI
            {
                Message();
            }

            return this;
        }

        /// <summary>
        /// Disallow to create via new
        /// </summary>
        private JUI()
        {
            _init = new Action<JUI>(t => { });
            _run = new Action<JUI>(t => { });
            _loop = new Action<JUI>(t => { });
            _end = new Action<JUI>(t => { });
            _message = new Action<JUI>(t => { });
        }

        #region OVERRIDE METHODS
        public override void Awake()
        {
            Pause = true;
            base.Awake();
        }

        private Action<JUI> _init;
        public sealed override void Init()
        {
            _init?.Invoke(this);
            base.Init();
        }

        private Action<JUI> _run;
        public sealed override void Run()
        {
            NotLoop = _bind;
            _run?.Invoke(this);
            base.Run();
        }

        private Action<JUI> _loop;
        public sealed override void Loop()
        {
            _loop?.Invoke(this);
        }

        private Action<JUI> _end;
        public sealed override void End()
        {
            _end?.Invoke(this);
        }
        #endregion

        private Action<JUI> _message;
        public void Message()
        {
            _message?.Invoke(this);
        }
    }
}