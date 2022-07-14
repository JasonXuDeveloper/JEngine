using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace BM
{
    /// <summary>
    /// 用于配置所有分包的构建信息
    /// </summary>
    public class AssetLoadTable : ScriptableObject
    {
        [Header("相对构建路径文件夹名称")]
        [Tooltip("构建的资源的相对路径(Assets同级目录下的路径)")] 
        public string BundlePath = "BuildBundles";

        [Header("是否启用绝对构建路径")]
        [Tooltip("启用后使用绝对路径")] 
        public bool EnableRelativePath = false;
        
        [Header("绝对路径")]
        [Tooltip("自己填的绝对路径，替换掉Assets同级路径")] 
        public string RelativePath = "";
        
        [Header("加密资源文件夹名称")]
        [Tooltip("加密的资源所在的和普通资源同级路径的文件夹名称")] 
        public string EncryptPathFolder = "EncryptAssets";

        [Header("初始场景")]
        [Tooltip("最后不打Bundle直接打进包体里的场景(Scene In Build 里填的场景)")] 
        public List<SceneAsset> InitScene = new List<SceneAsset>();

        [Header("是否生成路径字段代码脚本")]
        [Tooltip("字段匹配路径")] 
        public bool GeneratePathCode = false;
        
        [Header("相对构建路径文件夹名称")]
        [Tooltip("构建的资源的相对路径(Assets同级目录下的路径)")] 
        public string GenerateCodeScriptPath = "";
        
        /// <summary>
        /// 返回打包路径
        /// </summary>
        public string BuildBundlePath
        {
            get
            {
                if (EnableRelativePath)
                {
                    return Path.Combine(RelativePath, BundlePath);
                }
                string path = Path.Combine(Application.dataPath + "/../", BundlePath);
                DirectoryInfo info;
                if (!Directory.Exists(path))
                {
                    info = Directory.CreateDirectory(path);
                }
                else
                {
                    info = new DirectoryInfo(path);
                }
                return info.FullName;
            }
        }
        
        [FormerlySerializedAs("AssetsLoadSettings")]
        [Header("所有分包配置信息")]
        [Tooltip("每一个分包的配置信息")]
        public List<AssetsSetting> AssetsSettings = new List<AssetsSetting>();
    }
}