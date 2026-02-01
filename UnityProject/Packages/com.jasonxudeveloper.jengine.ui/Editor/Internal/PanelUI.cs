// PanelUI.cs
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

using System.Collections.Generic;
using System.IO;
using JEngine.Core.Editor;
using JEngine.Core.Editor.CustomEditor;
using JEngine.Core.Encrypt;
using JEngine.UI.Editor.Components.Button;
using JEngine.UI.Editor.Components.Feedback;
using JEngine.UI.Editor.Components.Form;
using JEngine.UI.Editor.Components.Layout;
using JEngine.UI.Editor.Components.Navigation;
using JEngine.UI.Editor.Theming;
using JEngine.UI.Editor.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Internal
{
    /// <summary>
    /// Enhanced Panel UI using JEngine UI components.
    /// </summary>
    internal static class PanelUI
    {
        private static JButton _buildAllButton;
        private static JButton _buildCodeButton;
        private static JButton _buildAssetsButton;
        private static JStatusBar _statusBar;
        private static JLogView _logView;
        private static BuildManager _buildManager;

        /// <summary>
        /// Creates the enhanced Panel content.
        /// </summary>
        public static VisualElement CreateContent(Panel panel, BuildManager buildManager, Settings settings)
        {
            // Create our own BuildManager with logging to our _logView (will be created below)
            // We can't use the one from Panel because it logs to Panel's UI which doesn't exist when using PanelUI
            _buildManager = new BuildManager(settings, LogMessage);

            var root = new VisualElement();
            root.style.flexGrow = 1;

            // Apply stylesheets
            StyleSheetManager.ApplyAllStyleSheets(root);

            // Apply padding only (no background - like Bootstrap inspector)
            root.style.paddingTop = Tokens.Spacing.MD;
            root.style.paddingRight = Tokens.Spacing.MD;
            root.style.paddingBottom = Tokens.Spacing.MD;
            root.style.paddingLeft = Tokens.Spacing.MD;

            // Create main scroll view
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;

            // Centered container for compact panel layout
            var container = new JContainer(ContainerSize.Xs);

            var content = new JStack(GapSize.Sm);

            // Header
            content.Add(CreateHeader());

            // JEngine Settings Section
            content.Add(CreateJEngineSettingsSection(settings));

            // Package Settings Section
            content.Add(CreatePackageSettingsSection(settings));

            // Build Options Section
            content.Add(CreateBuildOptionsSection(settings));

            // Hot Update Scenes Section
            content.Add(CreateHotUpdateScenesSection(settings));

            // Build Actions Section
            content.Add(CreateBuildActionsSection());

            // Status Section
            content.Add(CreateStatusSection());

            container.Add(content);
            scrollView.Add(container);
            root.Add(scrollView);

            return root;
        }

        private static VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.style.marginBottom = Tokens.Spacing.Xl;
            header.style.paddingBottom = Tokens.Spacing.MD;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = Tokens.Colors.BorderSubtle;

            var title = new Label("JEngine Panel");
            title.style.fontSize = Tokens.FontSize.Title;
            title.style.color = Tokens.Colors.TextHeader;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = Tokens.Spacing.Xs;

            var subtitle = new Label("Configure settings and build hot update resources");
            subtitle.style.fontSize = Tokens.FontSize.Base;
            subtitle.style.color = Tokens.Colors.TextMuted;

            header.Add(title);
            header.Add(subtitle);

            return header;
        }

        private static VisualElement CreateJEngineSettingsSection(Settings settings)
        {
            var section = new JSection("JEngine Settings");

            // Startup Scene
            var currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(settings.startUpScenePath);
            var sceneField = new JObjectField<SceneAsset>(false);
            sceneField.Value = currentScene;
            sceneField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(evt.newValue);
                    if (assetPath.EndsWith(".unity"))
                    {
                        settings.startUpScenePath = assetPath;
                        settings.Save();
                    }
                }
            });
            section.Add(new JFormField("Startup Scene", sceneField));

            // Jump to Startup Scene
            var jumpToggle = new JToggle(settings.jumpStartUp);
            jumpToggle.tooltip = "Jump to startup scene when launch";
            jumpToggle.OnValueChanged(value =>
            {
                settings.jumpStartUp = value;
                settings.Save();
            });
            section.Add(new JFormField("Jump to Startup Scene", jumpToggle));

            return section;
        }

        private static VisualElement CreatePackageSettingsSection(Settings settings)
        {
            var section = new JSection("Package Settings");

            // Package Name
            var packageChoices = EditorUtils.GetAvailableYooAssetPackages();
            var packageNameField = new JDropdown(
                packageChoices.Count > 0 ? packageChoices : new List<string> { settings.packageName },
                settings.packageName
            );
            packageNameField.OnValueChanged(value =>
            {
                settings.packageName = value;
                settings.Save();
            });
            section.Add(new JFormField("Package", packageNameField));

            // Build Target
            var buildTargetField = JDropdown<BuildTarget>.ForEnum(settings.buildTarget);
            buildTargetField.OnValueChanged(value =>
            {
                settings.buildTarget = value;
                settings.Save();
            });
            var buildTargetFormField = new JFormField("Build Target", buildTargetField);
            section.Add(buildTargetFormField);

            // Use Active button - wrap in form field for alignment
            var setActiveButton = new JButton("Use Active", () =>
            {
                settings.buildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildTargetField.Value = settings.buildTarget;
                settings.Save();
            }, ButtonVariant.Secondary);
            section.Add(new JFormField("", setActiveButton));

            return section;
        }

        private static VisualElement CreateBuildOptionsSection(Settings settings)
        {
            var section = new JSection("Build Options");

            // Clear Build Cache
            var clearCacheToggle = new JToggle(settings.clearBuildCache);
            clearCacheToggle.OnValueChanged(value =>
            {
                settings.clearBuildCache = value;
                settings.Save();
            });
            section.Add(new JFormField("Clear Build Cache", clearCacheToggle));

            // Use Asset Dependency DB
            var useAssetDBToggle = new JToggle(settings.useAssetDependDB);
            useAssetDBToggle.OnValueChanged(value =>
            {
                settings.useAssetDependDB = value;
                settings.Save();
            });
            section.Add(new JFormField("Use Asset Depend DB", useAssetDBToggle));

            // Encryption Option
            var bundleConfig = EncryptionMapping.GetBundleConfig(settings.encryptionOption);
            var encryptionField = JDropdown<EncryptionOption>.ForEnum(settings.encryptionOption);

            var manifestConfigField = new JObjectField<ScriptableObject>(false);
            manifestConfigField.Value = bundleConfig.ManifestConfigScriptableObject;
            manifestConfigField.RegisterValueChangedCallback(_ =>
            {
                var config = EncryptionMapping.GetBundleConfig(settings.encryptionOption);
                manifestConfigField.Value = config.ManifestConfigScriptableObject;
            });

            var bundleConfigField = new JObjectField<ScriptableObject>(false);
            bundleConfigField.Value = bundleConfig.BundleConfigScriptableObject;
            bundleConfigField.RegisterValueChangedCallback(_ =>
            {
                var config = EncryptionMapping.GetBundleConfig(settings.encryptionOption);
                bundleConfigField.Value = config.BundleConfigScriptableObject;
            });

            encryptionField.OnValueChanged(value =>
            {
                settings.encryptionOption = value;
                settings.Save();

                var newConfig = EncryptionMapping.GetBundleConfig(value);
                manifestConfigField.Value = newConfig.ManifestConfigScriptableObject;
                bundleConfigField.Value = newConfig.BundleConfigScriptableObject;
            });

            section.Add(new JFormField("Encryption", encryptionField));
            section.Add(new JFormField("Manifest Config", manifestConfigField));
            section.Add(new JFormField("Bundle Config", bundleConfigField));

            return section;
        }

        private static VisualElement CreateHotUpdateScenesSection(Settings settings)
        {
            var section = new JSection("Scenes");

            var sceneAssets = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/HotUpdate" });

            if (sceneAssets.Length == 0)
            {
                var infoLabel = new Label("No scenes in Assets/HotUpdate");
                infoLabel.style.color = Tokens.Colors.TextMuted;
                infoLabel.style.fontSize = Tokens.FontSize.Sm;
                section.Add(infoLabel);
            }
            else
            {
                // Scene list container
                var sceneList = new VisualElement();
                sceneList.style.backgroundColor = Tokens.Colors.BgSurface;
                sceneList.style.borderTopLeftRadius = Tokens.BorderRadius.Sm;
                sceneList.style.borderTopRightRadius = Tokens.BorderRadius.Sm;
                sceneList.style.borderBottomLeftRadius = Tokens.BorderRadius.Sm;
                sceneList.style.borderBottomRightRadius = Tokens.BorderRadius.Sm;
                // No vertical padding - let rows handle it
                sceneList.style.paddingLeft = Tokens.Spacing.MD;
                sceneList.style.paddingRight = Tokens.Spacing.MD;

                for (int i = 0; i < sceneAssets.Length; i++)
                {
                    var guid = sceneAssets[i];
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                    var sceneName = sceneAsset != null ? sceneAsset.name : Path.GetFileNameWithoutExtension(assetPath);

                    // Get package name from YooAsset collector settings
                    var packageName = EditorUtils.GetPackageNameForAsset(assetPath, settings.packageName);

                    // Scene row
                    var sceneRow = new VisualElement();
                    sceneRow.style.flexDirection = FlexDirection.Row;
                    sceneRow.style.alignItems = Align.Center;
                    sceneRow.style.paddingTop = Tokens.Spacing.MD;
                    sceneRow.style.paddingBottom = Tokens.Spacing.MD;
                    sceneRow.style.borderTopLeftRadius = Tokens.BorderRadius.Sm;
                    sceneRow.style.borderTopRightRadius = Tokens.BorderRadius.Sm;
                    sceneRow.style.borderBottomLeftRadius = Tokens.BorderRadius.Sm;
                    sceneRow.style.borderBottomRightRadius = Tokens.BorderRadius.Sm;
                    sceneRow.style.marginLeft = -Tokens.Spacing.Sm;
                    sceneRow.style.marginRight = -Tokens.Spacing.Sm;
                    sceneRow.style.paddingLeft = Tokens.Spacing.Sm;
                    sceneRow.style.paddingRight = Tokens.Spacing.Sm;

                    // Hover effect (no closure - use evt.currentTarget)
                    sceneRow.RegisterCallback<MouseEnterEvent>(OnSceneRowMouseEnter);
                    sceneRow.RegisterCallback<MouseLeaveEvent>(OnSceneRowMouseLeave);

                    // Breadcrumb: [package] › [scene]
                    var breadcrumb = JBreadcrumb.FromPath(packageName, sceneName);
                    sceneRow.Add(breadcrumb);

                    // Action buttons - minimal style
                    var path = assetPath; // Capture for closure

                    var openBtn = new JIconButton("O", () =>
                    {
                        EditorSceneManager.OpenScene(path);
                        GUIUtility.ExitGUI();
                    }, "Open scene");

                    var addBtn = new JIconButton("+", () =>
                    {
                        EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                        GUIUtility.ExitGUI();
                    }, "Load additive");

                    var removeBtn = new JIconButton("-", () =>
                    {
                        EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(path), true);
                        GUIUtility.ExitGUI();
                    }, "Unload scene");

                    sceneRow.Add(openBtn);
                    sceneRow.Add(addBtn);
                    sceneRow.Add(removeBtn);

                    sceneList.Add(sceneRow);

                    // Separator (except last)
                    if (i < sceneAssets.Length - 1)
                    {
                        var separator = new VisualElement();
                        separator.style.height = 1;
                        separator.style.backgroundColor = Tokens.Colors.BorderSubtle;
                        sceneList.Add(separator);
                    }
                }

                section.Add(sceneList);
            }

            return section;
        }

        // Hover callbacks without closures
        private static void OnSceneRowMouseEnter(MouseEnterEvent evt)
        {
            ((VisualElement)evt.currentTarget).style.backgroundColor = Tokens.Colors.BgHover;
        }

        private static void OnSceneRowMouseLeave(MouseLeaveEvent evt)
        {
            ((VisualElement)evt.currentTarget).style.backgroundColor = Color.clear;
        }

        private static VisualElement CreateBuildActionsSection()
        {
            var section = new JSection("Build Actions");

            // Build All - Primary action (larger and more prominent)
            _buildAllButton = new JButton("Build All", BuildAll).FullWidth();
            _buildAllButton.style.minHeight = 32; // Larger button
            _buildAllButton.style.fontSize = Tokens.FontSize.MD; // Slightly larger text
            section.Add(_buildAllButton);

            // Divider text
            var orLabel = new Label("or build individually:");
            orLabel.style.color = Tokens.Colors.TextMuted;
            orLabel.style.fontSize = Tokens.FontSize.Sm;
            orLabel.style.marginTop = Tokens.Spacing.MD;
            orLabel.style.marginBottom = Tokens.Spacing.Sm;
            section.Add(orLabel);

            // Secondary actions row - Code and Assets
            var secondaryRow = new VisualElement();
            secondaryRow.style.flexDirection = FlexDirection.Row;

            _buildCodeButton = new JButton("Code Only", BuildCodeOnly, ButtonVariant.Secondary);
            _buildCodeButton.style.flexGrow = 1;
            _buildCodeButton.style.marginRight = Tokens.Spacing.Sm;

            _buildAssetsButton = new JButton("Assets Only", BuildAssetsOnly, ButtonVariant.Secondary);
            _buildAssetsButton.style.flexGrow = 1;

            secondaryRow.Add(_buildCodeButton);
            secondaryRow.Add(_buildAssetsButton);
            section.Add(secondaryRow);

            // Important note for non-main packages
            var noteLabel = new Label("Note: For non-main packages, use 'Assets Only' to skip code compilation.");
            noteLabel.style.color = Tokens.Colors.TextMuted;
            noteLabel.style.fontSize = 10;
            noteLabel.style.marginTop = Tokens.Spacing.MD;
            noteLabel.style.whiteSpace = WhiteSpace.Normal;
            section.Add(noteLabel);

            return section;
        }

        private static VisualElement CreateStatusSection()
        {
            var section = new JSection("Build Status");

            _statusBar = new JStatusBar("Ready to build");
            section.Add(_statusBar);

            _logView = new JLogView(200)
                .WithMinHeight(150)
                .WithMaxHeight(400);
            section.Add(_logView);

            return section;
        }

        private static void BuildAll()
        {
            BuildHelper.ExecuteBuildAll(_buildManager, new BuildHelper.BuildCallbacks
            {
                SetButtonsEnabled = SetBuildButtonsEnabled,
                ClearLog = () => _logView.Clear(),
                UpdateStatus = msg => _statusBar.Text = msg,
                OnSuccess = () => _statusBar.SetStatus(StatusType.Success),
                OnError = () => _statusBar.SetStatus(StatusType.Error)
            });
        }

        private static void BuildCodeOnly()
        {
            BuildHelper.ExecuteBuildCodeOnly(_buildManager, new BuildHelper.BuildCallbacks
            {
                SetButtonsEnabled = SetBuildButtonsEnabled,
                ClearLog = () => _logView.Clear(),
                UpdateStatus = msg => _statusBar.Text = msg,
                OnSuccess = () => _statusBar.SetStatus(StatusType.Success),
                OnError = () => _statusBar.SetStatus(StatusType.Error)
            });
        }

        private static void BuildAssetsOnly()
        {
            BuildHelper.ExecuteBuildAssetsOnly(_buildManager, new BuildHelper.BuildCallbacks
            {
                SetButtonsEnabled = SetBuildButtonsEnabled,
                ClearLog = () => _logView.Clear(),
                UpdateStatus = msg => _statusBar.Text = msg,
                OnSuccess = () => _statusBar.SetStatus(StatusType.Success),
                OnError = () => _statusBar.SetStatus(StatusType.Error)
            });
        }

        private static void SetBuildButtonsEnabled(bool enabled)
        {
            _buildAllButton?.SetEnabled(enabled);
            _buildCodeButton?.SetEnabled(enabled);
            _buildAssetsButton?.SetEnabled(enabled);
        }

        /// <summary>
        /// Logs a message to the enhanced UI's log view.
        /// </summary>
        private static void LogMessage(string message, bool isError = false)
        {
            if (_logView != null)
            {
                if (isError)
                    _logView.LogError(message);
                else
                    _logView.LogInfo(message);
            }

            if (isError)
                Debug.LogError(message);
            else
                Debug.Log(message);
        }
    }
}