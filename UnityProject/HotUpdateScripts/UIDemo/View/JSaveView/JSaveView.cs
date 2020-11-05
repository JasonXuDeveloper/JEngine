using System;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HotUpdateScripts
{
    public class JSaveView : APanelBase
    {
        public static JSaveView Instance { get { return Singleton<JSaveView>.Instance; } }

        /// <summary>
        /// 声明存放位置，属于什么类型的面板
        /// </summary>
        public JSaveView() : base()
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
