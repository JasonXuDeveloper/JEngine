//
// JUIBehaviour.cs
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
using UnityEngine;
namespace JEngine.LifeCycle
{
    public class JUIBehaviour : MonoBehaviour, IJBehaviour
    {
        #region FIELDS
        /// <summary>
        /// Loop in frame or millisecond
        /// 帧模式或毫秒模式
        /// </summary>
        [HideInInspector] public bool frame = true;

        /// <summary>
        /// Frequency of loop, if frame = false, this field stands for milliseconds
        /// 循环频率，如果是毫秒模式，单位就是ms
        /// </summary>
        [HideInInspector] public int frequency = 1;
        #endregion

        #region METHODS
        /// <summary>
        /// Hides the UI gameObject
        /// 隐藏UI对象
        /// </summary>
        public void Hide()
        {
            if (this.gameObject != null)
            {
                this.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Shows the UI gameObject
        /// 显示UI对象
        /// </summary>
        public void Show()
        {
            if (this.gameObject != null)
            {
                this.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Call Init method in MonoBehaviour 
        /// 在MonoBehaviour中调用Init方法
        /// </summary>
        private void Awake()
        {
            Init();
        }

        /// <summary>
        /// Call Run method in MonoBehaviour
        /// 调用Run
        /// </summary>
        private void Start()
        {
            Run();
            StartCoroutine(DoLoop());
        }


        /// <summary>
        /// Do Loop
        /// 循环
        /// </summary>
        private IEnumerator DoLoop()
        {
            while (true && Application.isPlaying)
            {
                Loop();

                if (frame)
                {
                    for (int f = frequency; f > 0; f--)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                else
                {
                    yield return new WaitForSeconds((float)frequency / 1000);
                }
            }
        }

        /// <summary>
        /// Call end method
        /// 调用周期销毁
        /// </summary>
        private void OnDestroy()
        {
            End();
        }

        #endregion


        #region METHODS THAT ARE REWRITABLE
        public virtual void Init()
        {

        }

        public virtual void Run()
        {

        }

        public virtual void Loop()
        {

        }

        public virtual void End()
        {

        }

        #endregion
    }

    interface IJBehaviour
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Init();

        /// <summary>
        /// 开始
        /// </summary>
        void Run();

        /// <summary>
        /// 循环
        /// </summary>
        void Loop();

        /// <summary>
        /// 销毁
        /// </summary>
        void End();
    }
}