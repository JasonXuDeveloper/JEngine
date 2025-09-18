using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
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
            var styleSheet = CreateStyleSheet();
            _root.styleSheets.Add(styleSheet);

            // Server Settings Group
            CreateServerSettingsGroup();

            // Asset Settings Group
            CreateAssetSettingsGroup();

            // Security Settings Group
            CreateSecuritySettingsGroup();

            // UI Settings Group
            CreateUISettingsGroup();

#if UNITY_EDITOR
            // Development Settings Group
            CreateDevelopmentSettingsGroup();
#endif
            
            UpdateFallbackServerVisibility();

            return _root;
        }

        private void CreateServerSettingsGroup()
        {
            var serverGroup = CreateGroup("Server Settings");

            // Default Host Server
            var defaultHostField = new TextField("Default Host Server");
            defaultHostField.BindProperty(serializedObject.FindProperty("defaultHostServer"));
            serverGroup.Add(defaultHostField);

            // Toggle Fallback Server Button
            var toggleButton = new Button();
            toggleButton.clicked += () =>
            {
                _bootstrap.useDefaultAsFallback = !_bootstrap.useDefaultAsFallback;
                serializedObject.Update();
                UpdateFallbackButtonState(toggleButton);
                UpdateFallbackServerVisibility();
            };
            toggleButton.AddToClassList("toggle-button");
            UpdateFallbackButtonState(toggleButton);
            serverGroup.Add(toggleButton);

            // Custom Fallback Server (conditionally visible)
            var fallbackContainer = new VisualElement();
            fallbackContainer.name = "fallback-container";
            var fallbackField = new TextField("Custom Fallback Server");
            fallbackField.BindProperty(serializedObject.FindProperty("fallbackHostServer"));
            fallbackContainer.Add(fallbackField);
            serverGroup.Add(fallbackContainer);

            UpdateFallbackServerVisibility();
            _root.Add(serverGroup);
        }

        private void CreateAssetSettingsGroup()
        {
            var assetGroup = CreateGroup("Asset Settings");

            // Package Name Dropdown
            var packageNameField = CreateDropdownField(
                "Package Name",
                "packageName",
                EditorUtils.GetAvailableYooAssetPackages,
                "Package name cannot be empty"
            );
            assetGroup.Add(packageNameField);

            // Hot Code Assembly Dropdown
            var hotCodeField = CreateDropdownField(
                "Hot Code Assembly",
                "hotCodeName",
                GetAvailableAsmdefFiles,
                "Hot code name must end with .dll",
                value => value.EndsWith(".dll")
            );
            assetGroup.Add(hotCodeField);

            // Hot Scene Dropdown
            var hotSceneField = CreateDropdownField(
                "Hot Scene",
                "selectedHotScene",
                GetAvailableHotScenes,
                "Hot scene name cannot be empty"
            );
            assetGroup.Add(hotSceneField);

            // Hot Update Entry Class Dropdown
            var hotClassField = CreateDropdownField(
                "Hot Update Entry Class",
                "hotUpdateClassName",
                GetAvailableHotClasses,
                "Hot update class name cannot be empty"
            );
            assetGroup.Add(hotClassField);

            // Hot Update Entry Method Dropdown
            var hotMethodField = CreateDropdownField(
                "Hot Update Entry Method",
                "hotUpdateMethodName",
                GetAvailableHotMethods,
                "Hot update method name cannot be empty"
            );
            assetGroup.Add(hotMethodField);

            // AOT DLL List File Dropdown
            var aotField = CreateDropdownField(
                "AOT DLL List File",
                "aotDllListFilePath",
                GetAvailableAOTDataFiles,
                "AOT DLL list file path cannot be empty"
            );
            assetGroup.Add(aotField);

            _root.Add(assetGroup);
        }

        private void CreateSecuritySettingsGroup()
        {
            var securityGroup = CreateGroup("Security Settings");

            // Static Secret Key Dropdown (Resources folder)
            var staticKeyField = CreateDropdownField(
                "Static Secret Key (Resources)",
                "staticSecretKeyPath",
                GetAvailableStaticSecretKeys,
                "Static secret key path cannot be empty"
            );
            securityGroup.Add(staticKeyField);

            // Dynamic Secret Key Dropdown
            var dynamicKeyField = CreateDropdownField(
                "Dynamic Secret Key",
                "dynamicSecretKeyPath",
                GetAvailableDynamicSecretKeys,
                "Dynamic secret key path cannot be empty"
            );
            securityGroup.Add(dynamicKeyField);

            _root.Add(securityGroup);
        }

        private void CreateUISettingsGroup()
        {
            var uiGroup = CreateGroup("UI Settings");

            // Version Text
            var versionField = new ObjectField("Version Text")
            {
                objectType = typeof(TMPro.TextMeshProUGUI),
                allowSceneObjects = true
            };
            versionField.BindProperty(serializedObject.FindProperty("versionText"));
            uiGroup.Add(versionField);

            // Update Status Text
            var statusField = new ObjectField("Update Status Text")
            {
                objectType = typeof(TMPro.TextMeshProUGUI),
                allowSceneObjects = true
            };
            statusField.BindProperty(serializedObject.FindProperty("updateStatusText"));
            uiGroup.Add(statusField);

            // Download Progress Text
            var progressTextField = new ObjectField("Download Progress Text")
            {
                objectType = typeof(TMPro.TextMeshProUGUI),
                allowSceneObjects = true
            };
            progressTextField.BindProperty(serializedObject.FindProperty("downloadProgressText"));
            uiGroup.Add(progressTextField);

            // Download Progress Bar
            var progressBarField = new ObjectField("Download Progress Bar")
            {
                objectType = typeof(UnityEngine.UI.Slider),
                allowSceneObjects = true
            };
            progressBarField.BindProperty(serializedObject.FindProperty("downloadProgressBar"));
            uiGroup.Add(progressBarField);

            _root.Add(uiGroup);
        }

