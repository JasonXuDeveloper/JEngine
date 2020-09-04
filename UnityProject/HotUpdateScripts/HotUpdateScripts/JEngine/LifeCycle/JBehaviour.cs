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
using System.Threading.Tasks;
using UnityEngine;

namespace JEngine.LifeCycle
{
    /// <summary>
    /// JEngine's Behaviour
    /// </summary>
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
        [HideInInspector] private bool Paused;

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
        public JBehaviour Hide()
        {
            if (this.gameObject != null)
            {
                this.gameObject.SetActive(false);
            }
            return this;
        }

        /// <summary>
        /// Shows the UI gameObject
        /// 显示UI对象
        /// </summary>
        public JBehaviour Show()
        {
            if (this.gameObject != null)
            {
                this.gameObject.SetActive(true);
            }
            return this;
        }

        /// <summary>
        /// Pause the loop
        /// 暂停循环
        /// </summary>
        public JBehaviour Pause()
        {
            Paused = true;
            return this;
        }

        /// <summary>
        /// Resume the loop
        /// </summary>
        public JBehaviour Resume()
        {
            Paused = false;
            return this;
        }

        public JBehaviour Activate()
        {
            this.enabled = true;
            this.Awake();
            return this;
        }

        /// <summary>
        /// Launch the lifecycle
        /// 开始生命周期
        /// </summary>
        private async void Launch()
        {
            while (Paused)
            {
                await Task.Delay(25);
            }

            Init();
            Inited = true;

            while (!Inited)
            {
                await Task.Delay(25);
            }
            Run();
            HasRun = true;

            if (!NotLoop)
            {
                while (!HasRun)
                {
                    await Task.Delay(25);
                }

                DoLoop();
            }
            return;
        }

        /// <summary>
        /// Whether object is alive or not
        /// </summary>
        private bool _alive;
        /// <summary>
        /// Do the loop
        /// </summary>
        private async void DoLoop()
        {
            _alive = true;
            if (Frequency == 0)
            {
                Frequency = 1;
            }
            while (true && Application.isPlaying && _alive)
            {
                if (Paused) break;
                Loop();
                if (FrameMode)
                    await Task.Delay((int)(((float)Frequency / (float)Application.targetFrameRate) * 1000f));
                else
                    await Task.Delay(Frequency);
            }
        }

        /// <summary>
        /// Call end method
        /// 调用周期销毁
        /// </summary>
        private void OnDestroy()
        {
            _alive = false;
            End();
        }

        #endregion


        #region METHODS THAT ARE REWRITABLE
        /// <summary>
        /// Call to launch JBehaviour
        /// 启动生命周期
        /// </summary>
        public virtual void Awake()
        {
            Launch();
        }

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