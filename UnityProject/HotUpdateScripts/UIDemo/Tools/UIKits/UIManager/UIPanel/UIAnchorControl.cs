using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdateScripts
{
    public class UIAnchorControl
    {
        private APanelBase m_panel;

        public UIAnchorControl(APanelBase panel)
        {
            this.m_panel = panel;
        }

        #region UI适配
        //private UIAnchor 
        //    ac_top, 
        //    ac_topleft, 
        //    ac_topright,
        //    ac_bottom,
        //    ac_bottomleft,
        //    ac_bottomright;
        public void FindUIAnchor()
        {
            //    if (NeedAnchor)
            //    {
            //        ac_top = UIUtility.GetComponent<UIAnchor>(Trans, "Top");
            //        ac_topleft = UIUtility.GetComponent<UIAnchor>(Trans, "TopLeft");
            //        ac_topright = UIUtility.GetComponent<UIAnchor>(Trans, "TopRight");
            //        ac_bottom = UIUtility.GetComponent<UIAnchor>(Trans, "Bottom");
            //        ac_bottomleft = UIUtility.GetComponent<UIAnchor>(Trans, "BottomLeft");
            //        ac_bottomright = UIUtility.GetComponent<UIAnchor>(Trans, "BottomRight");
            //    }
        }
        /// <summary> 刘海屏适配 </summary>
        public void UpdateAnchor()
        {
            //if (NeedAnchor)
            //{
            //    ac_top.UpdateYAnchor();
            //    ac_topleft.UpdateYAnchor();
            //    ac_topright.UpdateYAnchor();
            //    //ac_center.UpdateYAnchor();
            //    //ac_bottom.UpdateYAnchor(-0.5f);
            //    //ac_bottomleft.UpdateYAnchor(-0.5f);
            //    //ac_bottomright.UpdateYAnchor(-0.5f);
            //}
        }
        #endregion
    }
}