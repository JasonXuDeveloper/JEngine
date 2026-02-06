// BootstrapText.cs
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
using UnityEngine;

namespace JEngine.Core
{
    /// <summary>
    /// All user-facing strings shown by <see cref="Bootstrap"/> during initialization.
    /// Customize per-project for localization or branding.
    /// </summary>
    [Serializable]
    public struct BootstrapText
    {
        // ── Package Initialization Status ──

        [Header("Package Initialization Status")]
        public string initializingPackage;
        public string gettingVersion;
        public string updatingManifest;
        public string checkingUpdate;
        public string downloadingResources;
        public string packageCompleted;
        public string initializationFailed;
        public string unknownPackageStatus;

        // ── Scene Load Status ──

        [Header("Scene Load Status")]
        public string sceneLoading;
        public string sceneCompleted;
        public string sceneFailed;
        public string unknownSceneStatus;

        // ── Inline Status ──

        [Header("Inline Status")]
        public string initializing;
        public string downloading;
        public string downloadCompletedLoading;
        public string loadingCode;
        public string decryptingResources;
        public string loadingScene;

        // ── Dialog Titles ──

        [Header("Dialog Titles")]
        public string dialogTitleError;
        public string dialogTitleWarning;
        public string dialogTitleNotice;

        // ── Dialog Buttons ──

        [Header("Dialog Buttons")]
        public string buttonOk;
        public string buttonCancel;
        public string buttonDownload;
        public string buttonRetry;
        public string buttonExit;

        // ── Dialog Content (format strings) ──

        [Header("Dialog Content")]
        [Tooltip("{0} = error message")]
        public string dialogInitFailed;

        [Tooltip("{0} = file count, {1} = total size in MB")]
        public string dialogDownloadPrompt;

        [Tooltip("{0} = current count, {1} = total count, {2} = current MB, {3} = total MB")]
        public string dialogDownloadProgress;

        [Tooltip("{0} = error message")]
        public string dialogSceneLoadFailed;

        [Tooltip("{0} = exception message")]
        public string dialogInitException;

        public string dialogCodeException;

        [Tooltip("{0} = error message")]
        public string dialogFunctionCallFailed;

        /// <summary>
        /// Default English text matching the original hardcoded strings.
        /// </summary>
        public static readonly BootstrapText Default = new()
        {
            initializingPackage = "Initializing resource package...",
            gettingVersion = "Getting resource package version...",
            updatingManifest = "Updating resource manifest...",
            checkingUpdate = "Checking resources to download...",
            downloadingResources = "Downloading resources...",
            packageCompleted = "Resource package initialization completed",
            initializationFailed = "Initialization failed",
            unknownPackageStatus = "Unknown status",

            sceneLoading = "Loading scene...",
            sceneCompleted = "Scene loading completed",
            sceneFailed = "Scene loading failed",
            unknownSceneStatus = "Unknown status",

            initializing = "Initializing...",
            downloading = "Downloading...",
            downloadCompletedLoading = "Download completed, loading...",
            loadingCode = "Loading code...",
            decryptingResources = "Decrypting resources...",
            loadingScene = "Loading scene...",

            dialogTitleError = "Error",
            dialogTitleWarning = "Warning",
            dialogTitleNotice = "Notice",

            buttonOk = "OK",
            buttonCancel = "Cancel",
            buttonDownload = "Download",
            buttonRetry = "Retry",
            buttonExit = "Exit",

            dialogInitFailed = "Initialization failed: {0}",
            dialogDownloadPrompt = "Need to download {0} files, total size {1}MB. Start download?",
            dialogDownloadProgress = "Downloading file {0}/{1} ({2}MB/{3}MB)",
            dialogSceneLoadFailed = "Scene loading failed: {0}",
            dialogInitException = "Exception occurred during initialization: {0}",
            dialogCodeException = "Code exception, please contact customer service",
            dialogFunctionCallFailed = "Function call failed: {0}",
        };
    }
}
