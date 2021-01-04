//
// UIRootView.cs
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
using JEngine.Core;
using JEngine.UI.ResKit;
using JEngine.UI.UIKit;
using System.Collections.Generic;
using UnityEngine;

namespace JEngine.UI
{
    public enum UIPanelType : sbyte
    {
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
        private static UIRootView _instance = null;
        public static UIRootView Instance { get { return _instance; } }

        /// <summary> 存储所有根节点 </summary>
        private Dictionary<UIPanelType, GameObject> _roots;

        //拖拽绑定
        public Transform Trans;
        public GameObject CanvasRoot;
        public Camera UICamera;
        public Canvas UICanvas;

        public static void InitUIRoot()
        {
            if(_instance != null)
            {
                Log.PrintError("UI Root已存在");
                return;
            }
            JPrefab UIRootPrefab = new JPrefab("UI Root.prefab",
                (success, prefab) =>
                {
                    if (!success)
                    {
                        Log.PrintError("UI Root预制体加载失败");
                        return;
                    }
                    GameObject root = prefab.Instantiate("UI Root");
                    JBehaviour.CreateOn<UIRootView>(root);
                });
            
        }

        public override void Init()
        {
            Log.Print("初始化 UIRootView成功");
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
            for (UIPanelType i = UIPanelType.One; i <= UIPanelType.Other; i++)
            {
                GameObject go = new GameObject(i.ToString());
                go.transform.SetParent(CanvasRoot.transform, false);
                RectTransform rect = go.AddComponent<RectTransform>();
                SetRectTransform(rect);
                _roots.Add(i, go);
            }
        }

        public override void Loop()
        {
            UIMgr.Instance.Update();
            LoadResMgr.Instance.Update();
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
