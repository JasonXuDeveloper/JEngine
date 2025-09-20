// PackageInitializationCallbacks.cs
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

using System;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace JEngine.Core.Update
{
    /// <summary>
    /// 资源包初始化回调接口
    /// </summary>
    public struct PackageInitializationCallbacks
    {
        public Action<PackageInitializationStatus> OnStatusUpdate { get; set; }
        public Action<string> OnVersionUpdate { get; set; }
        public Func<int, long, UniTask<bool>> OnDownloadPrompt { get; set; }
        public DownloaderOperation.DownloadUpdate OnDownloadProgress { get; set; }
        public Action OnDownloadStart { get; set; }
        public Action OnDownloadComplete { get; set; }
        public Func<Exception, UniTask> OnError { get; set; }
    }
}