// SettingsUIBuilder.cs
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
using System.Linq;
using JEngine.Core.Encrypt;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace JEngine.Core.Editor.CustomEditor
{
    /// <summary>
    /// Shared utility class for building Settings UI components
    /// Used by both Panel and SettingsEditor to avoid code duplication
    /// </summary>
    public static class SettingsUIBuilder
    {
        public static VisualElement CreatePackageSettingsGroup(Settings settings)
        {
            var packageGroup = EditorUIUtils.CreateGroup("Package Settings");

            // Package Name field (dropdown or text field based on useDropdown)
            var packageNameRow = EditorUIUtils.CreateFormRow("Package Name");

            // Use PopupField for Panel (with available packages)
            var packageChoices = EditorUtils.GetAvailableYooAssetPackages();
            var packageNameField = new PopupField<string>()
            {
                choices = packageChoices.Any() ? packageChoices : new List<string> { settings.packageName },
                value = settings.packageName
            };
            packageNameField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(packageNameField);
            packageNameField.RegisterValueChangedCallback(evt =>
            {
                settings.packageName = evt.newValue;
                settings.Save();
            });
            packageNameRow.Add(packageNameField);
            packageGroup.Add(packageNameRow);

            // Build Target field
            var buildTargetRow = EditorUIUtils.CreateFormRow("Build Target");
            var buildTargetField = new EnumField(settings.buildTarget);
            buildTargetField.RegisterValueChangedCallback(evt =>
            {
                settings.buildTarget = (BuildTarget)evt.newValue;
                settings.Save();
            });
            buildTargetField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(buildTargetField);
            buildTargetRow.Add(buildTargetField);
            packageGroup.Add(buildTargetRow);

            // Set to Active button (same width as dropdown)
            var setActiveContainer = EditorUIUtils.CreateFormRow("");
            var setActiveButton = new Button(() =>
            {
                settings.buildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildTargetField.value = settings.buildTarget;
                settings.Save();
            })
            {
                text = "Set to Current Active Target"
            };
            setActiveButton.AddToClassList("form-control");
            EditorUIUtils.MakeActionButtonResponsive(setActiveButton);
            setActiveContainer.Add(setActiveButton);
            packageGroup.Add(setActiveContainer);

            return packageGroup;
        }

        public static VisualElement CreateBuildOptionsGroup(Settings settings)
        {
            var buildGroup = EditorUIUtils.CreateGroup("Build Options");

            // Clear Build Cache Toggle
            var clearCacheRow = EditorUIUtils.CreateFormRow("Clear Build Cache");
            var clearCacheToggle = new Toggle()
            {
                value = settings.clearBuildCache
            };
            clearCacheToggle.RegisterValueChangedCallback(evt =>
            {
                settings.clearBuildCache = evt.newValue;
                settings.Save();
            });
            clearCacheToggle.AddToClassList("form-control");
            clearCacheRow.Add(clearCacheToggle);
            buildGroup.Add(clearCacheRow);

            // Use Asset Dependency DB Toggle
            var useAssetDBRow = EditorUIUtils.CreateFormRow("Use Asset Dependency DB");
            var useAssetDBToggle = new Toggle()
            {
                value = settings.useAssetDependDB
            };
            useAssetDBToggle.RegisterValueChangedCallback(evt =>
            {
                settings.useAssetDependDB = evt.newValue;
                settings.Save();
            });
            useAssetDBToggle.AddToClassList("form-control");
            useAssetDBRow.Add(useAssetDBToggle);
            buildGroup.Add(useAssetDBRow);

            // Encryption Option
            var bundleConfig = EncryptionMapping.GetBundleConfig(settings.encryptionOption);
            var manifestConfigFile = bundleConfig.ManifestConfigScriptableObject;
            var bundleConfigFile = bundleConfig.BundleConfigScriptableObject;

            var encryptionRow = EditorUIUtils.CreateFormRow("Encryption Option");
            var encryptionField = new EnumField(settings.encryptionOption);

            // Manifest Config Object Field
            var manifestConfigRow = EditorUIUtils.CreateFormRow("Manifest Config");
            var manifestConfigField = new ObjectField()
            {
                objectType = typeof(ScriptableObject),
                value = manifestConfigFile
            };
            manifestConfigField.RegisterValueChangedCallback(_ =>
            {
                // Prevent editing by reverting any changes
                var newBundleConfig = EncryptionMapping.GetBundleConfig(settings.encryptionOption);
                manifestConfigField.value = newBundleConfig.ManifestConfigScriptableObject;
            });
            manifestConfigField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(manifestConfigField);
            manifestConfigRow.Add(manifestConfigField);

            // Bundle Config Object Field
            var bundleConfigRow = EditorUIUtils.CreateFormRow("Bundle Config");
            var bundleConfigField = new ObjectField()
            {
                objectType = typeof(ScriptableObject),
                value = bundleConfigFile
            };
            bundleConfigField.RegisterValueChangedCallback(_ =>
            {
                // Prevent editing by reverting any changes
                var newBundleConfig = EncryptionMapping.GetBundleConfig(settings.encryptionOption);
                bundleConfigField.value = newBundleConfig.BundleConfigScriptableObject;
            });
            bundleConfigField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(bundleConfigField);
            bundleConfigRow.Add(bundleConfigField);

            encryptionField.RegisterValueChangedCallback(evt =>
            {
                settings.encryptionOption = (EncryptionOption)evt.newValue;
                settings.Save();

                // Refresh the config object fields when encryption option changes
                var newBundleConfig = EncryptionMapping.GetBundleConfig(settings.encryptionOption);
                manifestConfigField.value = newBundleConfig.ManifestConfigScriptableObject;
                bundleConfigField.value = newBundleConfig.BundleConfigScriptableObject;
            });
            encryptionField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(encryptionField);
            encryptionRow.Add(encryptionField);

            buildGroup.Add(encryptionRow);
            buildGroup.Add(manifestConfigRow);
            buildGroup.Add(bundleConfigRow);

            return buildGroup;
        }

        public static VisualElement CreateJEngineSettingsGroup(Settings settings)
        {
            var jengineGroup = EditorUIUtils.CreateGroup("JEngine Settings");

            // Startup Scene (only for Panel)
            var sceneRow = EditorUIUtils.CreateFormRow("Startup Scene");
            var currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(settings.startUpScenePath);
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
                        settings.startUpScenePath = assetPath;
                        settings.Save();
                    }
                }
            });
            sceneField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(sceneField);
            sceneRow.Add(sceneField);
            jengineGroup.Add(sceneRow);

            // Jump to Startup Scene
            var jumpRow = EditorUIUtils.CreateFormRow("Jump to Startup Scene");
            var jumpToggle = new Toggle()
            {
                value = settings.jumpStartUp
            };
            jumpToggle.tooltip = "Jump to startup scene when launch";
            jumpToggle.RegisterValueChangedCallback(evt =>
            {
                settings.jumpStartUp = evt.newValue;
                settings.Save();
            });
            jumpToggle.AddToClassList("form-control");
            jumpRow.Add(jumpToggle);
            jengineGroup.Add(jumpRow);

            return jengineGroup;
        }
    }
}