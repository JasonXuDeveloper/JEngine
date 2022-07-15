using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;
using UnityEngine.Rendering;

namespace BM
{
    public class BuildAssets : EditorWindow
    {
        /// <summary>
        /// 分包配置文件资源目录
        /// </summary>
        public static string AssetLoadTablePath = "Assets/Dependencies/BundleMaster/Editor/BundleMasterEditor/BuildSettings/AssetLoadTable.asset";
        
        [MenuItem("Tools/BuildAsset/创建分包配置文件")]
        [MenuItem("Assets/Create/BuildAsset/创建分包配置文件")]
        public static void CreateSingleSetting()
        {
            AssetsLoadSetting assetsLoadSetting = ScriptableObject.CreateInstance<AssetsLoadSetting>();
            AssetDatabase.CreateAsset(assetsLoadSetting, "Assets/Dependencies/BundleMaster/Editor/BundleMasterEditor/BuildSettings/AssetsLoadSetting.asset");
        }
        
        [MenuItem("Tools/BuildAsset/创建原生资源分包配置文件")]
        [MenuItem("Assets/Create/BuildAsset/创建原生资源分包配置文件")]
        public static void CreateOriginSetting()
        {
            AssetsOriginSetting assetsOriginSetting = ScriptableObject.CreateInstance<AssetsOriginSetting>();
            AssetDatabase.CreateAsset(assetsOriginSetting, "Assets/Dependencies/BundleMaster/Editor/BundleMasterEditor/BuildSettings/AssetsOriginSetting.asset");
        }
        
        // [MenuItem("Tools/BuildAsset/构建AssetBundle")]
        public static void BuildAllBundle()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            AssetLoadTable assetLoadTable = AssetDatabase.LoadAssetAtPath<AssetLoadTable>(AssetLoadTablePath);
            //清理不存在的AssetSetting
            assetLoadTable.AssetsSettings = assetLoadTable.AssetsSettings.FindAll(s => s != null);
            EditorUtility.SetDirty(assetLoadTable);
            AssetDatabase.SaveAssets();
            
            List<AssetsSetting> assetsSettings = assetLoadTable.AssetsSettings;
            //开始构建前剔除多余场景
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
            foreach (SceneAsset sceneAsset in assetLoadTable.InitScene)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true));
            }
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
            //清空加密资源的文件夹
            string encryptAssetFolderPath = Path.Combine(assetLoadTable.BuildBundlePath + "/../", assetLoadTable.EncryptPathFolder);
            if (!Directory.Exists(encryptAssetFolderPath))
            {
                Directory.CreateDirectory(encryptAssetFolderPath);
            }
            DeleteHelper.DeleteDir(encryptAssetFolderPath);
            //记录工程包含的Shader
            HashSet<string> alwaysIncludedShaders = new HashSet<string>();
            Object graphicsSettings = 
#if UNITY_2020_1_OR_NEWER
                GraphicsSettings.GetGraphicsSettings();
#else
                AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");

