//
// JEvent.cs
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
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using JEngine.Core;
using System.Threading.Tasks;
using System.Threading;

namespace JEngine.Event
{
    public class JEvent
    {
        private delegate void Events(object val, params object[] parameters);

        private Dictionary<object, List<(string, Events)>> _subscribeMethods = new Dictionary<object, List<(string, Events)>>(0);
        private List<object> _unsubscribed = new List<object>(0);

        public static bool ShowLog = false;


        /// <summary>
        /// Default static JEvent
        /// 默认静态JEvent
        /// </summary>
        public static JEvent defaultEvent
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new JEvent();
                }
                return _instance;
            }
        }
        private static JEvent _instance = new JEvent();

        /// <summary>
        /// Post parameters to all subscibed methods
        /// 将参数广播到全部监听方法
        /// </summary>
        /// <param name="parameters"></param>
        public void Post(params object[] parameters)
        {
            if (parameters.Contains(null))
            {
                Log.Print($"[JEvent] <color=#ffa673>暂时不支持包含null的参数</color>");
                return;
            }
            var allParams = string.Join(", ", parameters.Select(o => o.GetType().FullName));
            foreach (var s in _subscribeMethods)
            {
                object val = s.Key;
                if (val == null) continue;
                List<(string, Events)> todo = s.Value;
                if (todo.Count == 0) return;
                for (int i = 0; i < todo.Count; i++)
                {
                    var td = todo[i];
                    if (!_unsubscribed.Contains(val))
                    {
                        var types = td.Item1;
                        if (allParams != types) continue;
                        var method = td.Item2;
                        //丢主线程去分配Invoke，不然无法执行
                        Loom.QueueOnMainThread(obj =>
                        {
                            method.Invoke(val, parameters);
                        }, null);
                        //这边不log的时候，空标签会报错，于是我选择了log
                        Log.Print($"[JEvent] <color=#ffa673>广播参数为：'{allParams}'的方法，目前进度" +
                                $"{i + 1}/{todo.Count}</color>");
                    }
                    else
                    {
                        if (ShowLog)
                        {
                            Log.Print($"[JEvent] <color=#ffa673>通知参数为：'{allParams}'方法被取消</color>");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Unregister all subscribed methods in a type
        /// 取消注册某类型中全部被监听方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        public void Unregister<T>(T val) where T : new()
        {
            var type = val.GetType();
            if (!_subscribeMethods.ContainsKey(val))
            {
                Log.PrintError($"{type.FullName}还没在JEvent中注册");
                return;
            }

            if (!_unsubscribed.Contains(val))
            {
                _unsubscribed.Add(val);
            }

            if (ShowLog)
            {
                Log.Print($"[JEvent] <color=#ffa673>{type.FullName}的事件监听已被取消</color>");
            }
        }

        /// <summary>
        /// Register all subscribed methods in a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        public void Register<T>(T val) where T: new()
        {
            Register(val.GetType(), val);
        }

        private void Register<T>(Type type, T val)
        {
            if (_subscribeMethods.ContainsKey(val) && !_unsubscribed.Contains(val))
            {
                Log.PrintError($"{type.FullName}已在JEvent中注册");
                return;
            }

            //直接读缓存，从取消注册的里面重新获取
            if (_unsubscribed.Contains(val))
            {
                if (ShowLog)
                {
                    Log.Print($"[JEvent] <color=#ffa673>{type.FullName}的事件监听已从缓存中复原，跳过反射操作</color>");
                }
                _unsubscribed.Remove(val);
                return;
            }

            //先看看是不是整个类监听
            var typeAttr = type.GetCustomAttributes(typeof(SubscriberAttribute), false);
            bool AllMethods = typeAttr != null && typeAttr.Length > 0;
            var methods = type.GetMethods(
                BindingFlags.Default | BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            //把基类也给一起注册
            var t = type.BaseType;
            if (t != null)
            {
                t = type.BaseType;
                var _typeAttr = t.GetCustomAttributes(typeof(SubscriberAttribute), false);
                bool _AllMethods = _typeAttr != null && _typeAttr.Length > 0;
                var _methods = t.GetMethods(
                BindingFlags.Default | BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                foreach (var method in _methods)
                {
                    _Register(method, _AllMethods, _typeAttr, val);
                }
            }

            //注册他自己这个类
            foreach (var method in methods)
            {
                _Register(method, AllMethods, typeAttr, val);
            }


        }

        void _Register<T>(MethodInfo method, bool AllMethods, object[] typeAttr, T val)
        {
            //是不是方法有监听
            var methodAttr = method.GetCustomAttributes(typeof(SubscriberAttribute), false);
            var HasAttr = methodAttr != null && methodAttr.Length > 0;

            //没的话继续
            if (!AllMethods && !HasAttr)//不是整个类监听，且该方法没监听，跳过
            {
                return;
            }

            //运行线程
            bool RunInMain = false;
            if (AllMethods)
            {
                RunInMain = ((SubscriberAttribute)typeAttr[0]).ThreadMode == ThreadMode.Main;
            }
            else
            {
                RunInMain = ((SubscriberAttribute)methodAttr[0]).ThreadMode == ThreadMode.Main;
            }


            List<(string, Events)> _event;
            var types = method.GetParameters().Select(p => p.ParameterType).ToArray();
            //根据参数区分事件
            if (!_subscribeMethods.TryGetValue(val, out _event))
            {
                _event = new List<(string, Events)>(0);
                _subscribeMethods.Add(val, _event);
            }

            Events methodEvent = new Events(async (value, parameters) =>
            {
                await new JAction()
                .Do(() =>
                {
                    try
                    {
                        method.Invoke(value, parameters);
                    }
                    catch (Exception ex)
                    {
                        Log.PrintError($"[JEvent] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                    }
                })
                .ExecuteAsync(RunInMain);
            });

            //加入
            if (!_event.Contains((string.Join(", ", types.Select(o => o.FullName).ToArray()), methodEvent)))
            {
                _event.Add((string.Join(", ", types.Select(o => o.FullName).ToArray()), methodEvent));
            }
            _subscribeMethods[val] = _event;
        }
    }
}
