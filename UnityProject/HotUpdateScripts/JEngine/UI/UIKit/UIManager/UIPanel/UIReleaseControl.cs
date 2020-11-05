//
// UIReleaseControl.cs
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JEngine.UI.UIKit
{
    public class UIReleaseControl
    {
        private APanelBase m_panel;

        public UIReleaseControl(APanelBase panel)
        {
            m_panel = panel;
        }

        protected int _minUseTime = -1;
        protected static readonly int MaxUseTime = 100000;

        public ResReleaseType m_ReleaseType = ResReleaseType.Auto;
        public int UseTime { protected set; get; }

        public void InitRelease()
        {
            ResetUseTime();

            if (m_panel.isFilm)
            {
                m_ReleaseType = ResReleaseType.Always;
                UseTime = MaxUseTime;
            }
        }

        public void ResetUseTime()
        {
            if (m_panel.isFilm) return;
            UseTime = 0;
        }

        public void Safe_AddUseTime()
        {
            if (m_ReleaseType != ResReleaseType.Auto) return;
            UseTime = Mathf.Min(++UseTime, MaxUseTime - 1);
        }
    }
}