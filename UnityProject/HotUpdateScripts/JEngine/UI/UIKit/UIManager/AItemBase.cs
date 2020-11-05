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