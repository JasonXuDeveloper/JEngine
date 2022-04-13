using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Directory = UnityEngine.Windows.Directory;

namespace BM
{
    public static class BuildAssetsTools
    {
        /// <summary>
        /// 获取一个目录下所有的子文件
        /// </summary>
        public static void GetChildFiles(string basePath, HashSet<string> files)
        {
            DirectoryInfo basefolder = new DirectoryInfo(basePath);
            FileInfo[] basefil = basefolder.GetFiles();
            for (int i = 0; i < basefil.Length; i++)
            {
                    
                if (CantLoadType(basefil[i].FullName))
                {
                    files.Add(basePath + "/" + basefil[i].Name);
                }
            }
            Er(basePath);
            void Er(string subPath)
            {
                string[] subfolders = AssetDatabase.GetSubFolders(subPath);
                for (int i = 0; i < subfolders.Length; i++)
                {
                    DirectoryInfo subfolder = new DirectoryInfo(subfolders[i]);
                    FileInfo[] fil = subfolder.GetFiles();
                    for (int j = 0; j < fil.Length; j++)
                    {
                    
                        if (CantLoadType(fil[j].FullName))
                        {
                            files.Add(subfolders[i] + "/" + fil[j].Name);
                        }
                    }
                    Er(subfolders[i]);
                }
            }
        }
        
        /// <summary>
        /// 创建加密的AssetBundle
        /// </summary>
        public static void CreateEncryptAssets(string bundlePackagePath, string encryptAssetPath, AssetBundleManifest manifest, string secretKey)
        {
            string[] assetBundles = manifest.GetAllAssetBundles();
            foreach (string assetBundle in assetBundles)
            {
                string bundlePath = Path.Combine(bundlePackagePath, assetBundle);
                if (!Directory.Exists(encryptAssetPath))
                {
                    Directory.CreateDirectory(encryptAssetPath);
                }
                using (FileStream fs = new FileStream(Path.Combine(encryptAssetPath, assetBundle), FileMode.OpenOrCreate))
                {
                    byte[] encryptBytes = VerifyHelper.CreateEncryptData(bundlePath, secretKey);
                    fs.Write(encryptBytes, 0, encryptBytes.Length);
                }
            }
        }
        
        /// <summary>
        /// 需要忽略加载的格式
        /// </summary>
        public static bool CantLoadType(string fileFullName)
        {
            string suffix = Path.GetExtension(fileFullName);
            switch (suffix)
            {
                case ".dll":
                    return false;
                case ".cs":
                    return false;
                case ".meta":
                    return false;
                case ".js":
                    return false;
                case ".boo":
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// 是Shader资源
        /// </summary>
        public static bool IsShaderAsset(string fileFullName)
        {
            string suffix = Path.GetExtension(fileFullName);
            switch (suffix)
            {
                case ".shader":
                    return true;
                case ".shadervariants":
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取全部分包内的全部场景
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<SceneAsset> GetPackageSceneAssets(AssetLoadTable table)
        {
            List<SceneAsset> sceneAssetsLst = new List<SceneAsset>();
            foreach (var assetsLoadSetting in table.AssetsLoadSettings)
            {
                sceneAssetsLst.AddRange(GetPackageSceneAssets(assetsLoadSetting));
            }

            return sceneAssetsLst;
        }
        
        /// <summary>
        /// 获取分包内的场景
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static List<SceneAsset> GetPackageSceneAssets(AssetsLoadSetting setting)
        {
            List<SceneAsset> sceneAssetsLst = new List<SceneAsset>();
            foreach (var path in setting.ScenePath)
            {
                TryGetUnityObjectsOfTypeFromPath<SceneAsset>(path, out var lst);
                foreach (var asset in lst)
                {
                    if (asset is SceneAsset sa)
                    {
                        sceneAssetsLst.Add(sa);
                    }
                }
            }

            return sceneAssetsLst;
        }
        
        /// <summary>
        /// Adds newly (if not already in the list) found assets.
        /// Returns how many found (not how many added)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="assetsFound">Adds to this list if it is not already there</param>
        /// <returns></returns>
        public static int TryGetUnityObjectsOfTypeFromPath<T>(string path, out List<T> assetsFound) where T : UnityEngine.Object
        {
            string[] filePaths = System.IO.Directory.GetFiles(path,"*",SearchOption.AllDirectories);
            int countFound = 0;
            assetsFound = new List<T>();
            if (filePaths.Length > 0)
            {
                for (int i = 0; i < filePaths.Length; i++)
                {
                    UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(filePaths[i], typeof(T));
                    if (obj is T asset)
                    {
                        countFound++;
                        if (!assetsFound.Contains(asset))
                        {
                            assetsFound.Add(asset);
                        }
                    }
                }
            }
 
            return countFound;
        }
        
        /// <summary>
        /// 是否生成路径字段代码脚本
        /// </summary>
        public static void GeneratePathCode(HashSet<string> allAssetPaths, string scriptFilePaths)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(Application.dataPath, scriptFilePaths, "BPath.cs")))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("// ReSharper disable All\n");
                sb.Append("namespace BM\n");
                sb.Append("{\n");
                sb.Append("\tpublic class BPath\n");
                sb.Append("\t{\n");
                foreach (string assetPath in allAssetPaths)
                {
                    string name = assetPath.Replace("/", "_");
                    name = name.Replace(".", "__");
                    name = name.Replace("-", "_");
                    name = name.Replace(" ", "__");
                    name = RemoveSymbol(name);
                    sb.Append("\t\tpublic const string " + name + " = \"" + assetPath + "\";\n");
                }
                sb.Append("\t}\n");
                sb.Append("}");
                sw.WriteLine(sb.ToString());
            }
        }
        
        /// <summary>
        /// 使用正则表达式替换或去掉半角标点符号
        /// </summary>
        /// <param name="keyText"></param>
        /// <returns></returns>
        private static string RemoveSymbol(string keyText)
        {
            string pattern = @"[~!@#\$%\^&\*\(\)\+=\|\\\}\]\{\[:;<,>\?\/""]+";
            Regex seperatorReg = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            keyText = seperatorReg.Replace(keyText, "__").Trim();
            return keyText;
        }
    }
}