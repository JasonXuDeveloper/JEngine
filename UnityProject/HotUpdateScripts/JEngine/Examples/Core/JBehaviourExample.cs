//
// JBehaviourExample.cs
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
using UnityEngine;
using UnityEngine.UI;

namespace JEngine.Examples
{
    public class JBehaviourExample : JBehaviour
    {
        public GameObject Panel;

        public InputField FrequencyNumber;

        public Toggle OnFrameMode;

        public Text DemoText;
        public Text DescText;


        [SerializeField]private int i;


        /// <summary>
        /// Add value change listners
        /// </summary>
        public override void Init()
        {
            //when frequency number changes
            FrequencyNumber.onValueChanged.AddListener(s =>
            {
                ParseFrequency(s);
            });

            //when frame mode toggle clicked
            OnFrameMode.onValueChanged.AddListener(on =>
            {
                FrameMode = on;
                DescText.text = on ? "帧/循环" : "毫秒/循环";
            });
        }

        /// <summary>
        /// Initialize values
        /// </summary>
        public override void Run()
        {
            //Change the frequency of loop
            FrameMode = OnFrameMode.isOn;//Don't loop in frame
            ParseFrequency(FrequencyNumber.text);
            DescText.text = FrameMode ? "帧/循环" : "毫秒/循环";
            i = 0;
        }

        /// <summary>
        /// Loop
        /// </summary>
        public override void Loop()
        {
            if (!Panel.activeSelf) return;
            i++;
            DemoText.text = $"Hello JBehaviour * {i}  times!";
        }

        public override void End()
        {
            //anything
            Log.Print("Good bye from JBehaviour Example");
        }

        public override void OnShow()
        {
            Log.Print("JBehaviour has been shown");
        }

        public override void OnHide()
        {
            Log.Print("JBehaviour has been hidden");
        }

        /// <summary>
        /// Parse string to frequency
        /// </summary>
        /// <param name="text"></param>
        private void ParseFrequency(string text)
        {
            int _frequency;
            bool res = int.TryParse(text, out _frequency);//Run every 1000 milliseconds
            if (res)
            {
                Frequency = _frequency;
            }
            else
            {
                Log.PrintError("输入的频率不正确！"); 
            }
        }
    }
}
