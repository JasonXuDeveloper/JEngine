using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdateScripts
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