using UnityEngine.UIElements;
using JEngine.Core.Encrypt.Config;

namespace JEngine.Core.Editor.CustomEditor
{
    [UnityEditor.CustomEditor(typeof(XorConfig))]
    public class XorConfigEditor : EncryptConfigEditor<XorConfig>
    {
        protected override string InfoText => "XOR encryption uses a variable-length key (16-128 bytes).";

        protected override void CreateConfigFields(VisualElement parent)
        {
            var keyProperty = serializedObject.FindProperty(nameof(XorConfig.key));
            var keyField = EditorUIUtils.CreateByteArrayField("Key", keyProperty);
            parent.Add(keyField);
        }
    }
}