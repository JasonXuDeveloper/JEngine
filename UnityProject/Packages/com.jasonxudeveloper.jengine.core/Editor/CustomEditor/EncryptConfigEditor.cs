using UnityEditor;
using UnityEngine.UIElements;
using JEngine.Core.Encrypt.Config;

namespace JEngine.Core.Editor.CustomEditor
{
    public abstract class EncryptConfigEditor<T> : UnityEditor.Editor where T : EncryptConfig<T, byte[]>
    {
        private T _config;
        private VisualElement _root;

        protected abstract string InfoText { get; }

        public override VisualElement CreateInspectorGUI()
        {
            _config = (T)target;
            _root = new VisualElement();

            // Add USS styling
            var commonStyleSheet = StyleSheetLoader.LoadCommonStyleSheet();
            if (commonStyleSheet != null)
                _root.styleSheets.Add(commonStyleSheet);

            // Create main container group
            var mainGroup = EditorUIUtils.CreateGroup($"{typeof(T).Name} Settings");
            _root.Add(mainGroup);

            // Add specific config fields
            CreateConfigFields(mainGroup);

            // Add regenerate key button
            CreateRegenerateKeyButton(mainGroup);
            
            // Add info text after key fields
            CreateInfoSection(mainGroup);
            
            return _root;
        }

        protected abstract void CreateConfigFields(VisualElement parent);

        private void CreateInfoSection(VisualElement parent)
        {
            var infoLabel = new Label(InfoText);
            infoLabel.AddToClassList("info-text");
            EditorUIUtils.MakeTextResponsive(infoLabel);
            parent.Add(infoLabel);
        }

        private void CreateRegenerateKeyButton(VisualElement parent)
        {
            var buttonRow = EditorUIUtils.CreateFormRow("");
            var regenerateButton = new Button(() =>
            {
                Undo.RecordObject(_config, "Regenerate Encryption Key");
                _config.RegenerateKey();
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();

                // Refresh the inspector to show new values
                serializedObject.Update();

                // Force refresh of all hex displays
                RefreshHexDisplays();

                Repaint();
            })
            {
                text = "Regenerate Key"
            };

            EditorUIUtils.MakeActionButtonResponsive(regenerateButton);
            buttonRow.Add(regenerateButton);
            parent.Add(buttonRow);
        }

        private void RefreshHexDisplays()
        {
            // Query all hex fields and update their values
            var hexFields = _root.Query<TextField>(className: "byte-array-hex-field").ToList();
            foreach (var hexField in hexFields)
            {
                // The hex field should be updated by the property change callback
                // But we can trigger a manual refresh here if needed
                if (hexField.userData is SerializedProperty prop)
                {
                    hexField.value = EditorUIUtils.ByteArrayToHex(prop);
                }
            }
        }
    }
}