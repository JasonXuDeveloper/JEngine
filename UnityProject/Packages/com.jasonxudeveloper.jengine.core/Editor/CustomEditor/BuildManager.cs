// BuildManager.cs
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
using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using JEngine.Core.Encrypt;
using JEngine.Core.Update;
using Nino.Core;
using Obfuz.Settings;
using Obfuz4HybridCLR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using YooAsset;
using YooAsset.Editor;

namespace JEngine.Core.Editor.CustomEditor
{
    /// <summary>
    /// Manages the build process for JEngine hot update resources.
    /// Handles code compilation, obfuscation, and asset bundle building.
    /// </summary>
    public class BuildManager
    {
        private readonly Settings _settings;
        private readonly Action<string, bool> _logCallback;

        // Build state
        private enum BuildStep
        {
            None,
            GenerateEncryptionVM,
            GenerateSecretKey,
            GenerateAll,
            GeneratePolymorphicCodes,
            CopyAssemblies,
            BuildAssets,
            Complete
        }

        private BuildStep _currentStep = BuildStep.None;
        private bool _buildAll;
        private int _packageVersion;
        private int _progressId = -1;
        private BuildTarget _buildTarget;
        private int _errorCountAtStart;
        private Action _onComplete;
        private Action<Exception> _onError;

        /// <summary>
        /// Gets a value indicating whether a build operation is currently in progress.
        /// </summary>
        public bool IsBuilding => _currentStep != BuildStep.None;

        public BuildManager(Settings settings, Action<string, bool> logCallback)
        {
            _settings = settings;
            _logCallback = logCallback;
        }

