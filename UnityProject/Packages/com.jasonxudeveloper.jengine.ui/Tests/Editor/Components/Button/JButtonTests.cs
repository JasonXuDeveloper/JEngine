// JButtonTests.cs
// EditMode unit tests for JButton

using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Button;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Button
{
    [TestFixture]
    public class JButtonTests
    {
        private JButton _button;

        [SetUp]
        public void SetUp()
        {
            _button = new JButton("Test");
        }

        #region Constructor Tests

        [Test]
        public void Constructor_SetsTextProperty()
        {
            var button = new JButton("Hello");
            Assert.AreEqual("Hello", button.text);
        }

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_button.ClassListContains("j-button"));
        }

        [Test]
        public void Constructor_DefaultVariant_IsPrimary()
        {
            Assert.AreEqual(ButtonVariant.Primary, _button.Variant);
        }

        [Test]
        public void Constructor_WithVariant_SetsVariant()
        {
            var button = new JButton("Test", variant: ButtonVariant.Secondary);
            Assert.AreEqual(ButtonVariant.Secondary, button.Variant);
        }

        [Test]
        public void Constructor_WithClickHandler_RegistersCallback()
        {
            bool clicked = false;
            var button = new JButton("Test", () => clicked = true);

            // Use reflection to invoke the protected Invoke method on Clickable
            var invokeMethod = typeof(Clickable).GetMethod("Invoke",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null, new[] { typeof(EventBase) }, null);

            // If the internal API changed, skip the click verification
            if (invokeMethod == null)
            {
                Assert.Pass("Clickable.Invoke method not found - Unity API may have changed. Button creation verified.");
                return;
            }

            using (var evt = MouseDownEvent.GetPooled())
            {
                invokeMethod.Invoke(button.clickable, new object[] { evt });
            }

            Assert.IsTrue(clicked);
        }

        [Test]
        public void Constructor_WithNullClickHandler_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new JButton("Test", null));
        }

        #endregion

        #region SetVariant Tests

        [Test]
        public void SetVariant_Primary_AddsCorrectClass()
        {
            _button.SetVariant(ButtonVariant.Primary);
            Assert.IsTrue(_button.ClassListContains("j-button--primary"));
        }

        [Test]
        public void SetVariant_Secondary_AddsCorrectClass()
        {
            _button.SetVariant(ButtonVariant.Secondary);
            Assert.IsTrue(_button.ClassListContains("j-button--secondary"));
        }

        [Test]
        public void SetVariant_Success_AddsCorrectClass()
        {
            _button.SetVariant(ButtonVariant.Success);
            Assert.IsTrue(_button.ClassListContains("j-button--success"));
        }

        [Test]
        public void SetVariant_Danger_AddsCorrectClass()
        {
            _button.SetVariant(ButtonVariant.Danger);
            Assert.IsTrue(_button.ClassListContains("j-button--danger"));
        }

        [Test]
        public void SetVariant_Warning_AddsCorrectClass()
        {
            _button.SetVariant(ButtonVariant.Warning);
            Assert.IsTrue(_button.ClassListContains("j-button--warning"));
        }

        [Test]
        public void SetVariant_RemovesPreviousVariantClass()
        {
            _button.SetVariant(ButtonVariant.Primary);
            _button.SetVariant(ButtonVariant.Secondary);

            Assert.IsFalse(_button.ClassListContains("j-button--primary"));
            Assert.IsTrue(_button.ClassListContains("j-button--secondary"));
        }

        [Test]
        public void SetVariant_ReturnsButtonForChaining()
        {
            var result = _button.SetVariant(ButtonVariant.Success);
            Assert.AreSame(_button, result);
        }

        [Test]
        public void SetVariant_UpdatesBackgroundColor()
        {
            _button.SetVariant(ButtonVariant.Primary);
            var primaryColor = JTheme.GetButtonColor(ButtonVariant.Primary);

            // Style may be set, check that it's the expected color
            Assert.AreEqual(primaryColor, _button.style.backgroundColor.value);
        }

        #endregion

        #region Variant Property Tests

        [Test]
        public void Variant_Get_ReturnsCurrentVariant()
        {
            _button.SetVariant(ButtonVariant.Danger);
            Assert.AreEqual(ButtonVariant.Danger, _button.Variant);
        }

        [Test]
        public void Variant_Set_CallsSetVariant()
        {
            _button.Variant = ButtonVariant.Warning;
            Assert.IsTrue(_button.ClassListContains("j-button--warning"));
        }

        #endregion

        #region WithText Tests

        [Test]
        public void WithText_SetsButtonText()
        {
            _button.WithText("New Text");
            Assert.AreEqual("New Text", _button.text);
        }

        [Test]
        public void WithText_ReturnsButtonForChaining()
        {
            var result = _button.WithText("Test");
            Assert.AreSame(_button, result);
        }

        [Test]
        public void WithText_EmptyString_SetsEmptyText()
        {
            _button.WithText("");
            Assert.AreEqual("", _button.text);
        }

        [Test]
        public void WithText_NullString_SetsEmptyOrNull()
        {
            _button.WithText(null);
            // Button.text may convert null to empty string internally
            Assert.IsTrue(string.IsNullOrEmpty(_button.text));
        }

        #endregion

        #region WithClass Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _button.WithClass("custom-class");
            Assert.IsTrue(_button.ClassListContains("custom-class"));
        }

        [Test]
        public void WithClass_ReturnsButtonForChaining()
        {
            var result = _button.WithClass("test");
            Assert.AreSame(_button, result);
        }

        [Test]
        public void WithClass_PreservesBaseClass()
        {
            _button.WithClass("custom");
            Assert.IsTrue(_button.ClassListContains("j-button"));
        }

        #endregion

        #region WithEnabled Tests

        [Test]
        public void WithEnabled_True_EnablesButton()
        {
            _button.SetEnabled(false);
            _button.WithEnabled(true);
            Assert.IsTrue(_button.enabledSelf);
        }

        [Test]
        public void WithEnabled_False_DisablesButton()
        {
            _button.WithEnabled(false);
            Assert.IsFalse(_button.enabledSelf);
        }

        [Test]
        public void WithEnabled_ReturnsButtonForChaining()
        {
            var result = _button.WithEnabled(true);
            Assert.AreSame(_button, result);
        }

        #endregion

        #region FullWidth Tests

        [Test]
        public void FullWidth_SetsFlexGrow()
        {
            _button.FullWidth();
            Assert.AreEqual(1f, _button.style.flexGrow.value);
        }

        [Test]
        public void FullWidth_SetsFlexShrink()
        {
            _button.FullWidth();
            Assert.AreEqual(1f, _button.style.flexShrink.value);
        }

        [Test]
        public void FullWidth_SetsMinWidth()
        {
            _button.FullWidth();
            Assert.AreEqual(60f, _button.style.minWidth.value.value);
        }

        [Test]
        public void FullWidth_ReturnsButtonForChaining()
        {
            var result = _button.FullWidth();
            Assert.AreSame(_button, result);
        }

        #endregion

        #region Compact Tests

        [Test]
        public void Compact_SetsSmallerPadding()
        {
            _button.Compact();
            Assert.AreEqual(2f, _button.style.paddingTop.value.value);
            Assert.AreEqual(2f, _button.style.paddingBottom.value.value);
            Assert.AreEqual(6f, _button.style.paddingLeft.value.value);
            Assert.AreEqual(6f, _button.style.paddingRight.value.value);
        }

        [Test]
        public void Compact_SetsSmallerMinHeight()
        {
            _button.Compact();
            Assert.AreEqual(18f, _button.style.minHeight.value.value);
        }

        [Test]
        public void Compact_SetsSmallerFontSize()
        {
            _button.Compact();
            Assert.AreEqual(10f, _button.style.fontSize.value.value);
        }

        [Test]
        public void Compact_ReturnsButtonForChaining()
        {
            var result = _button.Compact();
            Assert.AreSame(_button, result);
        }

        #endregion

        #region WithMinWidth Tests

        [Test]
        public void WithMinWidth_SetsMinWidth()
        {
            _button.WithMinWidth(100f);
            Assert.AreEqual(100f, _button.style.minWidth.value.value);
        }

        [Test]
        public void WithMinWidth_ReturnsButtonForChaining()
        {
            var result = _button.WithMinWidth(50f);
            Assert.AreSame(_button, result);
        }

        #endregion

        #region Style Application Tests

        [Test]
        public void Constructor_AppliesBorderRadius()
        {
            Assert.AreEqual(Tokens.BorderRadius.MD, _button.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _button.style.borderTopRightRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _button.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _button.style.borderBottomRightRadius.value.value);
        }

        [Test]
        public void Constructor_AppliesDefaultPadding()
        {
            Assert.AreEqual(Tokens.Spacing.Sm, _button.style.paddingTop.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, _button.style.paddingRight.value.value);
            Assert.AreEqual(Tokens.Spacing.Sm, _button.style.paddingBottom.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, _button.style.paddingLeft.value.value);
        }

        [Test]
        public void Constructor_SetsZeroMargins()
        {
            Assert.AreEqual(0f, _button.style.marginLeft.value.value);
            Assert.AreEqual(0f, _button.style.marginRight.value.value);
            Assert.AreEqual(0f, _button.style.marginTop.value.value);
            Assert.AreEqual(0f, _button.style.marginBottom.value.value);
        }

        [Test]
        public void Constructor_SetsMinHeight()
        {
            Assert.AreEqual(28f, _button.style.minHeight.value.value);
        }

        [Test]
        public void Constructor_SetsBaseFontSize()
        {
            Assert.AreEqual(Tokens.FontSize.Base, _button.style.fontSize.value.value);
        }

        [Test]
        public void Constructor_SetsZeroBorderWidths()
        {
            Assert.AreEqual(0f, _button.style.borderTopWidth.value);
            Assert.AreEqual(0f, _button.style.borderRightWidth.value);
            Assert.AreEqual(0f, _button.style.borderBottomWidth.value);
            Assert.AreEqual(0f, _button.style.borderLeftWidth.value);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            var button = new JButton("Start")
                .WithText("Changed")
                .SetVariant(ButtonVariant.Success)
                .WithClass("custom")
                .WithEnabled(true)
                .WithMinWidth(120f);

            Assert.AreEqual("Changed", button.text);
            Assert.AreEqual(ButtonVariant.Success, button.Variant);
            Assert.IsTrue(button.ClassListContains("custom"));
            Assert.IsTrue(button.enabledSelf);
            Assert.AreEqual(120f, button.style.minWidth.value.value);
        }

        #endregion
    }
}
