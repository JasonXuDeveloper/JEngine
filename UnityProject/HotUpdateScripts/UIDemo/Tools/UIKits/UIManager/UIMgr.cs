using HotUpdateScripts;
using JEngine.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdateScripts
{
    public class UIPanelName
    {
        #region 声明界面
        public static string JTestView = "uiview_testView";
        public static string JUIView = "uiview_juiview";//测试界面
        public static string JBehaviourView = "uiview_jbehaviourview";
        public static string JSaveView = "uiview_jsaveview";
        #endregion
    }

    public class UIMgr : Singleton<UIMgr>
    {
        public Dictionary<string, APanelBase> m_uIbaseDic = new Dictionary<string, APanelBase>();
        public List<APanelBase> m_openUIbaseList = new List<APanelBase>();

        /// <summary>
        /// 注册界面
        /// </summary>
        public UIMgr()
        {
            #region 注册所有UI界面
            Register<JTestView>(UIPanelName.JTestView, JTestView.Instance);
            Register<JUIView>(UIPanelName.JUIView, JUIView.Instance);
            Register<JBehaviourView>(UIPanelName.JBehaviourView, JBehaviourView.Instance);
            Register<JSaveView>(UIPanelName.JSaveView, JSaveView.Instance);
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
            UpdateByCheck(UIPanelName.JUIView);
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