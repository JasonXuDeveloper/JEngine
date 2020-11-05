using System;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdateScripts
{
    public abstract class AResRefer
    {
        protected List<ReferInfo> _referList = new List<ReferInfo>();

        public abstract void Refer();

        public abstract void LoadRefer(Action call, bool isloadFromResource = false);

        public abstract void ReleaseRefer();

        public virtual void AddRefer(ResType type, string referStr, AssetType assetType)
        {
            ReferInfo refer = null;

            if (!string.IsNullOrEmpty(referStr))
            {
                refer = new ReferInfo(type, referStr, assetType);
                _referList.Add(refer);
            }
        }
        public List<ReferInfo> GetReferList { get { return _referList; } }
    }
}
