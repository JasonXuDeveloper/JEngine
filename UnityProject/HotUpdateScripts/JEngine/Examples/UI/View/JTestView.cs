//
// JTestView.cs
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

        //private JTestItem testItem;

        /// <summary>
        /// 注册函数，相当于Awake
        /// </summary>
        public override void Init()
        {
            base.Init();

            JBtnButton = UIUtility.BindClickEvent(Trans, "JBtnButton", OnClick);

            UIUtility.BindClickEvent(Trans, "BackButton", (obj,data) =>
            {
                JResource.LoadSceneAsync("Game.unity");
            });

            //注册子部件
            //JTestItem item = UIUtility.CreateItemNoClone<JTestItem>(Trans, "子部件名字");
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
