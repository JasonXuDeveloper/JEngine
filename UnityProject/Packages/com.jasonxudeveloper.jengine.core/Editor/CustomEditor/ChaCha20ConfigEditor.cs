using UnityEngine.UIElements;
using JEngine.Core.Encrypt.Config;

namespace JEngine.Core.Editor.CustomEditor
{
    [UnityEditor.CustomEditor(typeof(ChaCha20Config))]
    public class ChaCha20ConfigEditor : EncryptConfigEditor<ChaCha20Config>
    {
        protected override string InfoText => "ChaCha20 encryption uses a 256-bit key and 96-bit nonce.";

        protected override void CreateConfigFields(VisualElement parent)
        {
            var keyProperty = serializedObject.FindProperty(nameof(ChaCha20Config.key));
            var keyField = EditorUIUtils.CreateByteArrayField("Key (256-bit)", keyProperty);
            parent.Add(keyField);

            var nonceProperty = serializedObject.FindProperty(nameof(ChaCha20Config.nonce));
            var nonceField = EditorUIUtils.CreateByteArrayField("Nonce (96-bit)", nonceProperty);
            parent.Add(nonceField);
        }
    }
}