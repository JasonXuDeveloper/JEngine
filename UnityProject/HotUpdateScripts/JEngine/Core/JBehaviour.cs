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
using System;
using UnityEngine;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using JEngine.UI;

namespace JEngine.Core
{
    /// <summary>
    /// JEngine's Behaviour
    /// </summary>
    public class JBehaviour : IJBehaviour
    {
        /// <summary>
        /// Constuctor
        /// 构造函数
        /// </summary>
        public JBehaviour()
        {
            //添加实例ID
            _instanceID = JBehaviourMgr.Instance.GetJBehaviourInstanceID();
            JBehaviours.Add(_instanceID, this);

            LoopAwaitToken = new CancellationTokenSource();
        }

        /// <summary>
        /// Manager for JBehaviours
        /// </summary>
        private class JBehaviourMgr : MonoBehaviour
        {
            /// <summary>
            /// JBehaviour管理实例
            /// </summary>
            public static JBehaviourMgr Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new GameObject("JBehaviourMgr").AddComponent<JBehaviourMgr>();
                    }
                    return _instance;
                }
            }
            private static JBehaviourMgr _instance;

            /// <summary>
            /// Get Instance ID for JBehaviour
            /// </summary>
            /// <returns></returns>
            public string GetJBehaviourInstanceID()
            {
                var _instanceID = System.Guid.NewGuid().ToString("N");
                while (JBehaviours.ContainsKey(_instanceID))
                {
                    _instanceID = System.Guid.NewGuid().ToString("N");
                }
                return _instanceID;
            }

            /// <summary>
            /// Create new thread to cancel task.delay
            /// </summary>
            private void Awake()
            {
                DontDestroyOnLoad(this);
                StartCoroutine(RepeatCheckJBehaviour());
            }

            /// <summary>
            /// Coroutine to check JBehaviour
            /// </summary>
            /// <returns></returns>
            IEnumerator RepeatCheckJBehaviour()
            {
                while (true)
                {
                    CheckJBehaviour();
                    yield return null;
                }
            }

            /// <summary>
            /// Check and cancel task.delay
            /// </summary>
            private void CheckJBehaviour()
            {
                for (int i = 0; i < JBehaviours.Count; i++)
                {
                    var jb = JBehaviours.ElementAt(i);
                    if(jb.Value == null)
                    {
                        JBehaviours.Remove(jb.Key);
                        continue;
                    }
                    try
                    {
                        if (jb.Value._gameObject == null)
                        {
                            jb.Value.LoopAwaitToken?.Cancel();
                            JBehaviours[jb.Value._instanceID] = null;
                            JBehaviours.Remove(jb.Value._instanceID);
                            i--;
                        }
                        else
                        {
                            if (jb.Value._gameObject.activeInHierarchy == jb.Value.Hidden)
                            {
                                if (jb.Value.Hidden)
                                {
                                    jb.Value.OnShow();
                                }
                                else
                                {
                                    jb.Value.OnHide();
                                }
                                jb.Value.Hidden = !jb.Value.Hidden;
                            }
                        }
                    }
                    catch (MissingReferenceException)
                    {
                        jb.Value.LoopAwaitToken?.Cancel();
                        JBehaviours[jb.Value._instanceID] = null;
                        JBehaviours.Remove(jb.Value._instanceID);
                        i--;
                    }
                }
            }

            private void OnDestroy()
            {
                CheckJBehaviour();
                JBehaviours = null;
            }
        }

        #region STATIC METHODS

        /// <summary>
        /// Create a JBehaviour on a gameObject
        /// 在游戏对象上创建JBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="activeAfter"></param>
        /// <returns></returns>
        public static T CreateOn<T>(GameObject gameObject, bool activeAfter = true) where T : JBehaviour
        {
            //编辑器下可视化
            if (Application.isEditor)
            {
                var id = AddClassBind(gameObject, activeAfter, typeof(T));
                return (T)JBehaviours[id];
            }
            else//不然直接返回实例
            {
                T val = System.Activator.CreateInstance<T>();
                val._gameObject = gameObject;
                if (activeAfter) val.Activate();
                return val;
            }
        }

        /// <summary>
        /// Add a classbind to gameobject to visualize
        /// 加ClassBind至GameObject以可视化
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="activeAfter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string AddClassBind(GameObject gameObject, bool activeAfter, Type type)
        {

            var jBehaviour = type;
            var cb = gameObject.AddComponent<ClassBind>();
            var cd = new ClassData()
            {
                classNamespace = "",
                className = jBehaviour.FullName,
                activeAfter = activeAfter,
            };
            var id = (cb.AddClass(cd) as JBehaviour)._instanceID;
            cb.Active(cd);
            UnityEngine.Object.Destroy(cb);
            return id;
        }

        /// <summary>
        /// Get JBehaviour from a gameObject
        /// 通过游戏对象获取JBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T GetJBehaviour<T>(GameObject gameObject) where T : JBehaviour
        {
            return (T)JBehaviours.Values.ToList().Find(jb => jb._gameObject == gameObject && jb.GetType() == typeof(T));
        }

        /// <summary>
        /// Get JBehaviour from an instance id
        /// 通过实例ID获取JBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T GetJBehaviour<T>(string instanceID) where T : JBehaviour
        {
            return (T)JBehaviours.Values.ToList().Find(jb => jb._instanceID == instanceID);
        }

        /// <summary>
        /// Get All JBehaviour from a gameObject
        /// 通过游戏对象获取全部JBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T[] GetJBehaviours<T>(GameObject gameObject) where T : JBehaviour
        {
            return (T[])JBehaviours.Values.ToList().FindAll(jb => jb._gameObject == gameObject && jb.GetType() == typeof(T)).ToArray();
        }

        /// <summary>
        /// Remove a JBehaviour
        /// </summary>
        /// <param name="jBehaviour"></param>
        public static void RemoveJBehaviour(JBehaviour jBehaviour)
        {
            if (Application.isEditor)
            {
                Tools.DestroyHotComponent(jBehaviour.gameObject, jBehaviour);
            }
            jBehaviour.Destroy();
        }

        #endregion


        #region FIELDS + PROPERTIES

        /// <summary>
        /// All JBehaviuours
        /// 全部JBehaviour
        /// </summary>
        private static Dictionary<string, JBehaviour> JBehaviours = new Dictionary<string, JBehaviour>(0);


        /// <summary>
        /// Instance ID
        /// 实例ID
        /// </summary>
        public string InstanceID => _instanceID;
        [ClassBindIgnore] private string _instanceID;

        /// <summary>
        /// GameObject of this instance
        /// 游戏对象
        /// </summary>
        public GameObject gameObject => _gameObject;
        [ClassBindIgnore] private GameObject _gameObject;

        /// <summary>
        /// Loop in frame or millisecond
        /// 帧模式或毫秒模式
        /// </summary>
        [ClassBindIgnore] public bool FrameMode = true;

        /// <summary>
        /// Frequency of loop, if frame = false, this field stands for milliseconds
        /// 循环频率，如果是毫秒模式，单位就是ms
        /// </summary>
        [ClassBindIgnore] public int Frequency = 1;

        /// <summary>
        /// Total time that this JBehaviour has run
        /// 该JBehaviour运行总时长
        /// </summary>
        [ClassBindIgnore] public float TotalTime = 0;

        /// <summary>
        /// Deltatime of loop
        /// 循环耗时
        /// </summary>
        [ClassBindIgnore] public float LoopDeltaTime = 0;

        /// <summary>
        /// Loop counts
        /// 循环次数
        /// </summary>
        [ClassBindIgnore] public long LoopCounts = 0;


        /// <summary>
        /// Time scale
        /// 时间倍速
        /// </summary>
        [ClassBindIgnore] public float TimeScale = 1;

        /// <summary>
        /// Pause before init
        /// 是否暂停
        /// </summary>
        [ClassBindIgnore] private bool Paused = false;


        /// <summary>
        /// Is gameObject hidden
        /// 是否隐藏
        /// </summary>
        [ClassBindIgnore] private bool Hidden = false;

        #endregion

        #region METHODS
        /// <summary>
        /// Hides the UI gameObject
        /// 隐藏UI对象
        /// </summary>
        public JBehaviour Hide()
        {
            if (this._gameObject != null)
            {
                this._gameObject.SetActive(false);
            }
            Hidden = true;
            OnHide();
            return this;
        }

        /// <summary>
        /// Shows the UI gameObject
        /// 显示UI对象
        /// </summary>
        public JBehaviour Show()
        {
            if (this._gameObject != null)
            {
                this._gameObject.SetActive(true);
            }
            Hidden = false;
            OnShow();
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
        /// 恢复循环
        /// </summary>
        public JBehaviour Resume()
        {
            Paused = false;
            return this;
        }

        /// <summary>
        /// Activate the JBehaviour
        /// 激活
        /// </summary>
        /// <returns></returns>
        public JBehaviour Activate()
        {
            this.Awake();
            return this;
        }
        #endregion

        #region INTERNAL

        /// <summary>
        /// Launch the lifecycle
        /// 开始生命周期
        /// </summary>
        private protected async void Launch()
        {
            while (!LoopAwaitToken.IsCancellationRequested)
            {
                if (JBehaviours is null || _gameObject.activeSelf)
                {
                    break;
                }

                try
                {
                    await Task.Delay(10, LoopAwaitToken.Token);
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException)
                    {
                        return;
                    }
                }
            }

            if (JBehaviours is null || _gameObject is null)
            {
                return;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                Init();
            }
            catch (Exception e)
            {
                Log.PrintError($"{_gameObject.name}<{_instanceID}> Init failed: {e.Message}, {e.Data["StackTrace"]}, skipped init");
            }

            try
            {
                Run();
            }
            catch (Exception e)
            {
                Log.PrintError($"{_gameObject.name}<{_instanceID}> Run failed: {e.Message}, {e.Data["StackTrace"]}, skipped run");
            }

            sw.Stop();
            TotalTime += sw.ElapsedMilliseconds / 1000f;
            DoLoop();
        }

        /// <summary>
        /// Cancel delay
        /// 取消延迟
        /// </summary>
        [ClassBindIgnore] private CancellationTokenSource LoopAwaitToken;

        /// <summary>
        /// Do the loop
        /// </summary>
        private protected async void DoLoop()
        {
            Stopwatch sw = new Stopwatch();

            while (_gameObject != null && !LoopAwaitToken.IsCancellationRequested)
            {
                if (Paused)//暂停
                {
                    await Task.Delay(25);
                    continue;
                }


                //调整参数
                if (TimeScale < 0.001f)
                {
                    TimeScale = 1;
                }
                if (Frequency <= 0)
                {
                    Frequency = 1;
                }

                sw.Reset();
                sw.Start();

                try//循环
                {
                    Loop();
                }
                catch (Exception ex)
                {
                    Log.PrintError($"{_gameObject.name}<{_instanceID}> Loop failed: {ex.Message}, {ex.Data["StackTrace"]}");
                    return;
                }

                int duration;
                if (FrameMode)//等待
                {
                    duration = (int)(((float)Frequency / ((float)Application.targetFrameRate <= 0 ? GameStats.FPS : Application.targetFrameRate)) * 1000f);
                    duration = (int)(duration / TimeScale);
                }
                else
                {
                    duration = Frequency;
                    duration = (int)(duration / TimeScale);
                }
                if (duration < -1)
                {
                    duration = 1;
                }
                try
                {
                    await Task.Delay(duration, LoopAwaitToken.Token);
                }
                catch (Exception ex)
                {
                    //会抛出TaskCanceledException，表示等待被取消，直接Destory
                    if (ex is TaskCanceledException)
                    {
                        Destroy();
                        return;
                    }
                }

                sw.Stop();

                //操作时间
                var time = sw.ElapsedMilliseconds / 1000f;
                LoopCounts++;
                LoopDeltaTime = time;
                TotalTime += time;
            }
            Destroy();
        }

        /// <summary>
        /// Call end method
        /// 调用周期销毁
        /// </summary>
        private protected void Destroy()
        {
            _gameObject = null;
            LoopAwaitToken = null;
            if (Application.isPlaying)
            {
                End();
            }
        }

        /// <summary>
        /// Call to launch JBehaviour
        /// 启动生命周期
        /// </summary>
        private protected void Awake()
        {
            if (_gameObject == null)
            {
                _gameObject = new GameObject(_instanceID);//生成GameObject

                //编辑器下可视化
                if (Application.isEditor)
                {
                    //绑定上去
                    var id = AddClassBind(_gameObject, false, this.GetType());

                    //替换自己本身的id
                    JBehaviours.Remove(_instanceID);//移除构造函数创的自己的JBehaviour
                    this._instanceID = id;//更改id
                    this._gameObject.name = id;//改名
                    JBehaviours[id] = this;//覆盖字典里的值
                }
            }
            Launch();
        }

        private void ResetJBehaviour(GameObject go)
        {
            _gameObject = go;
            _instanceID = JBehaviourMgr.Instance.GetJBehaviourInstanceID();
            JBehaviours.Add(_instanceID, this);

            LoopAwaitToken = new CancellationTokenSource();
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

        public virtual void OnShow()
        {

        }

        public virtual void OnHide()
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

        /// <summary>
        /// 显示
        /// </summary>
        void OnShow();

        /// <summary>
        /// 隐藏
        /// </summary>
        void OnHide();
    }
}
