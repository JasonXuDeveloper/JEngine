//
// EmptyClass.cs
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JEngine.LifeCycle;

namespace HotUpdateScripts
{
    public class Sample : JUIBehaviour
    {
        public Text HelloText;

        public int times;

        public override void Init()
        {
            HelloText = GameObject.Find("Canvas/Text").GetComponent<Text>();
            times = 0;
        }

        public override void Run()
        {
            //Here in run method, we set up the frequency and mode of loop.

            frame = false;// Not loop in frame, but in milliseconds
            frequency = 1000;//Loop in 1000ms => 1 second

            /* OR:
             * frame = true;// Loop in frame
             * frequency = 10;//Loop in every 10 frames
             */
        }

        public override void Loop()
        {
            HelloText.text = "HELLO JEngine * " + times + " times";
            times++;
        }
    }
}
