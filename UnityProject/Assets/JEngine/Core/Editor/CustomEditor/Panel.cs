// Panel.cs
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
using System.Linq;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using Nino.Core;
using Obfuz.Settings;
using Obfuz4HybridCLR;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YooAsset;
using YooAsset.Editor;

namespace JEngine.Core.Editor.CustomEditor
{
    public class Panel : EditorWindow
    {
        private Settings _settings;
        private VisualElement _root;
        private Button _buildAllButton;
        private Button _buildCodeButton;
        private Button _buildAssetsButton;
        private Label _statusLabel;
        private ProgressBar _progressBar;
        private ScrollView _logScrollView;
        private bool _isBuilding;

        [MenuItem("JEngine/JEngine Panel", priority = 2001)]
        private static void OpenWindow()
        {
            var window = GetWindow<Panel>();
            window.titleContent = new GUIContent("JEngine Panel",
                EditorGUIUtility.IconContent("BuildSettings.Editor.Small").image);
            window.Show();
        }

        private void CreateGUI()
        {
            _settings = Settings.Instance;
            _root = rootVisualElement;

            // Load stylesheets
            var panelStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/JEngine/Core/Editor/CustomEditor/Panel.uss");
            if (panelStyleSheet != null)
                _root.styleSheets.Add(panelStyleSheet);

            // Load shared Bootstrap stylesheet for consistent styling
            var bootstrapStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/JEngine/Core/Editor/CustomEditor/BootstrapEditor.uss");
            if (bootstrapStyleSheet != null)
                _root.styleSheets.Add(bootstrapStyleSheet);

            CreateHeader();
            CreateSettingsSection();
            CreateBuildSection();
            CreateStatusSection();
        }

        private void CreateHeader()
        {
            var header = new VisualElement();
            header.AddToClassList("header");

            var panelTitle = new Label("JEngine Panel");
            panelTitle.AddToClassList("header-title");

            var subtitle = new Label("Configure JEngine settings and build hot update code/assets");
            subtitle.AddToClassList("header-subtitle");

            header.Add(panelTitle);
            header.Add(subtitle);
            _root.Add(header);
        }

        private void CreateSettingsSection()
        {
            var settingsGroup = CreateGroup("Build Settings");

            // Package Settings Group
            CreatePackageSettingsGroup(settingsGroup);

            // Build Options Group
            CreateBuildOptionsGroup(settingsGroup);

            _root.Add(settingsGroup);
        }

        private void CreatePackageSettingsGroup(VisualElement parent)
        {
            var packageGroup = CreateGroup("Package Settings");

            // Package Name Dropdown
            var packageNameContainer = new VisualElement();
            packageNameContainer.AddToClassList("field-container");

            var packageChoices = EditorUtils.GetAvailableYooAssetPackages();
            var packageNameField = new PopupField<string>("Package Name")
            {
                choices = packageChoices.Any() ? packageChoices : new List<string> { _settings.packageName },
                value = _settings.packageName
            };
            packageNameField.style.marginLeft = 0;
            packageNameField.style.marginRight = 0;
            packageNameField.RegisterValueChangedCallback(evt =>
            {
                _settings.packageName = evt.newValue;
                _settings.Save();
            });

            packageNameContainer.Add(packageNameField);
            packageGroup.Add(packageNameContainer);

            // Build Target with Set to Active button
            var buildTargetFieldContainer = new VisualElement();
            buildTargetFieldContainer.AddToClassList("field-container");

            var buildTargetContainer = new VisualElement();
            buildTargetContainer.AddToClassList("horizontal-container");
            buildTargetContainer.style.flexDirection = FlexDirection.Row;
            buildTargetContainer.style.alignItems = Align.Center;

            var buildTargetField = new EnumField("Build Target", _settings.buildTarget);
            buildTargetField.RegisterValueChangedCallback(evt =>
            {
                _settings.buildTarget = (BuildTarget)evt.newValue;
                _settings.Save();
            });
            buildTargetField.style.marginLeft = 0;
            buildTargetField.style.flexGrow = 1;
            buildTargetField.style.flexShrink = 1;
            buildTargetField.style.minWidth = 150; // Prevent it from getting too small
            buildTargetField.style.maxWidth = 250; // Prevent it from taking all space
            buildTargetField.style.marginRight = 5;
            buildTargetContainer.Add(buildTargetField);

            var setActiveButton = new Button(() =>
            {
                _settings.buildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildTargetField.value = _settings.buildTarget;
                _settings.Save();
            })
            {
                text = "Set to Active"
            };
            setActiveButton.AddToClassList("utility-button");
            setActiveButton.style.flexShrink = 0; // Don't shrink the button
            setActiveButton.style.alignSelf = Align.Center; // Center align with enum field
            buildTargetContainer.Add(setActiveButton);

            buildTargetFieldContainer.Add(buildTargetContainer);
            packageGroup.Add(buildTargetFieldContainer);
            parent.Add(packageGroup);
        }

