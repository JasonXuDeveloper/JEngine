using System;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HotUpdateScripts
{
    public class JBehaviourView : APanelBase
    {
        public static JBehaviourView Instance { get { return Singleton<JBehaviourView>.Instance; } }

        /// <summary>
        /// 声明存放位置，属于什么类型的面板
        /// </summary>
        public JBehaviourView() : base()
        {
            isFilm = true;
            m_Type = UIPanelType.One;
        }

        private GameObject btn_Back;

        public override void Init()
        {
            base.Init();

            btn_Back = UIUtility.BindClickEvent(Trans, "Back", OnClick);
        }

        public override void OnClick(GameObject obj, PointerEventData eventData)
        {
            base.OnClick(obj, eventData);

            if (obj.Equals(btn_Back)) 
            {
                CloseUI();
            }
        }
    }
}
