using System;
using System.Collections.Generic;
using JEngine.Core;
using JEngine.UI;
using JEngine.UI.UIKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JEngine.Examples
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

        private GameObject JBtnButton;

        /// <summary>
        /// 注册函数，相当于Awake
        /// </summary>
        public override void Init()
        {
            base.Init();

            JBtnButton = UIUtility.BindClickEvent(Trans, "JBtnButton", OnClick);
        }

        /// <summary>
        /// 周期函数，界面打开会自动执行
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
        }

        private void OnOpenJBtnExample()
        {
            UIMgr.Instance.ShowUI(JumpToUIDemo.JBtnViewPath,
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

            if (obj.Equals(JBtnButton))
            {
                OnOpenJBtnExample();
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
