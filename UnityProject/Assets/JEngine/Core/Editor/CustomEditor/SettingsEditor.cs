using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System;

namespace JEngine.Core.Editor.CustomEditor
{
    [UnityEditor.CustomEditor(typeof(Settings))]
    public class SettingsEditor : UnityEditor.Editor
    {
        private Settings _settings;
        private VisualElement _root;

        public override VisualElement CreateInspectorGUI()
        {
            _settings = (Settings)target;
            _root = new VisualElement();

            // Add USS styling
            var styleSheet = CreateStyleSheet();
            _root.styleSheets.Add(styleSheet);

            // Package Settings Group
            CreatePackageSettingsGroup();

            // Build Options Group
            CreateBuildOptionsGroup();

            return _root;
        }

        private void CreatePackageSettingsGroup()
        {
            var packageGroup = CreateGroup("Package Settings");

            // Package Name Dropdown
            var packageNameField = CreateDropdownField(
                "Package Name",
                "packageName",
                EditorUtils.GetAvailableYooAssetPackages,
                "Package name cannot be empty"
            );
            packageGroup.Add(packageNameField);

            // Build Target with Set to Active button
            var buildTargetContainer = new VisualElement();
            buildTargetContainer.AddToClassList("horizontal-container");

            var buildTargetField = new EnumField("Build Target", _settings.buildTarget);
            buildTargetField.BindProperty(serializedObject.FindProperty("buildTarget"));
            buildTargetField.RegisterValueChangedCallback(_ =>
            {
                _settings.Save();
            });
            buildTargetField.style.flexGrow = 1;
            buildTargetContainer.Add(buildTargetField);

            var setActiveButton = new Button(() =>
            {
                _settings.buildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildTargetField.value = _settings.buildTarget;
                serializedObject.Update();
                _settings.Save();
            })
            {
                text = "Set to Active"
            };
            setActiveButton.AddToClassList("utility-button");
            buildTargetContainer.Add(setActiveButton);

            packageGroup.Add(buildTargetContainer);

            _root.Add(packageGroup);
        }

        private void CreateBuildOptionsGroup()
        {
            var buildGroup = CreateGroup("Build Options");

            // Clear Build Cache Toggle
            var clearCacheToggle = new Toggle("Clear Build Cache");
            clearCacheToggle.BindProperty(serializedObject.FindProperty("clearBuildCache"));
            clearCacheToggle.tooltip =
                "Clear build cache before building. Uncheck to enable incremental builds (faster)";
            clearCacheToggle.RegisterValueChangedCallback(_ => _settings.Save());
            buildGroup.Add(clearCacheToggle);

            // Use Asset Depend DB Toggle
            var useAssetDBToggle = new Toggle("Use Asset Dependency DB");
            useAssetDBToggle.BindProperty(serializedObject.FindProperty("useAssetDependDB"));
            useAssetDBToggle.tooltip = "Use asset dependency database to improve build speed";
            useAssetDBToggle.RegisterValueChangedCallback(_ => _settings.Save());
            buildGroup.Add(useAssetDBToggle);

            _root.Add(buildGroup);
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
            string validationMessage)
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
                _settings.Save();

                // Validate
                bool isValid = !string.IsNullOrEmpty(evt.newValue);
                UpdateValidationState(container, isValid, validationMessage);
            });

            // Initial validation
            bool initialValid = !string.IsNullOrEmpty(property.stringValue);
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
            // Try to load the shared USS file
            var ussPath = "Assets/JEngine/Core/Editor/CustomEditor/BootstrapEditor.uss";
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);

            if (styleSheet == null)
            {
                // Fallback to empty stylesheet if USS file not found
                styleSheet = ScriptableObject.CreateInstance<StyleSheet>();
            }

            return styleSheet;
        }

    }
}