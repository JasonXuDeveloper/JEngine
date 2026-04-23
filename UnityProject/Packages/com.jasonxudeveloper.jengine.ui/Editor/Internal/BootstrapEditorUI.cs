// BootstrapEditorUI.cs
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
using JEngine.Core;
using JEngine.Core.Editor;
using JEngine.Core.Encrypt;
using JEngine.Core.Update;
using JEngine.UI.Editor.Components.Button;
using JEngine.UI.Editor.Components.Form;
using JEngine.UI.Editor.Components.Layout;
using JEngine.UI.Editor.Components.Navigation;
using JEngine.UI.Editor.Theming;
using JEngine.UI.Editor.Utilities;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;

namespace JEngine.UI.Editor.Internal
{
    /// <summary>
    /// Enhanced Bootstrap inspector UI using JEngine UI components.
    /// </summary>
    internal static class BootstrapEditorUI
    {
        private static SerializedObject _serializedObject;
        private static Bootstrap _bootstrap;
        private static VisualElement _fallbackContainer;
        private static VisualElement _currentRoot;

        // Signature of the last-seen external data (available packages / scenes / etc.) so the
        // inspector can detect renames and additions made in other windows and rebuild.
        private static string _lastExternalDataSignature;

        /// <summary>
        /// Creates the enhanced Bootstrap inspector.
        /// </summary>
        public static VisualElement CreateInspector(SerializedObject serializedObject, Bootstrap bootstrap)
        {
            _serializedObject = serializedObject;
            _bootstrap = bootstrap;

            // Unregister previous callbacks if they exist (inspector may be recreated).
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.focusChanged -= OnEditorFocusChanged;

            var root = new VisualElement();
            _currentRoot = root;

            // Apply stylesheets
            StyleSheetManager.ApplyAllStyleSheets(root);

            // Apply padding only (no background for inspector)
            root.style.paddingTop = Tokens.Spacing.MD;
            root.style.paddingLeft = Tokens.Spacing.MD;
            root.style.paddingRight = Tokens.Spacing.MD;
            root.style.paddingBottom = Tokens.Spacing.MD;

            BuildContent(root);

            // Rebuild on undo/redo and on editor focus regained (picks up renames made in
            // other windows like the AssetBundle Collector).
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.focusChanged += OnEditorFocusChanged;

            // Seed the signature and poll periodically so renames made in another Unity window
            // (not just another app) propagate without the user having to reselect the asset.
            _lastExternalDataSignature = ComputeExternalDataSignature();
            root.schedule.Execute(CheckForExternalDataChanges).Every(500);

            // Cleanup callback when element is detached.
            root.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            return root;
        }

        /// <summary>
        /// Builds the inspector content into the given root. Used by both initial create and
        /// subsequent rebuilds (undo/redo, focus refresh).
        /// </summary>
        private static void BuildContent(VisualElement root)
        {
            var container = new JContainer(ContainerSize.Xs);
            var content = new JStack(GapSize.Sm);

            content.Add(CreateHeader());
#if UNITY_EDITOR
            content.Add(CreateDevelopmentSettingsSection());
#endif
            content.Add(CreateServerSettingsSection());
            content.Add(CreateAssetSettingsSection());
            content.Add(CreateSecuritySettingsSection());
            content.Add(CreateUISettingsSection());
            content.Add(CreateTextSettingsSection());

            container.Add(content);
            root.Add(container);
        }

        private static void RebuildContent()
        {
            if (_currentRoot == null || _serializedObject == null || _bootstrap == null)
                return;

            _serializedObject.Update();
            _currentRoot.Clear();
            BuildContent(_currentRoot);
        }

        /// <summary>
        /// Called when element is detached from panel. Cleanup callback.
        /// </summary>
        private static void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.focusChanged -= OnEditorFocusChanged;
        }

        private static void OnUndoRedo() => RebuildContent();

        private static void OnEditorFocusChanged(bool hasFocus)
        {
            if (hasFocus) RebuildContent();
        }

        private static void CheckForExternalDataChanges()
        {
            if (_bootstrap == null) return;
            var signature = ComputeExternalDataSignature();
            if (signature == _lastExternalDataSignature) return;
            _lastExternalDataSignature = signature;
            RebuildContent();
        }

