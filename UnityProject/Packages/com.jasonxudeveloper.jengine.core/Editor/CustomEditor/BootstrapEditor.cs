using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using JEngine.Core.Encrypt;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace JEngine.Core.Editor.CustomEditor
{
    [UnityEditor.CustomEditor(typeof(Bootstrap))]
    public class BootstrapEditor : UnityEditor.Editor
    {
        private Bootstrap _bootstrap;
        private VisualElement _root;

        public override VisualElement CreateInspectorGUI()
        {
            _bootstrap = (Bootstrap)target;
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
            var defaultHostRow = CreateFormRow("Default Host Server");
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
                serializedObject.Update();
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
            var fallbackRow = CreateFormRow("Custom Fallback Server");
            var fallbackField = new TextField();
            fallbackField.BindProperty(serializedObject.FindProperty(nameof(_bootstrap.fallbackHostServer)));
            fallbackField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(fallbackField);
            fallbackRow.Add(fallbackField);
            fallbackContainer.Add(fallbackRow);
            serverGroup.Add(fallbackContainer);

            UpdateFallbackServerVisibility();
            _root.Add(serverGroup);
        }

        private void CreateAssetSettingsGroup()
        {
            var assetGroup = CreateGroup("Asset Settings");

            // Package Name Dropdown
            var packageNameRow = CreateFormRow("Package Name");
            var packageChoices = EditorUtils.GetAvailableYooAssetPackages();
            var packageNameField = new PopupField<string>()
            {
                choices = packageChoices.Any() ? packageChoices : new List<string> { _bootstrap.packageName },
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
            var hotCodeRow = CreateFormRow("Hot Code Assembly");
            var hotCodeChoices = GetAvailableAsmdefFiles();
            var hotCodeField = new PopupField<string>()
            {
                choices = hotCodeChoices.Any() ? hotCodeChoices : new List<string> { _bootstrap.hotCodeName },
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
            var hotSceneRow = CreateFormRow("Hot Scene");
            var hotSceneChoices = GetAvailableHotScenes();
            var hotSceneField = new PopupField<string>()
            {
                choices = hotSceneChoices.Any() ? hotSceneChoices : new List<string> { _bootstrap.selectedHotScene },
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
            var hotClassRow = CreateFormRow("Hot Update Entry Class");
            var hotClassChoices = GetAvailableHotClasses();
            var hotClassField = new PopupField<string>()
            {
                choices = hotClassChoices.Any() ? hotClassChoices : new List<string> { _bootstrap.hotUpdateClassName },
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
            var hotMethodRow = CreateFormRow("Hot Update Entry Method");
            var hotMethodChoices = GetAvailableHotMethods();
            var hotMethodField = new PopupField<string>()
            {
                choices = hotMethodChoices.Any()
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
            var aotRow = CreateFormRow("AOT DLL List File");
            var aotChoices = GetAvailableAOTDataFiles();
            var aotField = new PopupField<string>()
            {
                choices = aotChoices.Any() ? aotChoices : new List<string> { _bootstrap.aotDllListFilePath },
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

            // Static Secret Key Dropdown (Resources folder)
            var staticKeyRow = CreateFormRow("Static Secret Key (Resources)");
            var staticKeyChoices = GetAvailableStaticSecretKeys();
            var staticKeyField = new PopupField<string>()
            {
                choices = staticKeyChoices.Any()
                    ? staticKeyChoices
                    : new List<string> { _bootstrap.staticSecretKeyPath },
                value = _bootstrap.staticSecretKeyPath
            };
            staticKeyField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(staticKeyField);
            staticKeyField.RegisterValueChangedCallback(evt =>
            {
                serializedObject.FindProperty(nameof(_bootstrap.staticSecretKeyPath)).stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            staticKeyRow.Add(staticKeyField);
            securityGroup.Add(staticKeyRow);

            // Dynamic Secret Key Dropdown
            var dynamicKeyRow = CreateFormRow("Dynamic Secret Key");
            var dynamicKeyChoices = GetAvailableDynamicSecretKeys();
            var dynamicKeyField = new PopupField<string>()
            {
                choices = dynamicKeyChoices.Any()
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

            var encryptionRow = CreateFormRow("Encryption Option");
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
                serializedObject.FindProperty(nameof(_bootstrap.encryptionOption)).enumValueIndex =
                    (int)(EncryptionOption)evt.newValue;
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
            var progressTextRow = CreateFormRow("Download Progress Text");
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
            var progressBarRow = CreateFormRow("Download Progress Bar");
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
                serializedObject.Update();
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

        // Helper methods to get available options

        private List<string> GetAvailableAsmdefFiles()
        {
            var asmdefGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { "Assets/HotUpdate" });
            return asmdefGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(System.IO.Path.GetFileNameWithoutExtension)
                .Select(asmdefName => asmdefName + ".dll")
                .ToList();
        }

        private List<string> GetAvailableHotScenes()
        {
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/HotUpdate" });
            return sceneGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToList();
        }

        private List<string> GetAvailableHotClasses()
        {
            try
            {
                var assemblyName = System.IO.Path.GetFileNameWithoutExtension(_bootstrap.hotCodeName);
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == assemblyName);

                if (assembly != null)
                {
                    return assembly.GetTypes()
                        .Where(t => t.IsClass && t.IsPublic)
                        .Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static).Any())
                        .Select(t => t.FullName)
                        .OrderBy(n => n)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get YooAsset packages: {ex.Message}");
            }

            return new List<string>();
        }

        private List<string> GetAvailableHotMethods()
        {
            try
            {
                if (!string.IsNullOrEmpty(_bootstrap.hotUpdateClassName))
                {
                    var type = Type.GetType(_bootstrap.hotUpdateClassName);
                    if (type == null)
                    {
                        var assemblyName = System.IO.Path.GetFileNameWithoutExtension(_bootstrap.hotCodeName);
                        var assembly = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == assemblyName);
                        if (assembly != null)
                        {
                            type = assembly.GetType(_bootstrap.hotUpdateClassName);
                        }
                    }

                    if (type != null)
                    {
                        return type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .Where(m => m.ReturnType == typeof(void) ||
                                        m.ReturnType == typeof(UniTask) ||
                                        m.ReturnType == typeof(System.Threading.Tasks.Task))
                            .Where(m => m.GetParameters().Length == 0)
                            .Select(m => m.Name)
                            .OrderBy(n => n)
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get YooAsset packages: {ex.Message}");
            }

            return new List<string>();
        }

        private List<string> GetAvailableStaticSecretKeys()
        {
            var secretKeys = new List<string>();

            // Search for .bytes files in Resources folders
            var resourcePaths = new[] { "Assets/Resources" };
            var bytesGuids = AssetDatabase.FindAssets("t:TextAsset", resourcePaths);

            foreach (var guid in bytesGuids)
            {
                var fullPath = AssetDatabase.GUIDToAssetPath(guid);
                if (fullPath.EndsWith(".bytes"))
                {
                    // Convert the full path to a Resources-relative path (without extension)
                    var resourcesIndex = fullPath.LastIndexOf("/Resources/", StringComparison.Ordinal);
                    if (resourcesIndex >= 0)
                    {
                        var relativePath = fullPath.Substring(resourcesIndex + 11); // Skip "/Resources/"
                        relativePath = System.IO.Path.ChangeExtension(relativePath, null); // Remove .bytes extension
                        secretKeys.Add(relativePath);
                    }
                }
            }

            // Add default if no keys found or ensure default is in the list
            if (!secretKeys.Contains("Obfuz/StaticSecretKey"))
            {
                secretKeys.Insert(0, "Obfuz/StaticSecretKey");
            }

            return secretKeys;
        }

        private List<string> GetAvailableDynamicSecretKeys()
        {
            var secretKeys = new List<string>();

            // Search for .bytes files that might be secret keys
            var bytesGuids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets" });
            var secretKeyFiles = bytesGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".bytes") &&
                               (path.Contains("Secret") || path.Contains("Obfuz") || path.Contains("Key")))
                .ToList();

            if (secretKeyFiles.Any())
            {
                secretKeys.AddRange(secretKeyFiles);
            }

            // Add default if no keys found
            if (secretKeys.Count == 0)
            {
                secretKeys.Add("Assets/HotUpdate/Obfuz/DynamicSecretKey.bytes");
            }

            return secretKeys;
        }

        private List<string> GetAvailableAOTDataFiles()
        {
            var aotDataGuids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/HotUpdate/Compiled" });
            return aotDataGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".bytes"))
                .OrderBy(path => path)
                .ToList();
        }
    }
}