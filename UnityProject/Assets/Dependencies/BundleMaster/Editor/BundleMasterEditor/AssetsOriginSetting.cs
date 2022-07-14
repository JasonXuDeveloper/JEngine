using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BM
{
    /// <summary>
    /// 用于配置原生资源的更新
    /// </summary>
    public class AssetsOriginSetting : AssetsSetting
    {
        [Header("资源路径")]
        [Tooltip("需要打包的资源所在的路径(不需要包含依赖, 只包括需要主动加载的资源)")]
        public string OriginFilePath = "";
    }
}