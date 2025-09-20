using UnityEngine.UIElements;

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
            var commonStyleSheet = StyleSheetLoader.LoadCommonStyleSheet();
            if (commonStyleSheet != null)
                _root.styleSheets.Add(commonStyleSheet);

            var styleSheet = CreateStyleSheet();
            _root.styleSheets.Add(styleSheet);

            // Package Settings Group
            CreatePackageSettingsGroup();

            // Build Options Group
            CreateBuildOptionsGroup();

            // JEngine Settings Group
            CreateJEngineSettingsGroup();

            return _root;
        }

        private void CreatePackageSettingsGroup()
        {
            var packageGroup = SettingsUIBuilder.CreatePackageSettingsGroup(_settings);
            _root.Add(packageGroup);
        }

        private void CreateBuildOptionsGroup()
        {
            var buildGroup = SettingsUIBuilder.CreateBuildOptionsGroup(_settings);
            _root.Add(buildGroup);
        }

        private void CreateJEngineSettingsGroup()
        {
            // Don't include Panel-specific settings in inspector
            var jengineGroup = SettingsUIBuilder.CreateJEngineSettingsGroup(_settings);
            _root.Add(jengineGroup);
        }

        private StyleSheet CreateStyleSheet()
        {
            return StyleSheetLoader.LoadPackageStyleSheet<SettingsEditor>();
        }

    }
}