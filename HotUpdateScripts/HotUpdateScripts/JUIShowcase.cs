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
    [Serializable]
    public class Data
    {
        public int a = 0;
        public BindableProperty<int> b = new BindableProperty<int>(0);
    }

    /// <summary>
    /// This showcase shows how JUI works if an UI needs to update frequently
    /// </summary>
    public class Demo :MonoBehaviour
    {
        public static Demo Instance;

        public Data data;

        public void Awake()
        {
            Instance = this;
            data = new Data();//Create data
        }

        //You need Start here in ILRuntime so that it leads to update
        //If you dont have Start method when you inherit MonoBehaviour, ILRuntime will not call Update (It is an unknown bug and i will fix it soon as possible)
        public void Start()
        {
            
        }

        float seconds = 0;
        //pretends to modify data every second
        public void Update()
        {
            seconds += Time.deltaTime;

            if (seconds >= 1)
            {
                data.a++;//Pretending modifing data
                data.b.Value++;//Pretending modifing data
                seconds -= 1;
            }
        }
    }



    public class JUIShowcase : MonoBehaviour
    {
        GameObject a;
        GameObject b;

        #region NORMAL WAY TO UPDATE UI
        public void Awake()
        {
            //Add showcase data
            new GameObject("BindShowcase").AddComponent<Demo>();
            a = GameObject.Find("Canvas/A");//Bind the gameobject which has the UI element
        }
        //In normal way you need to update your UI in every frame so that you can make your text acurately present your data
        int times = 0;
        public void Update()
        {
            a.GetComponent<Text>().text = "(Without JUI)a="+Demo.Instance.data.a.ToString()+"\n<size=20>I have been run for "+times+" times</size>";//Update UI
            times++;
        }
        #endregion

        #region USE JUI TO UPDATE UI(With Bind)
        /*
         * ========================================================================
         * JUI bind demo
         * JUI绑定数据例子
         * ========================================================================
        */
        public void Start()
        {
            b = GameObject.Find("Canvas/B");//Bind gameobject to show data

            //In JUI it is easy to bind data with text
            int times2 = 0;
            var JUI = b.AddComponent<JUI>()//Add JUI to an gameobject
            .Bind(Demo.Instance.data.b)//Bind data.b to this gameobject
            .onInit(t =>//Counts since init
            {
                t.Element<Text>().text = "(With JUI)b=" + ((int)Demo.Instance.data.b).ToString() + "\n<size=20>I have been run for " + times2 + " times</size>";
                times2++;
            })
            .onMessage(t1 =>//Tells JUI what to do when the binded data has updated
            {
                //EG. we have update UI here
                t1.Element<Text>().text = "(With JUI)b=" + ((int)Demo.Instance.data.b).ToString() + "\n<size=20>I have been run for " + times2 + " times</size>";
                //You can convert bindable properties easily and get their values
                times2++;
            })
            .Activate();//Activate the UI
        }
        #endregion
    }
}