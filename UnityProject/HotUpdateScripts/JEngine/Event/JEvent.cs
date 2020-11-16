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
        private delegate void Events(params object[] parameters);

        private Dictionary<string, List<Events>> _subscribeMethods = new Dictionary<string, List<Events>>(0);
        private Dictionary<Type, List<Events>> _typeEvents = new Dictionary<Type, List<Events>>(0);
        private Dictionary<Type, List<Events>> _unsubscribed = new Dictionary<Type, List<Events>>(0);

        public static bool ShowLog = false;

        
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

        public void Post(params object[] parameters)
        {
            var ParamTypes = string.Join(",", parameters.ToList().Select(obj => obj.GetType()));

            if (ShowLog)
            {
                Log.Print($"[JEvent] <color=#ffa673>通知执行参数为：'{string.Join(",", parameters.Select(p => p.GetType()))}'方法</color>");
            }

            if (parameters.Contains(null))
            {
                Log.Print($"[JEvent] <color=#ffa673>暂时不支持包含null的参数</color>");
                return;
            }

            if (_subscribeMethods.TryGetValue(ParamTypes, out var todo))
            {
                if (todo.Count == 0) return;
                int index = 0;//这个用于做log，log的第一个事件是提示，需要忽略
                foreach (var td in todo)
                {
                    index++;
                    if (!_unsubscribed.Values.SelectMany(s => s.ToArray()).ToList().Contains(td))
                    {
                        try
                        {
                            td.Invoke(parameters);
                            //这边不log的时候，空标签会报错，于是我选择了log
                            Log.Print($"[JEvent] <color=#ffa673>广播参数为：'{string.Join(",", parameters.Select(p => p.GetType()))}'的方法，目前进度" +
                                    $"{index - 1}/{todo.Count - 1}</color>");
                        }
                        catch (Exception ex)
                        {
                            Log.PrintError(ex);
                        }
                    }
                    else
                    {
                        if (ShowLog)
                        {
                            Log.Print($"[JEvent] <color=#ffa673>通知参数为：'{string.Join(",", parameters.Select(p => p.GetType()))}'方法被取消</color>");
                        }
                    }
                }
            }
        }

        public void Unregister(Type type)
        {
            if (!_typeEvents.ContainsKey(type))
            {
                Log.PrintError($"{type.FullName}还没在JEvent中注册");
                return;
            }

            List<Events> es = new List<Events>(0);
            foreach (var _event in _typeEvents[type])
            {
                es.Add(_event);
            }
            _unsubscribed.Add(type, es);

            if (ShowLog)
            {
                Log.Print($"[JEvent] <color=#ffa673>{type.FullName}的事件监听已被取消</color>");
            }
        }

        public void Unregister<T>(T val)
        {
            Unregister(val.GetType());
        }

        public void Register(Type type)
        {
            Register(type, Activator.CreateInstance(type));
        }

        public void Register<T>(T val)
        {
            Register(val.GetType(), val);
        }

        private void Register<T>(Type type, T val)
        {
            if (_typeEvents.ContainsKey(type) && !_unsubscribed.ContainsKey(type))
            {
                Log.PrintError($"{type.FullName}已在JEvent中注册");
                return;
            }

            //直接读缓存，从取消注册的里面重新获取
            if (_unsubscribed.ContainsKey(type))
            {
                if (ShowLog)
                {
                    Log.Print($"[JEvent] <color=#ffa673>{type.FullName}的事件监听已从缓存中复原，跳过反射操作</color>");
                }
                _unsubscribed.Remove(type);
                return;
            }

            _typeEvents.Add(type, new List<Events>(0));

            //先看看是不是整个类监听
            var typeAttr = type.GetCustomAttributes(typeof(SubscriberAttribute), false);
            bool AllMethods = typeAttr != null && typeAttr.Length > 0;
            var methods = type.GetMethods();
            
            foreach (var method in methods)
            {
                //是不是方法有监听
                var methodAttr = method.GetCustomAttributes(typeof(SubscriberAttribute), false);
                var HasAttr = methodAttr != null && methodAttr.Length > 0;

                //没的话继续
                if (!AllMethods && !HasAttr)//不是整个类监听，且该方法没监听，跳过
                {
                    continue;
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

                //参数组合为字符串
                var _params = method.GetParameters().Select(pi => pi.ParameterType);
                string prStr = string.Join(",", _params);

                List<Events> _event = new List<Events>(0);
                //根据参数区分事件
                if (!_subscribeMethods.TryGetValue(prStr, out _event))
                {
                    _event = new List<Events>(0);
                    //创建
                    _event.Add(new Events((parameters) =>
                     {
                         if (ShowLog)
                         {
                             Log.Print($"[JEvent] <color=#ffa673>使用'{string.Join(",", parameters.Select(p => p.GetType()))}'参数的方法被调用了</color>");
                         }
                     }));
                    _subscribeMethods.Add(prStr, _event);
                }

                Events methodEvent = new Events(async (parameters) =>
                {
                    using JAction j = new JAction();
                    await j
                    .Do(() => method.Invoke(val, parameters))
                    .ExecuteAsync(RunInMain);

                });

                //加入
                _event.Add(methodEvent);
                _typeEvents[type].Add(methodEvent);
                _subscribeMethods[prStr] = _event;

                if (ShowLog)
                {
                    Log.Print($"[JEvent] <color=#ffa673>{type.Name}.{method.Name}已被加入到'{prStr}'JEvent中</color>");
                }
            }
        }
    }
}