//
// JUIExample.cs
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
using JEngine.Core;
using JEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace JEngine.Examples
{
    /// <summary>
    /// JUI的示例及对比
    /// JUI showcase, including comparasions to the normal way
    /// </summary>
    public class JUIExample : MonoBehaviour
    {
        /*
         * 显示文字
         * Show value on text
         */
        public Text JUIVal;
        public Text NormalVal;

        /*
         * 分别变化了多少次
         * How many times has it changed
         */
        public int JUITimes;
        public int NormalTimes;

        private void Start()
        {
            /*
             * 重置计数
             * Reset times
             */
            JUITimes = 0;
            NormalTimes = 0;

            //使用JUI绑定数据，已达到更新的目的 | JUI to bind data
            JUI jui = JUI.CreateOn(JUIVal.gameObject);//添加脚本 | Add JUI
            jui.Bind(DataModifyExample.data.BindableMoney)//绑定数据 | Bind Data
                .onMessage<long>((j,value) =>
                {
                    //数据更新后做什么 | What to do when data updated 
                    JUITimes++;
                    JUIVal.text = $"Money = {value}\n" +
                    $"JUI绑定方法，UI更新了{JUITimes}次";
                })
                .Activate();//激活 | Run JUI
        }

        //持续更新UI，因为你不知道什么时候UI会更新 | Keep update UI because you dont know when it will update
        private void Update()
        {
            if (!DataModifyExample.Panel.activeSelf) return;
            NormalTimes++;
            NormalVal.text = $"Money = {DataModifyExample.data.BindableMoney}\n" +
            $"常规Update方法，UI更新了{NormalTimes}次";
        }


    }



    public class DataModifyExample : JBehaviour
    {
        /*
         * Datas
         */
        public static DataClass data = new DataClass();

        public static GameObject Panel;


        public override void Init()
        {
            this.FrameMode = false;
            this.Frequency = 1000;//Loop in 1000ms ==> 1s
        }

        public override void Loop()
        {
            if (!Panel.activeSelf) return;
            data.BindableMoney.Value++;
            this.Frequency = UnityEngine.Random.Range(100, 5000);
        }
    }
}