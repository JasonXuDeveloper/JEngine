// PackageInitializationStatus.cs
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

namespace JEngine.Core.Update
{
    /// <summary>
    /// 资源包初始化状态枚举
    /// </summary>
    public enum PackageInitializationStatus
    {
        /// <summary>
        /// 正在初始化资源包
        /// </summary>
        InitializingPackage,

        /// <summary>
        /// 正在获取版本信息
        /// </summary>
        GettingVersion,

        /// <summary>
        /// 正在更新清单
        /// </summary>
        UpdatingManifest,

        /// <summary>
        /// 正在检查更新
        /// </summary>
        CheckingUpdate,

        /// <summary>
        /// 正在下载资源
        /// </summary>
        DownloadingResources,

        /// <summary>
        /// 初始化完成
        /// </summary>
        Completed,

        /// <summary>
        /// 初始化失败
        /// </summary>
        Failed
    }
}