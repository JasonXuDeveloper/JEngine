using UnityEngine.UIElements;
using JEngine.Core.Encrypt.Config;

namespace JEngine.Core.Editor.CustomEditor
{
    [UnityEditor.CustomEditor(typeof(AesConfig))]
    public class AesConfigEditor : EncryptConfigEditor<AesConfig>
    {
        protected override string InfoText => "AES-256 encryption uses a 256-bit key and 128-bit initialization vector.";

        protected override void CreateConfigFields(VisualElement parent)
        {
            var keyProperty = serializedObject.FindProperty(nameof(AesConfig.key));
            var keyField = EditorUIUtils.CreateByteArrayField("Key (256-bit)", keyProperty);
            parent.Add(keyField);

            var ivProperty = serializedObject.FindProperty(nameof(AesConfig.iv));
            var ivField = EditorUIUtils.CreateByteArrayField("IV (128-bit)", ivProperty);
            parent.Add(ivField);
        }
    }
}