#endif
            SerializedObject serializedObject = new SerializedObject(graphicsSettings);
            SerializedProperty serializedProperty = serializedObject.FindProperty("m_AlwaysIncludedShaders");
            if (serializedProperty.isArray)
            {
                for (int i = 0; i < serializedProperty.arraySize; i++)
                {
                    SerializedProperty property = serializedProperty.GetArrayElementAtIndex(i);
                    Object shaderInfo = property.objectReferenceValue;
                    if (!alwaysIncludedShaders.Contains(shaderInfo.name))
                    {
                        alwaysIncludedShaders.Add(shaderInfo.name);
                    }
                }
            }
            //记录所有加载路径
            HashSet<string> allAssetLoadPath = new HashSet<string>();
            //构建所有分包
            foreach (AssetsSetting assetsSetting in assetsSettings)
            {
                if (assetsSetting is AssetsLoadSetting)
                {
                    //获取单个Bundle的配置文件
                    Build(assetLoadTable, assetsSetting as AssetsLoadSetting, allAssetLoadPath, alwaysIncludedShaders);
                }
                else
                {
                    //处理原生资源
                    AnalysisOriginFile(assetLoadTable, assetsSetting as AssetsOriginSetting);

                }
            }
            //生成路径字段代码脚本
            if (assetLoadTable.GeneratePathCode)
            {
                BuildAssetsTools.GeneratePathCode(allAssetLoadPath, assetLoadTable.GenerateCodeScriptPath);
                AssetDatabase.Refresh();
            }
            //构建完成后索引自动+1 需要自己取消注释
            List<int> instanceIds = new List<int>();
            foreach (AssetsSetting assetsLoadSetting in assetLoadTable.AssetsSettings)
            {
                instanceIds.Add(assetsLoadSetting.GetInstanceID());
            }
            //保存配置文件
            foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(AssetsSetting)}"))
            {
                var obj = AssetDatabase.LoadAssetAtPath<AssetsSetting>(AssetDatabase.GUIDToAssetPath(guid));
                if (instanceIds.Contains(obj.GetInstanceID()))
                {
                    AssetLogHelper.Log($"{obj.BuildName}分包已打包成功，版本索引{obj.BuildIndex}->{++obj.BuildIndex}");
                    EditorUtility.SetDirty(obj);
                }
            }
            AssetDatabase.SaveAssets();
            //打包结束
            sw.Stop();
            AssetLogHelper.Log("打包结束, 耗时" + sw.Elapsed.TotalMilliseconds + " ms \n" + assetLoadTable.BuildBundlePath);
            AssetLogHelper.Log("打包目录：\n" + assetLoadTable.BuildBundlePath);
            AssetLogHelper.Log("如果使用了加密，将加密目录内的资源传到资源服务器下载路径即可，例如将生成的EncryptAssets下的子目录存入127.0.0.1/Bundles下，" +
                               "然后Updater的baseUrl写http://127.0.0.1/Bundles即可");
            AssetLogHelper.Log("如果没使用加密，将打包出来的未加密目录内的资源传到资源服务器下载路径即可，例如将生成的BuildBundles下的子目录存入127.0.0.1/Bundles下，" +
                               "然后Updater的baseUrl写http://127.0.0.1/Bundles即可");
        }
        
        [MenuItem("Tools/BuildAsset/Copy资源到StreamingAssets")]
        public static void CopyToStreamingAssets()
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            DeleteHelper.DeleteDir(Application.streamingAssetsPath);
            AssetLoadTable assetLoadTable = AssetDatabase.LoadAssetAtPath<AssetLoadTable>(AssetLoadTablePath);
            foreach (AssetsSetting assetsSetting in assetLoadTable.AssetsSettings)
            {
                if (!(assetsSetting is AssetsLoadSetting assetsLoadSetting))
                {
                    continue;
                }
                string assetPathFolder;
                if (assetsLoadSetting.EncryptAssets)
                {
                    assetPathFolder = Path.Combine(assetLoadTable.BuildBundlePath + "/../", assetLoadTable.EncryptPathFolder, assetsLoadSetting.BuildName);
                }
                else
                {
                    assetPathFolder = Path.Combine(assetLoadTable.BuildBundlePath, assetsLoadSetting.BuildName);
                }
                string directoryPath = Path.Combine(Application.streamingAssetsPath, assetsLoadSetting.BuildName);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                DirectoryInfo subBundlePath = new DirectoryInfo(assetPathFolder);
                FileInfo[] fileInfos = subBundlePath.GetFiles();
                foreach (FileInfo fileInfo in fileInfos)
                {
                    if (fileInfo.DirectoryName == null)
                    {
                        AssetLogHelper.LogError("找不到文件的路径: " + fileInfo.Name);
                        continue;
                    }
                    string filePath = Path.Combine(fileInfo.DirectoryName, fileInfo.Name);
                    string suffix = Path.GetExtension(filePath);
                    if ((!fileInfo.Name.StartsWith("shader_") && string.IsNullOrWhiteSpace(suffix)) || suffix == ".manifest")
                    {
                        continue;
                    }
                    File.Copy(filePath, Path.Combine(directoryPath, fileInfo.Name));
                }
            }
            foreach (AssetsSetting assetsSetting in assetLoadTable.AssetsSettings)
            {
                if (!(assetsSetting is AssetsOriginSetting assetsOriginSetting))
                {
                    continue;
                }
                string assetPathFolder = Path.Combine(assetLoadTable.BuildBundlePath, assetsOriginSetting.BuildName);
                string directoryPath = Path.Combine(Application.streamingAssetsPath, assetsOriginSetting.BuildName);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                //获取所有资源目录
                HashSet<string> files = new HashSet<string>();
                HashSet<string> dirs = new HashSet<string>();
                BuildAssetsTools.GetOriginsPath(assetPathFolder, files, dirs);
                //Copy资源
                foreach (string dir in dirs)
                {
                    Directory.CreateDirectory(dir.Replace(assetPathFolder, directoryPath));
                }
                foreach (string file in files)
                {
                    File.Copy(file, file.Replace(assetPathFolder, directoryPath), true);
                }
            }
            AssetDatabase.Refresh();
            AssetLogHelper.Log("已将资源复制到StreamingAssets");
        }
        
        private static void Build(AssetLoadTable assetLoadTable, AssetsLoadSetting assetsLoadSetting, HashSet<string> assetLoadPath, HashSet<string> alwaysIncludedShaders)
        {
            Dictionary<string, LoadFile> loadFileDic = new Dictionary<string, LoadFile>();
            Dictionary<string, LoadDepend> loadDependDic = new Dictionary<string, LoadDepend>();
            Dictionary<string, LoadGroup> loadGroupDic = new Dictionary<string, LoadGroup>();
            
            //需要主动加载的文件的路径以及它的依赖bundle名字
            Dictionary<string, string[]> allLoadBaseAndDepends = new Dictionary<string, string[]>();
            //所有需要主动加载的资源的路径
            string[] paths = assetsLoadSetting.AssetPath.ToArray();
            //一个被依赖的文件依赖的次数(依赖也是包含后缀的路径)
            Dictionary<string, int> dependenciesIndex = new Dictionary<string, int>();
            //所有shader的集合，shader单独一个包
            HashSet<string> shaders = new HashSet<string>();
            //所有选定的主动加载的文件(包含后缀)
            HashSet<string> files = new HashSet<string>();
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                //获取所有需要主动加载的资源
                BuildAssetsTools.GetChildFiles(path, files, assetsLoadSetting.BlacklistFile, assetsLoadSetting.BlacklistExtension);
            }
            //添加打包进去的场景
            var sceneAssets = BuildAssetsTools.GetPackageSceneAssets(assetsLoadSetting).ToArray();
            for (int i = 0; i < sceneAssets.Length; i++)
            {
                SceneAsset sceneAsset = sceneAssets[i];
                string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                if (!files.Contains(scenePath))
                {
                    files.Add(scenePath);
                }
            }
            //创建组包
            foreach (string assetGroupPath in assetsLoadSetting.AssetGroupPaths)
            {
                LoadGroup loadGroup = new LoadGroup();
                loadGroup.FilePath = assetGroupPath;
                loadGroup.AssetBundleName = GetBundleName(assetsLoadSetting, assetGroupPath) + "." + assetsLoadSetting.BundleVariant;
                loadGroupDic.Add(assetGroupPath, loadGroup);
            }
            //分析所有需要主动加载的资源
            List<string> needRemoveFile = new List<string>();
            Dictionary<string, List<string>> groupFileToRealDepends = new Dictionary<string, List<string>>();
            foreach (string file in files)
            {
                //Shader资源单独处理
                if (BuildAssetsTools.IsShaderAsset(file))
                {
                    Shader shaderObj = AssetDatabase.LoadAssetAtPath<Shader>(file);
                    if (shaderObj != null && alwaysIncludedShaders.Contains(shaderObj.name))
                    {
                        continue;
                    }
                    if (!shaders.Contains(file))
                    {
                        shaders.Add(file);
                    }
                    if (!needRemoveFile.Contains(file))
                    {
                        needRemoveFile.Add(file);
                    }
                    continue;
                }
                //判断是否是组资源
                string fileGroupPath = BuildAssetsTools.GetGroupAssetPath(file, assetsLoadSetting);
                if (fileGroupPath != null)
                {
                    needRemoveFile.Add(file);
                }
                else
                {
                    //创建主动加载文件的加载所需信息
                    LoadFile loadFile = new LoadFile();
                    loadFile.FilePath = file;
                    loadFile.AssetBundleName = GetBundleName(assetsLoadSetting, file) + "." + assetsLoadSetting.BundleVariant;
                    loadFileDic.Add(file, loadFile);
                }
                //获取依赖
                string[] depends = AssetDatabase.GetDependencies(file);
                //过滤出真正需要加载的依赖
                List<string> realDepends = new List<string>();
                //分析依赖情况
                for (int i = 0; i < depends.Length; i++)
                {
                    string depend = depends[i];
                    //脚本不能作为依赖
                    if (depend.EndsWith(".cs"))
                    {
                        continue;
                    }
                    //shader单独进包
                    if (BuildAssetsTools.IsShaderAsset(depend))
                    {
                        Shader shaderObj = AssetDatabase.LoadAssetAtPath<Shader>(depend);
                        if (shaderObj != null && alwaysIncludedShaders.Contains(shaderObj.name))
                        {
                            continue;
                        }
                        if (!shaders.Contains(depend))
                        {
                            shaders.Add(depend);
                        }
                        continue;
                    }
                    if (!depend.StartsWith("Assets/"))
                    {
                        continue;
                    }
                    if (!BuildAssetsTools.CantLoadFile(depend, assetsLoadSetting.BlacklistFile))
                    {
                        continue;
                    }
                    string dependGroup = BuildAssetsTools.GetGroupAssetPath(depend, assetsLoadSetting);
                    if (dependGroup != null)
                    {
                        if (dependGroup == fileGroupPath)
                        {
                            continue;
                        }
                        if (!realDepends.Contains(dependGroup))
                        {
                            realDepends.Add(dependGroup);
                        }
                    }
                    else
                    {
                        if (files.Contains(depend))
                        {
                            if (depend == file)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            //作为依赖计算被依赖的次数
                            if (dependenciesIndex.ContainsKey(depend))
                            {
                                dependenciesIndex[depend]++;
                            }
                            else
                            {
                                dependenciesIndex.Add(depend, 1);
                            }
                        }
                        //作为文件被依赖了直接添加就行
                        realDepends.Add(depend);
                    }
                }
                if (fileGroupPath != null)
                {
                    loadGroupDic[fileGroupPath].FilePathList.Add(file);
                    groupFileToRealDepends.Add(file, realDepends);
                }
                else
                {
                    allLoadBaseAndDepends.Add(file, realDepends.ToArray());
                }
            }
            foreach (LoadGroup loadGroup in loadGroupDic.Values)
            {
                foreach (string groupFile in loadGroup.FilePathList)
                {
                    foreach (string realDepend in groupFileToRealDepends[groupFile])
                    {
                        if (loadGroup.FilePathList.Contains(realDepend))
                        {
                            continue;
                        }
                        if (!dependenciesIndex.ContainsKey(realDepend))
                        {
                            string realDependGroup = BuildAssetsTools.GetGroupAssetPath(realDepend, assetsLoadSetting);
                            if (realDependGroup != null)
                            {
                                if (loadGroup.DependFileName.Contains(realDependGroup))
                                {
                                    continue;
                                }
                                loadGroup.DependFileName.Add(realDependGroup);
                            }
                            continue;
                        }
                        if (dependenciesIndex[realDepend] == 1)
                        {
                            continue;
                        }
                        else
                        {
                            //说明这个依赖是单独依赖并且被Group依赖
                            if (!loadDependDic.ContainsKey(realDepend))
                            {
                                LoadDepend loadDepend = new LoadDepend();
                                loadDepend.FilePath = realDepend;
                                loadDepend.AssetBundleName = GetBundleName(assetsLoadSetting, realDepend) + "." + assetsLoadSetting.BundleVariant;
                                loadDependDic.Add(realDepend, loadDepend);
                            }
                        }
                        if (loadGroup.DependFileName.Contains(realDepend))
                        {
                            continue;
                        }
                        loadGroup.DependFileName.Add(realDepend);
                    }
                }
            }
            foreach (string removeFile in needRemoveFile)
            {
                files.Remove(removeFile);
            }
            //被复合依赖的文件
            HashSet<string> compoundDepends = new HashSet<string>();
            foreach (var dependFile in dependenciesIndex)
            {
                if (dependFile.Value > 1)
                {
                    compoundDepends.Add(dependFile.Key);
                }
            }
            //添加依赖信息
            foreach (var fileAndDepend in allLoadBaseAndDepends)
            {
                LoadFile loadFile = loadFileDic[fileAndDepend.Key];
                List<string> depends = new List<string>();
                foreach (string depend in fileAndDepend.Value)
                {
                    if (compoundDepends.Contains(depend))
                    {
                        //说明这个被依赖项是一个单独的bundle
                        depends.Add(depend);
                        //被依赖项也要创建Load类
                        if (!loadDependDic.ContainsKey(depend))
                        {
                            LoadDepend loadDepend = new LoadDepend();
                            loadDepend.FilePath = depend;
                            loadDepend.AssetBundleName = GetBundleName(assetsLoadSetting, depend) + "." + assetsLoadSetting.BundleVariant;
                            loadDependDic.Add(depend, loadDepend);
                        }
                    }
                    else
                    {
                        //说明这是一个被依赖的File
                        if (files.Contains(depend))
                        {
                            depends.Add(depend);
                        }
                        //说名这个依赖是一个组资源
                        string groupPath = BuildAssetsTools.GetGroupAssetPath(depend, assetsLoadSetting);
                        if (groupPath != null)
                        {
                            if (!depends.Contains(groupPath))
                            {
                                depends.Add(groupPath);
                            }
                        }
                    }
                }
                loadFile.DependFileName = depends.ToArray();
            }
            //创建需要的Bundle包
            List<AssetBundleBuild> allAssetBundleBuild = new List<AssetBundleBuild>();
            //首先创建Shader
            AssetBundleBuild shaderBundle = new AssetBundleBuild();
            shaderBundle.assetBundleName = "shader_" + assetsLoadSetting.BuildName;
            shaderBundle.assetNames = shaders.ToArray();
            allAssetBundleBuild.Add(shaderBundle);
            //添加文件以及依赖的bundle包
            AddToAssetBundleBuilds(assetsLoadSetting, allAssetBundleBuild, files);
            AddToAssetBundleBuilds(assetsLoadSetting, allAssetBundleBuild, compoundDepends);
            foreach (LoadGroup loadGroup in loadGroupDic.Values)
            {
                AssetBundleBuild loadGroupBundle = new AssetBundleBuild();
                loadGroupBundle.assetBundleName = loadGroup.AssetBundleName;
                loadGroupBundle.assetNames = loadGroup.FilePathList.ToArray();
                allAssetBundleBuild.Add(loadGroupBundle);
            }
            if (!(allAssetBundleBuild.Count > 0))
            {
                AssetLogHelper.LogError("没有资源: " + assetsLoadSetting.BuildName);
                return;
            }
            //保存打包Log
            SaveLoadLog(assetLoadTable, assetsLoadSetting, loadFileDic, loadDependDic, loadGroupDic);
            //开始打包
            string bundlePackagePath = Path.Combine(assetLoadTable.BuildBundlePath, assetsLoadSetting.BuildName);
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(bundlePackagePath, allAssetBundleBuild.ToArray(), 
                assetsLoadSetting.BuildAssetBundleOptions, EditorUserBuildSettings.activeBuildTarget);
            //保存未加密的版本号文件
            SaveBundleVersionFile(bundlePackagePath, manifest, assetsLoadSetting, false);
            //如果此分包需要加密就生成加密的资源
            if (assetsLoadSetting.EncryptAssets)
            {
                string encryptAssetPath = Path.Combine(assetLoadTable.BuildBundlePath + "/../", assetLoadTable.EncryptPathFolder, assetsLoadSetting.BuildName);
                //创建加密的资源
                BuildAssetsTools.CreateEncryptAssets(bundlePackagePath, encryptAssetPath, manifest, assetsLoadSetting.SecretKey);
                //保存加密的版本号文件
                SaveBundleVersionFile(encryptAssetPath, manifest, assetsLoadSetting, true);
                //复制Log信息
                File.Copy(Path.Combine(assetLoadTable.BuildBundlePath, assetsLoadSetting.BuildName, "FileLogs.txt"), Path.Combine(encryptAssetPath, "FileLogs.txt"));
                File.Copy(Path.Combine(assetLoadTable.BuildBundlePath, assetsLoadSetting.BuildName, "DependLogs.txt"), Path.Combine(encryptAssetPath, "DependLogs.txt"));
                File.Copy(Path.Combine(assetLoadTable.BuildBundlePath, assetsLoadSetting.BuildName, "GroupLogs.txt"), Path.Combine(encryptAssetPath, "GroupLogs.txt"));
            }
            //存储记录所有资源的加载路径
            foreach (string assetPath in loadFileDic.Keys)
            {
                if (!assetLoadPath.Contains(assetPath))
                {
                    assetLoadPath.Add(assetPath);
                }
            }
            foreach (LoadGroup loadGroup in loadGroupDic.Values)
            {
                foreach (string filePathList in loadGroup.FilePathList)
                {
                    if (!assetLoadPath.Contains(filePathList))
                    {
                        assetLoadPath.Add(filePathList);
                    }
                }
            }
        }

        /// <summary>
        /// 分析原生资源包
        /// </summary>
        private static void AnalysisOriginFile(AssetLoadTable assetLoadTable, AssetsOriginSetting assetsOriginSetting)
        {
            assetsOriginSetting.OriginFilePath = new DirectoryInfo(assetsOriginSetting.OriginFilePath).FullName;
            string filePath = Path.Combine(assetLoadTable.BuildBundlePath, assetsOriginSetting.BuildName);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            DeleteHelper.DeleteDir(filePath);
            //获取所有资源目录
            HashSet<string> files = new HashSet<string>();
            HashSet<string> dirs = new HashSet<string>();
            BuildAssetsTools.GetOriginsPath(assetsOriginSetting.OriginFilePath, files, dirs);
            //Copy资源
            foreach (string dir in dirs)
            {
                Directory.CreateDirectory(dir.Replace(assetsOriginSetting.OriginFilePath, filePath));
            }
            List<string> letFilePaths = new List<string>();
            foreach (string file in files)
            {
#if !UNITY_EDITOR_OSX
                string letFilePath = file.Replace(assetsOriginSetting.OriginFilePath + "\\", null);
#else
                string letFilePath = file.Replace(assetsOriginSetting.OriginFilePath + "/", null);
#endif
                letFilePaths.Add(letFilePath);
                File.Copy(file, Path.Combine(filePath, letFilePath), true);
            }
            //生成版本文件
            SaveOriginFileVersionFile(filePath, letFilePaths.ToArray(), assetsOriginSetting);
        }
        
        /// <summary>
        /// 创建AssetBundleBuild并添加管理
        /// </summary>
        private static void AddToAssetBundleBuilds(AssetsLoadSetting assetsLoadSetting, List<AssetBundleBuild> assetBundleBuild, HashSet<string> filePaths)
        {
            foreach (string filePath in filePaths)
            {
                AssetBundleBuild abb = new AssetBundleBuild();
                abb.assetBundleName = GetBundleName(assetsLoadSetting, filePath);
                abb.assetNames = new string[] { filePath };
                abb.assetBundleVariant = assetsLoadSetting.BundleVariant;
                assetBundleBuild.Add(abb);
            }
        }

        /// <summary>
        /// 保存加载用的Log
        /// </summary>
        private static void SaveLoadLog(AssetLoadTable assetLoadTable, AssetsLoadSetting assetsLoadSetting, Dictionary<string, LoadFile> loadFiles, Dictionary<string, LoadDepend> loadDepends, Dictionary<string, LoadGroup> loadGroups)
        {
            if (!Directory.Exists(Path.Combine(assetLoadTable.BuildBundlePath, assetsLoadSetting.BuildName)))
            {
                Directory.CreateDirectory(Path.Combine(assetLoadTable.BuildBundlePath, assetsLoadSetting.BuildName));
            }
            using (StreamWriter sw = new StreamWriter(Path.Combine(assetLoadTable.BuildBundlePath, assetsLoadSetting.BuildName, "FileLogs.txt")))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var loadFile in loadFiles)
                {
                    string data = "<" + loadFile.Key + "|" + loadFile.Value.AssetBundleName + "|";
                    foreach (string depend in loadFile.Value.DependFileName)
                    {
                        data += depend + "|";
                    }
                    data = data.Substring(0, data.Length - 1);
                    data += ">" + "\n";
                    sb.Append(data);
                }
                sw.WriteLine(sb.ToString());
            }
            using (StreamWriter sw = new StreamWriter(Path.Combine(assetLoadTable.BuildBundlePath, assetsLoadSetting.BuildName, "DependLogs.txt")))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var loadDepend in loadDepends)
                {
                    string data = "<" + loadDepend.Key + "|" + loadDepend.Value.AssetBundleName + ">\n";
                    sb.Append(data);
                }
                sw.WriteLine(sb.ToString());
            }
            using (StreamWriter sw = new StreamWriter(Path.Combine(assetLoadTable.BuildBundlePath, assetsLoadSetting.BuildName, "GroupLogs.txt")))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var loadGroup in loadGroups)
                {
                    string data = "<" + loadGroup.Key + "|" + loadGroup.Value.AssetBundleName + "|";
                    foreach (string depend in loadGroup.Value.DependFileName)
                    {
                        data += depend + "|";
                    }
                    data = data.Substring(0, data.Length - 1);
                    data += ">" + "\n";
                    sb.Append(data);
                }
                sw.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// 保存Bundle的版本号文件
        /// </summary>
        private static void SaveBundleVersionFile(string bundlePackagePath, AssetBundleManifest manifest, AssetsLoadSetting assetsLoadSetting, bool encrypt)
        {
            string[] assetBundles = manifest.GetAllAssetBundles();
            using (StreamWriter sw = new StreamWriter(Path.Combine(bundlePackagePath, "VersionLogs.txt")))
            {
                StringBuilder sb = new StringBuilder();
                string versionHandler = System.DateTime.Now + "|" + assetsLoadSetting.BuildIndex + "|" + (encrypt).ToString() + "\n";
                sb.Append(versionHandler);
                foreach (string assetBundle in assetBundles)
                {
                    string bundlePath = Path.Combine(bundlePackagePath, assetBundle);
                    uint crc32 = VerifyHelper.GetCRC32(File.ReadAllBytes(bundlePath));
                    string info = assetBundle + "|" + VerifyHelper.GetFileLength(bundlePath) + "|" + crc32 + "\n";
                    sb.Append(info);
                }
                sw.WriteLine(sb.ToString());
            }
        }
        
        /// <summary>
        /// 保存原始的版本号文件
        /// </summary>
        private static void SaveOriginFileVersionFile(string bundlePackagePath, string[] filePaths, AssetsOriginSetting assetsOriginSetting)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(bundlePackagePath, "VersionLogs.txt")))
            {
                StringBuilder sb = new StringBuilder();
                string versionHandler = System.DateTime.Now + "|" + assetsOriginSetting.BuildIndex + "|" + "origin" + "\n";
                sb.Append(versionHandler);
                foreach (string filePath in filePaths)
                {
                    string bundlePath = Path.Combine(bundlePackagePath, filePath);
                    uint crc32 = VerifyHelper.GetCRC32(File.ReadAllBytes(bundlePath));
                    string info = filePath + "|" + VerifyHelper.GetFileLength(bundlePath) + "|" + crc32 + "\n";
                    sb.Append(info);
                }
                sw.WriteLine(sb.ToString());
            }
        }
    
        private static string GetBundleName(AssetsLoadSetting assetsLoadSetting, string filePath)
        {
            string bundlePackageName = assetsLoadSetting.BuildName.ToLower();
            if (assetsLoadSetting.NameByHash)
            {
                filePath = VerifyHelper.GetMd5Hash(bundlePackageName + filePath);
            }
            else
            {
                filePath = filePath.Replace("/", "_");
                filePath = filePath.Replace(".", "_");
                filePath = filePath.Replace(" ", "_");
                // filePath = filePath.Replace("+", "_");
                // filePath = filePath.Replace("-", "_");
                filePath = bundlePackageName + "_" + filePath;
                filePath = filePath.ToLower();
            }
            return filePath;
        }
        
        /// <summary>
        /// 使用正则表达式替换或去掉半角标点符号(如果资源命名非常混乱可尝试此方法，注意可能会引起文件重名，推荐使用Hash名)
        /// </summary>
        private static string RemoveSymbol(string keyText)
        {
            string pattern = @"[~!@#\$%\^&\*\(\)\+=\|\\\}\]\{\[:;<,>\?\/""]+";
            Regex seperatorReg = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            keyText = seperatorReg.Replace(keyText, "__").Trim();
            return keyText;
        }
    }
}