// PanelUITests.cs
// EditMode unit tests for PanelUI

using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using JEngine.Core.Editor;
using JEngine.Core.Editor.CustomEditor;
using JEngine.Core.Encrypt;
using JEngine.UI.Editor.Components.Button;
using JEngine.UI.Editor.Components.Feedback;
using JEngine.UI.Editor.Components.Form;
using JEngine.UI.Editor.Components.Layout;
using JEngine.UI.Editor.Internal;
using JEngine.UI.Tests;

namespace JEngine.UI.Tests.Editor.Internal
{
    [TestFixture]
    public class PanelUITests : JEngineTestBase
    {
        private Settings _settings;
        private BuildManager _buildManager;

        [SetUp]
        public override void BaseSetUp()
        {
            base.BaseSetUp();
            _settings = ScriptableObject.CreateInstance<Settings>();
            _settings.packageName = "main";
            _settings.encryptionOption = EncryptionOption.Xor;
            // Use active build target to ensure build support is available
            _settings.buildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
            _settings.clearBuildCache = false;
            _settings.useAssetDependDB = true;
            _settings.jumpStartUp = true;
            // Use empty path to skip scene handling during tests (avoids save prompts)
            _settings.startUpScenePath = string.Empty;

            _buildManager = new BuildManager(_settings, (msg, isError) => { });
        }

        [TearDown]
        public override void BaseTearDown()
        {
            base.BaseTearDown();
            if (_settings != null)
                UnityEngine.Object.DestroyImmediate(_settings);
        }

        #region UI Creation Tests

        [Test]
        public void CreateContent_ReturnsNonNullRoot()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            Assert.IsNotNull(root);
        }

