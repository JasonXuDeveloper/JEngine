//
// AItemBase.cs
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
using UnityEngine;
using System;
using JEngine.Core;

namespace JEngine.UI.UIKit
{
    public interface IItemBase
    {
        void setObj(GameObject obj);
        void Refresh();
        void Refresh<T>(T data);
        void Update();
        void OnClick(GameObject obj);
        void Destory();
        void sendPara(object args);         
        void Clear();
    }

    public abstract class AItemBase : IItemBase
    {
        public int index;

        protected AItemBase _ownItem = null;
        protected internal GameObject m_gameobj = null;
        protected internal Transform Trans = null;
        protected internal RectTransform RectTrans = null;

        public AItemBase() { }

        public virtual void setObj(GameObject obj)
        {
            if (obj == null) Log.Print("------------AItemBase中OBJ不同----------");

            this.m_gameobj = obj;

            this.Trans = obj.transform;
            this.RectTrans = obj.GetComponent<RectTransform>();
        }

        public virtual bool IsActive
        {
            set
            {
                if (null != m_gameobj)
                    m_gameobj.SetActive(value);
            }

            get { return m_gameobj.activeInHierarchy; }
        }

        public virtual void Refresh() { }
        public virtual void Refresh<T>(T data) { if (data == null) return; }
        public virtual void Update() { }
        public virtual void OnClick(GameObject obj) { }
        public virtual void Clear() { }
        public virtual void Destory()
        {
            if (null != m_gameobj)
                GameObject.Destroy(m_gameobj);
            m_gameobj = null;
        }

        public virtual void sendPara(object args) { }
        public virtual void ResetUI() { }
    }
}