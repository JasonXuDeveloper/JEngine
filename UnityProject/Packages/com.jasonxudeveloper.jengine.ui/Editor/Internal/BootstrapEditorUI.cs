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

        /// <summary>
        /// Creates the enhanced Bootstrap inspector.
        /// </summary>
        public static VisualElement CreateInspector(SerializedObject serializedObject, Bootstrap bootstrap)
        {
            _serializedObject = serializedObject;
            _bootstrap = bootstrap;

            // Unregister previous undo callback if exists
            Undo.undoRedoPerformed -= OnUndoRedo;

            var root = new VisualElement();
            _currentRoot = root;

            // Apply stylesheets
            StyleSheetManager.ApplyAllStyleSheets(root);

            // Apply padding only (no background for inspector)
            root.style.paddingTop = Tokens.Spacing.MD;
            root.style.paddingLeft = Tokens.Spacing.MD;
            root.style.paddingRight = Tokens.Spacing.MD;
            root.style.paddingBottom = Tokens.Spacing.MD;

            // Centered container for compact inspector layout
            var container = new JContainer(ContainerSize.Xs);

            var content = new JStack(GapSize.Sm);

            // Header
            content.Add(CreateHeader());

#if UNITY_EDITOR
            // Development Settings
            content.Add(CreateDevelopmentSettingsSection());
#endif

            // Server Settings
            content.Add(CreateServerSettingsSection());

            // Asset Settings
            content.Add(CreateAssetSettingsSection());

            // Security Settings
            content.Add(CreateSecuritySettingsSection());

            // UI Settings
            content.Add(CreateUISettingsSection());

            container.Add(content);
            root.Add(container);

            // Register undo/redo callback
            Undo.undoRedoPerformed += OnUndoRedo;

            // Cleanup callback when element is detached (no closure)
            root.RegisterCallback<DetachFromPanelEvent, Undo.UndoRedoCallback>(OnDetachFromPanel, OnUndoRedo);

            return root;
        }

        /// <summary>
        /// Called when element is detached from panel. Cleanup callback.
        /// </summary>
        private static void OnDetachFromPanel(DetachFromPanelEvent evt, Undo.UndoRedoCallback undoCallback)
        {
            Undo.undoRedoPerformed -= undoCallback;
        }

        /// <summary>
        /// Called when undo/redo is performed. Rebuilds the inspector UI.
        /// </summary>
        private static void OnUndoRedo()
        {
            if (_currentRoot == null || _serializedObject == null || _bootstrap == null)
                return;

            // Update serialized object to reflect undo/redo changes
            _serializedObject.Update();

            // Rebuild the entire UI
            _currentRoot.Clear();

            // Centered container for compact inspector layout
            var container = new JContainer(ContainerSize.Xs);

            // Recreate content
            var content = new JStack(GapSize.Sm);

            content.Add(CreateHeader());

#if UNITY_EDITOR
            content.Add(CreateDevelopmentSettingsSection());
#endif
            content.Add(CreateServerSettingsSection());
            content.Add(CreateAssetSettingsSection());
            content.Add(CreateSecuritySettingsSection());
            content.Add(CreateUISettingsSection());

            container.Add(content);
            _currentRoot.Add(container);
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
            var packageChoices = EditorUtils.GetAvailableYooAssetPackages();
            var packageNameField = new JDropdown(
                packageChoices.Count > 0 ? packageChoices : new List<string> { _bootstrap.packageName },
                _bootstrap.packageName
            );
            packageNameField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.packageName)).stringValue = value;
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Package", packageNameField));

            // Hot Code Assembly
            var hotCodeChoices = EditorUtils.GetAvailableAsmdefFiles();
            var hotCodeField = new JDropdown(
                hotCodeChoices.Count > 0 ? hotCodeChoices : new List<string> { _bootstrap.hotCodeName },
                _bootstrap.hotCodeName
            );
            hotCodeField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.hotCodeName)).stringValue = value;
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Code Assembly", hotCodeField));

            // Hot Scene
            var hotSceneChoices = EditorUtils.GetAvailableHotScenes();
            var hotSceneField = new JDropdown(
                hotSceneChoices.Count > 0 ? hotSceneChoices : new List<string> { _bootstrap.selectedHotScene },
                _bootstrap.selectedHotScene
            );
            hotSceneField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.selectedHotScene)).stringValue = value;
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Scene", hotSceneField));

            // Hot Update Entry Class
            var hotClassChoices = EditorUtils.GetAvailableHotClasses(_bootstrap.hotCodeName);
            var hotClassField = new JDropdown(
                hotClassChoices.Count > 0 ? hotClassChoices : new List<string> { _bootstrap.hotUpdateClassName },
                _bootstrap.hotUpdateClassName
            );
            hotClassField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.hotUpdateClassName)).stringValue = value;
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Entry Class", hotClassField));

            // Hot Update Entry Method
            var hotMethodChoices = EditorUtils.GetAvailableHotMethods(_bootstrap.hotCodeName, _bootstrap.hotUpdateClassName);
            var hotMethodField = new JDropdown(
                hotMethodChoices.Count > 0 ? hotMethodChoices : new List<string> { _bootstrap.hotUpdateMethodName },
                _bootstrap.hotUpdateMethodName
            );
            hotMethodField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.hotUpdateMethodName)).stringValue = value;
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("Entry Method", hotMethodField));

            // AOT DLL List File
            var aotChoices = EditorUtils.GetAvailableAOTDataFiles();
            var aotField = new JDropdown(
                aotChoices.Count > 0 ? aotChoices : new List<string> { _bootstrap.aotDllListFilePath },
                _bootstrap.aotDllListFilePath
            );
            aotField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.aotDllListFilePath)).stringValue = value;
                _serializedObject.ApplyModifiedProperties();
            });
            section.Add(new JFormField("AOT DLL List", aotField));

            return section;
        }

        private static VisualElement CreateSecuritySettingsSection()
        {
            var section = new JSection("Security Settings");

            // Dynamic Secret Key
            var dynamicKeyChoices = EditorUtils.GetAvailableDynamicSecretKeys();
            var dynamicKeyField = new JDropdown(
                dynamicKeyChoices.Count > 0 ? dynamicKeyChoices : new List<string> { _bootstrap.dynamicSecretKeyPath },
                _bootstrap.dynamicSecretKeyPath
            );
            dynamicKeyField.OnValueChanged(value =>
            {
                _serializedObject.FindProperty(nameof(_bootstrap.dynamicSecretKeyPath)).stringValue = value;
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
