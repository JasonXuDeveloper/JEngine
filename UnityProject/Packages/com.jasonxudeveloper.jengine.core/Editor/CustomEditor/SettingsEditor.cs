using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

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
            var commonStyleSheet = StyleSheetLoader.LoadPackageStyleSheet("JEngineCommon.uss");
            if (commonStyleSheet != null)
                _root.styleSheets.Add(commonStyleSheet);

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
            var packageNameRow = CreateFormRow("Package Name");
            var packageChoices = EditorUtils.GetAvailableYooAssetPackages();
            var packageNameField = new PopupField<string>()
            {
                choices = packageChoices.Any() ? packageChoices : new List<string> { _settings.packageName },
                value = _settings.packageName
            };
            packageNameField.AddToClassList("form-control");
            packageNameField.RegisterValueChangedCallback(evt =>
            {
                _settings.packageName = evt.newValue;
                _settings.Save();
            });
            packageNameRow.Add(packageNameField);
            packageGroup.Add(packageNameRow);

            // Build Target with Set to Active button
            var buildTargetRow = CreateFormRow("Build Target");
            var buildTargetField = new EnumField(_settings.buildTarget);
            buildTargetField.BindProperty(serializedObject.FindProperty("buildTarget"));
            buildTargetField.RegisterValueChangedCallback(_ =>
            {
                _settings.Save();
            });
            buildTargetField.AddToClassList("form-control-with-button");

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

            buildTargetRow.Add(buildTargetField);
            buildTargetRow.Add(setActiveButton);
            packageGroup.Add(buildTargetRow);

            _root.Add(packageGroup);
        }

        private void CreateBuildOptionsGroup()
        {
            var buildGroup = CreateGroup("Build Options");

            // Clear Build Cache Toggle
            var clearCacheRow = CreateFormRow("Clear Build Cache");
            var clearCacheToggle = new Toggle();
            clearCacheToggle.BindProperty(serializedObject.FindProperty("clearBuildCache"));
            clearCacheToggle.tooltip =
                "Clear build cache before building. Uncheck to enable incremental builds (faster)";
            clearCacheToggle.RegisterValueChangedCallback(_ => _settings.Save());
            clearCacheToggle.AddToClassList("form-control");
            clearCacheRow.Add(clearCacheToggle);
            buildGroup.Add(clearCacheRow);

            // Use Asset Depend DB Toggle
            var useAssetDBRow = CreateFormRow("Use Asset Dependency DB");
            var useAssetDBToggle = new Toggle();
            useAssetDBToggle.BindProperty(serializedObject.FindProperty("useAssetDependDB"));
            useAssetDBToggle.tooltip = "Use asset dependency database to improve build speed";
            useAssetDBToggle.RegisterValueChangedCallback(_ => _settings.Save());
            useAssetDBToggle.AddToClassList("form-control");
            useAssetDBRow.Add(useAssetDBToggle);
            buildGroup.Add(useAssetDBRow);

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

        private StyleSheet CreateStyleSheet()
        {
            return StyleSheetLoader.LoadPackageStyleSheet<SettingsEditor>();
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