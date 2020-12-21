//
// JumpToUIDemo.cs
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

        #region 声明界面
        public static string JTestViewPath = "uiview_testView";
        public static string JBtnViewPath = "uiview_btnview";
        #endregion

        private static bool Inited = false;

        public override void Init()
        {
            UIUtility.BindClickEvent(btn_JumpTo, OnJumpToDemo);

            if (!Inited)
            {
                //UIMgr注册界面（需要的）
                UIMgr.Instance.Register(
                    (JTestViewPath, JTestView.Instance),
                    (JBtnViewPath, JBtnView.Instance));
                Inited = true;
            }
        }        

        private void OnJumpToDemo(GameObject go, PointerEventData eventData)
        {
            JResource.LoadSceneAsync("UIDemo.unity", () =>
             {
                 UIRootView.InitUIRoot();//初始化UIRoot是需要的
                 UIMgr.Instance.ShowUI(JTestViewPath);
             });
        }
    }
}
