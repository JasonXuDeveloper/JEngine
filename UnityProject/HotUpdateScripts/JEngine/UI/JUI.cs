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
using UnityEngine;
using UnityEngine.EventSystems;

namespace JEngine.UI
{
    /// <summary>
    /// JEngine's UI
    /// </summary>
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
            if (!this.gameObject.GetComponent<T>())
            {
                result = this.gameObject.AddComponent<T>();
            }
            //Get
            else
            {
                result = this.gameObject.GetComponent<T>();
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
            if (_bind)
            {
                Log.PrintWarning($"已经绑定了一个BindableProperty<{_bindType}>，该可绑定值的绑定事件不会被取消，意味着多个可绑定值的变更都会调用到onMessage里的内容");
            }
            _bind = true;
            _bindType = typeof(T);
            _initialVal = val.Value;
            val.OnChangeWithOldVal += Message<T>;
            return this;
        }

        /// <summary>
        /// Whether has bind or not
        /// </summary>
        private bool _bind;
        private Type _bindType;
        private object _initialVal;

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
        public JUI onMessage<T>(Action<JUI, T> message)
        {
            if (CheckMessageBindable<T>())
            {
                _message = (jui, oldValue, newValue) =>
                {
                    message?.Invoke(jui, (T)newValue);
                };
            }
            return this;
        }

        /// <summary>
        /// Calls on message
        /// 通知时调用（如果要有老参数的话）
        /// Action的参数分别是JUI本身，OldVal，NewVal
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public JUI onMessageWithOldVal<T>(Action<JUI, T, T> message)
        {
            if (CheckMessageBindable<T>())
            {
                _message = (jui, oldValue, newValue) =>
                {
                    message?.Invoke(jui, (T)oldValue, (T)newValue);
                };
            }
            return this;
        }

        private bool CheckMessageBindable<T>()
        {
            if (!_bind)
            {
                Log.PrintError($"请先对JUI绑定数值（gameObject: {gameObject.name}）");
                return false;
            }
            if (_bindType != typeof(T))
            {
                Log.PrintError($"JUI数值绑定的监听方法的泛型参数必须是{_bindType.FullName}类型，当前注册的是{typeof(T)}类型（gameObject: {gameObject.name}）");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Activates an UI
        /// 激活UI
        /// </summary>
        /// <returns></returns>
        public new JUI Activate()
        {
            if (_bind)
            {
                _message?.Invoke(this, _initialVal, _initialVal);
            }
            else
            {
                base.Activate();
            }
            return this;
        }

        /// <summary>
        /// Create JUI on a gameObject
        /// 在游戏对象上创建JUI
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static JUI CreateOn(GameObject gameObject)
        {
            return JBehaviour.CreateOn<JUI>(gameObject, false);
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
            _message = new Action<JUI, object, object>((j, oldO, newO) => { });
        }

        #region OVERRIDE METHODS
        private Action<JUI> _init;
        private new void Init()
        {
            _init?.Invoke(this);
        }

        private Action<JUI> _run;
        private new void Run()
        {
            if (_bind)
            {
                Pause();
            }
            _run?.Invoke(this);
        }

        private Action<JUI> _loop;
        private new void Loop()
        {
            _loop?.Invoke(this);
        }

        private Action<JUI> _end;
        private new void End()
        {
            _end?.Invoke(this);
            _message = null;
        }
        #endregion

        private Action<JUI, object, object> _message;
        private void Message<T>(T oldVal, T newVal)
        {
            if (!_bind) return;
            _message?.Invoke(this, oldVal, newVal);
        }
    }
}