        /// <summary>
        /// Starts a full build (code + assets).
        /// </summary>
        public void StartBuildAll(Action onComplete, Action<Exception> onError)
        {
            if (IsBuilding) return;

            _buildAll = true;
            _packageVersion = GetNextPackageVersion();
            _buildTarget = EditorUserBuildSettings.activeBuildTarget;
            _errorCountAtStart = GetUnityErrorCount();
            _onComplete = onComplete;
            _onError = onError;

            Log("Starting full build process...");

            _progressId = Progress.Start("Building Hot Update Resources",
                "Building code and assets",
                Progress.Options.Managed);

            _currentStep = BuildStep.GenerateEncryptionVM;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        /// <summary>
        /// Starts a code-only build.
        /// </summary>
        public void StartBuildCodeOnly(Action onComplete, Action<Exception> onError)
        {
            if (IsBuilding) return;

            _buildAll = false;
            _buildTarget = EditorUserBuildSettings.activeBuildTarget;
            _errorCountAtStart = GetUnityErrorCount();
            _onComplete = onComplete;
            _onError = onError;

            Log("Starting code build...");

            _progressId = Progress.Start("Building Hot Update Code",
                "Compiling and obfuscating code",
                Progress.Options.Managed);

            _currentStep = BuildStep.GenerateEncryptionVM;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        /// <summary>
        /// Starts an assets-only build.
        /// </summary>
        public void StartBuildAssetsOnly(Action onComplete, Action<Exception> onError)
        {
            if (IsBuilding) return;

            _packageVersion = GetNextPackageVersion();
            _onComplete = onComplete;
            _onError = onError;

            Log("Starting assets build...");

            _progressId = Progress.Start("Building Hot Update Assets",
                "Building asset bundles",
                Progress.Options.Managed);

            try
            {
                UpdateProgress(0.5f, "Building Assets");
                string outputDirectory = BuildAssets(_packageVersion);
                UpdateProgress(1.0f, "Build Complete");

                Log($"Assets build completed successfully! Version: {_packageVersion} Output: {outputDirectory}");
                Progress.Finish(_progressId, Progress.Status.Succeeded);
                _onComplete?.Invoke();
            }
            catch (Exception e)
            {
                Log($"Assets build failed: {e.Message}", true);
                Progress.Finish(_progressId, Progress.Status.Failed);
                _onError?.Invoke(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// State machine update called every editor frame.
        /// </summary>
        private void Update()
        {
            if (_currentStep == BuildStep.None)
            {
                Cleanup();
                return;
            }

            try
            {
                ExecuteCurrentStep();
            }
            catch (Exception e)
            {
                Log($"Build failed at step {_currentStep}: {e.Message}", true);

                EditorUtility.ClearProgressBar();
                Progress.Finish(_progressId, Progress.Status.Failed);

                _currentStep = BuildStep.None;
                Cleanup();
                _onError?.Invoke(e);
            }
        }

        /// <summary>
        /// Executes the current build step and advances to the next.
        /// </summary>
        private void ExecuteCurrentStep()
        {
            switch (_currentStep)
            {
                case BuildStep.GenerateEncryptionVM:
                    Log("=== Phase 1: Building Code ===");
                    Log("Step 1/3: Generating Encryption VM");
                    UpdateProgress(0.1f, "Generating Encryption VM");

                    ExecuteMenuItem("Obfuz/GenerateEncryptionVM", "Step 1/3");
                    CheckForErrors("Step 1/3 failed");

                    _currentStep = BuildStep.GenerateSecretKey;
                    break;

                case BuildStep.GenerateSecretKey:
                    Log("Step 2/3: Generating Secret Key File");
                    UpdateProgress(0.2f, "Generating Secret Key");

                    ExecuteMenuItem("Obfuz/GenerateSecretKeyFile", "Step 2/3");
                    CheckForErrors("Step 2/3 failed");

                    _currentStep = BuildStep.GenerateAll;
                    break;

                case BuildStep.GenerateAll:
                    Log("Step 3/4: Running HybridCLR/ObfuzExtension/GenerateAll");
                    Log("This step compiles DLLs and obfuscates assemblies");
                    UpdateProgress(0.3f, "Compiling and Obfuscating");

                    ExecuteMenuItem("HybridCLR/ObfuzExtension/GenerateAll", "Step 3/4");
                    CheckForErrorsExcluding("Step 3/4 failed - HybridCLR generation failed",
                        "Create package main catalog file failed");

                    _currentStep = BuildStep.GeneratePolymorphicCodes;
                    break;

                case BuildStep.GeneratePolymorphicCodes:
                    Log("Step 4/4: Generating Polymorphic Codes");
                    UpdateProgress(0.4f, "Generating Polymorphic Codes");

                    ExecuteMenuItem("HybridCLR/ObfuzExtension/GeneratePolymorphicCodes", "Step 4/4");
                    CheckForErrors("Step 4/4 failed - Polymorphic code generation failed");

                    _currentStep = BuildStep.CopyAssemblies;
                    break;

                case BuildStep.CopyAssemblies:
                    Log("Step 4/4: Copying Assemblies");
                    UpdateProgress(0.5f, "Copying Assemblies");

                    CopyAssemblies(_buildTarget);
                    AssetDatabase.Refresh();

                    Log("Code build completed successfully!");

                    if (_buildAll)
                    {
                        _currentStep = BuildStep.BuildAssets;
                    }
                    else
                    {
                        _currentStep = BuildStep.Complete;
                    }
                    break;

                case BuildStep.BuildAssets:
                    Log("=== Phase 2: Building Assets ===");
                    UpdateProgress(0.6f, "Building Assets");

                    string outputDirectory = BuildAssets(_packageVersion);

                    UpdateProgress(1.0f, "Build Complete");

                    Log($"Build completed successfully! Version: {_packageVersion} Output: {outputDirectory}");

                    _currentStep = BuildStep.Complete;
                    break;

                case BuildStep.Complete:
                    EditorUtility.ClearProgressBar();
                    Progress.Finish(_progressId);

                    _currentStep = BuildStep.None;
                    Cleanup();
                    _onComplete?.Invoke();
                    break;
            }
        }

        private void CopyAssemblies(BuildTarget target)
        {
            const string codeDir = "Assets/HotUpdate/Compiled";
            if (!AssetDatabase.IsValidFolder("Assets/HotUpdate"))
            {
                AssetDatabase.CreateFolder("Assets", "HotUpdate");
            }

            if (!AssetDatabase.IsValidFolder(codeDir))
            {
                AssetDatabase.CreateFolder("Assets/HotUpdate", "Compiled");
            }

            string hotUpdateDllPath = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            string obfuscatedPath = PrebuildCommandExt.GetObfuscatedHotUpdateAssemblyOutputPath(target);
            var obfuscationNames = ObfuzSettings.Instance.assemblySettings.GetObfuscationRelativeAssemblyNames();

            // Copy hot update assemblies
            foreach (string assName in SettingsUtil.HotUpdateAssemblyNamesIncludePreserved)
            {
                string srcDir = obfuscationNames.Contains(assName) ? obfuscatedPath : hotUpdateDllPath;
                string srcFile = $"{srcDir}/{assName}.dll";
                string dstFile = $"{codeDir}/{assName}.dll.bytes";

                if (File.Exists(srcFile))
                {
                    File.Copy(srcFile, dstFile, true);
                    AssetDatabase.ImportAsset(dstFile);
                    Log($"Copied {srcFile} to {dstFile}");
                }
            }

            // Copy AOT assemblies
            CopyAOTAssemblies(target, codeDir);
        }

        private void CopyAOTAssemblies(BuildTarget target, string codeDir)
        {
            var aotAssemblies = new List<string>();
            string aotSrcDir = Path.Join(HybridCLRSettings.Instance.strippedAOTDllOutputRootDir, target.ToString());

            if (!Directory.Exists(aotSrcDir)) return;

            string aotDstDir = $"{codeDir}/AOT";
            if (!AssetDatabase.IsValidFolder(aotDstDir))
            {
                AssetDatabase.CreateFolder(codeDir, "AOT");
            }

            foreach (var dllFile in Directory.GetFiles(aotSrcDir, "*.dll"))
            {
                var dllBytesPath = $"{aotDstDir}/{Path.GetFileName(dllFile)}.bytes";
                aotAssemblies.Add(dllBytesPath);
                File.Copy(dllFile, dllBytesPath, true);
                AssetDatabase.ImportAsset(dllBytesPath);
                Log($"Copy AOT dll {dllFile} to {dllBytesPath}");
            }

            // Serialize AOT assemblies list
            var aotAssembliesBytes = NinoSerializer.Serialize(aotAssemblies);
            string ninoFilePath = "Assets/HotUpdate/Compiled/AOT.bytes";
            File.WriteAllBytes(ninoFilePath, aotAssembliesBytes);
            AssetDatabase.ImportAsset(ninoFilePath);
        }

        private string BuildAssets(int packageVersion)
        {
            var buildParameters = CreateBuildParameters(packageVersion);
            var pipeline = new ScriptableBuildPipeline();
            var result = pipeline.Run(buildParameters, true);

            if (!result.Success)
            {
                throw new Exception($"Asset build failed: {result.ErrorInfo}");
            }

            Log($"Assets built successfully: {result.OutputPackageDirectory}");
            return result.OutputPackageDirectory;
        }

        private ScriptableBuildParameters CreateBuildParameters(int packageVersion)
        {
            var buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            var streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            var bundleEncryption = EncryptionMapping.GetBundleConfig(_settings.encryptionOption);

            EBuildinFileCopyOption copyOption = EBuildinFileCopyOption.ClearAndCopyAll;
            if (!string.IsNullOrEmpty(_settings.startUpScenePath) && File.Exists(_settings.startUpScenePath))
            {
                var currentScene = SceneManager.GetActiveScene();
                if (currentScene.isDirty)
                {
                    EditorSceneManager.SaveScene(currentScene);
                }

                if (currentScene.path != _settings.startUpScenePath)
                {
                    EditorSceneManager.OpenScene(_settings.startUpScenePath);
                }

                var bootstrap = UnityEngine.Object.FindObjectOfType<Bootstrap>();
                copyOption = _settings.packageName == bootstrap.packageName ||
                             bootstrap.targetPlatform == TargetPlatform.Standalone
                    ? EBuildinFileCopyOption.ClearAndCopyAll
                    : EBuildinFileCopyOption.None;
            }

            return new ScriptableBuildParameters
            {
                BuildOutputRoot = buildOutputRoot,
                BuildinFileRoot = streamingAssetsRoot,
                BuildPipeline = nameof(EBuildPipeline.ScriptableBuildPipeline),
                BuildBundleType = (int)EBuildBundleType.AssetBundle,
                BuildTarget = _settings.buildTarget,
                PackageName = _settings.packageName,
                PackageVersion = packageVersion.ToString(),
                VerifyBuildingResult = true,
                FileNameStyle = EFileNameStyle.HashName,
                BuildinFileCopyOption = copyOption,
                BuildinFileCopyParams = string.Empty,
                CompressOption = ECompressOption.LZ4,
                ClearBuildCacheFiles = _settings.clearBuildCache,
                UseAssetDependencyDB = _settings.useAssetDependDB,
                ManifestProcessServices = bundleEncryption.ManifestEncryptionConfig.Encryption,
                ManifestRestoreServices = bundleEncryption.ManifestEncryptionConfig.Decryption,
                EncryptionServices = bundleEncryption.Encryption,
                BuiltinShadersBundleName = GetBuiltinShaderBundleName(),
                TrackSpriteAtlasDependencies = true,
                StripUnityVersion = true,
            };
        }

        private string GetBuiltinShaderBundleName()
        {
            var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            return packRuleResult.GetBundleName(_settings.packageName, uniqueBundleName);
        }

        private int GetNextPackageVersion()
        {
            var now = DateTime.UtcNow;
            var year = now.Year - 2000;
            var month = now.Month;
            var day = now.Day;
            var hour = now.Hour;
            var minute = now.Minute;
            var second = now.Second;

            return (year * 10000000) + (month * 100000) + (day * 1000) + (hour * 100) + (minute * 10) + (second / 6);
        }

        private void ExecuteMenuItem(string menuPath, string stepName)
        {
            Log($"{stepName}: Executing {menuPath}");
            if (!EditorApplication.ExecuteMenuItem(menuPath))
            {
                throw new Exception($"Failed to execute {menuPath} - menu item might not exist or execution failed");
            }
        }

        private void CheckForErrors(string context)
        {
            int currentErrorCount = GetUnityErrorCount();
            if (currentErrorCount > _errorCountAtStart)
            {
                throw new Exception($"{context}: {currentErrorCount - _errorCountAtStart} error(s) occurred. Check Unity Console for details.");
            }
        }

        private void CheckForErrorsExcluding(string context, string excludeMessageContaining)
        {
            int currentErrorCount = GetUnityErrorCount();
            if (currentErrorCount > _errorCountAtStart)
            {
                // Check if the new errors are only the excluded error
                int newErrorCount = currentErrorCount - _errorCountAtStart;
                int nonExcludedErrors = CountNonExcludedErrors(excludeMessageContaining, newErrorCount);

                if (nonExcludedErrors > 0)
                {
                    throw new Exception($"{context}: {nonExcludedErrors} error(s) occurred. Check Unity Console for details.");
                }

                // If we have errors but they're all excluded, log a warning but continue
                if (newErrorCount > 0)
                {
                    Log($"Note: {newErrorCount} known error(s) ignored (YooAsset catalog bug)");
                }
            }
        }

        private int CountNonExcludedErrors(string excludeMessageContaining, int recentErrorCount)
        {
            try
            {
                var logEntriesType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.LogEntries");
                if (logEntriesType == null) return recentErrorCount;

                var getCountMethod = logEntriesType.GetMethod("GetCount");
                var startGettingEntriesMethod = logEntriesType.GetMethod("StartGettingEntries");
                var getEntryInternalMethod = logEntriesType.GetMethod("GetEntryInternal");
                var endGettingEntriesMethod = logEntriesType.GetMethod("EndGettingEntries");
                var logEntryType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.LogEntry");

                if (getCountMethod == null || startGettingEntriesMethod == null ||
                    getEntryInternalMethod == null || endGettingEntriesMethod == null ||
                    logEntryType == null)
                {
                    return recentErrorCount;
                }

                int totalCount = (int)getCountMethod.Invoke(null, null);
                startGettingEntriesMethod.Invoke(null, null);

                int nonExcludedCount = 0;
                int checkedErrors = 0;

                // Check recent errors from the end of the log
                for (int i = totalCount - 1; i >= 0 && checkedErrors < recentErrorCount; i--)
                {
                    var entry = Activator.CreateInstance(logEntryType);
                    object[] args = { i, entry };
                    getEntryInternalMethod.Invoke(null, args);
                    entry = args[1];

                    var modeField = logEntryType.GetField("mode");
                    if (modeField != null)
                    {
                        int mode = (int)modeField.GetValue(entry);
                        // Mode 2 = Error
                        if (mode == 2)
                        {
                            checkedErrors++;
                            var messageField = logEntryType.GetField("message");
                            if (messageField != null)
                            {
                                string message = (string)messageField.GetValue(entry);
                                if (!message.Contains(excludeMessageContaining))
                                {
                                    nonExcludedCount++;
                                }
                            }
                            else
                            {
                                nonExcludedCount++;
                            }
                        }
                    }
                }

                endGettingEntriesMethod.Invoke(null, null);
                return nonExcludedCount;
            }
            catch
            {
                // If reflection fails, assume all errors are real
                return recentErrorCount;
            }
        }

        private int GetUnityErrorCount()
        {
            var logEntriesType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.LogEntries");
            if (logEntriesType != null)
            {
                var getCountsByTypeMethod = logEntriesType.GetMethod("GetCountsByType");
                if (getCountsByTypeMethod != null)
                {
                    int errorCount = 0, warningCount = 0, logCount = 0;
                    object[] args = { errorCount, warningCount, logCount };
                    getCountsByTypeMethod.Invoke(null, args);
                    return (int)args[0];
                }
            }
            return 0;
        }

        private void UpdateProgress(float progress, string description)
        {
            EditorUtility.DisplayProgressBar("Building", description, progress);
            Progress.Report(_progressId, progress, description);
        }

        private void Log(string message, bool isError = false)
        {
            _logCallback?.Invoke(message, isError);
        }

        private void Cleanup()
        {
            EditorApplication.update -= Update;
            EditorUtility.ClearProgressBar();
            _currentStep = BuildStep.None;
        }
    }
}
