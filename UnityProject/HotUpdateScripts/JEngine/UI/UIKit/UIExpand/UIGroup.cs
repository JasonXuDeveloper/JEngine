//
// UIGroup.cs
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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JEngine.UI.UIKit
{
    public class UIGroup<T, W> : AItemBase where T : ALoopItem
    {
        public LoopListView m_LoopGroup;

        //当前游戏物体
        protected List<T> _curList = new List<T>();
        public List<T> CurList { get { return _curList; } }

        //当前数据
        protected List<W> _curDataList = new List<W>();
        public List<W> CurDataList { get { return _curDataList; } }

        protected List<GameObject> m_Prefabs = new List<GameObject>();

        protected Func<W, int, string> _getPrefab;

        protected bool m_IsInit = false;

        public override void setObj(GameObject obj)
        {
            base.setObj(obj);

            m_Prefabs.Clear();

            m_LoopGroup = obj.GetComponent<LoopListView>();

            if (m_LoopGroup == null)
            {
                Log.PrintError($"游戏物体{obj.name}未指定<LoopListView>组件，请指定");
            }
            m_IsInit = false;
        }

        public void DefaultRefresh(List<W> dataList, Func<W, int, string> getPrefab)
        {
            _curDataList = dataList;
            _getPrefab = getPrefab;
            if (!m_IsInit)
            {
                m_IsInit = true;
                m_LoopGroup.InitListView(dataList.Count, OnGetItemByIndex);
            }
            else
            {
                m_LoopGroup.ResetListView();
                m_LoopGroup.SetListItemCount(_curDataList.Count, false);
            }
        }

        protected virtual LoopListItem OnGetItemByIndex(LoopListView listView, int index)
        {
            if (index < 0 || index >= listView.ItemTotalCount)
            {
                return null;
            }

            if (index < 0 || index >= _curDataList.Count)
            {
                return null;
            }

            //以下判定也可以根据需求，暴露出去用以重写

            W data = _curDataList[index];
            string name = _getPrefab(data, index);
            LoopListItem item = listView.NewListViewItem(name);
            item.Index = index;
            item.gameObject.name = index.ToString();

            T itemScript = _curList.Find(t => t.m_gameobj.Equals(item.gameObject));

            if (itemScript == null)
            {
                itemScript = Activator.CreateInstance<T>();
                //itemScript.index = index;
                itemScript.setObj(item.gameObject);
                _curList.Add(itemScript);
            }
            else
            {
                //itemScript.index = index;
            }

            if (item.IsInitHandlerCalled == false)
            {
                item.IsInitHandlerCalled = true;
            }

            itemScript.SetItemControl(item);
            itemScript.Refresh(data);
            return item;
        }


        public void MoveToIndex(int index, float offset = 0)
        {
            m_LoopGroup.MovePanelToItemIndex(index, offset);
        }

        public virtual void Reposition()
        {
            m_LoopGroup.ResetListView();
        }

        public virtual void ReBuildAll()
        {
            m_LoopGroup.RefreshAllShownItem();
        }

        public virtual void RefreshByChangeCount()
        {
            m_LoopGroup.SetListItemCount(_curDataList.Count, false);
        }
    }

    public class ALoopItem : AItemBase
    {
        public new int index { get { return ItemControl.Index; } }
        protected LoopListItem ItemControl { get; private set; }
        protected ScrollRect ScrollRect { get; private set; }
        public void SetItemControl(LoopListItem item)
        {
            ItemControl = item;
            ScrollRect = item.GetComponent<ScrollRect>();
        }

        public override void setObj(GameObject obj)
        {
            base.setObj(obj);

            UIUtility.BindDragBeginEvent(m_gameobj, OnBeginDrag);
            UIUtility.BindDragEvent(m_gameobj, OnDrag);
            UIUtility.BindDragEvent(m_gameobj, OnEndDrag);
        }

        protected virtual void OnBeginDrag(GameObject go, PointerEventData eventData)
        {
            if (ScrollRect == null) ScrollRect = Trans.GetComponentInParent<ScrollRect>();
            eventData.selectedObject = ScrollRect.gameObject;
            eventData.pointerDrag = ScrollRect.gameObject;
            ScrollRect.OnBeginDrag(eventData);
        }

        protected virtual void OnDrag(GameObject go, PointerEventData eventData)
        {
            if (ScrollRect == null) ScrollRect = Trans.GetComponentInParent<ScrollRect>();
            eventData.selectedObject = ScrollRect.gameObject;
            eventData.pointerDrag = ScrollRect.gameObject;
            ScrollRect.OnDrag(eventData);
        }

        protected virtual void OnEndDrag(GameObject go, PointerEventData eventData)
        {
            if (ScrollRect == null) ScrollRect = Trans.GetComponentInParent<ScrollRect>();
            eventData.selectedObject = ScrollRect.gameObject;
            eventData.pointerDrag = ScrollRect.gameObject;
            ScrollRect.OnEndDrag(eventData);
        }

        public virtual void SetItemSize(RectTransform.Axis axis, float size)
        {
            RectTrans.SetSizeWithCurrentAnchors(axis, size);
            ItemControl.ParentListView.OnItemSizeChanged(ItemControl.ItemIndex);
        }
    }

    public class ASelectLoopItem : ALoopItem
    {
        private bool _isSelect = false;
        /** 记录被选择状态 */
        public bool IsSelect
        {
            get { return _isSelect; }
            set
            {
                if (_isSelect == value && !value) return;
                if (value) { InSelect(); }
                else { NoSelect(); }
            }
        }

        /** 选中状态 */
        public virtual void InSelect() { _isSelect = true; }
        /** 非选中状态 */
        public virtual void NoSelect() { _isSelect = false; }
    }

    public class UISelectGroup<T, W> : UIGroup<T, W> where T : ASelectLoopItem
    {
        public int SelectIndex = -1;
        public T SelectItem = null;

        public void SetSelect(T item)
        {
            if (SelectItem != null && SelectItem != item)
            {
                SelectItem.IsSelect = false;
                SelectItem = null;
                SelectIndex = -1;
            }

            if (item == null) return;

            for (int i = 0; i < CurList.Count; i++)
            {
                if (CurList[i] == item)
                {
                    item.IsSelect = true;
                    SelectIndex = item.index;
                    SelectItem = item;
                    return;
                }
            }
        }
        
        public T GetNewSelectItem(int itemIndex)
        {
            for (int i = 0; i < CurList.Count; i++)
            {
                if (CurList[i].m_gameobj.activeSelf && CurList[i].index == itemIndex)
                {
                    return CurList[i];
                }
            }
            return null;
        }
    }
}