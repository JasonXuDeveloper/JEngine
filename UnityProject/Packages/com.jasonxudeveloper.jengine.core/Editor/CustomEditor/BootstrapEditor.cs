using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using JEngine.Core.Encrypt;
using JEngine.Core.Update;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace JEngine.Core.Editor.CustomEditor
{
    [UnityEditor.CustomEditor(typeof(Bootstrap))]
    public class BootstrapEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Handler for creating inspector UI. If set, this is used instead of default UI.
        /// Set by UI package via [InitializeOnLoad] to provide enhanced UI.
        /// </summary>
        /// <remarks>
        /// Parameters: SerializedObject, Bootstrap instance.
        /// Returns: VisualElement to use as inspector content.
        /// </remarks>
        public static Func<SerializedObject, Bootstrap, VisualElement> CreateInspectorHandler;

        private Bootstrap _bootstrap;
        private VisualElement _root;

        public override VisualElement CreateInspectorGUI()
        {
            _bootstrap = (Bootstrap)target;

            // If UI package provides enhanced editor, use it
            if (CreateInspectorHandler != null)
            {
                return CreateInspectorHandler(serializedObject, _bootstrap);
            }

            // Otherwise use default implementation
            return CreateDefaultInspectorGUI();
        }

        private VisualElement CreateDefaultInspectorGUI()
        {
            _root = new VisualElement();

            // Add USS styling
            var commonStyleSheet = StyleSheetLoader.LoadCommonStyleSheet();
            if (commonStyleSheet != null)
                _root.styleSheets.Add(commonStyleSheet);

            var styleSheet = CreateStyleSheet();
            _root.styleSheets.Add(styleSheet);

            // Apply responsive text sizing to all elements
            EditorUIUtils.MakeAllTextResponsive(_root);

#if UNITY_EDITOR
            // Development Settings Group
            CreateDevelopmentSettingsGroup();
#endif

            // Server Settings Group
            CreateServerSettingsGroup();

            // Asset Settings Group
            CreateAssetSettingsGroup();

            // Security Settings Group
            CreateSecuritySettingsGroup();

            // UI Settings Group
            CreateUISettingsGroup();

            UpdateFallbackServerVisibility();

            return _root;
        }

        private void CreateServerSettingsGroup()
        {
            var serverGroup = CreateGroup("Server Settings");

            // Default Host Server
            var defaultHostRow = CreateFormRow("Host Server");
            var defaultHostField = new TextField();
            defaultHostField.BindProperty(serializedObject.FindProperty(nameof(_bootstrap.defaultHostServer)));
            defaultHostField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(defaultHostField);
            defaultHostRow.Add(defaultHostField);
            serverGroup.Add(defaultHostRow);

            // Toggle Fallback Server Button
            var toggleButtonRow = CreateFormRow("Fallback Mode");
            var toggleButton = new Button();
            toggleButton.clicked += () =>
            {
                _bootstrap.useDefaultAsFallback = !_bootstrap.useDefaultAsFallback;
                serializedObject.FindProperty(nameof(_bootstrap.useDefaultAsFallback)).boolValue =
                    _bootstrap.useDefaultAsFallback;
                EditorUtility.SetDirty(_bootstrap);
                serializedObject.ApplyModifiedProperties();
                UpdateFallbackButtonState(toggleButton);
                UpdateFallbackServerVisibility();
            };
            toggleButton.AddToClassList("toggle-button");
            toggleButton.AddToClassList("form-control");
            EditorUIUtils.MakeFormWidthButton(toggleButton);
            UpdateFallbackButtonState(toggleButton);
            toggleButtonRow.Add(toggleButton);
            serverGroup.Add(toggleButtonRow);

            // Custom Fallback Server (conditionally visible)
            var fallbackContainer = new VisualElement();
            fallbackContainer.name = "fallback-container";
            var fallbackRow = CreateFormRow("Fallback Server");
            var fallbackField = new TextField();
            fallbackField.BindProperty(serializedObject.FindProperty(nameof(_bootstrap.fallbackHostServer)));
            fallbackField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(fallbackField);
            fallbackRow.Add(fallbackField);
            fallbackContainer.Add(fallbackRow);
            serverGroup.Add(fallbackContainer);

            // Toggle for appending time ticks
            var appendTimeTicksRow = CreateFormRow("Append Time Ticks");
            var appendTimeTicksButton = new Button();
            appendTimeTicksButton.clicked += () =>
            {
                _bootstrap.appendTimeTicks = !_bootstrap.appendTimeTicks;
                serializedObject.FindProperty(nameof(_bootstrap.appendTimeTicks)).boolValue =
                    _bootstrap.appendTimeTicks;
                EditorUtility.SetDirty(_bootstrap);
                serializedObject.ApplyModifiedProperties();
                UpdateAppendTimeTicksButtonState(appendTimeTicksButton);
            };
            appendTimeTicksButton.AddToClassList("toggle-button");
            appendTimeTicksButton.AddToClassList("form-control");
            EditorUIUtils.MakeFormWidthButton(appendTimeTicksButton);
            UpdateAppendTimeTicksButtonState(appendTimeTicksButton);
            appendTimeTicksRow.Add(appendTimeTicksButton);
            serverGroup.Add(appendTimeTicksRow);

            UpdateFallbackServerVisibility();
            _root.Add(serverGroup);
        }

        private void CreateAssetSettingsGroup()
        {
            var assetGroup = CreateGroup("Asset Settings");

            // Target Platform
            var targetPlatformRow = CreateFormRow("Platform");
            var targetPlatformField = new EnumField(_bootstrap.targetPlatform);
            targetPlatformField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(targetPlatformField);
            targetPlatformField.RegisterValueChangedCallback(evt =>
            {
                var enumProperty = serializedObject.FindProperty(nameof(_bootstrap.targetPlatform));
                enumProperty.enumValueIndex = Array.IndexOf(Enum.GetValues(typeof(TargetPlatform)), evt.newValue);
                serializedObject.ApplyModifiedProperties();
            });
            targetPlatformRow.Add(targetPlatformField);
            assetGroup.Add(targetPlatformRow);

            // Package Name Dropdown
            var packageNameRow = CreateFormRow("Package");
            var packageChoices = EditorUtils.GetAvailableYooAssetPackages();
            var packageNameField = new PopupField<string>()
            {
                choices = packageChoices.Count > 0 ? packageChoices : new List<string> { _bootstrap.packageName },
                value = _bootstrap.packageName
            };
            packageNameField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(packageNameField);
            packageNameField.RegisterValueChangedCallback(evt =>
            {
                serializedObject.FindProperty(nameof(_bootstrap.packageName)).stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            packageNameRow.Add(packageNameField);
            assetGroup.Add(packageNameRow);

            // Hot Code Assembly Dropdown
            var hotCodeRow = CreateFormRow("Code Assembly");
            var hotCodeChoices = EditorUtils.GetAvailableAsmdefFiles();
            var hotCodeField = new PopupField<string>()
            {
                choices = hotCodeChoices.Count > 0 ? hotCodeChoices : new List<string> { _bootstrap.hotCodeName },
                value = _bootstrap.hotCodeName
            };
            hotCodeField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(hotCodeField);
            hotCodeField.RegisterValueChangedCallback(evt =>
            {
                serializedObject.FindProperty(nameof(_bootstrap.hotCodeName)).stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            hotCodeRow.Add(hotCodeField);
            assetGroup.Add(hotCodeRow);

            // Hot Scene Dropdown
            var hotSceneRow = CreateFormRow("Scene");
            var hotSceneChoices = EditorUtils.GetAvailableHotScenes();
            var hotSceneField = new PopupField<string>()
            {
                choices = hotSceneChoices.Count > 0 ? hotSceneChoices : new List<string> { _bootstrap.selectedHotScene },
                value = _bootstrap.selectedHotScene
            };
            hotSceneField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(hotSceneField);
            hotSceneField.RegisterValueChangedCallback(evt =>
            {
                serializedObject.FindProperty(nameof(_bootstrap.selectedHotScene)).stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            hotSceneRow.Add(hotSceneField);
            assetGroup.Add(hotSceneRow);

            // Hot Update Entry Class Dropdown
            var hotClassRow = CreateFormRow("Entry Class");
            var hotClassChoices = EditorUtils.GetAvailableHotClasses(_bootstrap.hotCodeName);
            var hotClassField = new PopupField<string>()
            {
                choices = hotClassChoices.Count > 0 ? hotClassChoices : new List<string> { _bootstrap.hotUpdateClassName },
                value = _bootstrap.hotUpdateClassName
            };
            hotClassField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(hotClassField);
            hotClassField.RegisterValueChangedCallback(evt =>
            {
                serializedObject.FindProperty(nameof(_bootstrap.hotUpdateClassName)).stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            hotClassRow.Add(hotClassField);
            assetGroup.Add(hotClassRow);

            // Hot Update Entry Method Dropdown
            var hotMethodRow = CreateFormRow("Entry Method");
            var hotMethodChoices = EditorUtils.GetAvailableHotMethods(_bootstrap.hotCodeName, _bootstrap.hotUpdateClassName);
            var hotMethodField = new PopupField<string>()
            {
                choices = hotMethodChoices.Count > 0
                    ? hotMethodChoices
                    : new List<string> { _bootstrap.hotUpdateMethodName },
                value = _bootstrap.hotUpdateMethodName
            };
            hotMethodField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(hotMethodField);
            hotMethodField.RegisterValueChangedCallback(evt =>
            {
                serializedObject.FindProperty(nameof(_bootstrap.hotUpdateMethodName)).stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            hotMethodRow.Add(hotMethodField);
            assetGroup.Add(hotMethodRow);

            // AOT DLL List File Dropdown
            var aotRow = CreateFormRow("AOT DLL List");
            var aotChoices = EditorUtils.GetAvailableAOTDataFiles();
            var aotField = new PopupField<string>()
            {
                choices = aotChoices.Count > 0 ? aotChoices : new List<string> { _bootstrap.aotDllListFilePath },
                value = _bootstrap.aotDllListFilePath
            };
            aotField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(aotField);
            aotField.RegisterValueChangedCallback(evt =>
            {
                serializedObject.FindProperty(nameof(_bootstrap.aotDllListFilePath)).stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            aotRow.Add(aotField);
            assetGroup.Add(aotRow);

            _root.Add(assetGroup);
        }

        private void CreateSecuritySettingsGroup()
        {
            var securityGroup = CreateGroup("Security Settings");

            // Dynamic Secret Key Dropdown
            var dynamicKeyRow = CreateFormRow("Secret Key");
            var dynamicKeyChoices = EditorUtils.GetAvailableDynamicSecretKeys();
            var dynamicKeyField = new PopupField<string>()
            {
                choices = dynamicKeyChoices.Count > 0
                    ? dynamicKeyChoices
                    : new List<string> { _bootstrap.dynamicSecretKeyPath },
                value = _bootstrap.dynamicSecretKeyPath
            };
            dynamicKeyField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(dynamicKeyField);
            dynamicKeyField.RegisterValueChangedCallback(evt =>
            {
                serializedObject.FindProperty(nameof(_bootstrap.dynamicSecretKeyPath)).stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            dynamicKeyRow.Add(dynamicKeyField);
            securityGroup.Add(dynamicKeyRow);

            // Encryption Option
            var bundleConfig = EncryptionMapping.GetBundleConfig(_bootstrap.encryptionOption);
            var manifestConfigFile = bundleConfig.ManifestConfigScriptableObject;
            var bundleConfigFile = bundleConfig.BundleConfigScriptableObject;

            var encryptionRow = CreateFormRow("Encryption");
            var encryptionField = new EnumField(_bootstrap.encryptionOption);

            // Manifest Config Object Field
            var manifestConfigRow = CreateFormRow("Manifest Config");
            var manifestConfigField = new ObjectField()
            {
                objectType = typeof(ScriptableObject),
                value = manifestConfigFile
            };
            manifestConfigField.RegisterValueChangedCallback(_ =>
            {
                // Prevent editing by reverting any changes
                bundleConfig = EncryptionMapping.GetBundleConfig(_bootstrap.encryptionOption);
                manifestConfigFile = bundleConfig.ManifestConfigScriptableObject;
                manifestConfigField.value = manifestConfigFile;
            });
            manifestConfigField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(manifestConfigField);
            manifestConfigRow.Add(manifestConfigField);

            // Bundle Config Object Field
            var bundleConfigRow = CreateFormRow("Bundle Config");
            var bundleConfigField = new ObjectField()
            {
                objectType = typeof(ScriptableObject),
                value = bundleConfigFile
            };
            bundleConfigField.RegisterValueChangedCallback(_ =>
            {
                // Prevent editing by reverting any changes
                bundleConfig = EncryptionMapping.GetBundleConfig(_bootstrap.encryptionOption);
                bundleConfigFile = bundleConfig.BundleConfigScriptableObject;
                bundleConfigField.value = bundleConfigFile;
            });
            bundleConfigField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(bundleConfigField);
            bundleConfigRow.Add(bundleConfigField);

            encryptionField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(encryptionField);
            encryptionField.RegisterValueChangedCallback(evt =>
            {
                var enumProperty = serializedObject.FindProperty(nameof(_bootstrap.encryptionOption));
                enumProperty.enumValueIndex = Array.IndexOf(Enum.GetValues(typeof(EncryptionOption)), evt.newValue);
                serializedObject.ApplyModifiedProperties();

                // Refresh the config object fields when encryption option changes
                var newBundleConfig = EncryptionMapping.GetBundleConfig((EncryptionOption)evt.newValue);
                manifestConfigField.value = newBundleConfig.ManifestConfigScriptableObject;
                bundleConfigField.value = newBundleConfig.BundleConfigScriptableObject;
            });
            encryptionRow.Add(encryptionField);

            securityGroup.Add(encryptionRow);
            securityGroup.Add(manifestConfigRow);
            securityGroup.Add(bundleConfigRow);

            _root.Add(securityGroup);
        }

        private void CreateUISettingsGroup()
        {
            var uiGroup = CreateGroup("UI Settings");

            // Version Text
            var versionRow = CreateFormRow("Version Text");
            var versionField = new ObjectField()
            {
                objectType = typeof(TMPro.TextMeshProUGUI),
                allowSceneObjects = true
            };
            versionField.BindProperty(serializedObject.FindProperty(nameof(_bootstrap.versionText)));
            versionField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(versionField);
            versionRow.Add(versionField);
            uiGroup.Add(versionRow);

            // Update Status Text
            var statusRow = CreateFormRow("Update Status Text");
            var statusField = new ObjectField()
            {
                objectType = typeof(TMPro.TextMeshProUGUI),
                allowSceneObjects = true
            };
            statusField.BindProperty(serializedObject.FindProperty(nameof(_bootstrap.updateStatusText)));
            statusField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(statusField);
            statusRow.Add(statusField);
            uiGroup.Add(statusRow);

            // Download Progress Text
            var progressTextRow = CreateFormRow("Progress Text");
            var progressTextField = new ObjectField()
            {
                objectType = typeof(TMPro.TextMeshProUGUI),
                allowSceneObjects = true
            };
            progressTextField.BindProperty(serializedObject.FindProperty(nameof(_bootstrap.downloadProgressText)));
            progressTextField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(progressTextField);
            progressTextRow.Add(progressTextField);
            uiGroup.Add(progressTextRow);

            // Download Progress Bar
            var progressBarRow = CreateFormRow("Progress Bar");
            var progressBarField = new ObjectField()
            {
                objectType = typeof(UnityEngine.UI.Slider),
                allowSceneObjects = true
            };
            progressBarField.BindProperty(serializedObject.FindProperty(nameof(_bootstrap.downloadProgressBar)));
            progressBarField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(progressBarField);
            progressBarRow.Add(progressBarField);
            uiGroup.Add(progressBarRow);

            // Start Button
            var startButtonRow = CreateFormRow("Start Button");
            var startButtonField = new ObjectField()
            {
                objectType = typeof(UnityEngine.UI.Button),
                allowSceneObjects = true
            };
            startButtonField.BindProperty(serializedObject.FindProperty(nameof(_bootstrap.startButton)));
            startButtonField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(startButtonField);
            startButtonRow.Add(startButtonField);
            uiGroup.Add(startButtonRow);

            _root.Add(uiGroup);
        }

#if UNITY_EDITOR
        private void CreateDevelopmentSettingsGroup()
        {
            var devGroup = CreateGroup("Development Settings");

            // Toggle Play Mode Button
            var playModeRow = CreateFormRow("Editor Mode");
            var playModeButton = new Button();
            playModeButton.clicked += () =>
            {
                _bootstrap.useEditorDevMode = !_bootstrap.useEditorDevMode;
                serializedObject.FindProperty(nameof(_bootstrap.useEditorDevMode)).boolValue =
                    _bootstrap.useEditorDevMode;
                EditorUtility.SetDirty(_bootstrap);
                serializedObject.ApplyModifiedProperties();
                UpdatePlayModeButtonState(playModeButton);
            };
            playModeButton.AddToClassList("toggle-button");
            playModeButton.AddToClassList("form-control");
            EditorUIUtils.MakeFormWidthButton(playModeButton);
            UpdatePlayModeButtonState(playModeButton);
            playModeRow.Add(playModeButton);
            devGroup.Add(playModeRow);

            _root.Add(devGroup);
        }

        private void UpdatePlayModeButtonState(Button button)
        {
            if (_bootstrap.useEditorDevMode)
            {
                button.text = "Editor Dev Mode";
                EditorUIUtils.SwitchButtonColor(button, EditorUIUtils.ButtonType.Danger);
            }
            else
            {
                button.text = "Host Play Mode";
                EditorUIUtils.SwitchButtonColor(button, EditorUIUtils.ButtonType.Success);
            }
        }

        private void UpdateAppendTimeTicksButtonState(Button button)
        {
            if (_bootstrap.appendTimeTicks)
            {
                button.text = "Enabled";
                EditorUIUtils.SwitchButtonColor(button, EditorUIUtils.ButtonType.Success);
            }
            else
            {
                button.text = "Disabled";
                EditorUIUtils.SwitchButtonColor(button, EditorUIUtils.ButtonType.Danger);
            }
        }
#endif

        private void UpdateFallbackButtonState(Button button)
        {
            if (_bootstrap.useDefaultAsFallback)
            {
                button.text = "Using Default Server as Fallback";
                EditorUIUtils.SwitchButtonColor(button, EditorUIUtils.ButtonType.Success);
            }
            else
            {
                button.text = "Using Custom Server as Fallback";
                EditorUIUtils.SwitchButtonColor(button, EditorUIUtils.ButtonType.Warning);
            }
        }

        private void UpdateFallbackServerVisibility()
        {
            var fallbackContainer = _root.Q("fallback-container");
            if (fallbackContainer != null)
            {
                fallbackContainer.style.display =
                    _bootstrap.useDefaultAsFallback ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private VisualElement CreateGroup(string title)
        {
            var group = new VisualElement();
            group.AddToClassList("group-box");

            var header = new Label(title);
            header.AddToClassList("group-header");
            EditorUIUtils.MakeHeaderTextResponsive(header);
            group.Add(header);

            return group;
        }

        private StyleSheet CreateStyleSheet()
        {
            return StyleSheetLoader.LoadPackageStyleSheet<BootstrapEditor>();
        }

        private VisualElement CreateFormRow(string labelText)
        {
            var row = new VisualElement();
            row.AddToClassList("form-row");

            var label = new Label(labelText);
            label.AddToClassList("form-label");
            EditorUIUtils.MakeTextResponsive(label);
            row.Add(label);

            return row;
        }
    }
}