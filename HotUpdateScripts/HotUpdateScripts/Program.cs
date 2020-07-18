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
using System.Threading.Tasks;

namespace HotUpdateScripts
{
    public static class Program
    {
        public static void RunGame()
        {
            int i = 0;
            JUIText t = GameObject.Find("Canvas/Text").AddComponent<JUIText>()
                .onInit(t1 =>
                {
                    t1.Text.text = "I have been Inited!";
                    Debug.Log(t1.Text.text);
                })
                .onRun(t2 =>
                {
                    t2.Text.text = "I am Running!";
                    Debug.Log(t2.Text.text);

                    //Set the loop mode and frequency
                    t2.frame = false;//Run in milliseconds
                    t2.frequency = 1000;//Run in every 1000 ms (1 second)

                    UnityEngine.Object.Destroy(t2.gameObject,6);
                })
                .onLoop(t3 =>
                {
                    i++;
                    t3.Text.text = "This is the " + i + " times that I changed!";
                    if (i >= 5)
                    {
                        t3.Text.text = "I will be destoryed in 1 second!";
                    }
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
