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