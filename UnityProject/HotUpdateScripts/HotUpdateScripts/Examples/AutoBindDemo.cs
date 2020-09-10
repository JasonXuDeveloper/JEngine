﻿//
// AutoBindDemo.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 
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
using JEngine.LifeCycle;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JEngine.Examples
{
    public class AutoBindDemo1 : JBehaviour
    {
        public int IntField1;

        public string StringField1;

        public EventSystem EventSystemField1;

        public GameObject GameObjectField1;


        public override void Init()
        {
            Log.Print("[Autobind] AutoBindDemo1::Inited");
            GameObjectField1.SetActive(!GameObjectField1.activeSelf);
        }
    }

    public class AutoBindDemo2 : MonoBehaviour
    {
        public int PublicField2;

        [SerializeField] private string AttributeField2;

        public TextAsset txtFile;

        public void Awake()
        {
            Log.Print("[Autobind] AutoBindDemo2::Started");
            Log.Print($"[Autobind] txtFile value is: {txtFile.text}");
        }
    }
}
