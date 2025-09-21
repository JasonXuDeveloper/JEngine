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

            // JEngine Settings Group
            CreateJEngineSettingsGroup();

            return _root;
        }

        private void CreatePackageSettingsGroup()
        {
            var packageGroup = SettingsUIBuilder.CreatePackageSettingsGroup(_settings, true);
            _root.Add(packageGroup);
        }

        private void CreateBuildOptionsGroup()
        {
            var buildGroup = SettingsUIBuilder.CreateBuildOptionsGroup(_settings, true);
            _root.Add(buildGroup);
        }

        private void CreateJEngineSettingsGroup()
        {
            var jengineGroup = SettingsUIBuilder.CreateJEngineSettingsGroup(_settings, true);
            _root.Add(jengineGroup);
        }

        private StyleSheet CreateStyleSheet()
        {
            return StyleSheetLoader.LoadPackageStyleSheet<SettingsEditor>();
        }

    }
}