        private void CreateBuildOptionsGroup(VisualElement parent)
        {
            var buildGroup = CreateGroup("Build Options");

            // Clear Build Cache Toggle
            var clearCacheToggle = new Toggle("Clear Build Cache")
            {
                value = _settings.clearBuildCache
            };
            clearCacheToggle.tooltip = "Clear build cache before building. Uncheck to enable incremental builds (faster)";
            clearCacheToggle.RegisterValueChangedCallback(evt =>
            {
                _settings.clearBuildCache = evt.newValue;
                _settings.Save();
            });
            buildGroup.Add(clearCacheToggle);

            // Use Asset Dependency DB Toggle
            var useAssetDBToggle = new Toggle("Use Asset Dependency DB")
            {
                value = _settings.useAssetDependDB
            };
            useAssetDBToggle.tooltip = "Use asset dependency database to improve build speed";
            useAssetDBToggle.RegisterValueChangedCallback(evt =>
            {
                _settings.useAssetDependDB = evt.newValue;
                _settings.Save();
            });
            buildGroup.Add(useAssetDBToggle);

            parent.Add(buildGroup);
        }

        private void CreateBuildSection()
        {
            var buildGroup = new VisualElement();
            buildGroup.AddToClassList("build-group");

            var buildTitle = new Label("Build Actions");
            buildTitle.AddToClassList("section-title");
            buildGroup.Add(buildTitle);

            // Build All Button (main button)
            _buildAllButton = new Button(BuildAll)
            {
                text = "Build All Hot Update Res (Code + Assets)"
            };
            _buildAllButton.AddToClassList("build-button");
            buildGroup.Add(_buildAllButton);

            // Individual build buttons container
            var individualButtonsContainer = new VisualElement();
            individualButtonsContainer.AddToClassList("individual-buttons-container");

            // Build Code Button
            _buildCodeButton = new Button(BuildCodeOnly)
            {
                text = "Build Hot Update Code Only"
            };
            _buildCodeButton.AddToClassList("build-button-small");
            individualButtonsContainer.Add(_buildCodeButton);

            // Build Assets Button
            _buildAssetsButton = new Button(BuildAssetsOnly)
            {
                text = "Build Hot Update Assets Only"
            };
            _buildAssetsButton.AddToClassList("build-button-small");
            individualButtonsContainer.Add(_buildAssetsButton);

            buildGroup.Add(individualButtonsContainer);

            var infoLabel = new Label("Use 'Build All' for complete workflow, or individual buttons for specific tasks.");
            infoLabel.AddToClassList("info-label");
            buildGroup.Add(infoLabel);

            _root.Add(buildGroup);
        }

        private void CreateStatusSection()
        {
            var statusGroup = new VisualElement();
            statusGroup.AddToClassList("status-group");

            var statusTitle = new Label("Build Status");
            statusTitle.AddToClassList("section-title");
            statusGroup.Add(statusTitle);

            _statusLabel = new Label("Ready to build");
            _statusLabel.AddToClassList("status-label");
            statusGroup.Add(_statusLabel);

            _progressBar = new ProgressBar();
            _progressBar.AddToClassList("progress-bar");
            _progressBar.style.display = DisplayStyle.None;
            statusGroup.Add(_progressBar);

            // Log area
            _logScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            _logScrollView.AddToClassList("log-scroll");
            _logScrollView.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            _logScrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            statusGroup.Add(_logScrollView);

            _root.Add(statusGroup);
        }

