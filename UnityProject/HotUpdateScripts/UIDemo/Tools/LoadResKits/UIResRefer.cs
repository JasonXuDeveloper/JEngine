using System;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdateScripts
{
    /// <summary>
    /// UI资源依赖
    /// </summary>
    public abstract class UIResRefer : AResRefer
    {
        private static Dictionary<string, List<ReferInfo>> _referDic = new Dictionary<string, List<ReferInfo>>();

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="call"></param>
        /// <param name="isloadFromResource"></param>
        public override void LoadRefer(Action call, bool isloadFromResource = false)
        {
            if (_referList.Count != 0)
            {
                ReferInfo refer = null;
                for (int i = 0; i < _referList.Count; i++)
                {
                    refer = _referList[i];
                    if (refer != null)
                    {
                        if (!_referDic.ContainsKey(refer.m_resName))
                            _referDic.Add(refer.m_resName, new List<ReferInfo>());

                        _referDic[refer.m_resName].Add(refer);
                    }
                }

                LoadResMgr.Instance.AddGroupTask(_referList, call, false);
            }
            else
                call?.Invoke();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void ReleaseRefer()
        {
            if (_referList.Count != 0)
            {
                ReferInfo refer = null;
                for (int i = 0; i < _referList.Count; i++)
                {
                    refer = _referList[i];
                    _referDic[refer.m_resName].Remove(refer);

                    if (_referDic[refer.m_resName].Count == 0)
                        LoadResMgr.Instance.UnloadAsset(ResType.UI, refer.m_resName);
                }
            }
        }

    }
}
