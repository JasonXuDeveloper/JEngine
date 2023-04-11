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
using System;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;

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
        /// Get DLL path in the runtime (located in HotUpdateResources/Main/DLL)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetDllInRuntimePath(string name)
        {
            Tools.EnsureEndWith(ref name, ConstMgr.BytesExtension);
            return Path.Combine(ConstMgr.DLLBytesFolder, name);
        }

        /// <summary>
        /// Get PDB path in the editor (located at hidden file with .pdb extension)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetPdbInEditorPath(string name)
        {
            Tools.EnsureEndWith(ref name, ConstMgr.PdbExtension);
            return Path.Combine(ConstMgr.PdbSourceFolder, name);
        }

        /// <summary>
        /// Get PDB path in the runtime (located in HotUpdateResources/Main/DLL)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetPdbInRuntimePath(string name)
        {
            Tools.EnsureEndWith(ref name, ConstMgr.PdbExtension);
            Tools.EnsureEndWith(ref name, ConstMgr.BytesExtension);
            return Path.Combine(ConstMgr.PdbBytesFolder, name);
        }

        /// <summary>
        /// Get DLL binary data
        /// </summary>
        /// <param name="name">dll file name with extension</param>
        /// <param name="editor">is editor mode</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static async Task<byte[]> GetDllBytes(string name, bool editor = false)
        {
            string path;
            if (editor)
            {
                path = GetDllInEditorPath(name);
                //判断有没有dll
                if (File.Exists(path))
                {
                    return FileMgr.FileToBytes(path);
                }

                throw new FileNotFoundException($"DLL not found in: {path}");
            }

            path = GetDllInRuntimePath(name);
            var dllFile = await AssetMgr.LoadAsync<TextAsset>(path);
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
        /// <param name="editor"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<byte[]> GetPdbBytes(string name, bool editor = true)
        {
            if (editor)
            {
                //DLL文件
                var dllPath = GetDllInEditorPath(name);

                //PDB文件
                string path = GetPdbInEditorPath(name);
                //查看是否有PDB文件并且是最新的
                if (File.Exists(path) &&
                    (File.GetLastWriteTime(dllPath) - File.GetLastWriteTime(path)).Seconds < 30)
                {
                    return FileMgr.FileToBytes(path);
                }

                throw new InvalidOperationException("Pdb is invalid");
            }

            var pdbPath = GetPdbInRuntimePath(name);
            var pdbFile = await AssetMgr.LoadAsync<TextAsset>(pdbPath);
            if (pdbFile == null)
            {
                throw new FileNotFoundException($"Pdb not found in: {pdbPath}");
            }

            return pdbFile.bytes;
        }

        /// <summary>
        /// Simulate encryption on plaintext
        /// </summary>
        /// <param name="source">plaintext data</param>
        /// <param name="key">key</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SimulateEncryption(ref byte[] source, string key)
        {
            if (key.Length != 16)
            {
                throw new InvalidOperationException("key to encrypt has to be length of 16");
            }

            source = CryptoMgr.AesEncrypt(source, key);
        }
    }
}