        /// <summary>
        /// Concatenates the external data the dropdowns depend on (packages, asmdefs, scenes,
        /// classes/methods for the current assembly, AOT files, dynamic keys) into a signature
        /// string. When any of these change — e.g. a YooAsset package is renamed — the
        /// inspector rebuilds so stale values fall back to "None".
        /// </summary>
        private static string ComputeExternalDataSignature()
        {
            var sb = new System.Text.StringBuilder();
            AppendList(sb, EditorUtils.GetAvailableYooAssetPackages());
            AppendList(sb, EditorUtils.GetAvailableAsmdefFiles());
            AppendList(sb, EditorUtils.GetAvailableHotScenes());
            AppendList(sb, EditorUtils.GetAvailableHotClasses(_bootstrap.hotCodeName));
            AppendList(sb, EditorUtils.GetAvailableHotMethods(_bootstrap.hotCodeName, _bootstrap.hotUpdateClassName));
            AppendList(sb, EditorUtils.GetAvailableAOTDataFiles());
            AppendList(sb, EditorUtils.GetAvailableDynamicSecretKeys());
            return sb.ToString();
        }

        private static void AppendList(System.Text.StringBuilder sb, List<string> items)
        {
            sb.Append('|');
            if (items == null) return;
            foreach (var item in items) sb.Append(item).Append(';');
        }

        private static VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.style.marginBottom = Tokens.Spacing.Xl;
            header.style.paddingBottom = Tokens.Spacing.MD;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = Tokens.Colors.BorderSubtle;

            var title = new Label("Bootstrap Configuration");
            title.style.fontSize = Tokens.FontSize.Title;
            title.style.color = Tokens.Colors.TextHeader;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = Tokens.Spacing.Xs;

            var subtitle = new Label("Configure JEngine hot update bootstrap settings");
            subtitle.style.fontSize = Tokens.FontSize.Base;
            subtitle.style.color = Tokens.Colors.TextMuted;

            header.Add(title);
            header.Add(subtitle);

            return header;
        }

#if UNITY_EDITOR
        private static VisualElement CreateDevelopmentSettingsSection()
        {
            var section = new JSection("Development Settings");

            var playModeButton = new JToggleButton(
                "Editor Dev Mode",
                "Host Play Mode",
                _bootstrap.useEditorDevMode,
                ButtonVariant.Primary,    // Active state uses Primary
                ButtonVariant.Secondary,  // Inactive state uses Secondary
                value =>
                {
                    _bootstrap.useEditorDevMode = value;
                    _serializedObject.FindProperty(nameof(_bootstrap.useEditorDevMode)).boolValue = value;
                    EditorUtility.SetDirty(_bootstrap);
                    _serializedObject.ApplyModifiedProperties();
                }
            ).FullWidth();

            section.Add(new JFormField("Editor Mode", playModeButton));

            return section;
        }
