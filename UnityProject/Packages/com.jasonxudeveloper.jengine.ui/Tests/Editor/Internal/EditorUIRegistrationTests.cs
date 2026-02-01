// EditorUIRegistrationTests.cs
// EditMode unit tests for EditorUIRegistration

using NUnit.Framework;
using JEngine.Core.Editor.CustomEditor;
using JEngine.UI.Editor.Internal;

namespace JEngine.UI.Tests.Editor.Internal
{
    /// <summary>
    /// Tests for <see cref="JEngine.UI.Editor.Internal.EditorUIRegistration"/>.
    /// Since EditorUIRegistration uses [InitializeOnLoad], the static constructor
    /// runs automatically when the editor loads, before these tests execute.
    /// </summary>
    [TestFixture]
    public class EditorUIRegistrationTests
    {
        #region Handler Registration Tests

        [Test]
        public void CreatePanelContentHandler_IsRegistered()
        {
            // EditorUIRegistration sets Panel.CreatePanelContentHandler in its static constructor
            Assert.IsNotNull(Panel.CreatePanelContentHandler,
                "Panel.CreatePanelContentHandler should be registered by EditorUIRegistration");
        }

        [Test]
        public void CreateInspectorHandler_IsRegistered()
        {
            // EditorUIRegistration sets BootstrapEditor.CreateInspectorHandler in its static constructor
            Assert.IsNotNull(BootstrapEditor.CreateInspectorHandler,
                "BootstrapEditor.CreateInspectorHandler should be registered by EditorUIRegistration");
        }

        [Test]
        public void CreatePanelContentHandler_PointsToPanelUI()
        {
            // Verify the handler points to PanelUI.CreateContent
            Assert.AreEqual(
                nameof(PanelUI.CreateContent),
                Panel.CreatePanelContentHandler.Method.Name,
                "CreatePanelContentHandler should point to PanelUI.CreateContent");
            Assert.AreEqual(
                typeof(PanelUI),
                Panel.CreatePanelContentHandler.Method.DeclaringType,
                "CreatePanelContentHandler should be declared in PanelUI");
        }

        [Test]
        public void CreateInspectorHandler_PointsToBootstrapEditorUI()
        {
            // Verify the handler points to BootstrapEditorUI.CreateInspector
            Assert.AreEqual(
                nameof(BootstrapEditorUI.CreateInspector),
                BootstrapEditor.CreateInspectorHandler.Method.Name,
                "CreateInspectorHandler should point to BootstrapEditorUI.CreateInspector");
            Assert.AreEqual(
                typeof(BootstrapEditorUI),
                BootstrapEditor.CreateInspectorHandler.Method.DeclaringType,
                "CreateInspectorHandler should be declared in BootstrapEditorUI");
        }

        #endregion
    }
}
