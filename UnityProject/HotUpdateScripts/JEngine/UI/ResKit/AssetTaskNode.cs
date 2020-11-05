//
// AssetTaskNode.cs
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
using Object = UnityEngine.Object;

namespace JEngine.UI.ResKit
{
    public class AssetTaskNodeBase
    {
        public Action m_CallBack = null;
        public bool m_IsLoadCompleted = false;
        protected GameResMgr _mgr = null;

        public AssetTaskNodeBase(GameResMgr mgr) { this._mgr = mgr; }

        public virtual void AddNode<T>(T info, bool isShowWaitcircle = true, Action callBack = null)
        {
            this.m_CallBack = callBack;
        }

        public static AssetTaskNodeBase CreatLoadNode(GameResMgr mgr, bool isSingleAsset)
        {
            if (isSingleAsset) return new AssetSingleTaskNode(mgr);
            else return new AssetTaskNode(mgr);
        }

        public virtual void Update() { if (m_IsLoadCompleted) return; }
    }

    public class AssetTaskNode : AssetTaskNodeBase
    {
        public ReferInfo m_info;
        public Object m_Data = null;
        public static int ShowWaitcircleCount = 0;
        private bool _isRealShowWaitcircle = false;

        public AssetTaskNode(GameResMgr mgr) : base(mgr)
        {
        }

        public override void AddNode<T>(T info, bool isShowWaitcircle = true, Action callBack = null)
        {
            base.AddNode(info, isShowWaitcircle, callBack);

            this.m_info = info as ReferInfo;
            this._isRealShowWaitcircle = isShowWaitcircle;
            this.m_CallBack = callBack;

            _mgr.LoadAssetAsync<Object>(m_info.m_loadType, m_info.m_resName, m_info.m_assetType, (obj) =>
            {
                this.m_Data = obj;
                m_IsLoadCompleted = true;
            });
        }
    }

    public class AssetSingleTaskNode : AssetTaskNodeBase
    {
        public ReferInfo m_info;
        public Object m_Data = null;
        public static int ShowWaitcircleCount = 0;
        private bool _isRealShowWaitcircle = false;

        public AssetSingleTaskNode(GameResMgr mgr) : base(mgr)
        {
        }

        public override void AddNode<T>(T info, bool isShowWaitcircle = true, Action callBack = null)
        {
            base.AddNode(info, isShowWaitcircle, callBack);

            this.m_info = info as ReferInfo;
            this._isRealShowWaitcircle = isShowWaitcircle;
            this.m_CallBack = callBack;

            _mgr.LoadAssetAsync<Object>(m_info.m_loadType, m_info.m_resName, m_info.m_assetType, (obj) =>
            {
                this.m_Data = obj;
                m_IsLoadCompleted = true;
            });
        }
    }

    public class AssetGroupTaskNode : AssetTaskNodeBase
    {
        List<AssetTaskNodeBase> _listObj = new List<AssetTaskNodeBase>();

        public AssetGroupTaskNode(GameResMgr mgr) : base(mgr)
        {
        }

        public override void AddNode<T>(T nodes, bool isShowWaitcircle = true, Action callBack = null)
        {
            base.AddNode<T>(nodes, isShowWaitcircle, callBack);
            if (nodes == null) return;
            List<ReferInfo> nameList = nodes as List<ReferInfo>;
            if (nameList != null && nameList.Count != 0)
            {
                AssetTaskNodeBase curNode = null;
                for (int i = 0; i < nameList.Count; i++)
                {
                    if (nameList[i] == null) continue;
                    curNode = CreatLoadNode(_mgr, nameList[i].m_isSingleAsset);
                    curNode.AddNode(nameList[i], isShowWaitcircle);
                    _listObj.Add(curNode);
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (_listObj.Count != 0)
            {
                for (int i = _listObj.Count - 1; i >= 0; i--)
                {
                    if (_listObj[i].m_IsLoadCompleted)
                        _listObj.RemoveAt(i);
                }
            }
            m_IsLoadCompleted = _listObj.Count == 0;
        }
    }
}
