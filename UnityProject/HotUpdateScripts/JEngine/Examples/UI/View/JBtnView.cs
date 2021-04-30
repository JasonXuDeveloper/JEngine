//
// JBtnView.cs
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
    public class JBtnView : APanelBase
    {
        public static JBtnView Instance { get { return Singleton<JBtnView>.Instance; } }

        /// <summary>
        /// 声明存放位置，属于什么类型的面板
        /// </summary>
        public JBtnView() : base()
        {
            isFilm = true;
            m_Type = UIPanelType.One;
        }

        private GameObject btn_Back;

        private GameObject drag_buttion;

        private RectTransform drag_Rect;

        private GameObject double_button;

        private Text button_text;

        private Text double_text;

        public override void Refer()
        {
            base.Refer();

            //这里是注册加载绑定的特效和item组的
        }

        public override void Init()
        {
            base.Init();

            btn_Back = UIUtility.BindClickEvent(Trans, "Back", OnClick);

            double_button = UIUtility.BindDoubleClickEvent(Trans, "DoubleButton", OnClick);

            drag_buttion = UIUtility.Control("DragButton", m_gameobj);
            drag_Rect = drag_buttion.GetComponent<RectTransform>();

            UIUtility.BindDragBeginEvent(drag_buttion, OnDragBeginEvent);
            UIUtility.BindDragEvent(drag_buttion, OnDragEvent);
            UIUtility.BindDragEndEvent(drag_buttion, OnDragEndEvent);

            button_text = UIUtility.GetComponent<Text>(drag_Rect, "Text");
            double_text = UIUtility.GetComponent<Text>(double_button.GetComponent<RectTransform>(), "Text");
        }

        public override void Refresh()
        {
            base.Refresh();

            UIUtility.Safe_UGUI(ref button_text, "按住可以拖拽");
            UIUtility.Safe_UGUI(ref double_text, "尝试双击");
        }

        private void OnDragEndEvent(GameObject go, PointerEventData eventData)
        {
            UIUtility.Safe_UGUI(ref button_text, "结束拖拽");
            JAction j = new JAction()
                .Delay(1)
                .Do(() => UIUtility.Safe_UGUI(ref button_text, "按住可以拖拽"))
                .Execute(true);
        }

        private void OnDragEvent(GameObject go, PointerEventData eventData)
        {
            drag_Rect.anchoredPosition += eventData.delta;
        }

        private void OnDragBeginEvent(GameObject go, PointerEventData eventData)
        {
            UIUtility.Safe_UGUI(ref button_text, "开始拖拽");
        }

        private void OnDoubleClick() 
        {
            UIUtility.Safe_UGUI(ref double_text, "按钮被双击了");
            JAction j = new JAction()
                .Delay(1)
                .Do(() => UIUtility.Safe_UGUI(ref double_text, "尝试双击"))
                .Execute(true);
        }

        public override void OnClick(GameObject obj, PointerEventData eventData)
        {
            base.OnClick(obj, eventData);

            if (obj.Equals(btn_Back))
            {
                CloseUI();
            }
            else if (obj.Equals(double_button)) 
            {
                OnDoubleClick();
            }
        }
    }
}
