using JEngine.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdateScripts
{
    public enum UIPanelType : sbyte
    {
        Battle,//战斗场景
        One,//一级界面
        Two,//二级界面
        Three,//三级界面
        Other,//其他通用弹出
    }

    /// <summary>
    /// UI根路径，全局唯一
    /// </summary>
    public class UIRootView : JBehaviour
    {
        private static UIRootView _instance;
        public static UIRootView Instance { get { return _instance; } }

        /// <summary> 存储所有根节点 </summary>
        private Dictionary<UIPanelType, GameObject> _roots;

        //拖拽绑定
        public Transform Trans;
        public GameObject CanvasRoot;
        public Camera UICamera;
        public Canvas UICanvas;

        public Action OnComplete { get; set; }

        public override void Init()
        {
            Log.Print("初始化 UIRootView");
            _instance = this;
            _roots = new Dictionary<UIPanelType, GameObject>();
            GameObject.DontDestroyOnLoad(this.gameObject);
        }

        public override void Run()
        {
            gameObject.name = "UI Root";
            Trans = gameObject.transform;

            UICanvas = Trans.Find("Camera/Canvas").GetComponent<Canvas>();
            UICamera = Trans.Find("Camera").GetComponent<Camera>();
            CanvasRoot = UICanvas.gameObject;

           // DestroyChildren(CanvasRoot.transform);
            for (UIPanelType i = UIPanelType.Battle; i <= UIPanelType.Other; i++)
            {
                GameObject go = new GameObject(i.ToString());
                go.transform.SetParent(CanvasRoot.transform, false);
                RectTransform rect = go.AddComponent<RectTransform>();
                SetRectTransform(rect);
                _roots.Add(i, go);
            }
            OnComplete?.Invoke();
        }

        private void SetRectTransform(RectTransform rect)
        {
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
        }

        public GameObject GetRoot(UIPanelType type)
        {
            GameObject go = null;
            _roots.TryGetValue(type, out go);
            return go;
        }

        public void DestroyChildren(Transform trans)
        {
            if (trans == null) return;
            foreach (Transform item in trans)
            {
                if (item != null) 
                {
                    GameObject.Destroy(item.gameObject);
                }
            }
        }
    }
}
