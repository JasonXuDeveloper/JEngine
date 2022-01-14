//
// ClassBindDemo.cs
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
using UnityEngine;
using UnityEngine.EventSystems;

namespace JEngine.Examples
{
    public class ClassBindDemo1 : JBehaviour
    {
        public int IntField1;

        public string StringField1;

        public EventSystem EventSystemField1;

        public GameObject GameObjectField1;

        public bool BoolProperty
        {
            get => BoolPropertyInstance;
            set => BoolPropertyInstance = value;
        }

        [SerializeField] private bool BoolPropertyInstance;

        ClassBindDemo2 a2 = null;


        public override void Init()
        {
            Log.Print("[ClassBindDemo1] ClassBindDemo1::Inited");
            //GameObjectField1.SetActive(!GameObjectField1.activeSelf);
            Log.Print($"[ClassBindDemo1] a2 is null? {a2 is null}");
        }
    }

    public class ClassBindDemo2 : MonoBehaviour
    {
        public TextAsset txtFile;

        ClassBindDemo1 a1 = null;

        [HideInInspector] public float floatField;

        public void Awake()
        {
            Log.Print("[ClassBindDemo2] ClassBindDemo2::Awake");
            Log.Print($"[ClassBindDemo2] txtFile value is: {txtFile.text}");
            Log.Print($"[ClassBindDemo2] a1 is null? {a1 is null}");
            Log.Print($"[ClassBindDemo2] floatField: {floatField}");
        }

        private void Start()
        {
            Log.Print("[ClassBindDemo2] ClassBindDemo2::Start");
        }

        private void FixedUpdate()
        {
            Log.Print("[ClassBindDemo2] ClassBindDemo2::FixedUpdate");
        }

        private void Update()
        {
            Log.Print("[ClassBindDemo2] ClassBindDemo2::Update");
        }

        private void LateUpdate()
        {
            Log.Print("[ClassBindDemo2] ClassBindDemo2::LateUpdate");
        }

        private void OnEnable()
        {
            Log.Print("[ClassBindDemo2] ClassBindDemo2::OnEnable");
        }

        private void OnDisable()
        {
            Log.Print("[ClassBindDemo2] ClassBindDemo2::OnDisable");
        }

        private void OnDestroy()
        {
            Log.Print("[ClassBindDemo2] ClassBindDemo2::OnDestroy");
        }
    }

    public class ClassBindDemo3 : MonoBehaviour, AInterface
    {
        public int a { get; set; }
        public AInterface aIn { get; set; }
        public void Awake()
        {
            Debug.Log("[ClassBindDemo3] a=" + a);
            Debug.Log("[ClassBindDemo3] aIn == (AInterface)this: " + (aIn == (AInterface)this));
        }
    }

    public interface AInterface
    {
        int a { get; set; }
        AInterface aIn { get; set; }
        void Awake();
    }

    public class ClassBindDemo
    {
        public void Awake()
        {
            Debug.Log("<color=red>[ClassBind] 这个周期ClassBind正在初始化，同时会调用ClassBind创建的实例的Awake方法（可选）</color>");
        }
    }
}
