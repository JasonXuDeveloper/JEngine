using System.IO;
using JEngine.Core;
using UnityEngine;
using YooAsset;

/// <summary>
/// 资源文件偏移加载解密类
/// </summary>
public class FileOffsetDecryption : IDecryptionServices
{
    /// <summary>
    /// 同步方式获取解密的资源包对象
    /// 注意：加载流对象在资源包对象释放的时候会自动释放
    /// </summary>
    AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
    {
        managedStream = null;
        return AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
    }

    /// <summary>
    /// 异步方式获取解密的资源包对象
    /// 注意：加载流对象在资源包对象释放的时候会自动释放
    /// </summary>
    AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
    {
        managedStream = null;
        return AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
    }

    private static ulong GetFileOffset()
    {
        return 32;
    }
}