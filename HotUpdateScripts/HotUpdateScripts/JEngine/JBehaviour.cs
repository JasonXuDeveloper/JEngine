//
// JBehaviour.cs
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
using UnityEngine;
namespace JEngine.LifeCycle
{
    public class JBehaviour : MonoBehaviour, IJBehaviour
    {
        #region FIELDS
        /// <summary>
        /// Loop in frame or millisecond
        /// 帧模式或毫秒模式
        /// </summary>
        [HideInInspector] public bool FrameMode;

        /// <summary>
        /// Frequency of loop, if frame = false, this field stands for milliseconds
        /// 循环频率，如果是毫秒模式，单位就是ms
        /// </summary>
        [HideInInspector] public int Frequency;

        /// <summary>
        /// Pause before init
        /// 在初始化之前暂停
        /// </summary>
        [HideInInspector] public bool Pause;

        /// <summary>
        /// Whether inited or not
        /// 是否完成初始化
        /// </summary>
        [HideInInspector] private bool Inited;

        /// <summary>
        /// Whether has run or not
        /// 是否完成Run
        /// </summary>
        [HideInInspector] private bool HasRun;

        /// <summary>
        /// JUI runing mode, loop means the UI will loop in specific frequency, message mode means UI will update when it has been called
        /// JUI运行模式，Loop模式下UI将在特定频率更新，Message模式UI将在被通知后更新
        /// </summary>
        [HideInInspector] public bool NotLoop;
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
        /// Call to launch JBehaviour
        /// 启动生命周期
        /// </summary>
        public virtual void Awake()
        {
            StartCoroutine(Launch());
        }

        /// <summary>
        /// Launch the lifecycle
        /// 开始生命周期
        /// </summary>
        private IEnumerator Launch()
        {
            yield return new WaitUntil(() => !Pause);

            Init();

            yield return new WaitUntil(() => Inited);
            Run();

            if (!NotLoop)
            {
                yield return new WaitUntil(() => HasRun);

                while (true && Application.isPlaying)
                {
                    Loop();

                    if (FrameMode)
                    {
                        for (int i = 0; i < Frequency; i++)
                        {
                            yield return null;
                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds((float)Frequency / 1000);
                    }
                }
            }
            yield break;
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
            Inited = true;
        }

        public virtual void Run()
        {
            HasRun = true;
        }

        public virtual void Loop()
        {

        }

        public virtual void End()
        {

        }

        #endregion
    }

    /// <summary>
    /// JBehaviour Interface
    /// </summary>
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