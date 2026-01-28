// BuildHelper.cs
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
using JEngine.Core.Editor.CustomEditor;
using UnityEditor;

namespace JEngine.Core.Editor
{
    /// <summary>
    /// Helper class for build operations with UI update callbacks.
    /// </summary>
    public static class BuildHelper
    {
        /// <summary>
        /// Callbacks for build UI updates.
        /// </summary>
        public class BuildCallbacks
        {
            /// <summary>
            /// Called to enable/disable build buttons.
            /// </summary>
            public Action<bool> SetButtonsEnabled { get; set; }

            /// <summary>
            /// Called to clear the log view.
            /// </summary>
            public Action ClearLog { get; set; }

            /// <summary>
            /// Called to update status text.
            /// </summary>
            public Action<string> UpdateStatus { get; set; }

            /// <summary>
            /// Called when build completes successfully.
            /// </summary>
            public Action OnSuccess { get; set; }

            /// <summary>
            /// Called when build fails.
            /// </summary>
            public Action OnError { get; set; }
        }

        /// <summary>
        /// Executes BuildAll with standard UI callbacks.
        /// </summary>
        public static void ExecuteBuildAll(BuildManager buildManager, BuildCallbacks callbacks)
        {
            if (buildManager.IsBuilding) return;

            callbacks.SetButtonsEnabled?.Invoke(false);
            callbacks.ClearLog?.Invoke();

            buildManager.StartBuildAll(
                onComplete: () =>
                {
                    callbacks.SetButtonsEnabled?.Invoke(true);
                    callbacks.UpdateStatus?.Invoke("Build completed");
                    callbacks.OnSuccess?.Invoke();
                    EditorUtility.DisplayDialog("Build Successful", "Build completed successfully!", "OK");
                },
                onError: e =>
                {
                    callbacks.SetButtonsEnabled?.Invoke(true);
                    callbacks.UpdateStatus?.Invoke("Build failed");
                    callbacks.OnError?.Invoke();
                    EditorUtility.DisplayDialog("Build Failed", $"Build failed with error:\n{e.Message}", "OK");
                }
            );
        }

        /// <summary>
        /// Executes BuildCodeOnly with standard UI callbacks.
        /// </summary>
        public static void ExecuteBuildCodeOnly(BuildManager buildManager, BuildCallbacks callbacks)
        {
            if (buildManager.IsBuilding) return;

            callbacks.SetButtonsEnabled?.Invoke(false);
            callbacks.ClearLog?.Invoke();

            buildManager.StartBuildCodeOnly(
                onComplete: () =>
                {
                    callbacks.SetButtonsEnabled?.Invoke(true);
                    callbacks.UpdateStatus?.Invoke("Code build completed");
                    callbacks.OnSuccess?.Invoke();
                    EditorUtility.DisplayDialog("Code Build Successful", "Code build completed successfully!", "OK");
                },
                onError: e =>
                {
                    callbacks.SetButtonsEnabled?.Invoke(true);
                    callbacks.UpdateStatus?.Invoke("Code build failed");
                    callbacks.OnError?.Invoke();
                    EditorUtility.DisplayDialog("Code Build Failed", $"Code build failed with error:\n{e.Message}", "OK");
                }
            );
        }

        /// <summary>
        /// Executes BuildAssetsOnly with standard UI callbacks.
        /// </summary>
        public static void ExecuteBuildAssetsOnly(BuildManager buildManager, BuildCallbacks callbacks)
        {
            if (buildManager.IsBuilding) return;

            callbacks.SetButtonsEnabled?.Invoke(false);
            callbacks.ClearLog?.Invoke();

            buildManager.StartBuildAssetsOnly(
                onComplete: () =>
                {
                    callbacks.SetButtonsEnabled?.Invoke(true);
                    callbacks.UpdateStatus?.Invoke("Assets build completed");
                    callbacks.OnSuccess?.Invoke();
                    EditorUtility.DisplayDialog("Assets Build Successful", "Assets build completed successfully!", "OK");
                },
                onError: e =>
                {
                    callbacks.SetButtonsEnabled?.Invoke(true);
                    callbacks.UpdateStatus?.Invoke("Assets build failed");
                    callbacks.OnError?.Invoke();
                    EditorUtility.DisplayDialog("Assets Build Failed", $"Assets build failed with error:\n{e.Message}", "OK");
                }
            );
        }
    }
}
