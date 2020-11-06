//
// UIMgr.cs
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
using HotUpdateScripts;
using JEngine.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace JEngine.UI.UIKit
{
    public class UIMgr : Singleton<UIMgr>
    {
        public Dictionary<string, APanelBase> m_uIbaseDic = new Dictionary<string, APanelBase>();
        public List<APanelBase> m_openUIbaseList = new List<APanelBase>();

        /// <summary>
        /// 注册界面
        /// </summary>
        /// <param name="panels">全部界面，string是路径</param>
        public void Register(params (string,APanelBase)[] panels)
        {
            #region 注册所有UI界面
            for(int i = 0; i < panels.Length; i++)
            {
                Register(panels[i].Item1, panels[i].Item2);
            }
            #endregion
        }

        private void Register<T>(string name, T panel) where T : APanelBase
        {
            if (string.IsNullOrEmpty(name) || panel == null) return;
            if (m_uIbaseDic.ContainsKey(name))
            {
                Log.PrintError($"panel为[{panel.GetType()}]注册为{name}失败，{name}已定义为{m_uIbaseDic[name].GetType()}");
            }
            else
            {
                panel.m_strPanelViewName = name;
                panel.Refer();
                m_uIbaseDic.Add(name, panel);
            }
        }

        /// <summary>
        /// 打开UI
        /// </summary>
        /// <param name="uibaseName"></param>
        public void ShowUI(string uibaseName, bool isSaveShow = false, Action<APanelBase> closeCall = null, Action<APanelBase> openCall = null, bool isClearAll = true)
        {
            if (m_uIbaseDic.ContainsKey(uibaseName))
            {
                Action Call = () =>
                {
                    if (!isSaveShow)
                    {
                        if (!m_uIbaseDic[uibaseName].isFilm && isClearAll)
                            CloseAllUI(false);

                        closeCall += (panel) => { m_openUIbaseList.Remove(panel); };
                        m_uIbaseDic[uibaseName].ShowUI(uibaseName, openCall, closeCall);
                        m_openUIbaseList.Add(m_uIbaseDic[uibaseName]);
                    }
                    else
                    {
                        APanelBase lastBase = GetShowAndNoFilmUI();
                        m_uIbaseDic[uibaseName].ShowUI(uibaseName, openCall, closeCall);

                        if (!m_openUIbaseList.Contains(m_uIbaseDic[uibaseName]))
                            m_openUIbaseList.Add(m_uIbaseDic[uibaseName]);

                        if (lastBase != null)
                        {
                            lastBase.StartPauseHide();
                            m_uIbaseDic[uibaseName].
                            OnCloseEvent += (panel) => lastBase.EndPauseHide();
                        }
                    }
                };

                /*
                if (m_uIbaseDic[uibaseName].m_gameobj != null)
                    Call();
                else*/
                m_uIbaseDic[uibaseName].LoadRefer(Call, m_uIbaseDic[uibaseName].m_IsLoadFromResources);
                //JResource.LoadResAsync<GameObject>(ResPath.Instance.GetUIPath(uibaseName), Call, JResource.MatchMode.UI);
            }
        }


        private List<APanelBase> _tempHideList = new List<APanelBase>();

        /// <summary>
        /// 设定打开或者关闭已开UI,
        /// </summary>
        /// <param name="isOpen"></param>
        public void SetShowAllOpenUI(bool isOpen)
        {
            if (m_openUIbaseList == null || m_openUIbaseList.Count == 0) return;

            APanelBase curPanel = null;
            if (isOpen)
            {
                for (int i = 0; i < m_openUIbaseList.Count; i++)
                {
                    curPanel = m_openUIbaseList[i];
                    if (_tempHideList.Contains(curPanel) && !curPanel.isFilm && curPanel.m_IsKeepOpen)  //还原隐藏也只还原强制隐藏
                    {
                        curPanel.EndPauseHide();
                        _tempHideList.Remove(curPanel);
                    }
                }
            }
            else
            {
                _tempHideList.Clear();   //添加临时列表，记录强制隐藏的界面
                for (int i = 0; i < m_openUIbaseList.Count; i++)
                {
                    curPanel = m_openUIbaseList[i];
                    if (!curPanel.isFilm && !curPanel.m_IsKeepOpen && curPanel.IsOpen)
                    {
                        curPanel.StartPauseHide();
                        _tempHideList.Add(curPanel);
                    }
                }
            }
        }

        public APanelBase GetShowAndNoFilmUI()
        {
            if (m_openUIbaseList == null || m_openUIbaseList.Count == 0) return null;

            for (int i = 0; i < m_openUIbaseList.Count; i++)
            {
                if (!m_openUIbaseList[i].isFilm && !m_openUIbaseList[i].m_IsKeepOpen && (m_openUIbaseList[i].IsOpen))
                    return m_openUIbaseList[i];
            }
            return null;
        }

        public void PreLoadUI(string uibaseName)
        {
            if (m_uIbaseDic.ContainsKey(uibaseName))
            {
                Action Call = () =>
                {
                    m_uIbaseDic[uibaseName].PreLoadUI();
                };

                m_uIbaseDic[uibaseName].LoadRefer(Call, m_uIbaseDic[uibaseName].m_IsLoadFromResources);
            }
        }

        public APanelBase GetUI(string uibaseName)
        {
            APanelBase ui = null;
            if (m_uIbaseDic.ContainsKey(uibaseName))
            {
                ui = m_uIbaseDic[uibaseName];
                if (ui.m_gameobj == null)
                {
                    ui = null;
                }
            }
            return ui;
        }

        public void DestoryUI(string uibaseName)
        {
            APanelBase uibase = GetUI(uibaseName);

            if (uibase != null)
            {
                m_uIbaseDic.Remove(uibaseName);
                uibase.Destroy();
            }
        }

        public void CloseAndDestoryUI(string uibaseName)
        {
            APanelBase uibase = GetUI(uibaseName);

            if (uibase != null)
            {
                uibase.CloseUI();
                uibase.Destroy();
            }
        }

        public bool IsOpen(string planeName)
        {
            if (string.IsNullOrEmpty(planeName) || !m_uIbaseDic.ContainsKey(planeName))
                return false;
            return m_uIbaseDic[planeName].IsOpen;
        }

        public bool InHavePanel(string planeName) { return m_uIbaseDic.ContainsKey(planeName); }


        public void CloseUI(string uibaseName)
        {
            if (m_uIbaseDic.ContainsKey(uibaseName))
            {
                m_uIbaseDic[uibaseName].CloseUI();
                Log.Print("---->>>CloseUI  uibaseName ***************   " + uibaseName);
                m_openUIbaseList.Remove(m_uIbaseDic[uibaseName]);
            }
        }

        public void Clear(string uibaseName)
        {
            if (m_uIbaseDic.ContainsKey(uibaseName))
                m_uIbaseDic[uibaseName].Clear();
        }

        /// <summary>
        /// 获取UI
        /// </summary>
        /// <param name="uibaseName"></param>
        /// <returns></returns>
        public APanelBase Get(string uibaseName)
        {
            if (m_openUIbaseList == null || m_openUIbaseList.Count == 0) return null;

            for (int i = 0; i < m_openUIbaseList.Count; i++)
            {
                if (m_openUIbaseList[i].m_gameobj != null && m_openUIbaseList[i].m_gameobj.name == uibaseName)
                {
                    return m_openUIbaseList[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 关掉打开ui
        /// </summary>
        public void CloseAllUI(bool isFilm)
        {
            if (m_openUIbaseList == null || m_openUIbaseList.Count <= 0)
                return;

            for (int i = m_openUIbaseList.Count - 1; i >= 0; i--)
            {
                APanelBase ui = m_openUIbaseList[i];
                if (ui.isFilm.Equals(isFilm) && !ui.m_IsKeepOpen && !ui.m_IsAlwaysOpen)
                {
                    ui.OnCloseEvent = null;
                    ui.CloseUI();
                    m_openUIbaseList.Remove(ui);
                }
            }
        }

        public void CloseAllUI()
        {
            if (m_openUIbaseList == null || m_openUIbaseList.Count <= 0)
                return;

            for (int i = m_openUIbaseList.Count - 1; i >= 0; i--)
            {
                APanelBase ui = m_openUIbaseList[i];
                if (!ui.m_IsAlwaysOpen)
                {
                    ui.OnCloseEvent = null;
                    ui.CloseUI();
                    m_openUIbaseList.Remove(ui);
                }
            }
        }

        public void Update()
        {
            using (var ie = m_uIbaseDic.GetEnumerator())
            {
                while (ie.MoveNext())
                {
                    if (ie.Current.Value != null)
                    {
                        UpdateByCheck(ie.Current.Key);
                    }
                }
            }
        }


        public void UpdateByCheck(string uiName)
        {
            if (m_uIbaseDic.ContainsKey(uiName) && m_uIbaseDic[uiName] != null && IsOpen(uiName) &&
                m_openUIbaseList.Contains(m_uIbaseDic[uiName]) && (!_tempHideList.Contains(m_uIbaseDic[uiName])))
            {
                m_uIbaseDic[uiName].Update();
            }
        }

        public void LateUpdate()
        {
        }
    }

}