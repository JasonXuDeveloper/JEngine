using System;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdateScripts
{
    public class ReferInfo
    {
        public AssetType m_assetType;
        public ResType m_loadType;
        public string m_resName;
        public bool m_isSingleAsset = false;

        public ReferInfo(ResType type, string resName, AssetType loadType)
        {
            this.m_assetType = loadType;
            this.m_resName = resName;
            this.m_loadType = type;
        }

        public static ReferInfo Create(ResType type, string resName, AssetType assetType)
        {
            return new ReferInfo(type, resName, assetType);
        }
    }
}
