//
// UIResRefer.cs
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
using System;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;
using UnityEngine.UI;

namespace JEngine.UI.ResKit
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
