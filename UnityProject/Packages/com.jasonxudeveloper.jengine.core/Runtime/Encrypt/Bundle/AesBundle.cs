// AesBundle.cs
//
//  Author:
//        JasonXuDeveloper <jason@xgamedev.net>
//
//  Copyright (c) 2025 JEngine
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.

using System.IO;
using JEngine.Core.Encrypt.Config;
using JEngine.Core.Encrypt.Manifest;
using JEngine.Core.Encrypt.Shared;
using UnityEngine;
using YooAsset;

namespace JEngine.Core.Encrypt.Bundle
{
    public class AesBundle : BundleEncryptionConfig<AesConfig, AesManifest, AesConfig, AesEncryptionServices,
        AesDecryptionServices>
    {
        public override AesConfig ManifestConfig =>  AesConfig.Instance;
        public override AesConfig BundleConfig  =>  AesConfig.Instance;
    }

    public class AesEncryptionServices : IEncryptionServices
    {
        private readonly AesConfig _config;

        public AesEncryptionServices(AesConfig config)
        {
            _config = config;
        }

        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
#if UNITY_EDITOR
            var bytes = File.ReadAllBytes(fileInfo.FileLoadPath);

            // Use AES-256 CBC with PKCS7 padding for encryption
            var encryptedData = AesUtil.AesEncrypt(bytes, _config.key, _config.iv);

            EncryptResult result = new EncryptResult()
            {
                Encrypted = true,
                EncryptedData = encryptedData
            };
            return result;
#else
            throw new System.NotSupportedException("AES encryption is only supported in Unity Editor");
#endif
        }
    }

    public class AesDecryptionServices : IDecryptionServices
    {
        private readonly AesConfig _config;

        public AesDecryptionServices(AesConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            byte[] decryptedData = AesUtil.AesDecrypt(fileInfo.FileLoadPath, _config.key, _config.iv);

            var assetBundle = AssetBundle.LoadFromMemory(decryptedData);
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.Result = assetBundle;
            return decryptResult;
        }

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            byte[] decryptedData = AesUtil.AesDecrypt(fileInfo.FileLoadPath, _config.key, _config.iv);

            DecryptResult decryptResult = new DecryptResult();
            decryptResult.CreateRequest = AssetBundle.LoadFromMemoryAsync(decryptedData);
            return decryptResult;
        }

        /// <summary>
        /// 后备方式获取解密的资源包
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleFallback(DecryptFileInfo fileInfo)
        {
            byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);

            // Decrypt using AES-256 CBC with PKCS7 padding
            byte[] decryptedData = AesUtil.AesDecrypt(fileData, _config.key, _config.iv);

            var assetBundle = AssetBundle.LoadFromMemory(decryptedData);
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.Result = assetBundle;
            return decryptResult;
        }

        /// <summary>
        /// 获取解密的字节数据
        /// </summary>
        byte[] IDecryptionServices.ReadFileData(DecryptFileInfo fileInfo)
        {
            byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
            return AesUtil.AesDecrypt(fileData, _config.key, _config.iv);
        }

        /// <summary>
        /// 获取解密的文本数据
        /// </summary>
        string IDecryptionServices.ReadFileText(DecryptFileInfo fileInfo)
        {
            byte[] decryptedData = ((IDecryptionServices)this).ReadFileData(fileInfo);
            return System.Text.Encoding.UTF8.GetString(decryptedData);
        }
    }
}