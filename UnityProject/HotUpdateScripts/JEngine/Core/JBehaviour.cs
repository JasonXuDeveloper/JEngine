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
using System.Diagnostics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Local

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
        protected JBehaviour()
        {
            //添加实例ID
            _instanceID = GetJBehaviourInstanceID();
            JBehaviours.Add(_instanceID, this);
            JBehavioursList.Add(this);
        }

        /// <summary>
        /// Get Instance ID for JBehaviour
        /// </summary>
        /// <returns></returns>
        private static string GetJBehaviourInstanceID()
        {
            var instanceID = Guid.NewGuid().ToString("N");
            while (JBehaviours.ContainsKey(instanceID))
            {
                instanceID = Guid.NewGuid().ToString("N");
            }

            return instanceID;
        }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static JBehaviour()
        {
            //注册循环
            ThreadMgr.QueueOnMainThread(() =>
            {
                CoroutineMgr.Instance.StartCoroutine(JBehavioursLoop());
            });
        }

        /// <summary>
        /// Do the loop
        /// </summary>
        private static IEnumerator JBehavioursLoop()
        {
            Stopwatch sw = new Stopwatch();
            for (;;)
            {
                if (!Application.isPlaying) break;
                yield return ConstMgr.WaitFor1Sec;
                int cnt = LoopJBehaviours.Count;
                for (int i = 0; i < cnt; i++)
                {
                    var jb = LoopJBehaviours[i];
                    if (jb._gameObject != null)
                    {
                        if (jb._gameObject.activeInHierarchy == jb._hidden)
                        {
                            if (jb._hidden)
                            {
                                jb.OnShow();
                            }
                            else
                            {
                                jb.OnHide();
                            }

                            jb._hidden = !jb._hidden;
                        }

                        if (jb._paused || jb._hidden)
                        {
                            continue;
                        }


                        //调整参数
                        if (jb.TimeScale < 0.001f)
                        {
                            jb.TimeScale = 1;
                        }

                        if (jb.Frequency <= 0)
                        {
                            jb.Frequency = 1;
                        }

                        float duration;
                        if (jb.FrameMode) //等待
                        {
                            duration = jb.Frequency / ((float)Application.targetFrameRate <= 0
                                ? FpsMonitor.FPS
                                : Application.targetFrameRate);
                        }
                        else
                        {
                            duration = jb.Frequency / 1000f;
                        }

                        duration /= jb.TimeScale;

                        if (duration <= 0)
                        {
                            duration = 0.001f;
                        }
                        
                        if (Time.realtimeSinceStartup - jb._curTime < duration)
                        {
                            continue;
                        }

                        jb._curTime = Time.realtimeSinceStartup;

                        sw.Restart();
                        jb.DoLoop();
                        sw.Stop();

                        //操作时间
                        var time = sw.ElapsedMilliseconds / 1000f + duration;
                        jb.LoopCounts++;
                        jb.LoopDeltaTime = time;
                        jb.TotalTime += time;
                    }
                    else
                    {
                        LoopJBehaviours.RemoveAt(i);
                        i--;
                        cnt = LoopJBehaviours.Count;
                        jb.Destroy();
                    }
                }
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
                var ret = (T)JBehaviours[id];
                ret.Check();
                return ret;
            }

            //不然直接返回实例
            T val = Activator.CreateInstance<T>();
            val._gameObject = gameObject;
            val.Check();
            if (activeAfter) val.Activate();
            return val;
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
            var id = ((JBehaviour)cb.AddClass(cd))._instanceID;
            cb.Active(cd);
            UnityEngine.Object.Destroy(cb);
            LifeCycleMgr.Instance.ExecuteOnceTask();
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
            if (!GameObjectJBehaviours.ContainsKey(gameObject))
            {
                return null;
            }

            foreach (var jb in GameObjectJBehaviours[gameObject])
            {
                if (jb.CanAssignTo(typeof(T)))
                {
                    return (T)jb;
                }
            }

            return null;
        }

        /// <summary>
        /// Get JBehaviour from an instance id
        /// 通过实例ID获取JBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instanceID"></param>
        /// <returns></returns>
        public static T GetJBehaviour<T>(string instanceID) where T : JBehaviour
        {
            return (T)JBehavioursList.Find(jb => jb._instanceID == instanceID);
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
            if (!GameObjectJBehaviours.ContainsKey(gameObject))
            {
                return Array.Empty<T>();
            }

            return (T[])GameObjectJBehaviours[gameObject].ToList().FindAll(jb => jb.CanAssignTo(typeof(T))).ToArray();
        }

        /// <summary>
        /// Find a JBehaviour that is the given type
        /// 通过指定类型寻找一个JBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FindJBehaviourOfType<T>() where T : JBehaviour
        {
            return (T)JBehavioursList.First(j => j is T);
        }

        /// <summary>
        /// Find all JBehaviours that are the given type
        /// 通过指定类型寻找全部JBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] FindJBehavioursOfType<T>() where T : JBehaviour
        {
            return (T[])JBehavioursList.FindAll(j => j is T).ToArray();
        }

        /// <summary>
        /// Remove a JBehaviour
        /// 删除一个JBehaviour
        /// </summary>
        /// <param name="jBehaviour"></param>
        public static void RemoveJBehaviour(JBehaviour jBehaviour)
        {
            if (Application.isEditor)
            {
                jBehaviour.gameObject.DestroyHotComponent(jBehaviour);
            }

            jBehaviour.Destroy();
        }

        #endregion


        #region FIELDS + PROPERTIES

        /// <summary>
        /// All JBehaviuours
        /// 全部JBehaviour
        /// </summary>
        private static readonly Dictionary<string, JBehaviour> JBehaviours = new Dictionary<string, JBehaviour>(17);

        /// <summary>
        /// All JBehaviours list
        /// 全部JBehaviour的列表
        /// </summary>
        private static readonly List<JBehaviour> JBehavioursList = new List<JBehaviour>(17);

        /// <summary>
        /// All JBehaviours to loop
        /// 全部待循环的JBehaviour
        /// </summary>
        private static readonly List<JBehaviour> LoopJBehaviours = new List<JBehaviour>(17);

        /// <summary>
        /// All JBehaviour in specefic GameObject
        /// 全部在某个GameObject上的JBehaviour
        /// </summary>
        private static readonly Dictionary<GameObject, HashSet<JBehaviour>> GameObjectJBehaviours =
            new Dictionary<GameObject, HashSet<JBehaviour>>(17);

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
        [ClassBindIgnore] public float TotalTime;

        /// <summary>
        /// Deltatime of loop
        /// 循环耗时
        /// </summary>
        [ClassBindIgnore] public float LoopDeltaTime;

        /// <summary>
        /// Loop counts
        /// 循环次数
        /// </summary>
        [ClassBindIgnore] public long LoopCounts;

        /// <summary>
        /// Time scale
        /// 时间倍速
        /// </summary>
        [ClassBindIgnore] public float TimeScale = 1;

        /// <summary>
        /// Pause before init
        /// 是否暂停
        /// </summary>
        [ClassBindIgnore] private bool _paused;

        /// <summary>
        /// Is gameObject hidden
        /// 是否隐藏
        /// </summary>
        [ClassBindIgnore] private bool _hidden;

        /// <summary>
        /// Current loop time
        /// 当前循环的时间
        /// </summary>
        [ClassBindIgnore] private float _curTime;


        #endregion

        #region METHODS

        /// <summary>
        /// Hides the UI gameObject
        /// 隐藏UI对象
        /// </summary>
        public JBehaviour Hide()
        {
            if (_gameObject != null)
            {
                _gameObject.SetActive(false);
            }

            _hidden = true;
            OnHide();
            return this;
        }

        /// <summary>
        /// Shows the UI gameObject
        /// 显示UI对象
        /// </summary>
        public JBehaviour Show()
        {
            if (_gameObject != null)
            {
                _gameObject.SetActive(true);
            }

            _hidden = false;
            OnShow();
            return this;
        }

        /// <summary>
        /// Pause the loop
        /// 暂停循环
        /// </summary>
        public JBehaviour Pause()
        {
            _paused = true;
            return this;
        }

        /// <summary>
        /// Resume the loop
        /// 恢复循环
        /// </summary>
        public JBehaviour Resume()
        {
            _paused = false;
            return this;
        }

        /// <summary>
        /// Activate the JBehaviour
        /// 激活
        /// </summary>
        /// <returns></returns>
        public JBehaviour Activate()
        {
            //主线程
            ThreadMgr.QueueOnMainThread(() =>
            {
                Awake();
                var duration = 1f / ((float)Application.targetFrameRate <= 0
                    ? FpsMonitor.FPS
                    : Application.targetFrameRate);
                duration = duration / TimeScale;
                ThreadMgr.QueueOnMainThread(() =>
                {
                    OnEnable();
                    Start();
                }, duration);
            });
            return this;
        }

        #endregion

        #region INTERNAL

        /// <summary>
        /// Launch the lifecycle
        /// 开始生命周期
        /// </summary>
        private async void Launch()
        {
            while (!Application.isPlaying)
            {
                if (JBehaviours is null || _gameObject.activeSelf)
                {
                    break;
                }

                await Task.Delay(1);
            }

            if (JBehaviours is null || _gameObject is null)
            {
                return;
            }

            try
            {
                Init();
            }
            catch (Exception e)
            {
                Log.PrintError(
                    $"{_gameObject.name}<{_instanceID}> Init failed: {e.Message}, {e.Data["StackTrace"]}, skipped init");
            }
        }
        
        /// <summary>
        /// Do loop once
        /// 执行一次循环
        /// </summary>
        private void DoLoop()
        {
            try //循环
            {
                Loop();
            }
            catch (Exception ex)
            {
                Log.PrintError(
                    $"{_gameObject.name}<{_instanceID}> Loop failed: {ex.Message}, {ex.Data["StackTrace"]}");
            }
        }

        /// <summary>
        /// 设置Hidden状态
        /// </summary>
        private void SetHidden()
        {
            _hidden = !_gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Call end method
        /// 调用周期销毁
        /// </summary>
        private void Destroy()
        {
            var index = LoopJBehaviours.IndexOf(this);
            if (index != -1)
            {
                LoopJBehaviours.RemoveAt(index);
            }

            JBehaviours.Remove(_instanceID);
            GameObjectJBehaviours.Remove(_gameObject);
            _gameObject = null;
            if (Application.isPlaying)
            {
                End();
            }
        }


        [ClassBindIgnore] private bool _checked;

        /// <summary>
        /// 检测字典，应当在Awake之前被执行
        /// </summary>
        private void Check()
        {
            if (_checked) return;
            if (_gameObject == null)
            {
                _gameObject = new GameObject(_instanceID); //生成GameObject

                //编辑器下可视化
                if (Application.isEditor)
                {
                    //绑定上去
                    var id = AddClassBind(_gameObject, false, GetType());
                    JBehaviours.TryGetValue(InstanceID, out var item);
                    //替换自己本身的id
                    JBehaviours.Remove(_instanceID); //移除构造函数创的自己的JBehaviour
                    if (item != null)
                    {
                        JBehavioursList.Remove(item);
                    }

                    _instanceID = id; //更改id
                    _gameObject.name = id; //改名
                    JBehaviours[id] = this; //覆盖字典里的值
                }
            }

            if (!JBehavioursList.Contains(this))
            {
                JBehavioursList.Add(this);
            }

            if (!GameObjectJBehaviours.TryGetValue(_gameObject, out var lst))
            {
                lst = new HashSet<JBehaviour>();
                GameObjectJBehaviours.Add(_gameObject, lst);
            }

            lst.Add(this);
            _checked = true;
        }

        /// <summary>
        /// Call to launch JBehaviour
        /// 启动生命周期
        /// </summary>
        private void Awake()
        {
            Launch();
        }

        /// <summary>
        /// Awake后的下一帧检测Show或Hide然后Run
        /// </summary>
        private void OnEnable()
        {
            SetHidden();
            try
            {
                if (gameObject.activeInHierarchy)
                {
                    OnShow();
                }
                else
                {
                    OnHide();
                }
            }
            catch (Exception e)
            {
                Log.PrintError(
                    $"{_gameObject.name}<{_instanceID}> OnShow/OnHide failed: {e.Message}, {e.Data["StackTrace"]}, skipped OnShow/OnHide");
            }

            try
            {
                Run();
            }
            catch (Exception e)
            {
                Log.PrintError(
                    $"{_gameObject.name}<{_instanceID}> Run failed: {e.Message}, {e.Data["StackTrace"]}, skipped run");
            }
        }

        /// <summary>
        /// Run后下一帧开始Loop
        /// </summary>
        private void Start()
        {
            LoopJBehaviours.Add(this);
        }

        private void ResetJBehaviour(GameObject go)
        {
            _gameObject = go;
            JBehaviours.Remove(_instanceID);
            _instanceID = GetJBehaviourInstanceID();
            JBehaviours[_instanceID] = this;
            if (!JBehavioursList.Contains(this))
                JBehavioursList.Add(this);
            if (!GameObjectJBehaviours.TryGetValue(_gameObject, out var lst))
            {
                lst = new HashSet<JBehaviour>();
                GameObjectJBehaviours.Add(_gameObject, lst);
            }

            lst.Add(this);
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