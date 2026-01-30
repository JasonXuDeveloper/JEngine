// JStatusBarTests.cs
// EditMode unit tests for JStatusBar

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Feedback;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Feedback
{
    [TestFixture]
    public class JStatusBarTests
    {
        private JStatusBar _statusBar;

        [SetUp]
        public void SetUp()
        {
            _statusBar = new JStatusBar("Test message");
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_statusBar.ClassListContains("j-status-bar"));
        }

        [Test]
        public void Constructor_SetsText()
        {
            Assert.AreEqual("Test message", _statusBar.Text);
        }

        [Test]
        public void Constructor_Default_StatusIsInfo()
        {
            var bar = new JStatusBar();
            Assert.AreEqual(StatusType.Info, bar.Status);
        }

        [Test]
        public void Constructor_WithStatus_SetsStatus()
        {
            var bar = new JStatusBar("msg", StatusType.Error);
            Assert.AreEqual(StatusType.Error, bar.Status);
        }

        [Test]
        public void Constructor_SetsRowDirection()
        {
            Assert.AreEqual(FlexDirection.Row, _statusBar.style.flexDirection.value);
        }

        [Test]
        public void Constructor_SetsCenterAlignment()
        {
            Assert.AreEqual(Align.Center, _statusBar.style.alignItems.value);
        }

        [Test]
        public void Constructor_SetsPadding()
        {
            Assert.AreEqual(Tokens.Spacing.MD, _statusBar.style.paddingTop.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, _statusBar.style.paddingRight.value.value);
            Assert.AreEqual(Tokens.Spacing.MD, _statusBar.style.paddingBottom.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, _statusBar.style.paddingLeft.value.value);
        }

        [Test]
        public void Constructor_SetsBorderRadius()
        {
            Assert.AreEqual(Tokens.BorderRadius.MD, _statusBar.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _statusBar.style.borderTopRightRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _statusBar.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _statusBar.style.borderBottomRightRadius.value.value);
        }

        [Test]
        public void Constructor_SetsBottomMargin()
        {
            Assert.AreEqual(Tokens.Spacing.MD, _statusBar.style.marginBottom.value.value);
        }

        [Test]
        public void Constructor_SetsLeftBorderWidth()
        {
            Assert.AreEqual(3f, _statusBar.style.borderLeftWidth.value);
        }

        [Test]
        public void Constructor_CreatesTextLabel()
        {
            Assert.IsNotNull(_statusBar.TextLabel);
        }

        [Test]
        public void Constructor_TextLabelHasCorrectClass()
        {
            Assert.IsTrue(_statusBar.TextLabel.ClassListContains("j-status-bar__text"));
        }

        [Test]
        public void Constructor_TextLabelHasBaseFontSize()
        {
            Assert.AreEqual(Tokens.FontSize.Base, _statusBar.TextLabel.style.fontSize.value.value);
        }

        #endregion

        #region Text Property Tests

        [Test]
        public void Text_Get_ReturnsCurrentText()
        {
            Assert.AreEqual("Test message", _statusBar.Text);
        }

        [Test]
        public void Text_Set_UpdatesText()
        {
            _statusBar.Text = "New message";
            Assert.AreEqual("New message", _statusBar.Text);
        }

        [Test]
        public void Text_Set_Empty_SetsEmptyText()
        {
            _statusBar.Text = "";
            Assert.AreEqual("", _statusBar.Text);
        }

        #endregion

        #region Status Property Tests

        [Test]
        public void Status_Get_ReturnsCurrentStatus()
        {
            var bar = new JStatusBar("msg", StatusType.Warning);
            Assert.AreEqual(StatusType.Warning, bar.Status);
        }

        [Test]
        public void Status_Set_UpdatesStatus()
        {
            _statusBar.Status = StatusType.Error;
            Assert.AreEqual(StatusType.Error, _statusBar.Status);
        }

        #endregion

        #region SetStatus Tests

        [Test]
        public void SetStatus_Info_AddsInfoClass()
        {
            _statusBar.SetStatus(StatusType.Info);
            Assert.IsTrue(_statusBar.ClassListContains("j-status-bar--info"));
        }

        [Test]
        public void SetStatus_Success_AddsSuccessClass()
        {
            _statusBar.SetStatus(StatusType.Success);
            Assert.IsTrue(_statusBar.ClassListContains("j-status-bar--success"));
        }

        [Test]
        public void SetStatus_Warning_AddsWarningClass()
        {
            _statusBar.SetStatus(StatusType.Warning);
            Assert.IsTrue(_statusBar.ClassListContains("j-status-bar--warning"));
        }

        [Test]
        public void SetStatus_Error_AddsErrorClass()
        {
            _statusBar.SetStatus(StatusType.Error);
            Assert.IsTrue(_statusBar.ClassListContains("j-status-bar--error"));
        }

        [Test]
        public void SetStatus_RemovesPreviousStatusClass()
        {
            _statusBar.SetStatus(StatusType.Info);
            _statusBar.SetStatus(StatusType.Error);

            Assert.IsFalse(_statusBar.ClassListContains("j-status-bar--info"));
            Assert.IsTrue(_statusBar.ClassListContains("j-status-bar--error"));
        }

        [Test]
        public void SetStatus_ReturnsStatusBarForChaining()
        {
            var result = _statusBar.SetStatus(StatusType.Success);
            Assert.AreSame(_statusBar, result);
        }

        [Test]
        public void SetStatus_SetsSurfaceBackgroundColor()
        {
            _statusBar.SetStatus(StatusType.Success);
            Assert.AreEqual(Tokens.Colors.BgSurface, _statusBar.style.backgroundColor.value);
        }

        [Test]
        public void SetStatus_SetsLeftBorderColor()
        {
            _statusBar.SetStatus(StatusType.Info);
            Assert.AreEqual(Tokens.Colors.Border, _statusBar.style.borderLeftColor.value);
        }

        #endregion

        #region WithText Tests

        [Test]
        public void WithText_UpdatesText()
        {
            _statusBar.WithText("Updated text");
            Assert.AreEqual("Updated text", _statusBar.Text);
        }

        [Test]
        public void WithText_ReturnsStatusBarForChaining()
        {
            var result = _statusBar.WithText("test");
            Assert.AreSame(_statusBar, result);
        }

        #endregion

        #region TextLabel Property Tests

        [Test]
        public void TextLabel_ReturnsLabelElement()
        {
            Assert.IsInstanceOf<Label>(_statusBar.TextLabel);
        }

        [Test]
        public void TextLabel_IsSameInstanceOnMultipleCalls()
        {
            var label1 = _statusBar.TextLabel;
            var label2 = _statusBar.TextLabel;
            Assert.AreSame(label1, label2);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            _statusBar
                .SetStatus(StatusType.Warning)
                .WithText("Chained message");

            Assert.AreEqual(StatusType.Warning, _statusBar.Status);
            Assert.AreEqual("Chained message", _statusBar.Text);
        }

        #endregion

        #region Inherited JComponent Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _statusBar.WithClass("custom-class");
            Assert.IsTrue(_statusBar.ClassListContains("custom-class"));
        }

        [Test]
        public void WithName_SetsElementName()
        {
            _statusBar.WithName("test-status");
            Assert.AreEqual("test-status", _statusBar.name);
        }

        [Test]
        public void WithVisibility_False_HidesStatusBar()
        {
            _statusBar.WithVisibility(false);
            Assert.AreEqual(DisplayStyle.None, _statusBar.style.display.value);
        }

        #endregion
    }
}
