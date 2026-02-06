using System.Collections.Generic;
using System.IO;
using JEngine.Core.Encrypt;
using UnityEngine;

namespace YooAsset
{
    public class PreprocessBuildCatalog : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        /// <summary>
        /// 在构建应用程序前自动生成内置资源目录文件。
        /// 原理：搜索StreamingAssets目录下的所有资源文件，将这些文件信息写入文件，然后在运行时做查询用途。
        /// </summary>
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            YooLogger.Log("Begin to create catalog file !");

            string rootPath = YooAssetSettingsData.GetYooDefaultBuildinRoot();
            DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
            if (rootDirectory.Exists == false)
            {
                Debug.LogWarning($"Can not found StreamingAssets root directory : {rootPath}");
                return;
            }

            // 搜索所有Package目录
            DirectoryInfo[] subDirectories = rootDirectory.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                string packageName = subDirectory.Name;
                string pacakgeDirectory = subDirectory.FullName;
                if (!TryCreateCatalogFile(packageName, pacakgeDirectory))
                {
                    Debug.LogError($"Create package {packageName} catalog file failed ! See the detail error in console !");
                }
            }
        }

        private static bool TryCreateCatalogFile(string packageName, string packageDirectory)
        {
            // Try without decryption first (unencrypted manifests)
            if (TryCreate(null, packageName, packageDirectory))
                return true;

            // Try each registered encryption method
            foreach (var kvp in EncryptionMapping.Mapping)
            {
                var restore = kvp.Value.ManifestEncryptionConfig.Decryption;
                if (TryCreate(restore, packageName, packageDirectory))
                {
                    YooLogger.Log($"Package '{packageName}' catalog created using {kvp.Key} decryption.");
                    return true;
                }
            }

            return false;
        }

        private static bool TryCreate(IManifestRestoreServices services,
            string packageName, string packageDirectory)
        {
            try
            {
                return CatalogTools.CreateCatalogFile(services, packageName, packageDirectory);
            }
            catch
            {
                return false;
            }
        }
    }
}