using System;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HotUpdateScripts
{
    public class JTestView : APanelBase
    {
        public static JTestView Instance { get { return Singleton<JTestView>.Instance; } }

        /// <summary>
        /// 声明存放位置，属于什么类型的面板
        /// </summary>
        public JTestView() : base()
        {
            isFilm = true;
            m_Type = UIPanelType.One;
        }

        private GameObject JBehaviourButton;
        private GameObject JUIButton;
        private GameObject JSaverButton;

        /// <summary>
        /// 注册函数，相当于Awake
        /// </summary>
        public override void Init()
        {
            base.Init();

            JBehaviourButton = UIUtility.BindClickEvent(Trans, "JBehaviourButton", OnClick);
            JUIButton = UIUtility.BindClickEvent(Trans, "JUIButton", OnClick);
            JSaverButton = UIUtility.BindClickEvent(Trans, "JSaverButton", OnClick);
        }

        /// <summary>
        /// 周期函数，界面打开会自动执行
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
        }

        private void OnOpenJBehaviourExample() 
        {
            UIMgr.Instance.ShowUI(UIPanelName.JBehaviourView,
                openCall:(p)=> 
                {
                    //当界面完全打开后调用的事件
                    CloseUI();
                },
                closeCall:(p)=>
                {
                    //当界面关闭前调用的事件
                    UIMgr.Instance.ShowUI(m_strPanelViewName);
                });
        }

        private void OnOpenJUIExample()
        {

            UIMgr.Instance.ShowUI(UIPanelName.JUIView,
                openCall: (p) =>
                {
                    CloseUI();
                },
                closeCall: (p) =>
                {
                    UIMgr.Instance.ShowUI(m_strPanelViewName);
                });
        }

        private void OnOpenJSaverExample()
        {
            UIMgr.Instance.ShowUI(UIPanelName.JSaveView,
                openCall: (p) =>
                {
                    CloseUI();
                },
                closeCall: (p) =>
                {
                    UIMgr.Instance.ShowUI(m_strPanelViewName);
                });
        }

        public override void OnClick(GameObject obj, PointerEventData eventData)
        {
            base.OnClick(obj, eventData);

            if (obj.Equals(JBehaviourButton)) 
            {
                OnOpenJBehaviourExample();
            }
            else if (obj.Equals(JUIButton))
            {
                OnOpenJUIExample();
            }
            else if (obj.Equals(JSaverButton))
            {
                OnOpenJSaverExample();
            }
        }

        /// <summary>
        /// 周期函数，界面关闭会自动执行，
        /// 也可以外部调用，进行关闭面板
        /// </summary>
        public override void CloseUI()
        {
            base.CloseUI();
        }
    }
}
