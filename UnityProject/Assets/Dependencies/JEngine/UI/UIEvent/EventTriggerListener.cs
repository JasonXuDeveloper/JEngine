//
// EventTriggerListener.cs
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
using UnityEngine.EventSystems;

namespace JEngine.UI
{
    public delegate void UIEventHandle<T>(GameObject go, T eventData) where T : BaseEventData;

    public class EventTriggerListener :
        MonoBehaviour,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerUpHandler,
        ISelectHandler,
        IUpdateSelectedHandler,
        IDeselectHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IScrollHandler,
        IMoveHandler
    {
        public class EventHandle<T> where T : BaseEventData
        {
            private event UIEventHandle<T> m_Handle;

            public void AddListener(UIEventHandle<T> handle)
            {
                m_Handle += handle;
            }

            public void RemoveListener(UIEventHandle<T> handle)
            {
                m_Handle -= handle;
            }

            public void RemoveAllListener()
            {
                m_Handle -= m_Handle;
                m_Handle = null;
            }

            public void Invoke(GameObject go, T eventData)
            {
                m_Handle?.Invoke(go, eventData);
            }
        }

        public EventHandle<PointerEventData> onClick = new EventHandle<PointerEventData>();
        public EventHandle<PointerEventData> onDoubleClick = new EventHandle<PointerEventData>();
        public EventHandle<PointerEventData> onPress = new EventHandle<PointerEventData>();
        public EventHandle<PointerEventData> onUp = new EventHandle<PointerEventData>();
        public EventHandle<PointerEventData> onDown = new EventHandle<PointerEventData>();
        public EventHandle<PointerEventData> onEnter = new EventHandle<PointerEventData>();
        public EventHandle<PointerEventData> onExit = new EventHandle<PointerEventData>();
        public EventHandle<BaseEventData> onSelect = new EventHandle<BaseEventData>();
        public EventHandle<BaseEventData> onUpdateSelect = new EventHandle<BaseEventData>();
        public EventHandle<BaseEventData> onDeselect = new EventHandle<BaseEventData>();
        public EventHandle<PointerEventData> onBeginDrag = new EventHandle<PointerEventData>();
        public EventHandle<PointerEventData> onDrag = new EventHandle<PointerEventData>();
        public EventHandle<PointerEventData> onEndDrag = new EventHandle<PointerEventData>();
        public EventHandle<PointerEventData> onDrop = new EventHandle<PointerEventData>();
        public EventHandle<PointerEventData> onScroll = new EventHandle<PointerEventData>();
        public EventHandle<AxisEventData> onMove = new EventHandle<AxisEventData>();

        public void OnPointerClick(PointerEventData eventData)
        {
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onEnter.Invoke(gameObject, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onExit.Invoke(gameObject, eventData);
        }

        public void OnSelect(BaseEventData eventData)
        {
            onSelect.Invoke(gameObject, eventData);
        }

        public void OnUpdateSelected(BaseEventData eventData)
        {
            onUpdateSelect.Invoke(gameObject, eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            onDeselect.Invoke(gameObject, eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_IsDraging = true;
            m_Delta = eventData.delta;
            onBeginDrag.Invoke(gameObject, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            m_IsDraging = true;
            m_IsTryClick = false;
            onDrag.Invoke(gameObject, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_IsDraging = false;
            onEndDrag.Invoke(gameObject, eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            onDrop.Invoke(gameObject, eventData);
        }

        public void OnScroll(PointerEventData eventData)
        {
            onScroll.Invoke(gameObject, eventData);
        }

        public void OnMove(AxisEventData eventData)
        {
            onMove.Invoke(gameObject, eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_IsPointDown = true;
            m_IsPress = false;
            m_IsTryClick = true;
            m_IsDraging = false;

            m_CurrDownTime = Time.unscaledTime;
            onDown?.Invoke(gameObject, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_IsPointDown = false;
            m_OnUpEventData = eventData;
            if (!m_IsPress)
            {
                m_ClickCount++;
            }
        }

        public static EventTriggerListener Get(GameObject go)
        {
            if (go == null)
                return null;
            EventTriggerListener eventTrigger = go.GetComponent<EventTriggerListener>();
            if (eventTrigger == null) eventTrigger = go.AddComponent<EventTriggerListener>();
            return eventTrigger;
        }

        private const float DOUBLE_CLICK_TIME = 0.2f;
        private const float PRESS_TIME = 0.5F;

        private Vector2 m_Delta;
        private float m_CurrDownTime;
        private bool m_IsPointDown;
        private bool m_IsPress;
        private bool m_IsDraging;
        private bool m_IsTryClick;
        private int m_ClickCount;
        private PointerEventData m_OnUpEventData;

        public bool IsPress
        {
            get { return m_IsPress; }
        }

        private void Update()
        {
            if (m_IsPointDown)
            {
                if (Time.unscaledTime - m_CurrDownTime >= PRESS_TIME)
                {
                    m_IsPress = true;
                    m_IsPointDown = false;
                    m_CurrDownTime = 0f;
                    onPress?.Invoke(gameObject, null);
                }
            }

            if (m_ClickCount > 0)
            {
                if (Time.unscaledTime - m_CurrDownTime >= DOUBLE_CLICK_TIME)
                {
                    if (m_ClickCount < 2)
                    {
                        onUp?.Invoke(gameObject, m_OnUpEventData);
                        if (m_IsTryClick && !m_IsDraging)
                            onClick?.Invoke(gameObject, m_OnUpEventData);
                        m_OnUpEventData = null;
                    }

                    m_ClickCount = 0;
                }

                if (m_ClickCount > 1)
                {
                    onDoubleClick?.Invoke(gameObject, m_OnUpEventData);
                    m_OnUpEventData = null;
                    m_ClickCount = 0;
                }
            }
        }

        private void OnDestroy()
        {
            RemoveUIListener();
        }

        private void RemoveUIListener()
        {
            onClick.RemoveAllListener();
            onDoubleClick.RemoveAllListener();
            onDown.RemoveAllListener();
            onEnter.RemoveAllListener();
            onExit.RemoveAllListener();
            onUp.RemoveAllListener();
            onSelect.RemoveAllListener();
            onUpdateSelect.RemoveAllListener();
            onDeselect.RemoveAllListener();
            onDrag.RemoveAllListener();
            onEndDrag.RemoveAllListener();
            onDrop.RemoveAllListener();
            onScroll.RemoveAllListener();
            onMove.RemoveAllListener();
        }
    }
}