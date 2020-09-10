﻿//
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
using UnityEngine;
using UnityEngine.UI;
using JEngine.UI;
using JEngine.Core;
using JEngine.Examples;

namespace HotUpdateScripts
{
    public static class Program
    {
        public static void RunGame()
        {
            /*
            * ====================================
            *                Demos
            * ====================================
            */



            /*
            * ====================================
            *           JSaver EXAMPLE
            * ====================================
            */
            JSaver.SaveAsString("data to save", "dataName", "1234567890987654");//Set a data to local storage
            var encryptStr = JSaver.SaveAsString("data to save", "dataName", "1234567890987654");//set and get the encrypted data string
            Log.Print($"[JSaver] Str Encrypted result: {encryptStr}");
            var decryptStr = JSaver.GetString("dataName", "1234567890987654");
            Log.Print($"[JSaver] Str Decrypted result: {decryptStr}");

            //save custom class
            DataClass data = new DataClass
            {
                id = 666,
                name = "JEngine牛逼",
                money = 999999,
                diamond = 999999,
                gm = true
            };
            encryptStr = JSaver.SaveAsJSON(data, "playerData", "password_is_this");
            Log.Print($"[JSaver] Custom Class Encrypted result: {encryptStr}");
            decryptStr = JSaver.GetString("playerData", "password_is_this");//Can convert to string
            Log.Print($"[JSaver] JSON Str Decrypted result: {decryptStr}");

            DataClass newData = JSaver.GetObject<DataClass>("playerData", "password_is_this");//Can covert to class


            /*
            * ====================================
            *           JResource EXAMPLE
            * ====================================
            */
            var txt = JResource.LoadRes<TextAsset>("Text.txt");
            Log.Print("[JResource] Get Resource with Sync method: " + txt.text);

            JResource.LoadResAsync<TextAsset>("Text.txt",(txt)=>
            {
                Log.Print("[JResource] Get Resource with Async method: " + txt.text);
            });

            /*
            * ====================================
            *           JAction EXAMPLE
            * ====================================
            */
            int num = 0;
            int repeatCounts = 3;
            float repeatDuration = 0.5f;
            float timeout = 10f;

            //Simple use
            JAction j = new JAction();
            j.Do(() => Log.Print("[JAction] [j] Hello from JAction!"))
                .Execute();

            //Until
            JAction j1 = new JAction();
            j1.Until(() => true)
                .Do(() => Log.Print("[JAction] [j1] until condition has done"))
                .Execute();

            //Repeat
            JAction j2 = new JAction();
            j2.Repeat(() =>
            {
                num++;
                Log.Print($"[JAction] [j2] num is: {num}");
            }, repeatCounts, repeatDuration)
                .Execute();

            //Repeat when
            JAction j3 = new JAction();
            j3.RepeatWhen(() =>
            {
                Log.Print($"[JAction] [j3] num is more than 0, num--");
                num--;
            },
            () => num > 0, repeatDuration, timeout)
                .Execute();

            //Repeat until
            JAction j4 = new JAction();
            j4.RepeatUntil(() =>
            {
                Log.Print($"[JAction] [j4] num is less than 3, num++");
                num++;
            }, () => num < 3, repeatDuration, timeout)
                .Execute();

            //Delay
            JAction j5 = new JAction();
            j5.Do(() => Log.Print("[JAction] [j5] JAction will do something else in 3 seconds"))
                .Delay(3.0f)
                .Do(() => Log.Print("[JAction] [j5] Bye from JAction"))
                .Execute();

            //Execute Async
            JAction j6 = new JAction();
            _ = j6.Do(() => Log.Print("[JAction] [j6] This is an async JAction"))
                .ExecuteAsync();

            //Execute Async With Callback
            JAction j7 = new JAction();
            j7.Do(() => Log.Print("[JAction] [j7] This is an async JAction but runs parallel, callback will be called after it has done"))
                .ExecuteAsync(() => Log.Print("[JAction] [j7] Done"));

            //Cancel a JAction
            JAction j8 = new JAction();
            j8.RepeatWhen(() => Log.Print("[JAction] [j8] I am repeating!!!"), () => true, 1, timeout)
                .ExecuteAsync();
            //You can either add a cancel callback
            j8.OnCancel(() => Log.Print("[JAction] [j8] has been cancelled!"));

            JAction j9 = new JAction();
            j9.Delay(5)
                .Do(() =>
                {
                    j8.Cancel();
                    Log.Print("[JAction] [j9] cancelled j8");
                })
                .Execute();


            //Reset a JAction
            j1.Reset();





            Transform Canvas = GameObject.Find("Canvas").transform;

            /*
            * ====================================
            *           JUI LOOP EXAMPLE
            * ====================================
            */
            var JUILoopExampleGO = new GameObject("JUILoopExampleBtn");
            JUILoopExampleGO.transform.SetParent(Canvas, false);

            var JUILoopExampleText = JUILoopExampleGO.AddComponent<Text>();
            JUILoopExampleText.text = "[Press me to see LOOP example]";
            JUILoopExampleText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            JUILoopExampleText.fontSize = 30;
            JUILoopExampleText.color = Color.red;
            JUILoopExampleText.alignment = TextAnchor.MiddleCenter;

            JUILoopExampleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 75);
            JUILoopExampleGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(-250, -100);
            var JUILoopExampleBtn = JUILoopExampleGO.AddComponent<Button>();
            JUILoopExampleBtn.onClick.AddListener(
                () =>
                {
                    var JUILoopBG = new GameObject("JUILoopBG").AddComponent<Image>();
                    JUILoopBG.transform.SetParent(Canvas);
                    JUILoopBG.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
                    JUILoopBG.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    JUILoopBG.color = new Color(0.2f, 0.2f, 0.2f);

                    GameObject Showcase = new GameObject("CountdownShowcase");

                    //JUI DEMO
                    int i = 10;
                    var JUI = Showcase.AddComponent<JUI>()
                    .onInit(t =>
                    {
                        Log.Print("[JUI] JUI Loop Example has been inited");

                        var text = t.Element<Text>();

                        Showcase.transform.SetParent(JUILoopBG.transform);
                        Showcase.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, 100);
                        Showcase.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

                        text.text = "I will be sestroyed in 10 seconds";
                        text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                        text.fontSize = 50;
                        text.color = Color.white;
                        text.alignment = TextAnchor.MiddleCenter;

                        t.FrameMode = false;//Run in ms
                        t.Frequency = 1000;//Loop each 1s

                        UnityEngine.Object.Destroy(JUILoopBG.gameObject, 10);
                    })
                    .onLoop(t1 =>
                    {
                        i--;
                        t1.Element<Text>().text = "I will be destroyed in " + i + " seconds";
                        Log.Print("[JUI] JUI Loop Example is doing loop!");
                    })
                    .onEnd(t2 =>
                    {
                        Log.Print("[JUI] JUI Loop Example has been destroyed!");
                    })
                    .Activate();

                });