#endif

        private static VisualElement CreateServerSettingsSection()
        {
            var section = new JSection("Server Settings");

            // Default Host Server
            var defaultHostField = new JTextField();
            defaultHostField.BindProperty(_serializedObject.FindProperty(nameof(_bootstrap.defaultHostServer)));
            section.Add(new JFormField("Host Server", defaultHostField));

            // Fallback Mode Toggle
            var fallbackToggle = new JToggleButton(
                "Using Default Server as Fallback",
                "Using Custom Server as Fallback",
                _bootstrap.useDefaultAsFallback,
                ButtonVariant.Primary,    // Active state uses Primary
                ButtonVariant.Secondary,  // Inactive state uses Secondary
                value =>
                {
                    _bootstrap.useDefaultAsFallback = value;
                    _serializedObject.FindProperty(nameof(_bootstrap.useDefaultAsFallback)).boolValue = value;
                    EditorUtility.SetDirty(_bootstrap);
                    _serializedObject.ApplyModifiedProperties();
                    UpdateFallbackVisibility();
                }
            );
            section.Add(new JFormField("Fallback Mode", fallbackToggle));

            // Custom Fallback Server (conditionally visible)
            _fallbackContainer = new VisualElement();
            var fallbackField = new JTextField();
            fallbackField.BindProperty(_serializedObject.FindProperty(nameof(_bootstrap.fallbackHostServer)));
            _fallbackContainer.Add(new JFormField("Fallback Server", fallbackField));
            section.Add(_fallbackContainer);

            // Append Time Ticks Toggle
            var appendTimeTicksToggle = new JToggle(_bootstrap.appendTimeTicks);
            appendTimeTicksToggle.tooltip = "Append time ticks to resource URLs to prevent caching";
            appendTimeTicksToggle.OnValueChanged(value =>
            {
                _bootstrap.appendTimeTicks = value;
                _serializedObject.FindProperty(nameof(_bootstrap.appendTimeTicks)).boolValue = value;
                EditorUtility.SetDirty(_bootstrap);
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Append Time Ticks", appendTimeTicksToggle));

            UpdateFallbackVisibility();

            return section;
        }

        private static VisualElement CreateAssetSettingsSection()
        {
            var section = new JSection("Asset Settings");

            // Target Platform
            var targetPlatformField = JDropdown<TargetPlatform>.ForEnum(_bootstrap.targetPlatform);
            targetPlatformField.OnValueChanged(value =>
            {
                var enumProperty = _serializedObject.FindProperty(nameof(_bootstrap.targetPlatform));
                enumProperty.enumValueIndex = Array.IndexOf(Enum.GetValues(typeof(TargetPlatform)), value);
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Platform", targetPlatformField));

            // Package Name
            var packageOptions = EditorUtils.WithNoneOption(EditorUtils.GetAvailableYooAssetPackages());
            var packageNameField = new JDropdown(
                packageOptions,
                EditorUtils.ResolveDropdownValue(_bootstrap.packageName, packageOptions)
            );
            packageNameField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.packageName)).stringValue = EditorUtils.NormalizeDropdownSelection(value);
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Package", packageNameField));

            // Hot Code Assembly
            var hotCodeOptions = EditorUtils.WithNoneOption(EditorUtils.GetAvailableAsmdefFiles());
            var hotCodeField = new JDropdown(
                hotCodeOptions,
                EditorUtils.ResolveDropdownValue(_bootstrap.hotCodeName, hotCodeOptions)
            );
            hotCodeField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.hotCodeName)).stringValue = EditorUtils.NormalizeDropdownSelection(value);
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Code Assembly", hotCodeField));

            // Hot Scene
            var hotSceneOptions = EditorUtils.WithNoneOption(EditorUtils.GetAvailableHotScenes());
            var hotSceneField = new JDropdown(
                hotSceneOptions,
                EditorUtils.ResolveDropdownValue(_bootstrap.selectedHotScene, hotSceneOptions)
            );
            hotSceneField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.selectedHotScene)).stringValue = EditorUtils.NormalizeDropdownSelection(value);
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Scene", hotSceneField));

            // Hot Update Entry Class
            var hotClassOptions = EditorUtils.WithNoneOption(EditorUtils.GetAvailableHotClasses(_bootstrap.hotCodeName));
            var hotClassField = new JDropdown(
                hotClassOptions,
                EditorUtils.ResolveDropdownValue(_bootstrap.hotUpdateClassName, hotClassOptions)
            );
            hotClassField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.hotUpdateClassName)).stringValue = EditorUtils.NormalizeDropdownSelection(value);
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Entry Class", hotClassField));

            // Hot Update Entry Method
            var hotMethodOptions = EditorUtils.WithNoneOption(EditorUtils.GetAvailableHotMethods(_bootstrap.hotCodeName, _bootstrap.hotUpdateClassName));
            var hotMethodField = new JDropdown(
                hotMethodOptions,
                EditorUtils.ResolveDropdownValue(_bootstrap.hotUpdateMethodName, hotMethodOptions)
            );
            hotMethodField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.hotUpdateMethodName)).stringValue = EditorUtils.NormalizeDropdownSelection(value);
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Entry Method", hotMethodField));

            // AOT DLL List File
            var aotOptions = EditorUtils.WithNoneOption(EditorUtils.GetAvailableAOTDataFiles());
            var aotField = new JDropdown(
                aotOptions,
                EditorUtils.ResolveDropdownValue(_bootstrap.aotDllListFilePath, aotOptions)
            );
            aotField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.aotDllListFilePath)).stringValue = EditorUtils.NormalizeDropdownSelection(value);
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("AOT DLL List", aotField));

            return section;
        }

        private static VisualElement CreateSecuritySettingsSection()
        {
            var section = new JSection("Security Settings");

            // Dynamic Secret Key
            var dynamicKeyOptions = EditorUtils.WithNoneOption(EditorUtils.GetAvailableDynamicSecretKeys());
            var dynamicKeyField = new JDropdown(
                dynamicKeyOptions,
                EditorUtils.ResolveDropdownValue(_bootstrap.dynamicSecretKeyPath, dynamicKeyOptions)
            );
            dynamicKeyField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.dynamicSecretKeyPath)).stringValue = EditorUtils.NormalizeDropdownSelection(value);
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Secret Key", dynamicKeyField));

            // Encryption Option
            var bundleConfig = EncryptionMapping.GetBundleConfig(_bootstrap.encryptionOption);
            var encryptionField = JDropdown<EncryptionOption>.ForEnum(_bootstrap.encryptionOption);

            var manifestConfigField = new JObjectField<ScriptableObject>(false);
            manifestConfigField.Value = bundleConfig.ManifestConfigScriptableObject;
            manifestConfigField.RegisterValueChangedCallback(_ =>
            {
                var config = EncryptionMapping.GetBundleConfig(_bootstrap.encryptionOption);
                manifestConfigField.Value = config.ManifestConfigScriptableObject;
            });

            var bundleConfigField = new JObjectField<ScriptableObject>(false);
            bundleConfigField.Value = bundleConfig.BundleConfigScriptableObject;
            bundleConfigField.RegisterValueChangedCallback(_ =>
            {
                var config = EncryptionMapping.GetBundleConfig(_bootstrap.encryptionOption);
                bundleConfigField.Value = config.BundleConfigScriptableObject;
            });

            encryptionField.OnValueChanged(value =>
            {
                var enumProperty = _serializedObject.FindProperty(nameof(_bootstrap.encryptionOption));
                enumProperty.enumValueIndex = Array.IndexOf(Enum.GetValues(typeof(EncryptionOption)), value);
                _serializedObject.ApplyModifiedProperties();

                var newConfig = EncryptionMapping.GetBundleConfig(value);
                manifestConfigField.Value = newConfig.ManifestConfigScriptableObject;
                bundleConfigField.Value = newConfig.BundleConfigScriptableObject;
            });

            section.Add(new JFormField("Encryption", encryptionField));
            section.Add(new JFormField("Manifest Config", manifestConfigField));
            section.Add(new JFormField("Bundle Config", bundleConfigField));

            return section;
        }

        private static VisualElement CreateUISettingsSection()
        {
            var section = new JSection("UI Settings");

            // Version Text
            var versionField = new JObjectField<TextMeshProUGUI>();
            versionField.BindProperty(_serializedObject.FindProperty(nameof(_bootstrap.versionText)));
            section.Add(new JFormField("Version Text", versionField));

            // Update Status Text
            var statusField = new JObjectField<TextMeshProUGUI>();
            statusField.BindProperty(_serializedObject.FindProperty(nameof(_bootstrap.updateStatusText)));
            section.Add(new JFormField("Update Status Text", statusField));

            // Download Progress Text
            var progressTextField = new JObjectField<TextMeshProUGUI>();
            progressTextField.BindProperty(_serializedObject.FindProperty(nameof(_bootstrap.downloadProgressText)));
            section.Add(new JFormField("Progress Text", progressTextField));

            // Download Progress Bar
            var progressBarField = new JObjectField<Slider>();
            progressBarField.BindProperty(_serializedObject.FindProperty(nameof(_bootstrap.downloadProgressBar)));
            section.Add(new JFormField("Progress Bar", progressBarField));

            // Start Button
            var startButtonField = new JObjectField<Button>();
            startButtonField.BindProperty(_serializedObject.FindProperty(nameof(_bootstrap.startButton)));
            section.Add(new JFormField("Start Button", startButtonField));

            return section;
        }

        private static VisualElement CreateTextSettingsSection()
        {
            var section = new JSection("Text Settings");

            var textProperty = _serializedObject.FindProperty("text");

            // Package Initialization Status
            var packageInitTab = new VisualElement();
            AddTextField(packageInitTab, textProperty, nameof(BootstrapText.initializingPackage), "Initializing");
            AddTextField(packageInitTab, textProperty, nameof(BootstrapText.gettingVersion), "Getting Version");
            AddTextField(packageInitTab, textProperty, nameof(BootstrapText.updatingManifest), "Updating Manifest");
            AddTextField(packageInitTab, textProperty, nameof(BootstrapText.checkingUpdate), "Checking Update");
            AddTextField(packageInitTab, textProperty, nameof(BootstrapText.downloadingResources), "Downloading");
            AddTextField(packageInitTab, textProperty, nameof(BootstrapText.packageCompleted), "Completed");
            AddTextField(packageInitTab, textProperty, nameof(BootstrapText.initializationFailed), "Failed");
            AddTextField(packageInitTab, textProperty, nameof(BootstrapText.unknownPackageStatus), "Unknown Status");

            // Scene Load Status
            var sceneLoadTab = new VisualElement();
            AddTextField(sceneLoadTab, textProperty, nameof(BootstrapText.sceneLoading), "Loading");
            AddTextField(sceneLoadTab, textProperty, nameof(BootstrapText.sceneCompleted), "Completed");
            AddTextField(sceneLoadTab, textProperty, nameof(BootstrapText.sceneFailed), "Failed");
            AddTextField(sceneLoadTab, textProperty, nameof(BootstrapText.unknownSceneStatus), "Unknown Status");

            // Inline Status
            var inlineStatusTab = new VisualElement();
            AddTextField(inlineStatusTab, textProperty, nameof(BootstrapText.initializing), "Initializing");
            AddTextField(inlineStatusTab, textProperty, nameof(BootstrapText.downloading), "Downloading");
            AddTextField(inlineStatusTab, textProperty, nameof(BootstrapText.downloadCompletedLoading), "Download Done");
            AddTextField(inlineStatusTab, textProperty, nameof(BootstrapText.loadingCode), "Loading Code");
            AddTextField(inlineStatusTab, textProperty, nameof(BootstrapText.decryptingResources), "Decrypting");
            AddTextField(inlineStatusTab, textProperty, nameof(BootstrapText.loadingScene), "Loading Scene");

            // Dialog Titles
            var dialogTitlesTab = new VisualElement();
            AddTextField(dialogTitlesTab, textProperty, nameof(BootstrapText.dialogTitleError), "Error Title");
            AddTextField(dialogTitlesTab, textProperty, nameof(BootstrapText.dialogTitleWarning), "Warning Title");
            AddTextField(dialogTitlesTab, textProperty, nameof(BootstrapText.dialogTitleNotice), "Notice Title");

            // Dialog Buttons
            var dialogButtonsTab = new VisualElement();
            AddTextField(dialogButtonsTab, textProperty, nameof(BootstrapText.buttonOk), "OK");
            AddTextField(dialogButtonsTab, textProperty, nameof(BootstrapText.buttonCancel), "Cancel");
            AddTextField(dialogButtonsTab, textProperty, nameof(BootstrapText.buttonDownload), "Download");
            AddTextField(dialogButtonsTab, textProperty, nameof(BootstrapText.buttonRetry), "Retry");
            AddTextField(dialogButtonsTab, textProperty, nameof(BootstrapText.buttonExit), "Exit");

            // Dialog Content
            var dialogContentTab = new VisualElement();
            AddTextField(dialogContentTab, textProperty, nameof(BootstrapText.dialogInitFailed), "Init Failed");
            AddTextField(dialogContentTab, textProperty, nameof(BootstrapText.dialogDownloadPrompt), "Download Prompt");
            AddTextField(dialogContentTab, textProperty, nameof(BootstrapText.dialogDownloadProgress), "Download Progress");
            AddTextField(dialogContentTab, textProperty, nameof(BootstrapText.dialogSceneLoadFailed), "Scene Failed");
            AddTextField(dialogContentTab, textProperty, nameof(BootstrapText.dialogInitException), "Init Exception");
            AddTextField(dialogContentTab, textProperty, nameof(BootstrapText.dialogCodeException), "Code Exception");
            AddTextField(dialogContentTab, textProperty, nameof(BootstrapText.dialogFunctionCallFailed), "Call Failed");

            var tabView = new JTabView(maxTabsPerRow: 3)
                .AddTab("Package Init", packageInitTab)
                .AddTab("Scene Load", sceneLoadTab)
                .AddTab("Inline Status", inlineStatusTab)
                .AddTab("Dialog Titles", dialogTitlesTab)
                .AddTab("Dialog Buttons", dialogButtonsTab)
                .AddTab("Dialog Content", dialogContentTab);

            section.Add(tabView);

            // Reset to Defaults button
            var resetButton = new JButton("Reset to Defaults", () =>
            {
                Undo.RecordObject(_bootstrap, "Reset Bootstrap Text to Defaults");
                var textProp = _serializedObject.FindProperty("text");
                var defaults = BootstrapText.Default;
                var fields = typeof(BootstrapText).GetFields(
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var prop = textProp.FindPropertyRelative(field.Name);
                    if (prop != null && prop.propertyType == SerializedPropertyType.String)
                    {
                        prop.stringValue = (string)field.GetValue(defaults);
                    }
                }
                _serializedObject.ApplyModifiedProperties();
            }, ButtonVariant.Warning);
            section.Add(resetButton);

            return section;
        }

        private static void AddTextField(VisualElement container, SerializedProperty parentProperty,
            string fieldName, string label)
        {
            var prop = parentProperty.FindPropertyRelative(fieldName);
            var textField = new JTextField();
            textField.BindProperty(prop);
            container.Add(new JFormField(label, textField));
        }

        private static void UpdateFallbackVisibility()
        {
            if (_fallbackContainer != null)
            {
                _fallbackContainer.style.display = _bootstrap.useDefaultAsFallback
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }
        }
    }
}
