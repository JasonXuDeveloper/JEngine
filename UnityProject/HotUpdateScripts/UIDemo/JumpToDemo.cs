using System;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HotUpdateScripts
{
    public class JumpToDemo : JBehaviour
    {
        /// <summary> class binding </summary>
        public GameObject btn_JumpTo;

        private GameObject _rootPrefab;

        public override void Init()
        {
            JResource.LoadResAsync<GameObject>("UI Root.prefab",(obj)=> { _rootPrefab = obj; }, JResource.MatchMode.Prefab);
            UIUtility.BindClickEvent(btn_JumpTo, OnJumpToDemo);
            GameObject.DontDestroyOnLoad(gameObject);
        }        

        private void OnJumpToDemo(GameObject go, PointerEventData eventData)
        {
            if (_rootPrefab == null) { return; }
            JResource.LoadSceneAsync("UIDemo.unity", () =>
             {
                 GameObject root = GameObject.Instantiate(_rootPrefab);
                 JBehaviour.CreateOn<UIRootView>(root).Activate();
                 UIMgr.Instance.ShowUI(UIPanelName.JTestView);
             });
        }

        public override void Loop()
        {
            UIMgr.Instance.Update();
            LoadResMgr.Instance.Update();
        }
    }
}
