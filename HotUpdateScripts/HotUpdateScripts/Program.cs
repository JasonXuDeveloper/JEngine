//
// Program.cs
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JEngine.UI;
using JEngine.Core;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace HotUpdateScripts
{
    public static class Program
    {
        public static void RunGame()
        {
            /*
             * ========================================================================
             * 10 seconds countdown demo
             * 10秒倒计时例子
             * ========================================================================
             */

            int i = 10;

            JUI t = GameObject.Find("Canvas/Text").AddComponent<JUI>()//给一个GameObject绑定JUI，该GameObject可以不包含任何UI控件
                .onInit(t1 =>
                {
                    var text = t1.Element<Text>();
                    text.text = "I have been Inited!";
                    Debug.Log(text.text);
                })
                .onRun(t2 =>
                {
                    var text = t2.Element<Text>();
                    text.text = "I am Running!";
                    Debug.Log(text.text);

                    //Set the loop mode and frequency
                    t2.frame = false;//Run in milliseconds
                    t2.frequency = 1000;//Run in every 1000 ms (1 second)

                    UnityEngine.Object.Destroy(t2.gameObject, 10);
                })
                .onLoop(t3 =>
                {
                    i--;
                    var text = t3.Element<Text>();
                    text.text = "I will be destroyed in " + i +" seconds!";
                })
                .onEnd(t4 =>
                {
                    Debug.Log("My lifecycle has been ended!");
                })
                .Activate();

            var JUIShowcase = new GameObject("JUIShowcase").AddComponent<JUIShowcase>();
        }
    }
}