#if UNITY_EDITOR
        private void CreateDevelopmentSettingsGroup()
        {
            var devGroup = CreateGroup("Development Settings");

            // Toggle Play Mode Button
            var playModeButton = new Button();
            playModeButton.clicked += () =>
            {
                _bootstrap.useEditorDevMode = !_bootstrap.useEditorDevMode;
                serializedObject.Update();
                UpdatePlayModeButtonState(playModeButton);
            };
            playModeButton.AddToClassList("toggle-button");
            UpdatePlayModeButtonState(playModeButton);
            devGroup.Add(playModeButton);

            _root.Add(devGroup);
        }

        private void UpdatePlayModeButtonState(Button button)
        {
            if (_bootstrap.useEditorDevMode)
            {
                button.text = "Editor Dev Mode";
                button.style.backgroundColor = new Color(1f, 0.3f, 0.3f);
            }
            else
            {
                button.text = "Host Play Mode";
                button.style.backgroundColor = new Color(0.3f, 1f, 0.3f);
            }
        }
#endif

        private void UpdateFallbackButtonState(Button button)
        {
            if (_bootstrap.useDefaultAsFallback)
            {
                button.text = "Using Default Server as Fallback";
                button.style.backgroundColor = new Color(0.3f, 1f, 0.3f);
            }
            else
            {
                button.text = "Using Custom Server as Fallback";
                button.style.backgroundColor = new Color(1f, 1f, 0.3f);
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
            group.Add(header);

            return group;
        }

        private VisualElement CreateDropdownField(
            string label,
            string propertyName,
            Func<List<string>> getChoices,
            string validationMessage,
            Func<string, bool> validator = null)
        {
            var container = new VisualElement();
            container.AddToClassList("field-container");

            var choices = getChoices();
            var property = serializedObject.FindProperty(propertyName);

            var dropdown = new PopupField<string>(label)
            {
                choices = choices.Any() ? choices : new List<string> { property.stringValue },
                value = property.stringValue
            };

            dropdown.RegisterValueChangedCallback(evt =>
            {
                property.stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();

                // Validate
                bool isValid = !string.IsNullOrEmpty(evt.newValue);
                if (validator != null)
                    isValid = isValid && validator(evt.newValue);

                UpdateValidationState(container, isValid, validationMessage);
            });

            // Initial validation
            bool initialValid = !string.IsNullOrEmpty(property.stringValue);
            if (validator != null)
                initialValid = initialValid && validator(property.stringValue);
            UpdateValidationState(container, initialValid, validationMessage);

            container.Add(dropdown);
            return container;
        }

        private void UpdateValidationState(VisualElement container, bool isValid, string message)
        {
            var existingError = container.Q<Label>("validation-error");

            if (!isValid)
            {
                if (existingError == null)
                {
                    var errorLabel = new Label(message);
                    errorLabel.name = "validation-error";
                    errorLabel.AddToClassList("validation-error");
                    container.Add(errorLabel);
                }
            }
            else
            {
                existingError?.RemoveFromHierarchy();
            }
        }

        private StyleSheet CreateStyleSheet()
        {
            // Load the USS file from the same directory
            var ussPath = "Assets/JEngine/Core/Editor/CustomEditor/BootstrapEditor.uss";
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);

            if (styleSheet == null)
            {
                // Fallback to empty stylesheet if USS file not found
                styleSheet = CreateInstance<StyleSheet>();
            }

            return styleSheet;
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