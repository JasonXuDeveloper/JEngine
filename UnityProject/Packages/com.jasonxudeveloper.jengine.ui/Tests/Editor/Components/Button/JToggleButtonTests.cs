// JToggleButtonTests.cs
// EditMode unit tests for JToggleButton

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Button;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Button
{
    [TestFixture]
    public class JToggleButtonTests
    {
        private JToggleButton _toggleButton;

        [SetUp]
        public void SetUp()
        {
            _toggleButton = new JToggleButton("On", "Off");
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_toggleButton.ClassListContains("j-toggle-button"));
        }

        [Test]
        public void Constructor_Default_ValueIsFalse()
        {
            Assert.IsFalse(_toggleButton.Value);
        }

        [Test]
        public void Constructor_Default_DisplaysOffText()
        {
            Assert.AreEqual("Off", _toggleButton.text);
        }

        [Test]
        public void Constructor_WithTrueValue_ValueIsTrue()
        {
            var button = new JToggleButton("On", "Off", true);
            Assert.IsTrue(button.Value);
        }

        [Test]
        public void Constructor_WithTrueValue_DisplaysOnText()
        {
            var button = new JToggleButton("On", "Off", true);
            Assert.AreEqual("On", button.text);
        }

        [Test]
        public void Constructor_DefaultOnVariant_IsSuccess()
        {
            // Create button with true to test on variant
            var button = new JToggleButton("On", "Off", true);
            var successColor = JTheme.GetButtonColor(ButtonVariant.Success);
            Assert.AreEqual(successColor, button.style.backgroundColor.value);
        }

        [Test]
        public void Constructor_DefaultOffVariant_IsDanger()
        {
            var dangerColor = JTheme.GetButtonColor(ButtonVariant.Danger);
            Assert.AreEqual(dangerColor, _toggleButton.style.backgroundColor.value);
        }

        [Test]
        public void Constructor_WithCustomVariants_AppliesCorrectly()
        {
            var button = new JToggleButton("On", "Off", false, ButtonVariant.Primary, ButtonVariant.Secondary);
            var secondaryColor = JTheme.GetButtonColor(ButtonVariant.Secondary);
            Assert.AreEqual(secondaryColor, button.style.backgroundColor.value);
        }

        [Test]
        public void Constructor_WithCallback_RegistersCallback()
        {
            bool callbackInvoked = false;
            var button = new JToggleButton("On", "Off", false, ButtonVariant.Success, ButtonVariant.Danger, _ => callbackInvoked = true);

            button.Value = true;

            Assert.IsTrue(callbackInvoked);
        }

        #endregion

        #region Value Property Tests

        [Test]
        public void Value_Get_ReturnsCurrentValue()
        {
            Assert.IsFalse(_toggleButton.Value);
        }

        [Test]
        public void Value_Set_True_UpdatesValue()
        {
            _toggleButton.Value = true;
            Assert.IsTrue(_toggleButton.Value);
        }

        [Test]
        public void Value_Set_False_UpdatesValue()
        {
            var button = new JToggleButton("On", "Off", true);
            button.Value = false;
            Assert.IsFalse(button.Value);
        }

        [Test]
        public void Value_Set_True_UpdatesText()
        {
            _toggleButton.Value = true;
            Assert.AreEqual("On", _toggleButton.text);
        }

        [Test]
        public void Value_Set_False_UpdatesText()
        {
            var button = new JToggleButton("On", "Off", true);
            button.Value = false;
            Assert.AreEqual("Off", button.text);
        }

        [Test]
        public void Value_Set_True_UpdatesBackgroundColor()
        {
            _toggleButton.Value = true;
            var successColor = JTheme.GetButtonColor(ButtonVariant.Success);
            Assert.AreEqual(successColor, _toggleButton.style.backgroundColor.value);
        }

        [Test]
        public void Value_Set_False_UpdatesBackgroundColor()
        {
            var button = new JToggleButton("On", "Off", true);
            button.Value = false;
            var dangerColor = JTheme.GetButtonColor(ButtonVariant.Danger);
            Assert.AreEqual(dangerColor, button.style.backgroundColor.value);
        }

        #endregion

        #region SetValue Tests

        [Test]
        public void SetValue_WithNotify_InvokesCallback()
        {
            bool callbackInvoked = false;
            _toggleButton.OnValueChanged = _ => callbackInvoked = true;

            _toggleButton.SetValue(true, notify: true);

            Assert.IsTrue(callbackInvoked);
        }

        [Test]
        public void SetValue_WithoutNotify_DoesNotInvokeCallback()
        {
            bool callbackInvoked = false;
            _toggleButton.OnValueChanged = _ => callbackInvoked = true;

            _toggleButton.SetValue(true, notify: false);

            Assert.IsFalse(callbackInvoked);
        }

        [Test]
        public void SetValue_WithoutNotify_StillUpdatesValue()
        {
            _toggleButton.SetValue(true, notify: false);
            Assert.IsTrue(_toggleButton.Value);
        }

        [Test]
        public void SetValue_WithoutNotify_StillUpdatesText()
        {
            _toggleButton.SetValue(true, notify: false);
            Assert.AreEqual("On", _toggleButton.text);
        }

        #endregion

        #region OnValueChanged Property Tests

        [Test]
        public void OnValueChanged_Get_ReturnsCallback()
        {
            System.Action<bool> callback = _ => { };
            _toggleButton.OnValueChanged = callback;

            Assert.AreSame(callback, _toggleButton.OnValueChanged);
        }

        [Test]
        public void OnValueChanged_Set_ReplacesCallback()
        {
            int firstCount = 0;
            int secondCount = 0;

            _toggleButton.OnValueChanged = _ => firstCount++;
            _toggleButton.OnValueChanged = _ => secondCount++;

            _toggleButton.Value = true;

            Assert.AreEqual(0, firstCount);
            Assert.AreEqual(1, secondCount);
        }

        #endregion

        #region WithOnText Tests

        [Test]
        public void WithOnText_UpdatesOnText()
        {
            _toggleButton.WithOnText("Enabled");
            _toggleButton.Value = true;

            Assert.AreEqual("Enabled", _toggleButton.text);
        }

        [Test]
        public void WithOnText_ReturnsButtonForChaining()
        {
            var result = _toggleButton.WithOnText("New");
            Assert.AreSame(_toggleButton, result);
        }

        #endregion

        #region WithOffText Tests

        [Test]
        public void WithOffText_UpdatesOffText()
        {
            _toggleButton.WithOffText("Disabled");
            Assert.AreEqual("Disabled", _toggleButton.text);
        }

        [Test]
        public void WithOffText_ReturnsButtonForChaining()
        {
            var result = _toggleButton.WithOffText("New");
            Assert.AreSame(_toggleButton, result);
        }

        #endregion

        #region WithOnVariant Tests

        [Test]
        public void WithOnVariant_UpdatesOnVariant()
        {
            _toggleButton.WithOnVariant(ButtonVariant.Primary);
            _toggleButton.Value = true;

            var primaryColor = JTheme.GetButtonColor(ButtonVariant.Primary);
            Assert.AreEqual(primaryColor, _toggleButton.style.backgroundColor.value);
        }

        [Test]
        public void WithOnVariant_ReturnsButtonForChaining()
        {
            var result = _toggleButton.WithOnVariant(ButtonVariant.Primary);
            Assert.AreSame(_toggleButton, result);
        }

        #endregion

        #region WithOffVariant Tests

        [Test]
        public void WithOffVariant_UpdatesOffVariant()
        {
            _toggleButton.WithOffVariant(ButtonVariant.Secondary);

            var secondaryColor = JTheme.GetButtonColor(ButtonVariant.Secondary);
            Assert.AreEqual(secondaryColor, _toggleButton.style.backgroundColor.value);
        }

        [Test]
        public void WithOffVariant_ReturnsButtonForChaining()
        {
            var result = _toggleButton.WithOffVariant(ButtonVariant.Secondary);
            Assert.AreSame(_toggleButton, result);
        }

        #endregion

        #region FullWidth Tests

        [Test]
        public void FullWidth_SetsFlexGrow()
        {
            _toggleButton.FullWidth();
            Assert.AreEqual(1f, _toggleButton.style.flexGrow.value);
        }

        [Test]
        public void FullWidth_SetsMaxHeight()
        {
            _toggleButton.FullWidth();
            Assert.AreEqual(24f, _toggleButton.style.maxHeight.value.value);
        }

        [Test]
        public void FullWidth_ReturnsButtonForChaining()
        {
            var result = _toggleButton.FullWidth();
            Assert.AreSame(_toggleButton, result);
        }

        #endregion

        #region WithClass Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _toggleButton.WithClass("custom-class");
            Assert.IsTrue(_toggleButton.ClassListContains("custom-class"));
        }

        [Test]
        public void WithClass_ReturnsButtonForChaining()
        {
            var result = _toggleButton.WithClass("test");
            Assert.AreSame(_toggleButton, result);
        }

        [Test]
        public void WithClass_PreservesBaseClass()
        {
            _toggleButton.WithClass("custom");
            Assert.IsTrue(_toggleButton.ClassListContains("j-toggle-button"));
        }

        #endregion

        #region Style Tests

        [Test]
        public void Constructor_SetsBorderRadius()
        {
            Assert.AreEqual(Tokens.BorderRadius.Sm, _toggleButton.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _toggleButton.style.borderTopRightRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _toggleButton.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _toggleButton.style.borderBottomRightRadius.value.value);
        }

        [Test]
        public void Constructor_SetsZeroMargins()
        {
            Assert.AreEqual(0f, _toggleButton.style.marginLeft.value.value);
            Assert.AreEqual(0f, _toggleButton.style.marginRight.value.value);
            Assert.AreEqual(0f, _toggleButton.style.marginTop.value.value);
            Assert.AreEqual(0f, _toggleButton.style.marginBottom.value.value);
        }

        [Test]
        public void Constructor_SetsMinAndMaxHeight()
        {
            Assert.AreEqual(22f, _toggleButton.style.minHeight.value.value);
            Assert.AreEqual(24f, _toggleButton.style.maxHeight.value.value);
        }

        [Test]
        public void Constructor_SetsZeroBorderWidths()
        {
            Assert.AreEqual(0f, _toggleButton.style.borderTopWidth.value);
            Assert.AreEqual(0f, _toggleButton.style.borderRightWidth.value);
            Assert.AreEqual(0f, _toggleButton.style.borderBottomWidth.value);
            Assert.AreEqual(0f, _toggleButton.style.borderLeftWidth.value);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            var result = _toggleButton
                .WithOnText("Active")
                .WithOffText("Inactive")
                .WithOnVariant(ButtonVariant.Primary)
                .WithOffVariant(ButtonVariant.Secondary)
                .WithClass("custom")
                .FullWidth();

            Assert.AreSame(_toggleButton, result);
            Assert.IsTrue(_toggleButton.ClassListContains("custom"));
            Assert.AreEqual(1f, _toggleButton.style.flexGrow.value);
        }

        #endregion
    }
}
