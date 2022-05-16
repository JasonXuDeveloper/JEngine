//
// DllMgr.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 JEngine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using BM;
using System;
using System.IO;
using UnityEngine;

namespace JEngine.Core
{
    public static partial class DllMgr
    {
        /// <summary>
        /// Get DLL path in the editor (located at hidden file with .dll extension)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetDllInEditorPath(string name)
        {
            Tools.EnsureEndWith(ref name, ConstMgr.DLLExtension);
            return Path.Combine(ConstMgr.DLLSourceFolder, name);
        }
        
        /// <summary>
        /// Get DLL path in the runtime (located in HotUpdateResources/DLL)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetDllInRuntimePath(string name)
        {
            Tools.EnsureEndWith(ref name, ConstMgr.BytesExtension);
            return Path.Combine(ConstMgr.DLLBytesFolder, name);
        }
        
        /// <summary>
        /// Get DLL binary data
        /// </summary>
        /// <param name="name">dll file name with extension</param>
        /// <param name="editor">is editor mode</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static byte[] GetDllBytes(string name, bool editor = false)
        {
            string path;
            if (editor)
            {
                path = GetDllInEditorPath(name);
                //判断有没有dll
                if (File.Exists(path))
                {
                    return FileMgr.FileToByte(path);
                }
                throw new FileNotFoundException($"DLL not found in: {path}");
            }

            path = GetDllInRuntimePath(name);
            var dllFile = (TextAsset)AssetMgr.Load(path,
                AssetComponentConfig.DefaultBundlePackageName);
            if (dllFile == null)
            {
                throw new FileNotFoundException($"DLL not found in: {path}");
            }

            return dllFile.bytes;
        }

        /// <summary>
        /// Get PDB binary data
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static byte[] GetPdbBytes(string name)
        {
            //DLL文件
            string dllName = name;
            Tools.EnsureEndWith(ref dllName, ConstMgr.DLLExtension);
            var dllPath = Path.Combine(ConstMgr.DLLSourceFolder, name);
            
            //PDB文件
            Tools.EnsureEndWith(ref name, ConstMgr.PdbExtension);
            string path = Path.Combine(ConstMgr.PdbSourceFolder, name);
            //查看是否有PDB文件并且是最新的
            if (File.Exists(path) && 
                (File.GetLastWriteTime(dllPath) - File.GetLastWriteTime(path)).Seconds < 30)
            {
                return FileMgr.FileToByte(path);
            }

            throw new InvalidOperationException("Pdb is invalid");
        }

        /// <summary>
        /// Simulate encryption on plaintext
        /// </summary>
        /// <param name="source">plaintext data</param>
        /// <param name="key">key</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SimulateEncryption(ref byte[] source,string key)
        {
            if (key.Length != 16)
            {
                throw new InvalidOperationException("key to encrypt has to be length of 16");
            }
            source = CryptoMgr.AesEncrypt(source, key);
        }
    }
}