        [Test]
        public void CreateContent_RootHasFlexGrow()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            Assert.AreEqual(1f, root.style.flexGrow.value);
        }

        [Test]
        public void CreateContent_ContainsScrollView()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var scrollView = root.Q<ScrollView>();
            Assert.IsNotNull(scrollView);
        }

        [Test]
        public void CreateContent_ContainsJEngineSettingsSection()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "JEngine Settings");
            Assert.IsNotNull(section);
        }

        [Test]
        public void CreateContent_ContainsPackageSettingsSection()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Package Settings");
            Assert.IsNotNull(section);
        }

        [Test]
        public void CreateContent_ContainsBuildOptionsSection()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Build Options");
            Assert.IsNotNull(section);
        }

        [Test]
        public void CreateContent_ContainsBuildActionsSection()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Build Actions");
            Assert.IsNotNull(section);
        }

        [Test]
        public void CreateContent_ContainsStatusSection()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Build Status");
            Assert.IsNotNull(section);
        }

        [Test]
        public void CreateContent_ContainsScenesSection()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Scenes");
            Assert.IsNotNull(section);
        }

        #endregion

        #region Build Button Tests

        [Test]
        public void BuildAllButton_Exists()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var button = FindButtonByText(root, "Build All");
            Assert.IsNotNull(button);
        }

        [Test]
        public void BuildCodeOnlyButton_Exists()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var button = FindButtonByText(root, "Code Only");
            Assert.IsNotNull(button);
        }

        [Test]
        public void BuildAssetsOnlyButton_Exists()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var button = FindButtonByText(root, "Assets Only");
            Assert.IsNotNull(button);
        }

        [Test]
        public void BuildAllButton_IsJButton()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var button = FindButtonByText(root, "Build All");
            Assert.IsInstanceOf<JButton>(button);
        }

        [Test]
        public void BuildCodeOnlyButton_IsJButton()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var button = FindButtonByText(root, "Code Only");
            Assert.IsInstanceOf<JButton>(button);
        }

        [Test]
        public void BuildAssetsOnlyButton_IsJButton()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var button = FindButtonByText(root, "Assets Only");
            Assert.IsInstanceOf<JButton>(button);
        }

        [Test]
        public void BuildAllButton_HasLargerMinHeight()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var button = FindButtonByText(root, "Build All");
            Assert.AreEqual(32f, button.style.minHeight.value.value);
        }

        [UnityTest]
        [Category("Integration")]
        public IEnumerator BuildAllButton_Click_CompletesSuccessfully()
        {
            // MUST set this to prevent test failure from Unity internal errors like "Bake Ambient Probe"
            LogAssert.ignoreFailingMessages = true;

            // Use the init scene which already has proper lighting setup
            const string initScenePath = "Assets/Init.unity";
            EditorSceneManager.OpenScene(initScenePath);

            // Wait for Unity editor to stabilize after scene change
            yield return null;

            // Clear build cache to avoid "output directory exists" error from previous test runs
            _settings.clearBuildCache = true;
            _settings.startUpScenePath = initScenePath;

            // Track build completion state
            bool buildCompleted = false;
            bool buildSucceeded = false;
            Exception buildError = null;

            // Create a BuildManager with callbacks to track completion
            var testBuildManager = new BuildManager(_settings, (msg, isError) =>
            {
                if (isError)
                    Debug.LogError($"[BuildTest] {msg}");
                else
                    Debug.Log($"[BuildTest] {msg}");
            });

            // Start the build with completion callbacks
            testBuildManager.StartBuildAll(
                onComplete: () =>
                {
                    buildCompleted = true;
                    buildSucceeded = true;
                },
                onError: (ex) =>
                {
                    buildCompleted = true;
                    buildSucceeded = false;
                    buildError = ex;
                }
            );

            // Wait for build to complete (timeout after 5 minutes)
            float timeout = 300f;
            float elapsed = 0f;
            while (!buildCompleted && elapsed < timeout)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            // Assert build completed successfully
            Assert.IsTrue(buildCompleted, $"Build did not complete within {timeout} seconds");
            Assert.IsTrue(buildSucceeded, $"Build failed with error: {buildError?.Message}");
        }

        [UnityTest]
        [Category("Integration")]
        public IEnumerator BuildAssetsOnly_Addon1_Xor_CompletesSuccessfully()
        {
            yield return BuildAssetsOnlyTest("addon1", EncryptionOption.Xor);
        }

        [UnityTest]
        [Category("Integration")]
        public IEnumerator BuildAssetsOnly_Addon1_Aes_CompletesSuccessfully()
        {
            yield return BuildAssetsOnlyTest("addon1", EncryptionOption.Aes);
        }

        [UnityTest]
        [Category("Integration")]
        public IEnumerator BuildAssetsOnly_Addon1_ChaCha20_CompletesSuccessfully()
        {
            yield return BuildAssetsOnlyTest("addon1", EncryptionOption.ChaCha20);
        }

        private IEnumerator BuildAssetsOnlyTest(string packageName, EncryptionOption encryption)
        {
            // MUST set this to prevent test failure from Unity internal errors like "Bake Ambient Probe"
            LogAssert.ignoreFailingMessages = true;

            // Use the init scene which already has proper lighting setup
            const string initScenePath = "Assets/Init.unity";
            EditorSceneManager.OpenScene(initScenePath);

            // Wait for Unity editor to stabilize after scene change
            yield return null;

            // Configure settings for this test
            _settings.packageName = packageName;
            _settings.encryptionOption = encryption;
            _settings.startUpScenePath = initScenePath;
            // Clear build cache to avoid "output directory exists" error from previous test runs
            _settings.clearBuildCache = true;

            // Track build completion state
            bool buildCompleted = false;
            bool buildSucceeded = false;
            Exception buildError = null;

            // Create a BuildManager with callbacks to track completion
            var testBuildManager = new BuildManager(_settings, (msg, isError) =>
            {
                if (isError)
                    Debug.LogError($"[BuildTest-{packageName}-{encryption}] {msg}");
                else
                    Debug.Log($"[BuildTest-{packageName}-{encryption}] {msg}");
            });

            // Start assets-only build with completion callbacks
            testBuildManager.StartBuildAssetsOnly(
                onComplete: () =>
                {
                    buildCompleted = true;
                    buildSucceeded = true;
                },
                onError: (ex) =>
                {
                    buildCompleted = true;
                    buildSucceeded = false;
                    buildError = ex;
                }
            );

            // Wait for build to complete (timeout after 2 minutes for assets-only)
            float timeout = 120f;
            float elapsed = 0f;
            while (!buildCompleted && elapsed < timeout)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            // Assert build completed successfully
            Assert.IsTrue(buildCompleted, $"Build did not complete within {timeout} seconds");
            Assert.IsTrue(buildSucceeded, $"Build failed for {packageName} with {encryption}: {buildError?.Message}");
        }

        #endregion

        #region Status Bar Tests

        [Test]
        public void StatusBar_Exists()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var statusBar = root.Q<JStatusBar>();
            Assert.IsNotNull(statusBar);
        }

        [Test]
        public void StatusBar_InitialText_IsReadyToBuild()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var statusBar = root.Q<JStatusBar>();
            Assert.AreEqual("Ready to build", statusBar.Text);
        }

        [Test]
        public void LogView_Exists()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var logView = root.Q<JLogView>();
            Assert.IsNotNull(logView);
        }

        #endregion

        #region Toggle Callback Tests

        [Test]
        public void JumpToStartupToggle_ExistsInJEngineSettings()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "JEngine Settings");
            var toggle = section?.Q<JToggle>();
            Assert.IsNotNull(toggle);
        }

        [Test]
        public void ClearBuildCacheToggle_ExistsInBuildOptions()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Build Options");
            var toggles = section?.Query<JToggle>().ToList();
            Assert.IsNotNull(toggles);
            Assert.GreaterOrEqual(toggles.Count, 1);
        }

        [Test]
        public void UseAssetDependDBToggle_ExistsInBuildOptions()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Build Options");
            var toggles = section?.Query<JToggle>().ToList();
            Assert.IsNotNull(toggles);
            Assert.GreaterOrEqual(toggles.Count, 2);
        }

        #endregion

        #region Dropdown Tests

        [Test]
        public void PackageDropdown_ExistsInPackageSettings()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Package Settings");
            var dropdown = section?.Q<JDropdown>();
            Assert.IsNotNull(dropdown);
        }

        [Test]
        public void BuildTargetDropdown_ExistsInPackageSettings()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Package Settings");
            var dropdowns = section?.Query(className: "j-dropdown").ToList();
            Assert.IsNotNull(dropdowns);
            Assert.GreaterOrEqual(dropdowns.Count, 2);
        }

        [Test]
        public void EncryptionDropdown_ExistsInBuildOptions()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Build Options");
            var dropdown = section?.Q(className: "j-dropdown");
            Assert.IsNotNull(dropdown);
        }

        #endregion

        #region Encryption Config Tests

        [Test]
        public void EncryptionDropdown_Xor_ShowsCorrectManifestConfig()
        {
            _settings.encryptionOption = EncryptionOption.Xor;
            _ = PanelUI.CreateContent(null, _buildManager, _settings);

            var bundleConfig = EncryptionMapping.GetBundleConfig(EncryptionOption.Xor);
            Assert.IsNotNull(bundleConfig.ManifestConfigScriptableObject);
        }

        [Test]
        public void EncryptionDropdown_Aes_ShowsCorrectManifestConfig()
        {
            _settings.encryptionOption = EncryptionOption.Aes;
            _ = PanelUI.CreateContent(null, _buildManager, _settings);

            var bundleConfig = EncryptionMapping.GetBundleConfig(EncryptionOption.Aes);
            Assert.IsNotNull(bundleConfig.ManifestConfigScriptableObject);
        }

        [Test]
        public void EncryptionDropdown_ChaCha20_ShowsCorrectManifestConfig()
        {
            _settings.encryptionOption = EncryptionOption.ChaCha20;
            _ = PanelUI.CreateContent(null, _buildManager, _settings);

            var bundleConfig = EncryptionMapping.GetBundleConfig(EncryptionOption.ChaCha20);
            Assert.IsNotNull(bundleConfig.ManifestConfigScriptableObject);
        }

        [Test]
        public void EncryptionDropdown_Xor_ShowsCorrectBundleConfig()
        {
            _settings.encryptionOption = EncryptionOption.Xor;
            _ = PanelUI.CreateContent(null, _buildManager, _settings);

            var bundleConfig = EncryptionMapping.GetBundleConfig(EncryptionOption.Xor);
            Assert.IsNotNull(bundleConfig.BundleConfigScriptableObject);
        }

        [Test]
        public void EncryptionDropdown_Aes_ShowsCorrectBundleConfig()
        {
            _settings.encryptionOption = EncryptionOption.Aes;
            _ = PanelUI.CreateContent(null, _buildManager, _settings);

            var bundleConfig = EncryptionMapping.GetBundleConfig(EncryptionOption.Aes);
            Assert.IsNotNull(bundleConfig.BundleConfigScriptableObject);
        }

        [Test]
        public void EncryptionDropdown_ChaCha20_ShowsCorrectBundleConfig()
        {
            _settings.encryptionOption = EncryptionOption.ChaCha20;
            _ = PanelUI.CreateContent(null, _buildManager, _settings);

            var bundleConfig = EncryptionMapping.GetBundleConfig(EncryptionOption.ChaCha20);
            Assert.IsNotNull(bundleConfig.BundleConfigScriptableObject);
        }

        [TestCase(EncryptionOption.Xor)]
        [TestCase(EncryptionOption.Aes)]
        [TestCase(EncryptionOption.ChaCha20)]
        public void EncryptionMapping_AllOptions_ReturnValidConfig(EncryptionOption option)
        {
            var bundleConfig = EncryptionMapping.GetBundleConfig(option);

            Assert.IsNotNull(bundleConfig);
            Assert.IsNotNull(bundleConfig.ManifestConfigScriptableObject);
            Assert.IsNotNull(bundleConfig.BundleConfigScriptableObject);
            Assert.IsNotNull(bundleConfig.ManifestEncryptionConfig);
        }

        #endregion

        #region Package-Specific Tests

        [Test]
        public void MainPackage_UICreation_Succeeds()
        {
            _settings.packageName = "main";
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            Assert.IsNotNull(root);
            Assert.IsNotNull(FindButtonByText(root, "Build All"));
        }

        [Test]
        public void Addon1Package_UICreation_Succeeds()
        {
            _settings.packageName = "addon1";
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            Assert.IsNotNull(root);
            Assert.IsNotNull(FindButtonByText(root, "Assets Only"));
        }

        [Test]
        public void NonMainPackage_NoteLabel_Exists()
        {
            _settings.packageName = "addon1";
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Build Actions");
            var labels = section?.Query<Label>().ToList();

            bool foundNote = false;
            foreach (var label in labels)
            {
                if (label.text.Contains("non-main packages"))
                {
                    foundNote = true;
                    break;
                }
            }

            Assert.IsTrue(foundNote);
        }

        #endregion

        #region Header Tests

        [Test]
        public void Header_ContainsTitle()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var labels = root.Query<Label>().ToList();
            bool foundTitle = false;
            foreach (var label in labels)
            {
                if (label.text == "JEngine Panel")
                {
                    foundTitle = true;
                    break;
                }
            }

            Assert.IsTrue(foundTitle);
        }

        [Test]
        public void Header_ContainsSubtitle()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var labels = root.Query<Label>().ToList();
            bool foundSubtitle = false;
            foreach (var label in labels)
            {
                if (label.text.Contains("Configure settings and build"))
                {
                    foundSubtitle = true;
                    break;
                }
            }

            Assert.IsTrue(foundSubtitle);
        }

        #endregion

        #region Object Field Tests

        [Test]
        public void StartupSceneField_Exists()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "JEngine Settings");
            var objectFields = section?.Query(className: "j-object-field").ToList();
            Assert.IsNotNull(objectFields);
            Assert.GreaterOrEqual(objectFields.Count, 1);
        }

        [Test]
        public void ManifestConfigField_ExistsInBuildOptions()
        {
            var root = PanelUI.CreateContent(null, _buildManager, _settings);

            var section = FindSectionByTitle(root, "Build Options");
            var objectFields = section?.Query(className: "j-object-field").ToList();
            Assert.IsNotNull(objectFields);
            Assert.GreaterOrEqual(objectFields.Count, 2);
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

        private static Button FindButtonByText(VisualElement root, string text)
        {
            var buttons = root.Query<Button>().ToList();
            foreach (var button in buttons)
            {
                if (button.text == text)
                {
                    return button;
                }
            }
            return null;
        }

        #endregion
    }
}