        private void BuildAll()
        {
            if (_isBuilding) return;

            try
            {
                _isBuilding = true;
                SetBuildButtonsEnabled(false);
                _progressBar.style.display = DisplayStyle.Flex;
                _progressBar.value = 0;

                ClearLog();
                LogMessage("Starting full build process...");

                BuildCode();
                int packageVersion = GetNextPackageVersion();
                string outputDirectory = BuildAssets(packageVersion);

                LogMessage($"Build completed successfully! Version: {packageVersion} Output: {outputDirectory}");
                EditorUtility.DisplayDialog("Build Successful",
                    $"Build completed successfully!\nVersion: {packageVersion}\nOutput: {outputDirectory}", "OK");
            }
            catch (Exception e)
            {
                LogMessage($"Build failed: {e.Message}", true);
                EditorUtility.DisplayDialog("Build Failed", $"Build failed with error: {e.Message}", "OK");
            }
            finally
            {
                _isBuilding = false;
                SetBuildButtonsEnabled(true);
                _progressBar.style.display = DisplayStyle.None;
                _statusLabel.text = "Build completed";
            }
        }

        private void BuildCodeOnly()
        {
            if (_isBuilding) return;

            try
            {
                _isBuilding = true;
                SetBuildButtonsEnabled(false);
                _progressBar.style.display = DisplayStyle.Flex;
                _progressBar.value = 0;

                ClearLog();
                LogMessage("Starting code build...");
                BuildCode();
                _progressBar.value = 1.0f;

                LogMessage("Code build completed successfully!");
                EditorUtility.DisplayDialog("Code Build Successful", "Code build completed successfully!", "OK");
            }
            catch (Exception e)
            {
                LogMessage($"Code build failed: {e.Message}", true);
                EditorUtility.DisplayDialog("Code Build Failed", $"Code build failed with error: {e.Message}", "OK");
            }
            finally
            {
                _isBuilding = false;
                SetBuildButtonsEnabled(true);
                _progressBar.style.display = DisplayStyle.None;
                _statusLabel.text = "Code build completed";
            }
        }

        private void BuildAssetsOnly()
        {
            if (_isBuilding) return;

            try
            {
                _isBuilding = true;
                SetBuildButtonsEnabled(false);
                _progressBar.style.display = DisplayStyle.Flex;
                _progressBar.value = 0;

                ClearLog();
                LogMessage("Starting assets build...");
                int packageVersion = GetNextPackageVersion();
                string outputDirectory = BuildAssets(packageVersion);

                LogMessage($"Assets build completed successfully! Version: {packageVersion} Output: {outputDirectory}");
                EditorUtility.DisplayDialog("Assets Build Successful",
                    $"Assets build completed successfully!\nVersion: {packageVersion}\nOutput: {outputDirectory}", "OK");
            }
            catch (Exception e)
            {
                LogMessage($"Assets build failed: {e.Message}", true);
                EditorUtility.DisplayDialog("Assets Build Failed", $"Assets build failed with error: {e.Message}", "OK");
            }
            finally
            {
                _isBuilding = false;
                SetBuildButtonsEnabled(true);
                _progressBar.style.display = DisplayStyle.None;
                _statusLabel.text = "Assets build completed";
            }
        }

        private void SetBuildButtonsEnabled(bool enabled)
        {
            _buildAllButton.SetEnabled(enabled);
            _buildCodeButton.SetEnabled(enabled);
            _buildAssetsButton.SetEnabled(enabled);
        }

