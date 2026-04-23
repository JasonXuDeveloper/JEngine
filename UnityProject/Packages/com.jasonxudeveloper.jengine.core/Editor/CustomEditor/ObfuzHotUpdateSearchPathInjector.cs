// ObfuzHotUpdateSearchPathInjector.cs
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
using System.Linq;
using HybridCLR.Editor;
using Obfuz.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace JEngine.Core.Editor.CustomEditor
{
    /// <summary>
    /// Makes the HybridCLR hot-update DLL output directory visible to Obfuz's
    /// player-build obfuscation pass so <c>Assembly-CSharp</c>'s reference to
    /// <c>HotUpdate.Code</c> (when present) can be resolved.
    /// </summary>
    /// <remarks>
    /// <para>
    /// During a Unity player build, HybridCLR's <c>FilterHotFixAssemblies</c>
    /// strips hot-update assemblies from the player staging area (correct — they
    /// must not ship as baked code). Obfuz's
    /// <c>ObfuscationProcess.OnPostBuildPlayerScriptDLLs</c> then obfuscates the
    /// remaining player DLLs and, via <c>AssemblyCache.LoadModule</c>, recursively
    /// resolves every <c>GetAssemblyRefs()</c> entry through a flat
    /// <c>PathAssemblyResolver</c>. If any obfuscated player assembly references
    /// <c>HotUpdate.Code</c>, the resolver can't find it and Obfuz throws
    /// <c>FileNotFoundException: Assembly HotUpdate.Code not found</c>.
    /// </para>
    /// <para>
    /// This processor adds <c>HybridCLRData/HotUpdateDlls/&lt;target&gt;/</c> to
    /// <c>ObfuzSettings.additionalAssemblySearchPaths</c> at the start of the
    /// build (in-memory only — <c>Obfuz.asset</c> on disk is never modified) and
    /// restores the original list afterwards.
    /// </para>
    /// </remarks>
    internal sealed class ObfuzHotUpdateSearchPathInjector
        : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        // Run before Obfuz's IPostBuildPlayerScriptDLLs (callbackOrder = 10000).
        public int callbackOrder => 0;

        private string[] _originalSearchPaths;
        private bool _injected;

        public void OnPreprocessBuild(BuildReport report)
        {
            AssemblySettings assemblySettings = ObfuzSettings.Instance.assemblySettings;
            _originalSearchPaths = assemblySettings.additionalAssemblySearchPaths
                                   ?? System.Array.Empty<string>();
            _injected = true;

            string hotUpdateDir =
                SettingsUtil.GetHotUpdateDllsOutputDirByTarget(report.summary.platform);

            if (string.IsNullOrEmpty(hotUpdateDir) || !Directory.Exists(hotUpdateDir))
            {
                Debug.LogWarning(
                    $"[JEngine] HybridCLR hot-update directory not found: " +
                    $"'{hotUpdateDir}'. If the player build fails with " +
                    $"'Assembly HotUpdate.Code not found' during Obfuz obfuscation, " +
                    $"run 'Build Main Package (code)' from the JEngine Panel first " +
                    $"so HybridCLR compiles the hot-update assemblies for this target.");
                return;
            }

            if (_originalSearchPaths.Any(p => AreSamePath(p, hotUpdateDir)))
            {
                return;
            }

            assemblySettings.additionalAssemblySearchPaths =
                _originalSearchPaths.Concat(new[] { hotUpdateDir }).ToArray();

            Debug.Log(
                $"[JEngine] Injected HybridCLR hot-update dir into Obfuz search " +
                $"paths for this build: {hotUpdateDir}");
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (!_injected) return;

            ObfuzSettings.Instance.assemblySettings.additionalAssemblySearchPaths =
                _originalSearchPaths;
            _originalSearchPaths = null;
            _injected = false;
        }

        private static bool AreSamePath(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
            try
            {
                return Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar)
                    == Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar);
            }
            catch
            {
                return false;
            }
        }
    }
}
