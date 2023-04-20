//
// ConstMgr.cs
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
using UnityEngine;

namespace JEngine.Core
{
    public static partial class ConstMgr
    {
        public const string DLLSourceFolder = "Assets/HotUpdateResources/Main/Dll/Hidden~/";
        public const string PdbSourceFolder = "Assets/HotUpdateResources/Main/Dll/Hidden~/";
        public const string PdbBytesFolder = "Assets/HotUpdateResources/Main/Dll/";
        public const string DLLBytesFolder = "Assets/HotUpdateResources/Main/Dll/";
        public const string MainHotDLLName = "HotUpdateScripts";
        public const string ExprFlag = "_expr";
        public const string DLLExtension = ".dll";
        public const string PdbExtension = ".pdb";
        public const string BytesExtension = ".bytes";
        
        public const float Bytes2Mb = 1f / (1024 * 1024);
        
        public static byte[] NullBytes => Array.Empty<byte>();
        public static object[] NullObjects => Array.Empty<object>();
        
        public static WaitForSecondsRealtime WaitFor1Sec = new WaitForSecondsRealtime(0.001f);
    }
}