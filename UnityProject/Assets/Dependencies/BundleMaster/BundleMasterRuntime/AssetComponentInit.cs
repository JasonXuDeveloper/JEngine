using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using ET;
using UnityEngine.Networking;

namespace BM
{
    public static partial class AssetComponent
    {
        /// <summary>
        /// Bundle初始化的信息
        /// </summary>
        internal static readonly Dictionary<string, BundleRuntimeInfo> BundleNameToRuntimeInfo = new Dictionary<string, BundleRuntimeInfo>();

        /// <summary>
        /// 初始化
        /// </summary>
        public static async ETTask Initialize(string bundlePackageName, string secretKey = null)
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                AssetLogHelper.Log("AssetLoadMode = Develop 不需要初始化Bundle配置文件");
                return;
            }
            if (BundleNameToRuntimeInfo.ContainsKey(bundlePackageName))
            {
                AssetLogHelper.LogError(bundlePackageName + " 重复初始化");
                return;
            }
            BundleRuntimeInfo bundleRuntimeInfo = new BundleRuntimeInfo(bundlePackageName, secretKey);
            BundleNameToRuntimeInfo.Add(bundlePackageName, bundleRuntimeInfo);

            ETTask fileTcs= ETTask.Create();
            string filePath = BundleFileExistPath(bundlePackageName, "FileLogs.txt");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(filePath))
            {
                UnityWebRequestAsyncOperation weq = webRequest.SendWebRequest();
                weq.completed += (o) =>
                {
                    fileTcs.SetResult();
                };
                await fileTcs;
#if UNITY_2020_1_OR_NEWER
                if (webRequest.result != UnityWebRequest.Result.Success)
#else
                if (!string.IsNullOrEmpty(webRequest.error))
#endif
                {
                    AssetLogHelper.LogError("没有找到 " + bundlePackageName + " Bundle的FileLogs\n" + filePath);
                    return;
                }
                string fileLogs = webRequest.downloadHandler.text;
                Regex reg = new Regex(@"\<(.+?)>");
                MatchCollection matchCollection = reg.Matches(fileLogs);
                List<string> dependFileName = new List<string>();
                foreach (Match m in matchCollection)
                {
                    string[] fileLog = m.Groups[1].Value.Split('|');
                    LoadFile loadFile = new LoadFile();
                    loadFile.FilePath = fileLog[0];
                    loadFile.AssetBundleName = fileLog[1];
                    
                    if (fileLog.Length > 2)
                    {
                        for (int i = 2; i < fileLog.Length; i++)
                        {
                            dependFileName.Add(fileLog[i]);
                        }
                    }
                    loadFile.DependFileName = dependFileName.ToArray();
                    dependFileName.Clear();
                    bundleRuntimeInfo.LoadFileDic.Add(loadFile.FilePath, loadFile);
                }
            }
            ETTask dependTcs = ETTask.Create();
            string dependPath = BundleFileExistPath(bundlePackageName, "DependLogs.txt");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(dependPath))
            {
                UnityWebRequestAsyncOperation weq = webRequest.SendWebRequest();
                weq.completed += (o) =>
                {
                    dependTcs.SetResult();
                };
                await dependTcs;
#if UNITY_2020_1_OR_NEWER
                if (webRequest.result != UnityWebRequest.Result.Success)
#else
                if (!string.IsNullOrEmpty(webRequest.error))
#endif
                {
                    AssetLogHelper.LogError("没有找到 " + bundlePackageName + " Bundle的DependLogs\n" + dependPath);
                    return;
                }
                string dependLogs = webRequest.downloadHandler.text;
                Regex reg = new Regex(@"\<(.+?)>");
                MatchCollection matchCollection = reg.Matches(dependLogs);
                foreach (Match m in matchCollection)
                {
                    string[] dependLog = m.Groups[1].Value.Split('|');
                    LoadDepend loadDepend = new LoadDepend();
                    loadDepend.FilePath = dependLog[0];
                    loadDepend.AssetBundleName = dependLog[1];
                    bundleRuntimeInfo.LoadDependDic.Add(loadDepend.FilePath, loadDepend);
                }
            }
            //加载当前分包的shader
            await LoadShader(bundlePackageName);
        }
        
        /// <summary>
        /// 加载Shader文件
        /// </summary>
        private static async ETTask LoadShader(string bundlePackageName)
        {
            ETTask tcs = ETTask.Create();
            string shaderPath = BundleFileExistPath(bundlePackageName, "shader_" + bundlePackageName.ToLower());
            byte[] shaderData;
            if (BundleNameToRuntimeInfo[bundlePackageName].Encrypt)
            {
                shaderData = await VerifyHelper.GetDecryptDataAsync(shaderPath, null, BundleNameToRuntimeInfo[bundlePackageName].SecretKey);
            }
            else
            {
                shaderData = await VerifyHelper.GetDecryptDataAsync(shaderPath);
            }
            if (shaderData == null)
            {
                tcs.SetResult();
            }
            else
            {
                AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(shaderData);
                request.completed += operation =>
                {
                    BundleNameToRuntimeInfo[bundlePackageName].Shader = request.assetBundle;
                    tcs.SetResult();
                };
            }
            await tcs;
        }
        
    }
}