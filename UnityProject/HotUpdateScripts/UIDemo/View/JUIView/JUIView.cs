using System;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HotUpdateScripts
{
    public class JUIView : APanelBase
    {
        public static JUIView Instance { get { return Singleton<JUIView>.Instance; } }

        public JUIView() : base()
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
