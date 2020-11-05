//
// UISoundControl.cs
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
    /// <summary>
    /// UI声音控制
    /// </summary>
    public class UISoundControl
    {
        private APanelBase m_panel;

        public UISoundControl(APanelBase panel)
        {
            this.m_panel = panel;
        }
        public virtual void PlayOpenSound()
        {
            if (m_panel.IsPlayOpenSound)
            {
                //GameInterface.Instance.PlaySoundMatchVolume(SoundLayer.UI, ResType.Sound, Const.Sound_UI_OpenView);
            }
        }
        public void PlayCloseSound()
        {
            if (m_panel.IsPlayCloseSound)
            {
                //GameInterface.Instance.PlaySoundMatchVolume(SoundLayer.UI, ResType.Sound, Const.Sound_UI_CloseView);
            }
        }

        public void PlayButtonSound(GameObject obj)
        {
            if (m_panel.IsPlayCloseSound)
            {
                //GameInterface.Instance.PlaySoundMatchVolume(SoundLayer.UI, ResType.Sound, Const.Sound_UI_Button);
            }
        }

        public void PlayMusic()
        {
            //if (m_Data.m_IsAffectMusic)
            //    MusicControl.Instance.ChangeMusicState(m_StageType, m_MusicType);
        }

        public void ResetMusic()
        {
            //if (m_IsAffectMusic && m_IsResetMusic)
            //    MusicControl.Instance.ResetMusic();
        }
    }
}