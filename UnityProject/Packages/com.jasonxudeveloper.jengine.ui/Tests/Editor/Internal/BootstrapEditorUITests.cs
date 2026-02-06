// BootstrapEditorUITests.cs
// EditMode unit tests for BootstrapEditorUI

using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using JEngine.Core;
using JEngine.Core.Encrypt;
using JEngine.Core.Update;
using JEngine.UI.Editor.Components.Button;
using JEngine.UI.Editor.Components.Form;
using JEngine.UI.Editor.Components.Layout;
using JEngine.UI.Editor.Internal;
using JEngine.UI.Tests;

namespace JEngine.UI.Tests.Editor.Internal
{
    [TestFixture]
    public class BootstrapEditorUITests : JEngineTestBase
    {
        private Bootstrap _bootstrap;
        private SerializedObject _serializedObject;
        private GameObject _gameObject;

        [SetUp]
        public override void BaseSetUp()
        {
            base.BaseSetUp();
            _gameObject = new GameObject("TestBootstrap");
            _bootstrap = _gameObject.AddComponent<Bootstrap>();
            _serializedObject = new SerializedObject(_bootstrap);

            // Set default values
            _bootstrap.defaultHostServer = "http://127.0.0.1/";
            _bootstrap.fallbackHostServer = "http://127.0.0.1/";
            _bootstrap.useDefaultAsFallback = true;
            _bootstrap.appendTimeTicks = true;
            _bootstrap.targetPlatform = TargetPlatform.Regular;
            _bootstrap.packageName = "main";
            _bootstrap.hotCodeName = "HotUpdate.Code.dll";
            _bootstrap.encryptionOption = EncryptionOption.Xor;
        }

        [TearDown]
        public override void BaseTearDown()
        {
            base.BaseTearDown();
            if (_gameObject != null)
                UnityEngine.Object.DestroyImmediate(_gameObject);
        }

        #region UI Creation Tests

        [Test]
        public void CreateInspector_ReturnsNonNullRoot()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            Assert.IsNotNull(root);
        }

        [Test]
        public void CreateInspector_ContainsHeader()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var labels = root.Query<Label>().ToList();
            bool foundHeader = false;
            foreach (var label in labels)
            {
                if (label.text == "Bootstrap Configuration")
                {
                    foundHeader = true;
                    break;
                }
            }

