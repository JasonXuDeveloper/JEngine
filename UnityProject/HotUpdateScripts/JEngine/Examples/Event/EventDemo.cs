//
// EventDemo.cs
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
using System.Threading.Tasks;
using JEngine.Core;
using JEngine.Event;
using UnityEngine;
using UnityEngine.UI;

namespace JEngine.Examples
{
    public class EventDemo : JBehaviour
    {
        public UIManager UIManager;

        public override async void Init()
        {
            var ExtensionManager = new ExtensionManager();
            var GMToolsManager = new GMToolsManager();

            //JEvent.ShowLog = true;//是否显示一些log

            JEvent.defaultEvent.Register(UIManager);
            JEvent.defaultEvent.Register(ExtensionManager);
            JEvent.defaultEvent.Register(GMToolsManager);

            //先搞一个登录失败数据
            LoginErrorData d = new LoginErrorData
            {
                username = "test",
                errorMsg = "故意让它错误的",
            };

            //广播错误数据
            JEvent.defaultEvent.Post(d);

            //取消注册ExtensionManager，这样下次不会post到这个实例内的方法
            JEvent.defaultEvent.Unregister(ExtensionManager);

            await Task.Delay(3000);

            //创建独立的JEvent
            JEvent e = new JEvent();

            //登录成功数据
            LoginSuccessData dt = new LoginSuccessData
            {
                username = "杰哥",
                money = 10000
            };

            //给独立的JEvent注册方法
            e.Register(UIManager);
            e.Register(GMToolsManager);
            //广播
            e.Post(dt);
        }
    }

    public class LoginSuccessData
    {
        public string username;
        public int money;
    }

    public class LoginErrorData
    {
        public string username;
        public string errorMsg;
    }

    //监听整个类里面的方法，主线程执行，unity方法必须主线程执行，除了Debug.Log外
    [Subscriber(ThreadMode.Main)]
    public class UIManager
    {
        /*
         * 只要UIManager的实例还在，这些字段就可以用
         */

        public GameObject SuccessPanel;
        public GameObject ErrorPanel;
        public GameObject GamePanel;
        public Text UsernameText;
        public Text MoneyText;
        public Text ErrorMsgText;

        /// <summary>
        /// 登录成功的时候的UI界面更新
        /// </summary>
        /// <param name="data"></param>
        public void OnSuccess(LoginSuccessData data)
        {
            SuccessPanel.SetActive(true);
            new JAction().Delay(3).Do(() =>
            {
                SuccessPanel.SetActive(false);
                UsernameText.text = data.username;
                //因为在GMTools那边更新了money数据，所以用那边的静态实例数据
                //自己写的时候也要注意，每个方法的data参数哪怕进行了更改也不会影响其他方法内的data
                //必须自己把它单独保存到一个其他方法也能读到的地方，才能在其他方法里同步对数据的修改
                //同时多线程处理数据请自行考虑线程安全，脏数据就得自己处理了
                MoneyText.text = $"￥{GMToolsManager.successData.money}";
                GamePanel.SetActive(true);
            })
            .Delay(3)
            .Do(() =>
            {
                GamePanel.transform.parent.gameObject.SetActive(false);
            }).Execute(true);
        }

        /// <summary>
        /// 登录失败的时候的UI界面更新
        /// </summary>
        /// <param name="data"></param>
        public void OnError(LoginErrorData data)
        {
            ErrorMsgText.text = $"账号：{data.username}登录失败，{data.errorMsg}";
            ErrorPanel.SetActive(true);
        }
    }

    //监听整个类里面的方法，子线程执行，不调用unity本身的东西就可以在子线程执行
    [Subscriber(ThreadMode.Other)]
    public class ExtensionManager
    {
        /// <summary>
        /// Log错误信息
        /// </summary>
        /// <param name="data"></param>
        public void ProcessErrorMsg(LoginErrorData data)
        {
            Log.PrintError("登录失败：" + data.errorMsg);
        }

        /// <summary>
        /// logcat测试的时候log一下登录成功的账号
        /// </summary>
        /// <param name="data"></param>
        public void LogcatSuccessData(LoginSuccessData data)
        {
            Log.Print($"{data.username}登录成功");
        }
    }

    //个别方法被监听
    public class GMToolsManager
    {
        public static LoginSuccessData successData;

        /// <summary>
        /// 让钱翻一百倍
        /// </summary>
        /// <param name="data"></param>
        [Subscriber(ThreadMode.Main)]//跑主线程
        public void GetMoreMoney(LoginSuccessData data)
        {
            successData = data;
            successData.money *= 100;
        }
    }
}