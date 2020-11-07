//
// APanelBase.cs
//
// Author:
//       L-Fone <275757115@qq.com>
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
using JEngine.Core;
using JEngine.UI.ResKit;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JEngine.UI.UIKit
{
    public interface IPlayTween
    {
        void PlayTween(bool isForward);
    }

    public enum ResReleaseType
    {
        Non = 0,      //未打开初始化
        Once,         //使用一次后马上释放
        Auto,         //自动增加和减少
        Always,       //永驻停留
    }

    public abstract class APanelBase : UIResRefer
    {
        public bool IsPlayOpenSound = true;
        public bool IsPlayCloseSound = true;
        public bool IsPlayButtonSound = true;
        public bool isFilm = false;
        public bool m_IsKeepOpen = false;
        public bool m_IsLoadFromResources = false;
        public bool m_IsAlwaysOpen = false;
        public bool NeedAnchor = false;//刘海屏幕适配
        public UIPanelType m_Type = UIPanelType.Two;

        public string m_strPanelViewName = null;
        public GameObject m_gameobj = null;
        public RectTransform Trans = null;
        public Action<APanelBase> OnCloseEvent = null;
        public Action<APanelBase> OnOpenEvent = null;
        public static Vector3 HidePos = Vector3.left * 10000.0f;
        private Vector3 _srcLocalPos;

        public bool m_IsAffectMusic = false;//是否影响声音
        public bool m_IsResetMusic = false;//是否还原声音

        /// <summary> 资源释放策略 </summary>
        public UIReleaseControl m_Release { get; set; }

        public APanelBase()
        {
            this.m_Release = new UIReleaseControl(this);
        }

        public string Name
        {
            private set;
            get;
        }

        public override void Refer()
        {
            ReferInfo refer = new ReferInfo(ResType.UI, m_strPanelViewName, AssetType.Prefab);
            _referList.Add(refer);           
        }

        public virtual void Init()
        {
            m_Release.InitRelease();
        }

        public void ShowUI(string uiName, Action<APanelBase> openCall, Action<APanelBase> closeCall)
        {
            this.Name = uiName;
            this.OnOpenEvent += openCall;
            this.OnCloseEvent += closeCall;
            ShowUI(uiName);
        }

        /// <summary>
        /// 打开UI界面，不要重载这个函数，用重载Refresh替代，本函数将取消虚方法
        /// </summary>
        /// <param name="uiName"></param>
        public void ShowUI(string uiName)
        {
            
            if (m_gameobj == null)
            {
                BindObj(LoadResMgr.Instance.GetObject<GameObject>(ResType.UI, m_strPanelViewName));
            }

            m_gameobj.SetActive(true);

            Refresh();
            Trans.SetAsLastSibling();

            //回调函数只执行一次
            OnOpenEvent?.Invoke(this);
            OnOpenEvent = null;

            if (!m_gameobj.activeSelf)
            {
                Log.PrintError($"{uiName}显示异常");
            }
        }

        public virtual void PreLoadUI()
        {
            if (m_gameobj == null)
                BindObj(LoadResMgr.Instance.GetObject<GameObject>(ResType.UI, m_strPanelViewName));
        }

        public void BindObj(GameObject uiobj)
        {
            GameObject rootObj = UIRootView.Instance.GetRoot(m_Type);

            if (rootObj != null)
            {                
                uiobj.transform.SetParent(rootObj.transform, false);
                m_gameobj = uiobj;
                m_gameobj.name = m_strPanelViewName;
                Trans = m_gameobj.GetComponent<RectTransform>();
                Trans.anchoredPosition = Vector2.zero;
                Trans.localScale = Vector3.one;
            }

            Init();
            m_gameobj.SetActive(false);
        }

        public bool IsHasBeenOpened { get { return m_gameobj != null; } }

        public bool IsOpen { get { return IsHasBeenOpened && m_gameobj.activeInHierarchy; } }

        public virtual void CloseUI()
        {
            EndPauseHide(true);
            m_gameobj.SetActive(false);
            OnCloseEvent?.Invoke(this);
            OnCloseEvent = null;
        }

        public virtual void StartPauseHide()
        {
            if (Trans != null)
            {
                _srcLocalPos = Trans.anchoredPosition;
                Trans.anchoredPosition = HidePos;
                //ResetMusic();
            }
            m_IsKeepOpen = true;
        }

        public virtual void EndPauseHide(bool close = false)
        {
            if (Trans != null)
            {
                Trans.anchoredPosition = _srcLocalPos;
                if (!close)
                {
                    //PlayMusic();
                }
            }
            m_IsKeepOpen = false;
        }

        public virtual void Refresh()
        {
            m_Release.Safe_AddUseTime();
            //PlayMusic();
            ResetUI();
        }

        public virtual void Refresh<T>(T data) { ResetUI(); }

        public virtual void Update() { }

        public virtual void LateUpdate() { }

        public virtual void Destroy()
        {
            if (IsHasBeenOpened)
            {
                if (m_gameobj != null) 
                {
                    GameObject.Destroy(m_gameobj);
                    m_gameobj = null;
                }
                ReleaseRefer();
                m_Release.ResetUseTime();
            }
        }

        //清理UI，在Refresh()的时候调用
        public virtual void ResetUI() { }
        public virtual void OnClick(GameObject obj, PointerEventData eventData) { }
        public virtual void OnPress(GameObject obj, bool isPress) { }
        public virtual void Clear() { }

        public bool IsActive
        {
            set
            {
                if (m_gameobj != null)
                    m_gameobj.SetActive(value);
            }
            get
            {
                if (m_gameobj != null)
                    return m_gameobj.activeInHierarchy;
                return false;
            }
        }
    }

    public abstract class AFilmPanelBase : APanelBase
    {
        public AFilmPanelBase() { isFilm = true; }
    }

    public abstract class APanelBaseFormRes : APanelBase
    {
        public APanelBaseFormRes() { m_IsLoadFromResources = true; }
    }

    public abstract class AFilmPanelBaseFormRes : APanelBase
    {
        public AFilmPanelBaseFormRes() { isFilm = true; m_IsLoadFromResources = true; }
    }
}