            /*
            * ====================================
            *           JUI Bind EXAMPLE
            * ====================================
            */
            var JUIBindExampleGO = new GameObject("JUIBindExampleBtn");
            JUIBindExampleGO.transform.SetParent(Canvas, false);

            var JUIBindExampleText = JUIBindExampleGO.AddComponent<Text>();
            JUIBindExampleText.text = "[Press me to see BIND example]";
            JUIBindExampleText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            JUIBindExampleText.fontSize = 30;
            JUIBindExampleText.color = Color.red;
            JUIBindExampleText.alignment = TextAnchor.MiddleCenter;

            JUIBindExampleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 75);
            JUIBindExampleGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(250, -100);
            var JUIBindExampleBtn = JUIBindExampleGO.AddComponent<Button>();
            JUIBindExampleBtn.onClick.AddListener(
                () =>
                {
                    var JUIBindBG = new GameObject("JUIBindBG").AddComponent<Image>();
                    JUIBindBG.transform.SetParent(Canvas);
                    JUIBindBG.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
                    JUIBindBG.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    JUIBindBG.color = new Color(0.2f, 0.2f, 0.2f);

                    var Description = new GameObject("Description");
                    Description.transform.SetParent(JUIBindBG.transform, false);
                    var DescriptionText = Description.AddComponent<Text>();
                    DescriptionText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                    DescriptionText.fontSize = 35;
                    DescriptionText.color = Color.white;
                    DescriptionText.alignment = TextAnchor.MiddleCenter;
                    Description.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, 75);
                    Description.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
                    DescriptionText.text = "Below is a JUI Bind demo, which data updates in each second";

                    var A = new GameObject("A");
                    A.transform.SetParent(JUIBindBG.transform, false);
                    var AText = A.AddComponent<Text>();
                    AText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                    AText.fontSize = 27;
                    AText.color = Color.red;
                    AText.alignment = TextAnchor.MiddleCenter;
                    A.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 75);
                    A.GetComponent<RectTransform>().anchoredPosition = new Vector2(-250, -100);

                    var B = new GameObject("B");
                    B.transform.SetParent(JUIBindBG.transform, false);
                    var BText = B.AddComponent<Text>();
                    BText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                    BText.fontSize = 27;
                    BText.color = Color.red;
                    BText.alignment = TextAnchor.MiddleCenter;
                    B.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 75);
                    B.GetComponent<RectTransform>().anchoredPosition = new Vector2(250, -100);

                    JUIBindBG.gameObject.AddComponent<JUIShowcase>();

                    var Close = new GameObject("Close");
                    Close.transform.SetParent(JUIBindBG.transform, false);
                    var CloseText = Close.AddComponent<Text>();
                    CloseText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                    CloseText.fontSize = 20;
                    CloseText.color = Color.white;
                    CloseText.alignment = TextAnchor.MiddleCenter;
                    Close.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, 75);
                    Close.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -300);
                    CloseText.text = "[Close this Example]";
                    var CloseBtn = Close.AddComponent<Button>();
                    CloseBtn.onClick.AddListener(() =>
                    {
                        UnityEngine.Object.Destroy(JUIBindBG.gameObject);
                        UnityEngine.Object.Destroy(GameObject.Find("BindShowcase"));
                    });
                });


            /*
            * ====================================
            *           JBehaviour Example
            * ====================================
            */
            var JBehaviourExampleGO = new GameObject("JBehaviourExampleBtn");
            JBehaviourExampleGO.transform.SetParent(Canvas, false);

            var JBehaviourExampleText = JBehaviourExampleGO.AddComponent<Text>();
            JBehaviourExampleText.text = "[Press me to see JBehaviour example]";
            JBehaviourExampleText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            JBehaviourExampleText.fontSize = 30;
            JBehaviourExampleText.color = Color.red;
            JBehaviourExampleText.alignment = TextAnchor.MiddleCenter;

            JBehaviourExampleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 75);
            JBehaviourExampleGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -300);
            var JBehaviourExampleBtn = JBehaviourExampleGO.AddComponent<Button>();
            JBehaviourExampleBtn.onClick.AddListener(
                () =>
                {
                    new GameObject("JBehaviourShowcase").AddComponent<JBehaviourExample>();
                });
        }
    }
}