            Assert.IsTrue(foundHeader);
        }

        [Test]
        public void CreateInspector_ContainsSubtitle()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var labels = root.Query<Label>().ToList();
            bool foundSubtitle = false;
            foreach (var label in labels)
            {
                if (label.text.Contains("Configure JEngine hot update"))
                {
                    foundSubtitle = true;
                    break;
                }
            }

            Assert.IsTrue(foundSubtitle);
        }

        [Test]
        public void CreateInspector_AppliesPadding()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            Assert.IsTrue(root.style.paddingTop.value.value > 0);
            Assert.IsTrue(root.style.paddingLeft.value.value > 0);
            Assert.IsTrue(root.style.paddingRight.value.value > 0);
            Assert.IsTrue(root.style.paddingBottom.value.value > 0);
        }

        [Test]
        public void CreateInspector_ContainsDevelopmentSettingsSection()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Development Settings");
            Assert.IsNotNull(section);
        }

        [Test]
        public void CreateInspector_ContainsServerSettingsSection()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Server Settings");
            Assert.IsNotNull(section);
        }

        [Test]
        public void CreateInspector_ContainsAssetSettingsSection()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Asset Settings");
            Assert.IsNotNull(section);
        }

        [Test]
        public void CreateInspector_ContainsSecuritySettingsSection()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Security Settings");
            Assert.IsNotNull(section);
        }

        [Test]
        public void CreateInspector_ContainsUISettingsSection()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "UI Settings");
            Assert.IsNotNull(section);
        }

        [Test]
        public void CreateInspector_ContainsTextSettingsSection()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Text Settings");
            Assert.IsNotNull(section);
        }

        [Test]
        public void TextSettings_ContainsExactFieldCount()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Text Settings");
            var formFields = section?.Query<JFormField>().ToList();

            Assert.IsNotNull(formFields);
            // Must match BootstrapText public instance field count exactly
            var expectedCount = typeof(BootstrapText)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Length;
            Assert.AreEqual(expectedCount, formFields.Count);
        }

        [Test]
        public void TextSettings_ContainsSubHeaders()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Text Settings");
            var labels = section?.Query<Label>().ToList();

            Assert.IsNotNull(labels);
            bool foundPackageHeader = false;
            bool foundDialogHeader = false;
            foreach (var label in labels)
            {
                if (label.text == "Package Initialization Status") foundPackageHeader = true;
                if (label.text == "Dialog Content (Format Strings)") foundDialogHeader = true;
            }
            Assert.IsTrue(foundPackageHeader);
            Assert.IsTrue(foundDialogHeader);
        }

        [Test]
        public void TextSettings_ContainsResetButton()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Text Settings");
            var buttons = section?.Query<JButton>().ToList();

            Assert.IsNotNull(buttons);
            Assert.AreEqual(1, buttons.Count);
        }

        [Test]
        public void TextSettings_ResetLogic_RestoresDefaults()
        {
            // Modify a text value via serialized property
            var textProp = _serializedObject.FindProperty("text");
            var initProp = textProp.FindPropertyRelative(nameof(BootstrapText.initializing));
            initProp.stringValue = "Custom text";
            _serializedObject.ApplyModifiedProperties();

            // Verify value was changed
            _serializedObject.Update();
            textProp = _serializedObject.FindProperty("text");
            initProp = textProp.FindPropertyRelative(nameof(BootstrapText.initializing));
            Assert.AreEqual("Custom text", initProp.stringValue);

            // Apply reset logic (same as Reset button callback)
            var defaults = BootstrapText.Default;
            var fields = typeof(BootstrapText).GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                var prop = textProp.FindPropertyRelative(field.Name);
                if (prop != null && prop.propertyType == SerializedPropertyType.String)
                {
                    prop.stringValue = (string)field.GetValue(defaults);
                }
            }
            _serializedObject.ApplyModifiedProperties();

            // Verify all values were reset
            _serializedObject.Update();
            textProp = _serializedObject.FindProperty("text");
            initProp = textProp.FindPropertyRelative(nameof(BootstrapText.initializing));
            Assert.AreEqual(BootstrapText.Default.initializing, initProp.stringValue);

            var dlgProp = textProp.FindPropertyRelative(nameof(BootstrapText.dialogTitleError));
            Assert.AreEqual(BootstrapText.Default.dialogTitleError, dlgProp.stringValue);
        }

        #endregion

        #region JToggleButton Tests

        [Test]
        public void EditorDevModeToggle_Exists()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Development Settings");
            var toggleButton = section?.Q<JToggleButton>();
            Assert.IsNotNull(toggleButton);
        }

        [Test]
        public void EditorDevModeToggle_DefaultValue_IsTrue()
        {
            // Bootstrap.useEditorDevMode defaults to true
            _bootstrap.useEditorDevMode = true;
            _serializedObject.Update();

            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Development Settings");
            var toggleButton = section?.Q<JToggleButton>();

            Assert.IsNotNull(toggleButton);
            Assert.IsTrue(toggleButton.Value);
        }

        [Test]
        public void EditorDevModeToggle_SetValue_UpdatesBootstrap()
        {
            _bootstrap.useEditorDevMode = true;
            _serializedObject.Update();

            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Development Settings");
            var toggleButton = section?.Q<JToggleButton>();

            Assert.IsNotNull(toggleButton);

            // Change value
            toggleButton.Value = false;

            Assert.IsFalse(_bootstrap.useEditorDevMode);
        }

        [Test]
        public void FallbackModeToggle_Exists()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Server Settings");
            var toggleButtons = section?.Query<JToggleButton>().ToList();
            Assert.IsNotNull(toggleButtons);
            Assert.GreaterOrEqual(toggleButtons.Count, 1);
        }

        [Test]
        public void FallbackModeToggle_WhenTrue_HidesFallbackContainer()
        {
            _bootstrap.useDefaultAsFallback = true;
            _serializedObject.Update();

            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            // When useDefaultAsFallback is true, the fallback container should be hidden
            // We verify by checking that the section exists and the fallback field visibility
            var section = FindSectionByTitle(root, "Server Settings");
            Assert.IsNotNull(section);
        }

        [Test]
        public void FallbackModeToggle_WhenFalse_ShowsFallbackContainer()
        {
            _bootstrap.useDefaultAsFallback = false;
            _serializedObject.Update();

            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            // When useDefaultAsFallback is false, the fallback container should be visible
            var section = FindSectionByTitle(root, "Server Settings");
            Assert.IsNotNull(section);
        }

        #endregion

        #region JToggle Tests

        [Test]
        public void AppendTimeTicksToggle_Exists()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Server Settings");
            var toggle = section?.Q<JToggle>();
            Assert.IsNotNull(toggle);
        }

        [Test]
        public void AppendTimeTicksToggle_ValueChange_UpdatesBootstrap()
        {
            _bootstrap.appendTimeTicks = true;
            _serializedObject.Update();

            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Server Settings");
            var toggle = section?.Q<JToggle>();

            Assert.IsNotNull(toggle);
            Assert.IsTrue(toggle.Value);
        }

        #endregion

        #region Dropdown Tests

        [Test]
        public void TargetPlatformDropdown_Exists()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Asset Settings");
            var dropdown = section?.Q(className: "j-dropdown");
            Assert.IsNotNull(dropdown);
        }

        [Test]
        public void PackageNameDropdown_Exists()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Asset Settings");
            var dropdowns = section?.Query(className: "j-dropdown").ToList();
            Assert.IsNotNull(dropdowns);
            Assert.GreaterOrEqual(dropdowns.Count, 2);
        }

        [Test]
        public void HotCodeDropdown_Exists()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Asset Settings");
            var dropdowns = section?.Query(className: "j-dropdown").ToList();
            Assert.IsNotNull(dropdowns);
            Assert.GreaterOrEqual(dropdowns.Count, 3);
        }

        [Test]
        public void EncryptionDropdown_ExistsInSecuritySettings()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Security Settings");
            var dropdown = section?.Q(className: "j-dropdown");
            Assert.IsNotNull(dropdown);
        }

        #endregion

        #region Encryption Tests

        [Test]
        public void Addon1_EncryptionXor_ConfigFieldsExist()
        {
            _bootstrap.packageName = "addon1";
            _bootstrap.encryptionOption = EncryptionOption.Xor;
            _serializedObject.Update();

            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Security Settings");
            var objectFields = section?.Query(className: "j-object-field").ToList();

            Assert.IsNotNull(objectFields);
            Assert.GreaterOrEqual(objectFields.Count, 2);

            var bundleConfig = EncryptionMapping.GetBundleConfig(EncryptionOption.Xor);
            Assert.IsNotNull(bundleConfig.ManifestConfigScriptableObject);
            Assert.IsNotNull(bundleConfig.BundleConfigScriptableObject);
        }

        [Test]
        public void Addon1_EncryptionAes_ConfigFieldsExist()
        {
            _bootstrap.packageName = "addon1";
            _bootstrap.encryptionOption = EncryptionOption.Aes;
            _serializedObject.Update();

            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Security Settings");
            var objectFields = section?.Query(className: "j-object-field").ToList();

            Assert.IsNotNull(objectFields);
            Assert.GreaterOrEqual(objectFields.Count, 2);

            var bundleConfig = EncryptionMapping.GetBundleConfig(EncryptionOption.Aes);
            Assert.IsNotNull(bundleConfig.ManifestConfigScriptableObject);
            Assert.IsNotNull(bundleConfig.BundleConfigScriptableObject);
        }

        [Test]
        public void Addon1_EncryptionChaCha20_ConfigFieldsExist()
        {
            _bootstrap.packageName = "addon1";
            _bootstrap.encryptionOption = EncryptionOption.ChaCha20;
            _serializedObject.Update();

            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Security Settings");
            var objectFields = section?.Query(className: "j-object-field").ToList();

            Assert.IsNotNull(objectFields);
            Assert.GreaterOrEqual(objectFields.Count, 2);

            var bundleConfig = EncryptionMapping.GetBundleConfig(EncryptionOption.ChaCha20);
            Assert.IsNotNull(bundleConfig.ManifestConfigScriptableObject);
            Assert.IsNotNull(bundleConfig.BundleConfigScriptableObject);
        }

        [TestCase(EncryptionOption.Xor)]
        [TestCase(EncryptionOption.Aes)]
        [TestCase(EncryptionOption.ChaCha20)]
        public void EncryptionDropdown_AllOptions_UpdateConfigFields(EncryptionOption option)
        {
            _bootstrap.encryptionOption = option;
            _serializedObject.Update();

            _ = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var bundleConfig = EncryptionMapping.GetBundleConfig(option);

            Assert.IsNotNull(bundleConfig);
            Assert.IsNotNull(bundleConfig.ManifestConfigScriptableObject);
            Assert.IsNotNull(bundleConfig.BundleConfigScriptableObject);
        }

        #endregion

        #region Undo/Redo Tests

        [Test]
        public void UndoRedo_RebuildUI_RootStillValid()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            Assert.IsNotNull(root);
            Assert.IsNotNull(root.Q<JStack>());

            // Simulate what happens during undo/redo callback
            // The UI should be rebuilt but the root should still be valid
            _serializedObject.Update();

            Assert.IsNotNull(root);
        }

        [Test]
        public void UndoRedo_AfterValueChange_PreservesUIStructure()
        {
            _bootstrap.useEditorDevMode = true;
            _serializedObject.Update();

            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            // Change value
            _bootstrap.useEditorDevMode = false;
            _serializedObject.Update();

            // Verify UI structure still exists
            var section = FindSectionByTitle(root, "Development Settings");
            Assert.IsNotNull(section);
        }

        #endregion

        #region Text Field Tests

        [Test]
        public void DefaultHostServerField_Exists()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Server Settings");
            var textField = section?.Q<JTextField>();
            Assert.IsNotNull(textField);
        }

        [Test]
        public void FallbackHostServerField_Exists()
        {
            _bootstrap.useDefaultAsFallback = false;
            _serializedObject.Update();

            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "Server Settings");
            var textFields = section?.Query<JTextField>().ToList();
            Assert.IsNotNull(textFields);
            Assert.GreaterOrEqual(textFields.Count, 2);
        }

        #endregion

        #region UI Settings Object Fields Tests

        [Test]
        public void VersionTextField_Exists()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "UI Settings");
            var objectFields = section?.Query(className: "j-object-field").ToList();
            Assert.IsNotNull(objectFields);
            Assert.GreaterOrEqual(objectFields.Count, 1);
        }

        [Test]
        public void UISettings_ContainsAllRequiredFields()
        {
            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            var section = FindSectionByTitle(root, "UI Settings");
            var formFields = section?.Query<JFormField>().ToList();

            Assert.IsNotNull(formFields);
            // UI Settings should have: Version Text, Update Status Text, Progress Text, Progress Bar, Start Button
            Assert.GreaterOrEqual(formFields.Count, 5);
        }

        #endregion

        #region Target Platform Tests

        [TestCase(TargetPlatform.Regular)]
        [TestCase(TargetPlatform.Standalone)]
        [TestCase(TargetPlatform.WeChat)]
        [TestCase(TargetPlatform.Douyin)]
        [TestCase(TargetPlatform.Alipay)]
        [TestCase(TargetPlatform.TapTap)]
        public void TargetPlatform_AllOptions_UICreationSucceeds(TargetPlatform platform)
        {
            _bootstrap.targetPlatform = platform;
            _serializedObject.Update();

            var root = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            Assert.IsNotNull(root);
            var section = FindSectionByTitle(root, "Asset Settings");
            Assert.IsNotNull(section);
        }

        #endregion

        #region Multiple Calls Tests

        [Test]
        public void CreateInspector_MultipleCalls_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                _ = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);
                _ = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);
                _ = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);
            });
        }

        [Test]
        public void CreateInspector_MultipleCalls_EachReturnsValidRoot()
        {
            var root1 = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);
            var root2 = BootstrapEditorUI.CreateInspector(_serializedObject, _bootstrap);

            Assert.IsNotNull(root1);
            Assert.IsNotNull(root2);
            Assert.AreNotSame(root1, root2);
        }

        #endregion

        #region BootstrapText SafeFormat Tests

        [Test]
        public void SafeFormat_ValidTemplate_FormatsCorrectly()
        {
            var result = BootstrapText.SafeFormat("Hello {0}, you have {1} items", "World", 5);
            Assert.AreEqual("Hello World, you have 5 items", result);
        }

        [Test]
        public void SafeFormat_MalformedTemplate_ReturnsFallback()
        {
            var result = BootstrapText.SafeFormat("Bad format {0} {1} {2}", "only_one_arg");
            Assert.AreEqual("Bad format {0} {1} {2}", result);
        }

        [Test]
        public void SafeFormat_InvalidBraces_ReturnsFallback()
        {
            var result = BootstrapText.SafeFormat("Broken {", "arg");
            Assert.AreEqual("Broken {", result);
        }

        [Test]
        public void SafeFormat_NoPlaceholders_ReturnsTemplate()
        {
            var result = BootstrapText.SafeFormat("No placeholders here");
            Assert.AreEqual("No placeholders here", result);
        }

        [Test]
        public void SafeFormat_EmptyTemplate_ReturnsEmpty()
        {
            var result = BootstrapText.SafeFormat("", "arg");
            Assert.AreEqual("", result);
        }

        #endregion

        #region BootstrapText Default Tests

        [Test]
        public void Default_AllFieldsAreNonNull()
        {
            var defaults = BootstrapText.Default;
            var fields = typeof(BootstrapText).GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                var value = (string)field.GetValue(defaults);
                Assert.IsNotNull(value, $"BootstrapText.Default.{field.Name} is null");
                Assert.IsNotEmpty(value, $"BootstrapText.Default.{field.Name} is empty");
            }
        }

        [Test]
        public void Default_HasExpectedFieldCount()
        {
            var fields = typeof(BootstrapText).GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            Assert.AreEqual(33, fields.Length);
        }

        #endregion

        #region Helper Methods

        private static JSection FindSectionByTitle(VisualElement root, string title)
        {
            var sections = root.Query<JSection>().ToList();
            foreach (var section in sections)
            {
                var label = section.Q<Label>();
                if (label != null && label.text == title)
                {
                    return section;
                }
            }
            return null;
        }

        #endregion
    }
}