        private void BuildCode()
        {
            LogMessage("=== Phase 1: Building Code ===");
            _progressBar.value = 0.1f;

            // Execute menu items for code generation
            ExecuteMenuItem("Obfuz/GenerateEncryptionVM", "Step 1/4");
            _progressBar.value = 0.2f;

            ExecuteMenuItem("Obfuz/GenerateSecretKeyFile", "Step 2/4");
            _progressBar.value = 0.3f;

            ExecuteMenuItem("HybridCLR/ObfuzExtension/GenerateAll", "Step 3/4");
            _progressBar.value = 0.4f;

            // Compile and obfuscate
            LogMessage("Step 4/4: Compiling and Obfuscating");
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            CompileDllCommand.CompileDll(target);

            string obfuscatedPath = PrebuildCommandExt.GetObfuscatedHotUpdateAssemblyOutputPath(target);
            ObfuscateUtil.ObfuscateHotUpdateAssemblies(target, obfuscatedPath);

            CopyAssemblies(target);
            AssetDatabase.Refresh();
            _progressBar.value = 0.5f;
            LogMessage("Code build completed successfully!");
        }

        private string BuildAssets(int packageVersion)
        {
            LogMessage("=== Phase 2: Building Assets ===");
            LogMessage($"Building assets for {_settings.packageName} version {packageVersion}");
            _progressBar.value = 0.6f;

            var buildParameters = CreateBuildParameters(packageVersion);
            var pipeline = new ScriptableBuildPipeline();
            var result = pipeline.Run(buildParameters, true);

            if (!result.Success)
            {
                throw new Exception($"Asset build failed: {result.ErrorInfo}");
            }

            _progressBar.value = 1.0f;
            LogMessage($"Assets built successfully: {result.OutputPackageDirectory}");
            return result.OutputPackageDirectory;
        }

        private static void ExecuteMenuItem(string menuPath, string stepName)
        {
            Debug.Log($"{stepName}: Executing {menuPath}");
            if (!EditorApplication.ExecuteMenuItem(menuPath))
            {
                Debug.LogWarning($"Failed to execute {menuPath} - menu item might not exist");
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
                    LogMessage($"Copied {srcFile} to {dstFile}");
                }
            }

            // Copy AOT assemblies
            CopyAOTAssemblies(target, codeDir);
        }

        private void CopyAOTAssemblies(BuildTarget target, string codeDir)
        {
            var aotAssemblies = new List<string>();
            string aotSrcDir = $"HybridCLRData/AssembliesPostIl2CppStrip/{target}";

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
                LogMessage($"Copy AOT dll {dllFile} to {dllBytesPath}");
            }

            // Serialize AOT assemblies list
            var aotAssembliesBytes = NinoSerializer.Serialize(aotAssemblies);
            string ninoFilePath = "Assets/HotUpdate/Compiled/AOT.bytes";
            File.WriteAllBytes(ninoFilePath, aotAssembliesBytes);
            AssetDatabase.ImportAsset(ninoFilePath);
        }

        private ScriptableBuildParameters CreateBuildParameters(int packageVersion)
        {
            var buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            var streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

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
                BuildinFileCopyOption = EBuildinFileCopyOption.None,
                BuildinFileCopyParams = string.Empty,
                CompressOption = ECompressOption.LZ4,
                ClearBuildCacheFiles = _settings.clearBuildCache,
                UseAssetDependencyDB = _settings.useAssetDependDB,
                EncryptionServices = new FileStreamEncryption(),
                BuiltinShadersBundleName = GetBuiltinShaderBundleName()
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

            return year * 10000000 + month * 100000 + day * 1000 + hour * 100 + minute * 10 + second / 6;
        }

        private void LogMessage(string message, bool isError = false)
        {
            var logEntry = new Label(message);
            logEntry.AddToClassList(isError ? "log-error" : "log-info");

            _logScrollView.Add(logEntry);
            _logScrollView.ScrollTo(logEntry);

            _statusLabel.text = message;

            if (isError)
                Debug.LogError(message);
            else
                Debug.Log(message);
        }

        private void ClearLog()
        {
            _logScrollView.Clear();
            _statusLabel.text = "Ready to build";
        }

        private VisualElement CreateGroup(string groupTitle)
        {
            var group = new VisualElement();
            group.AddToClassList("group-box");

            var header = new Label(groupTitle);
            header.AddToClassList("group-header");
            group.Add(header);

            return group;
        }


    }
}