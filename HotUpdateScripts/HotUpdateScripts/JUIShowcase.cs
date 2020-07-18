//
// JUIShowcase.cs
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
using JEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdateScripts
{
    public class Data
    {
        public int a = 0;
        public float b = 0f;
    }

    /// <summary>
    /// This showcase shows how JUI works if an UI needs to update frequently
    /// </summary>
    public class JUIShowcase : MonoBehaviour
    {
        #region NORMAL WAY TO UPDATE UI
        Data data;
        Text a;
        Text b;

        public void Awake()
        {
            data = new Data();
            a = GameObject.Find("Canvas/A").GetComponent<Text>();
        }

        public void Update()
        {
            data.a++;
            a.text = "(Without JUI)a="+data.a.ToString();
        }
        #endregion

        #region USE JUI TO UPDATE UI
        public void Start()
        {
            data = new Data();
            b = GameObject.Find("Canvas/B").GetComponent<Text>();

            b.gameObject.AddComponent<JUIText>()
                .onLoop(t1 =>
                {
                    data.b ++;
                    t1.Text.text = "(With JUI)b=" + data.b.ToString();
                })
                .Activate();
        }
        #endregion
    }
}