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
using System.Diagnostics;
using System.Collections.Generic;

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
            JBehavioursList.Add(this);
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
            /// Create new thread to cancel JEngine.Core.TimeMgr.Delay
            /// </summary>
            private void Awake()
            {
                DontDestroyOnLoad(this);
                if (JBehaviours == null)
                {
                    JBehaviours = new Dictionary<string, JBehaviour>(101);
                }
                if (GameObjectJBehaviours == null)
                {
                    GameObjectJBehaviours = new Dictionary<GameObject, HashSet<JBehaviour>>();
                }
            }

            /// <summary>
            /// Check and cancel JEngine.Core.TimeMgr.Delay
            /// </summary>
            private void Update()
            {
                int cnt = JBehavioursList.Count;
                for(int i=0;i<cnt;i++)
                {
                    var jb = JBehavioursList[i];
                    string k = jb.InstanceID;
                    if (k == null || !JBehaviours.ContainsKey(k))
                    {
                        JBehavioursCheckNull(k);
                        GameObjectDictCheckNullJBehaviour();
                        JBehavioursList.RemoveAt(i);
                        i--;
                        cnt--;
                        continue;
                    }
                    if (jb == null)
                    {
                        JBehaviours.Remove(k);
                        GameObjectDictCheckNullJBehaviour();
                        JBehavioursList.RemoveAt(i);
                        i--;
                        cnt--;
                        continue;
                    }
                    try
                    {
                        if (jb._gameObject == null)
                        {
                            GameObjectDictCheckNull();
                            jb.LoopAwaitToken?.Cancel();
                            JBehaviours.Remove(k);
                            JBehavioursList.RemoveAt(i);
                            i--;
                            cnt--;
                        }
                        else
                        {
                            if (jb.EnableHiddenMonitoring && jb._gameObject.activeInHierarchy == jb.Hidden)
                            {
                                if (jb.Hidden)
                                {
                                    jb.OnShow();
                                }
                                else
                                {
                                    jb.OnHide();
                                }
                                jb.Hidden = !jb.Hidden;
                            }
                        }
                    }
                    catch (MissingReferenceException)
                    {
                        jb.LoopAwaitToken?.Cancel();
                        JBehaviours.Remove(k);
                        JBehavioursList.RemoveAt(i);
                        i--;
                        cnt--;
                    }
                }
            }

            private void OnDestroy()
            {
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
                var ret = (T)JBehaviours[id];
                ret.Check();
                return ret;
            }
            else//不然直接返回实例
            {
                T val = System.Activator.CreateInstance<T>();
                val._gameObject = gameObject;
                val.Check();
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
            if (!GameObjectJBehaviours.ContainsKey(gameObject))
            {
                return null;
            }
            foreach(var jb in GameObjectJBehaviours[gameObject])
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
        /// <param name="gameObject"></param>
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
                return new T[0];
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
        private static Dictionary<string, JBehaviour> JBehaviours = new Dictionary<string, JBehaviour>(101);

        /// <summary>
        /// All JBehaviours list
        /// 全部JBehaviour的列表
        /// </summary>
        private static List<JBehaviour> JBehavioursList = new List<JBehaviour>(101);

        /// <summary>
        /// All JBehaviour in specefic GameObject
        /// 全部在某个GameObject上的JBehaviour
        /// </summary>
        private static Dictionary<GameObject, HashSet<JBehaviour>> GameObjectJBehaviours = new Dictionary<GameObject, HashSet<JBehaviour>>();

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


        /// <summary>
        /// Whether or not monitoring hidden status
        /// 是否监听Hidden
        /// </summary>
        [ClassBindIgnore] private bool EnableHiddenMonitoring = false;

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
            //主线程
            Loom.QueueOnMainThread(async () =>
            {
                Awake();
                int duration;
                duration = (int)((1f / ((float)Application.targetFrameRate <= 0 ? GameStats.FPS : Application.targetFrameRate)) * 1000f);
                duration = (int)(duration / TimeScale);
                await TimeMgr.Delay(duration);
                OnEnable();
                Start();
            });
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

                await JEngine.Core.TimeMgr.Delay(1);
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
                Log.PrintError($"{_gameObject.name}<{_instanceID}> Init failed: {e.Message}, {e.Data["StackTrace"]}, skipped init");
            }
        }

        /// <summary>
        /// 设置Hidden状态
        /// </summary>
        private protected void SetHidden()
        {
            Hidden = !_gameObject.activeInHierarchy;
        }

        /// <summary>
        /// 设置Hidden状态监听
        /// </summary>
        private protected void SetHiddenMonitoring(bool enable)
        {
            EnableHiddenMonitoring = enable;
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
                if (Paused || Hidden)//暂停或没Active
                {
                    await JEngine.Core.TimeMgr.Delay(10);
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

                await JEngine.Core.TimeMgr.Delay(duration);

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


        [ClassBindIgnore] private bool _checked = false;
        /// <summary>
        /// 检测字典，应当在Awake之前被执行
        /// </summary>
        private void Check()
        {
            if (_checked) return;
            if (_gameObject == null)
            {
                _gameObject = new GameObject(_instanceID);//生成GameObject

                //编辑器下可视化
                if (Application.isEditor)
                {
                    //绑定上去
                    var id = AddClassBind(_gameObject, false, this.GetType());
                    JBehaviours.TryGetValue(InstanceID, out var item);
                    //替换自己本身的id
                    JBehaviours.Remove(_instanceID);//移除构造函数创的自己的JBehaviour
                    if (item != null)
                    {
                        JBehavioursList.Remove(item);
                    }
                    this._instanceID = id;//更改id
                    this._gameObject.name = id;//改名
                    JBehaviours[id] = this;//覆盖字典里的值
                }
            }
            AddJBehaviourToGameObjectDict(_gameObject, this);
            JBehavioursList.Add(this);
            _checked = true;
        }

        /// <summary>
        /// Call to launch JBehaviour
        /// 启动生命周期
        /// </summary>
        private protected void Awake()
        {
            SetHiddenMonitoring(false);
            Launch();
        }

        /// <summary>
        /// Awake后的下一帧检测Show或Hide然后Run
        /// </summary>
        private protected void OnEnable()
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
                Log.PrintError($"{_gameObject.name}<{_instanceID}> OnShow/OnHide failed: {e.Message}, {e.Data["StackTrace"]}, skipped OnShow/OnHide");
            }
            SetHiddenMonitoring(true);

            try
            {
                Run();
            }
            catch (Exception e)
            {
                Log.PrintError($"{_gameObject.name}<{_instanceID}> Run failed: {e.Message}, {e.Data["StackTrace"]}, skipped run");
            }
        }

        /// <summary>
        /// Run后下一帧开始Loop
        /// </summary>
        private protected void Start()
        {
            DoLoop();
        }

        private protected static void AddJBehaviourToGameObjectDict(GameObject go, JBehaviour jb)
        {
            GameObjectJBehaviours.TryGetValue(go, out var h);
            if (h == null)
            {
                h = new HashSet<JBehaviour>();
            }
            if (!h.Contains(jb))
            {
                h.Add(jb);
            }
            GameObjectJBehaviours[go] = h;
        }

        private protected static void RemoveJBehaviourToGameObjectDict(GameObject go, JBehaviour jb)
        {
            GameObjectJBehaviours.TryGetValue(go, out var h);
            if (h == null)
            {
                h = new HashSet<JBehaviour>();
            }
            if (h.Contains(jb))
            {
                h.Remove(jb);
            }
            GameObjectJBehaviours[go] = h;
        }

        private protected static void GameObjectDictCheckNullJBehaviour()
        {
            bool hasNullKey = false;
            foreach (var k in GameObjectJBehaviours.Keys)
            {
                if (k == null)
                {
                    hasNullKey = true;
                    continue;
                }
                var h = GameObjectJBehaviours[k];
                h.RemoveWhere(j => j is null);
            }
            if (hasNullKey)
            {
                GameObjectDictCheckNull();
            }
        }

        private protected static void GameObjectDictCheckNull()
        {
            Dictionary<GameObject, HashSet<JBehaviour>> s = new Dictionary<GameObject, HashSet<JBehaviour>>(GameObjectJBehaviours.Count);

            foreach (var k in GameObjectJBehaviours.Keys)
            {
                if (k != null)
                {
                    var h = GameObjectJBehaviours[k];
                    s[k] = h;
                }
            }
            GameObjectJBehaviours = s;
        }

        private protected static void JBehavioursCheckNull(string key)
        {
            JBehaviours.Remove(key);
        }

        private void ResetJBehaviour(GameObject go)
        {
            _gameObject = go;
            _instanceID = JBehaviourMgr.Instance.GetJBehaviourInstanceID();
            JBehaviours.Add(_instanceID, this);
            JBehavioursList.Add(this);

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