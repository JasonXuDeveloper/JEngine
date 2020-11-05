using System;
using System.Collections.Generic;
using JEngine.Core;
using JEngine.UI;
using JEngine.UI.ResKit;
using JEngine.UI.UIKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JEngine.Examples
{
    public class JumpToUIDemo : JBehaviour
    {
        /// <summary> class binding </summary>
        public GameObject btn_JumpTo;

        private GameObject _rootPrefab;

        #region 声明界面
        public static string JTestViewPath = "uiview_testView";
        public static string JBtnViewPath = "uiview_btnview";
        #endregion

        public override void Init()
        {
            JResource.LoadResAsync<GameObject>("UI Root.prefab",(obj)=> { _rootPrefab = obj; }, JResource.MatchMode.Prefab);
            UIUtility.BindClickEvent(btn_JumpTo, OnJumpToDemo);
            GameObject.DontDestroyOnLoad(gameObject);

            //UIMgr注册界面
            UIMgr.Instance.Register(
                (JTestViewPath, JTestView.Instance),
                (JBtnViewPath, JBtnView.Instance));
        }        

        private void OnJumpToDemo(GameObject go, PointerEventData eventData)
        {
            if (_rootPrefab == null) { return; }
            JResource.LoadSceneAsync("UIDemo.unity", () =>
             {
                 GameObject root = GameObject.Instantiate(_rootPrefab);
                 var uiroot = JBehaviour.CreateOn<UIRootView>(root);
                 UIMgr.Instance.ShowUI(JTestViewPath);
             });
        }

        public override void Loop()
        {
            UIMgr.Instance.Update();
            LoadResMgr.Instance.Update();
        }
    }
}
