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
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        [MenuItem("JEngine/JEngine Panel #&J", priority = 1998)]
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

            // Load stylesheets - Panel first, then Common to override
            var panelStyleSheet = StyleSheetLoader.LoadPackageStyleSheet<Panel>();
            if (panelStyleSheet != null)
                _root.styleSheets.Add(panelStyleSheet);

            var commonStyleSheet = StyleSheetLoader.LoadPackageStyleSheet("JEngineCommon.uss");
            if (commonStyleSheet != null)
                _root.styleSheets.Add(commonStyleSheet);

            // Create main scroll view
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.AddToClassList("main-scroll-view");

            CreateHeader(scrollView);
            CreateJEngineSettingsSection(scrollView);
            CreateSettingsSection(scrollView);
            CreateHotUpdateScenesSection(scrollView);
            CreateBuildSection(scrollView);
            CreateStatusSection(scrollView);

            _root.Add(scrollView);
        }

        private void CreateHeader(VisualElement parent)
        {
            var header = new VisualElement();
            header.AddToClassList("header");

            var panelTitle = new Label("JEngine Panel");
            panelTitle.AddToClassList("header-title");
            EditorUIUtils.MakeTitleTextResponsive(panelTitle);

            var subtitle = new Label("Configure JEngine settings and build hot update code/assets");
            subtitle.AddToClassList("header-subtitle");
            EditorUIUtils.MakeSubtitleTextResponsive(subtitle);

            header.Add(panelTitle);
            header.Add(subtitle);
            parent.Add(header);
        }

        private void CreateSettingsSection(VisualElement parent)
        {
            // Package Settings Group
            CreatePackageSettingsGroup(parent);

            // Build Options Group
            CreateBuildOptionsGroup(parent);
        }

        private void CreatePackageSettingsGroup(VisualElement parent)
        {
            var packageGroup = CreateGroup("Package Settings");

            // Package Name Dropdown
            var packageNameRow = CreateFormRow("Package Name");
            var packageChoices = EditorUtils.GetAvailableYooAssetPackages();
            var packageNameField = new PopupField<string>()
            {
                choices = packageChoices.Any() ? packageChoices : new List<string> { _settings.packageName },
                value = _settings.packageName
            };
            packageNameField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(packageNameField);
            packageNameField.RegisterValueChangedCallback(evt =>
            {
                _settings.packageName = evt.newValue;
                _settings.Save();
            });
            packageNameRow.Add(packageNameField);
            packageGroup.Add(packageNameRow);

            // Build Target field
            var buildTargetRow = CreateFormRow("Build Target");
            var buildTargetField = new EnumField(_settings.buildTarget);
            buildTargetField.RegisterValueChangedCallback(evt =>
            {
                _settings.buildTarget = (BuildTarget)evt.newValue;
                _settings.Save();
            });
            buildTargetField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(buildTargetField);
            buildTargetRow.Add(buildTargetField);
            packageGroup.Add(buildTargetRow);

            // Set to Active button (same width as dropdown)
            var setActiveContainer = CreateFormRow("");
            var setActiveButton = new Button(() =>
            {
                _settings.buildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildTargetField.value = _settings.buildTarget;
                _settings.Save();
            })
            {
                text = "Set to Current Active Target"
            };
            setActiveButton.AddToClassList("form-control");
            EditorUIUtils.MakeActionButtonResponsive(setActiveButton, EditorUIUtils.ButtonType.Primary);
            setActiveContainer.Add(setActiveButton);
            packageGroup.Add(setActiveContainer);
            parent.Add(packageGroup);
        }

        private void CreateBuildOptionsGroup(VisualElement parent)
        {
            var buildGroup = CreateGroup("Build Options");

            // Clear Build Cache Toggle
            var clearCacheRow = CreateFormRow("Clear Build Cache");
            var clearCacheToggle = new Toggle()
            {
                value = _settings.clearBuildCache
            };
            clearCacheToggle.tooltip =
                "Clear build cache before building. Uncheck to enable incremental builds (faster)";
            clearCacheToggle.RegisterValueChangedCallback(evt =>
            {
                _settings.clearBuildCache = evt.newValue;
                _settings.Save();
            });
            clearCacheToggle.AddToClassList("form-control");
            clearCacheRow.Add(clearCacheToggle);
            buildGroup.Add(clearCacheRow);

            // Use Asset Dependency DB Toggle
            var useAssetDBRow = CreateFormRow("Use Asset Dependency DB");
            var useAssetDBToggle = new Toggle()
            {
                value = _settings.useAssetDependDB
            };
            useAssetDBToggle.tooltip = "Use asset dependency database to improve build speed";
            useAssetDBToggle.RegisterValueChangedCallback(evt =>
            {
                _settings.useAssetDependDB = evt.newValue;
                _settings.Save();
            });
            useAssetDBToggle.AddToClassList("form-control");
            useAssetDBRow.Add(useAssetDBToggle);
            buildGroup.Add(useAssetDBRow);

            parent.Add(buildGroup);
        }

        private void CreateJEngineSettingsSection(VisualElement parent)
        {
            var jengineGroup = CreateGroup("JEngine Settings");

            // Language Selection
            var languageRow = CreateFormRow("Display Language");
            var languageField = new EnumField(_settings.language);
            languageField.RegisterValueChangedCallback(evt =>
            {
                _settings.language = (JEngineLanguage)evt.newValue;
                _settings.Save();
            });
            languageField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(languageField);
            languageRow.Add(languageField);
            jengineGroup.Add(languageRow);

            // Encrypt Password
            var passwordRow = CreateFormRow("Encrypt DLL Password");
            var passwordField = new TextField()
            {
                value = _settings.encryptPassword,
                isPasswordField = true
            };
            passwordField.RegisterValueChangedCallback(evt =>
            {
                _settings.encryptPassword = evt.newValue;
                _settings.Save();
            });
            passwordField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(passwordField);
            passwordRow.Add(passwordField);
            jengineGroup.Add(passwordRow);

            // Startup Scene
            var sceneRow = CreateFormRow("Startup Scene");
            var currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(_settings.startUpScenePath);
            var sceneField = new ObjectField()
            {
                objectType = typeof(SceneAsset),
                value = currentScene
            };
            sceneField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(evt.newValue);
                    if (assetPath.EndsWith(".unity"))
                    {
                        _settings.startUpScenePath = assetPath;
                        _settings.Save();
                    }
                }
            });
            sceneField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(sceneField);
            sceneRow.Add(sceneField);
            jengineGroup.Add(sceneRow);

            // Jump to Startup Scene
            var jumpRow = CreateFormRow("Jump to Startup Scene");
            var jumpToggle = new Toggle()
            {
                value = _settings.jumpStartUp
            };
            jumpToggle.tooltip = "Jump to startup scene when launch";
            jumpToggle.RegisterValueChangedCallback(evt =>
            {
                _settings.jumpStartUp = evt.newValue;
                _settings.Save();
            });
            jumpToggle.AddToClassList("form-control");
            jumpRow.Add(jumpToggle);
            jengineGroup.Add(jumpRow);

            parent.Add(jengineGroup);
        }

        private int _currentPage;
        private int _totalPages;
        private const int ScenesPerPage = 5;
        private VisualElement _scenesContainer;
        private Button _prevPageButton;
        private Button _nextPageButton;
        private Label _pageLabel;

        private void CreateHotUpdateScenesSection(VisualElement parent)
        {
            var scenesGroup = CreateGroup("Hot Update Scenes");

            // Get all scene files from HotUpdate directory
            var sceneAssets = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/HotUpdate" });

            if (sceneAssets.Length == 0)
            {
                // Full width container for empty state
                var emptyContainer = new VisualElement();
                emptyContainer.style.width = Length.Percent(100);
                emptyContainer.style.marginLeft = 0;
                emptyContainer.style.marginRight = 0;

                var noScenesLabel = new Label("No hot update scenes found in Assets/HotUpdate");
                noScenesLabel.AddToClassList("info-label");
                noScenesLabel.style.width = Length.Percent(100);
                EditorUIUtils.MakeTextResponsive(noScenesLabel);
                emptyContainer.Add(noScenesLabel);
                scenesGroup.Add(emptyContainer);
            }
            else
            {
                _totalPages = Mathf.CeilToInt((float)sceneAssets.Length / ScenesPerPage);
                _currentPage = 0;

                // Container for scenes (full width, spanning both columns)
                _scenesContainer = new VisualElement();
                _scenesContainer.style.width = Length.Percent(100);
                _scenesContainer.style.marginLeft = 0;
                _scenesContainer.style.marginRight = 0;
                scenesGroup.Add(_scenesContainer);

                // Pagination controls - always create them for testing, but show based on pages
                var paginationContainer = new VisualElement();
                paginationContainer.style.flexDirection = FlexDirection.Row;
                paginationContainer.style.justifyContent = Justify.SpaceBetween;
                paginationContainer.style.alignItems = Align.Center;
                paginationContainer.style.width = Length.Percent(100);
                paginationContainer.style.marginTop = 12;
                paginationContainer.style.marginBottom = 8;
                paginationContainer.style.paddingTop = 8;
                paginationContainer.style.paddingBottom = 8;
                paginationContainer.style.marginLeft = 0;
                paginationContainer.style.marginRight = 0;
                paginationContainer.style.borderTopWidth = 1;
                paginationContainer.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

                _prevPageButton = new Button(() => ChangePage(-1))
                {
                    text = "◀ Previous"
                };
                EditorUIUtils.MakeActionButtonResponsive(_prevPageButton, EditorUIUtils.ButtonType.Secondary);

                _pageLabel = new Label();
                _pageLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                _pageLabel.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                _pageLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                _pageLabel.style.minWidth = 80;
                EditorUIUtils.MakeTextResponsive(_pageLabel);

                _nextPageButton = new Button(() => ChangePage(1))
                {
                    text = "Next ▶"
                };
                EditorUIUtils.MakeActionButtonResponsive(_nextPageButton, EditorUIUtils.ButtonType.Secondary);

                paginationContainer.Add(_prevPageButton);
                paginationContainer.Add(_pageLabel);
                paginationContainer.Add(_nextPageButton);

                // Always add pagination container, but control visibility
                scenesGroup.Add(paginationContainer);
                paginationContainer.style.display = _totalPages > 1 ? DisplayStyle.Flex : DisplayStyle.None;

                RefreshScenesPage(sceneAssets);
            }

            parent.Add(scenesGroup);
        }

        private void ChangePage(int direction)
        {
            _currentPage = Mathf.Clamp(_currentPage + direction, 0, _totalPages - 1);
            var sceneAssets = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/HotUpdate" });
            RefreshScenesPage(sceneAssets);
        }

        private void RefreshScenesPage(string[] sceneAssets)
        {
            _scenesContainer.Clear();

            int startIndex = _currentPage * ScenesPerPage;
            int endIndex = Mathf.Min(startIndex + ScenesPerPage, sceneAssets.Length);

            for (int i = startIndex; i < endIndex; i++)
            {
                var guid = sceneAssets[i];
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);

                // Full width container for each scene - vertical layout like build section
                var sceneContainer = new VisualElement();
                sceneContainer.style.flexDirection = FlexDirection.Column;
                sceneContainer.style.width = Length.Percent(100);
                sceneContainer.style.marginBottom = 12;
                sceneContainer.style.marginLeft = 0;
                sceneContainer.style.marginRight = 0;

                // Object field for the scene (full width on first row)
                var sceneField = new ObjectField()
                {
                    objectType = typeof(SceneAsset),
                    value = sceneAsset
                };
                sceneField.SetEnabled(false);
                sceneField.AddToClassList("form-control");
                sceneField.style.width = Length.Percent(100);
                sceneField.style.marginBottom = 4;
                EditorUIUtils.MakeTextResponsive(sceneField);

                // Action buttons with fixed percentage widths
                var openButton = new Button(() =>
                {
                    EditorSceneManager.OpenScene(assetPath);
                    GUIUtility.ExitGUI();
                })
                {
                    text = "Open"
                };
                EditorUIUtils.MakeActionButtonResponsive(openButton, EditorUIUtils.ButtonType.Success);

                var loadButton = new Button(() =>
                {
                    EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Additive);
                    GUIUtility.ExitGUI();
                })
                {
                    text = "Load"
                };
                EditorUIUtils.MakeActionButtonResponsive(loadButton, EditorUIUtils.ButtonType.Primary);

                var unloadButton = new Button(() =>
                {
                    EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(assetPath), true);
                    GUIUtility.ExitGUI();
                })
                {
                    text = "Unload"
                };
                EditorUIUtils.MakeActionButtonResponsive(unloadButton, EditorUIUtils.ButtonType.Danger);

                // Create responsive button row that wraps when needed (second row)
                var actionsContainer = EditorUIUtils.CreateFlexButtonRow(openButton, loadButton, unloadButton);

                sceneContainer.Add(sceneField);
                sceneContainer.Add(actionsContainer);

                _scenesContainer.Add(sceneContainer);
            }

            // Update pagination controls
            if (_prevPageButton != null && _nextPageButton != null && _pageLabel != null)
            {
                _prevPageButton.SetEnabled(_currentPage > 0);
                _nextPageButton.SetEnabled(_currentPage < _totalPages - 1);
                _pageLabel.text = $"Page {_currentPage + 1} of {_totalPages}";

                // Ensure pagination is visible if we have multiple pages
                var paginationContainer = _prevPageButton.parent;
                if (paginationContainer != null)
                {
                    paginationContainer.style.display = _totalPages > 1 ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }


        private void CreateBuildSection(VisualElement parent)
        {
            var buildGroup = new VisualElement();
            buildGroup.AddToClassList("build-group");

            var buildTitle = new Label("Build Actions");
            buildTitle.AddToClassList("section-title");
            EditorUIUtils.MakeSubtitleTextResponsive(buildTitle);
            buildGroup.Add(buildTitle);

            // Build All Button (main button)
            _buildAllButton = new Button(BuildAll)
            {
                text = "Build All Hot Update Res (Code + Assets)"
            };
            EditorUIUtils.MakeActionButtonResponsive(_buildAllButton, EditorUIUtils.ButtonType.Success);
            buildGroup.Add(_buildAllButton);

            // Build Code Button
            _buildCodeButton = new Button(BuildCodeOnly)
            {
                text = "Build Hot Update Code Only"
            };
            EditorUIUtils.MakeActionButtonResponsive(_buildCodeButton, EditorUIUtils.ButtonType.Primary);

            // Build Assets Button
            _buildAssetsButton = new Button(BuildAssetsOnly)
            {
                text = "Build Hot Update Assets Only"
            };
            EditorUIUtils.MakeActionButtonResponsive(_buildAssetsButton, EditorUIUtils.ButtonType.Warning);

            // Create responsive button row for build buttons
            var individualButtonsContainer = EditorUIUtils.CreateFlexButtonRow(_buildCodeButton, _buildAssetsButton);
            individualButtonsContainer.AddToClassList("individual-buttons-container");

            buildGroup.Add(individualButtonsContainer);

            var infoLabel =
                new Label("Use 'Build All' for complete workflow, or individual buttons for specific tasks.");
            infoLabel.AddToClassList("info-label");
            infoLabel.AddToClassList("text-wrap-multiline");
            EditorUIUtils.MakeTextResponsive(infoLabel);
            buildGroup.Add(infoLabel);

            var packageInfoLabel =
                new Label("Note: For packages other than 'main', only 'Build Hot Update Assets' is required.");
            packageInfoLabel.AddToClassList("info-label");
            packageInfoLabel.AddToClassList("text-wrap-multiline");
            EditorUIUtils.MakeTextResponsive(packageInfoLabel);
            buildGroup.Add(packageInfoLabel);

            parent.Add(buildGroup);
        }

        private void CreateStatusSection(VisualElement parent)
        {
            var statusGroup = new VisualElement();
            statusGroup.AddToClassList("status-group");

            var statusTitle = new Label("Build Status");
            statusTitle.AddToClassList("section-title");
            EditorUIUtils.MakeSubtitleTextResponsive(statusTitle);
            statusGroup.Add(statusTitle);

            _statusLabel = new Label("Ready to build");
            _statusLabel.AddToClassList("status-label");
            statusGroup.Add(_statusLabel);

            _progressBar = new ProgressBar();
            _progressBar.AddToClassList("progress-bar");
            _progressBar.AddToClassList("progress-bar-hidden");
            statusGroup.Add(_progressBar);

            // Log area
            _logScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            _logScrollView.AddToClassList("log-scroll");
            _logScrollView.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            _logScrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            statusGroup.Add(_logScrollView);

            parent.Add(statusGroup);
        }

        private void BuildAll()
        {
            if (_isBuilding) return;

            try
            {
                _isBuilding = true;
                SetBuildButtonsEnabled(false);
                _progressBar.RemoveFromClassList("progress-bar-hidden");
                _progressBar.AddToClassList("progress-bar-visible");
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
                _progressBar.RemoveFromClassList("progress-bar-visible");
                _progressBar.AddToClassList("progress-bar-hidden");
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
                _progressBar.RemoveFromClassList("progress-bar-hidden");
                _progressBar.AddToClassList("progress-bar-visible");
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
                _progressBar.RemoveFromClassList("progress-bar-visible");
                _progressBar.AddToClassList("progress-bar-hidden");
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
                _progressBar.RemoveFromClassList("progress-bar-hidden");
                _progressBar.AddToClassList("progress-bar-visible");
                _progressBar.value = 0;

                ClearLog();
                LogMessage("Starting assets build...");
                int packageVersion = GetNextPackageVersion();
                string outputDirectory = BuildAssets(packageVersion);

                LogMessage($"Assets build completed successfully! Version: {packageVersion} Output: {outputDirectory}");
                EditorUtility.DisplayDialog("Assets Build Successful",
                    $"Assets build completed successfully!\nVersion: {packageVersion}\nOutput: {outputDirectory}",
                    "OK");
            }
            catch (Exception e)
            {
                LogMessage($"Assets build failed: {e.Message}", true);
                EditorUtility.DisplayDialog("Assets Build Failed", $"Assets build failed with error: {e.Message}",
                    "OK");
            }
            finally
            {
                _isBuilding = false;
                SetBuildButtonsEnabled(true);
                _progressBar.RemoveFromClassList("progress-bar-visible");
                _progressBar.AddToClassList("progress-bar-hidden");
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
            EditorUIUtils.MakeHeaderTextResponsive(header);
            group.Add(header);

            return group;
        }


        private VisualElement CreateFormRow(string labelText)
        {
            var row = new VisualElement();
            row.AddToClassList("form-row");

            var label = new Label(labelText);
            label.AddToClassList("form-label");
            row.Add(label);

            return row;
        }
    }
}