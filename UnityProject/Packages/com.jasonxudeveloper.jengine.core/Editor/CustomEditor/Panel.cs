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

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
        private ScrollView _logScrollView;
        private BuildManager _buildManager;

        private void CreateGUI()
        {
            _settings = Settings.Instance;
            _root = rootVisualElement;

            // Initialize build manager
            _buildManager = new BuildManager(_settings, LogMessage);

            // Load stylesheets - Panel first, then Common to override
            var panelStyleSheet = StyleSheetLoader.LoadPackageStyleSheet<Panel>();
            if (panelStyleSheet != null)
                _root.styleSheets.Add(panelStyleSheet);

            var commonStyleSheet = StyleSheetLoader.LoadCommonStyleSheet();
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
            // Use SettingsUIBuilder for Package Settings
            var packageGroup = SettingsUIBuilder.CreatePackageSettingsGroup(_settings);
            parent.Add(packageGroup);

            // Use SettingsUIBuilder for Build Options
            var buildGroup = SettingsUIBuilder.CreateBuildOptionsGroup(_settings);
            parent.Add(buildGroup);
        }

        private void CreateJEngineSettingsSection(VisualElement parent)
        {
            // Use SettingsUIBuilder for JEngine Settings (with all settings)
            var jengineGroup = SettingsUIBuilder.CreateJEngineSettingsGroup(_settings);
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
            var scenesGroup = EditorUIUtils.CreateGroup("Hot Update Scenes");

            // Get all scene files from HotUpdate directory
            var sceneAssets = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/HotUpdate" });

            if (sceneAssets.Length == 0)
            {
                // Full width container for empty state
                var emptyContainer = new VisualElement();
                emptyContainer.AddToClassList("scenes-container");

                var noScenesLabel = new Label("No hot update scenes found in Assets/HotUpdate");
                noScenesLabel.AddToClassList("info-label");
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
                _scenesContainer.AddToClassList("scenes-container");
                scenesGroup.Add(_scenesContainer);

                // Pagination controls - always create them for testing, but show based on pages
                var paginationContainer = new VisualElement();
                paginationContainer.AddToClassList("pagination-container");

                _prevPageButton = new Button(() => ChangePage(-1))
                {
                    text = "◀ Previous"
                };
                EditorUIUtils.MakeActionButtonResponsive(_prevPageButton, EditorUIUtils.ButtonType.Secondary);

                _pageLabel = new Label();
                _pageLabel.AddToClassList("page-label");
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
                sceneContainer.AddToClassList("scene-item-container");

                // Object field for the scene (full width on first row)
                var sceneField = new ObjectField()
                {
                    objectType = typeof(SceneAsset),
                    value = sceneAsset
                };
                sceneField.SetEnabled(false);
                sceneField.AddToClassList("scene-field");
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
                EditorUIUtils.MakeActionButtonResponsive(loadButton);

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
            EditorUIUtils.MakeActionButtonResponsive(_buildCodeButton);

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
            if (_buildManager.IsBuilding) return;

            SetBuildButtonsEnabled(false);
            ClearLog();

            _buildManager.StartBuildAll(
                onComplete: () =>
                {
                    SetBuildButtonsEnabled(true);
                    _statusLabel.text = "Build completed";
                    EditorUtility.DisplayDialog("Build Successful", "Build completed successfully!", "OK");
                },
                onError: e =>
                {
                    SetBuildButtonsEnabled(true);
                    _statusLabel.text = "Build failed";
                    EditorUtility.DisplayDialog("Build Failed", $"Build failed with error:\n{e.Message}", "OK");
                }
            );
        }

        private void BuildCodeOnly()
        {
            if (_buildManager.IsBuilding) return;

            SetBuildButtonsEnabled(false);
            ClearLog();

            _buildManager.StartBuildCodeOnly(
                onComplete: () =>
                {
                    SetBuildButtonsEnabled(true);
                    _statusLabel.text = "Code build completed";
                    EditorUtility.DisplayDialog("Code Build Successful", "Code build completed successfully!", "OK");
                },
                onError: e =>
                {
                    SetBuildButtonsEnabled(true);
                    _statusLabel.text = "Code build failed";
                    EditorUtility.DisplayDialog("Code Build Failed", $"Code build failed with error:\n{e.Message}", "OK");
                }
            );
        }

        private void BuildAssetsOnly()
        {
            if (_buildManager.IsBuilding) return;

            SetBuildButtonsEnabled(false);
            ClearLog();

            _buildManager.StartBuildAssetsOnly(
                onComplete: () =>
                {
                    SetBuildButtonsEnabled(true);
                    _statusLabel.text = "Assets build completed";
                    EditorUtility.DisplayDialog("Assets Build Successful", "Assets build completed successfully!", "OK");
                },
                onError: e =>
                {
                    SetBuildButtonsEnabled(true);
                    _statusLabel.text = "Assets build failed";
                    EditorUtility.DisplayDialog("Assets Build Failed", $"Assets build failed with error:\n{e.Message}", "OK");
                }
            );
        }

        private void SetBuildButtonsEnabled(bool enabled)
        {
            _buildAllButton.SetEnabled(enabled);
            _buildCodeButton.SetEnabled(enabled);
            _buildAssetsButton.SetEnabled(enabled);
        }

        /// <summary>
        /// State machine update called every editor frame during build.
        /// </summary>

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
    }
}