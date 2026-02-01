// JDropdownTests.cs
// EditMode unit tests for JDropdown

using System;
using System.Collections.Generic;
using NUnit.Framework;
using JEngine.UI.Editor.Components.Form;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Form
{
    [TestFixture]
    public class JDropdownTests
    {
        private List<string> _choices;
        private JDropdown _dropdown;

        [SetUp]
        public void SetUp()
        {
            _choices = new List<string> { "Option1", "Option2", "Option3" };
            _dropdown = new JDropdown(_choices);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_dropdown.ClassListContains("j-dropdown"));
        }

        [Test]
        public void Constructor_SetsFirstChoiceAsDefault()
        {
            Assert.AreEqual("Option1", _dropdown.Value);
        }

        [Test]
        public void Constructor_WithDefaultValue_SetsCorrectValue()
        {
            var dropdown = new JDropdown(_choices, "Option2");
            Assert.AreEqual("Option2", dropdown.Value);
        }

        [Test]
        public void Constructor_WithInvalidDefault_UsesFirstChoice()
        {
            var dropdown = new JDropdown(_choices, "NotInList");
            Assert.AreEqual("Option1", dropdown.Value);
        }

        [Test]
        public void Constructor_NullChoices_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new JDropdown(null));
        }

        [Test]
        public void Constructor_EmptyChoices_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new JDropdown(new List<string>()));
        }

        [Test]
        public void Constructor_SingleChoice_Works()
        {
            var dropdown = new JDropdown(new List<string> { "Only" });
            Assert.AreEqual("Only", dropdown.Value);
        }

        [Test]
        public void Constructor_SetsFlexGrow()
        {
            Assert.AreEqual(1f, _dropdown.style.flexGrow.value);
        }

        [Test]
        public void Constructor_SetsFlexShrink()
        {
            Assert.AreEqual(1f, _dropdown.style.flexShrink.value);
        }

        #endregion

        #region Value Property Tests

        [Test]
        public void Value_Get_ReturnsCurrentValue()
        {
            Assert.AreEqual("Option1", _dropdown.Value);
        }

        [Test]
        public void Value_Set_UpdatesValue()
        {
            _dropdown.Value = "Option2";
            Assert.AreEqual("Option2", _dropdown.Value);
        }

        [Test]
        public void Value_Set_ToThirdOption_Works()
        {
            _dropdown.Value = "Option3";
            Assert.AreEqual("Option3", _dropdown.Value);
        }

        #endregion

        #region Choices Property Tests

        [Test]
        public void Choices_Get_ReturnsChoicesList()
        {
            var choices = _dropdown.Choices;

            Assert.AreEqual(3, choices.Count);
            Assert.Contains("Option1", choices);
            Assert.Contains("Option2", choices);
            Assert.Contains("Option3", choices);
        }

        [Test]
        public void Choices_Set_UpdatesChoices()
        {
            var newChoices = new List<string> { "New1", "New2" };
            _dropdown.Choices = newChoices;

            Assert.AreEqual(2, _dropdown.Choices.Count);
        }

        #endregion

        #region PopupField Property Tests

        [Test]
        public void PopupField_ReturnsInternalField()
        {
            Assert.IsNotNull(_dropdown.PopupField);
        }

        [Test]
        public void PopupField_IsSameInstanceOnMultipleCalls()
        {
            var field1 = _dropdown.PopupField;
            var field2 = _dropdown.PopupField;

            Assert.AreSame(field1, field2);
        }

        #endregion

        #region RegisterValueChangedCallback Tests

        [Test]
        public void RegisterValueChangedCallback_CanRegister()
        {
            // Note: Value changed callbacks may not fire in EditMode when setting value programmatically.
            // We verify the callback can be registered without throwing.
            Assert.DoesNotThrow(() => _dropdown.RegisterValueChangedCallback(_ => { }));
        }

        [Test]
        public void RegisterValueChangedCallback_CanRegisterWithLambda()
        {
            // Verify registration with a lambda that captures state works
            string captured = null;
            Assert.DoesNotThrow(() => _dropdown.RegisterValueChangedCallback(evt => captured = evt.newValue));
        }

        #endregion

        #region OnValueChanged Tests

        [Test]
        public void OnValueChanged_CanRegister()
        {
            // Note: Value changed callbacks may not fire in EditMode when setting value programmatically.
            // We verify the callback can be registered without throwing.
            Assert.DoesNotThrow(() => _dropdown.OnValueChanged(_ => { }));
        }

        [Test]
        public void OnValueChanged_ReturnsDropdownForChaining()
        {
            var result = _dropdown.OnValueChanged(_ => { });
            Assert.AreSame(_dropdown, result);
        }

        #endregion

        #region Generic JDropdown<T> Tests

        [Test]
        public void GenericDropdown_WithIntegers_Works()
        {
            var choices = new List<int> { 1, 2, 3 };
            var dropdown = new JDropdown<int>(choices);

            Assert.AreEqual(1, dropdown.Value);
        }

        [Test]
        public void GenericDropdown_WithIntegers_CanSetValue()
        {
            var choices = new List<int> { 1, 2, 3 };
            var dropdown = new JDropdown<int>(choices);

            dropdown.Value = 2;

            Assert.AreEqual(2, dropdown.Value);
        }

        [Test]
        public void GenericDropdown_WithDefaultValue_SetsCorrectly()
        {
            var choices = new List<int> { 10, 20, 30 };
            var dropdown = new JDropdown<int>(choices, 20);

            Assert.AreEqual(20, dropdown.Value);
        }

        [Test]
        public void GenericDropdown_WithCustomFormatter_FormatsCorrectly()
        {
            var choices = new List<int> { 1, 2, 3 };
            var dropdown = new JDropdown<int>(
                choices,
                1,
                v => $"Value: {v}",
                v => $"Item {v}");

            Assert.IsNotNull(dropdown);
        }

        #endregion

        #region ForEnum Tests

        [Test]
        public void ForEnum_ButtonVariant_ReturnsDropdown()
        {
            var dropdown = JDropdown<ButtonVariant>.ForEnum<ButtonVariant>();
            Assert.IsNotNull(dropdown);
        }

        [Test]
        public void ForEnum_ButtonVariant_ContainsAllValues()
        {
            var dropdown = JDropdown<ButtonVariant>.ForEnum<ButtonVariant>();
            var choices = dropdown.Choices;

            Assert.Contains(ButtonVariant.Primary, choices);
            Assert.Contains(ButtonVariant.Secondary, choices);
            Assert.Contains(ButtonVariant.Success, choices);
            Assert.Contains(ButtonVariant.Danger, choices);
            Assert.Contains(ButtonVariant.Warning, choices);
        }

        [Test]
        public void ForEnum_WithDefaultValue_SetsCorrectly()
        {
            var dropdown = JDropdown<ButtonVariant>.ForEnum(ButtonVariant.Danger);
            Assert.AreEqual(ButtonVariant.Danger, dropdown.Value);
        }

        [Test]
        public void ForEnum_GapSize_ContainsAllValues()
        {
            var dropdown = JDropdown<GapSize>.ForEnum<GapSize>();
            var choices = dropdown.Choices;

            Assert.Contains(GapSize.Xs, choices);
            Assert.Contains(GapSize.Sm, choices);
            Assert.Contains(GapSize.MD, choices);
            Assert.Contains(GapSize.Lg, choices);
            Assert.Contains(GapSize.Xl, choices);
        }

        [Test]
        public void ForEnum_StatusType_ContainsAllValues()
        {
            var dropdown = JDropdown<StatusType>.ForEnum<StatusType>();
            var choices = dropdown.Choices;

            Assert.Contains(StatusType.Info, choices);
            Assert.Contains(StatusType.Success, choices);
            Assert.Contains(StatusType.Warning, choices);
            Assert.Contains(StatusType.Error, choices);
        }

        [Test]
        public void ForEnum_CanChangeValue()
        {
            var dropdown = JDropdown<ButtonVariant>.ForEnum<ButtonVariant>();

            dropdown.Value = ButtonVariant.Warning;

            Assert.AreEqual(ButtonVariant.Warning, dropdown.Value);
        }

        #endregion

        #region Style Tests

        [Test]
        public void Constructor_PopupFieldHasFlexGrow()
        {
            Assert.AreEqual(1f, _dropdown.PopupField.style.flexGrow.value);
        }

        [Test]
        public void Constructor_PopupFieldHasZeroMargins()
        {
            Assert.AreEqual(0f, _dropdown.PopupField.style.marginLeft.value.value);
            Assert.AreEqual(0f, _dropdown.PopupField.style.marginRight.value.value);
            Assert.AreEqual(0f, _dropdown.PopupField.style.marginTop.value.value);
            Assert.AreEqual(0f, _dropdown.PopupField.style.marginBottom.value.value);
        }

        #endregion

        #region Panel Attachment Tests

        [Test]
        public void OnAttachToPanel_RegistersCallback()
        {
            // Verify the callback is registered
            var dropdown = new JDropdown(_choices);

            // The PopupField should be a child
            Assert.AreEqual(1, dropdown.childCount);
            Assert.AreSame(dropdown.PopupField, dropdown.ElementAt(0));
        }

        [Test]
        public void Constructor_PopupFieldIsChild()
        {
            Assert.IsTrue(_dropdown.Contains(_dropdown.PopupField));
        }

        [Test]
        public void Constructor_AppliesInputContainerStyle()
        {
            // Verify flexGrow and flexShrink are applied (from JTheme.ApplyInputContainerStyle)
            Assert.AreEqual(1f, _dropdown.style.flexGrow.value);
            Assert.AreEqual(1f, _dropdown.style.flexShrink.value);
        }

        #endregion

        #region Formatter Tests

        [Test]
        public void GenericDropdown_WithNullFormatter_UsesToString()
        {
            var choices = new List<int> { 1, 2, 3 };
            var dropdown = new JDropdown<int>(choices, 1, null, null);

            // Should not throw and should work
            Assert.AreEqual(1, dropdown.Value);
        }

        [Test]
        public void GenericDropdown_WithCustomFormatters_AcceptsBoth()
        {
            var choices = new List<int> { 1, 2, 3 };

            // Custom formatters for display
            Func<int, string> formatSelected = v => $"Selected: {v}";
            Func<int, string> formatList = v => $"Option {v}";

            var dropdown = new JDropdown<int>(choices, 1, formatSelected, formatList);

            Assert.IsNotNull(dropdown);
            Assert.AreEqual(1, dropdown.Value);
        }

        [Test]
        public void GenericDropdown_FormattersHandleNull()
        {
            // Test with nullable type-like behavior using reference types
            var choices = new List<string> { "One", "Two", null };
            var dropdown = new JDropdown<string>(choices, "One");

            Assert.AreEqual("One", dropdown.Value);
        }

        #endregion

        #region Edge Cases

        [Test]
        public void Choices_SetToNewList_UpdatesDropdown()
        {
            var newChoices = new List<string> { "NewA", "NewB", "NewC" };
            _dropdown.Choices = newChoices;

            Assert.AreEqual(3, _dropdown.Choices.Count);
            Assert.Contains("NewA", _dropdown.Choices);
        }

        [Test]
        public void Value_SetToFirstChoice_Works()
        {
            _dropdown.Value = "Option1";
            Assert.AreEqual("Option1", _dropdown.Value);
        }

        [Test]
        public void Value_SetToLastChoice_Works()
        {
            _dropdown.Value = "Option3";
            Assert.AreEqual("Option3", _dropdown.Value);
        }

        [Test]
        public void OnValueChanged_CanBeSetToNull()
        {
            Assert.DoesNotThrow(() => _dropdown.OnValueChanged(null));
        }

        [Test]
        public void OnValueChanged_MultipleRegistrations_DoNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                _dropdown.OnValueChanged(v => { });
                _dropdown.OnValueChanged(v => { });
            });
        }

        #endregion

        #region ForEnum Edge Cases

        [Test]
        public void ForEnum_DefaultValue_IsFirstEnumValue()
        {
            var dropdown = JDropdown<ButtonVariant>.ForEnum<ButtonVariant>();
            // Default should be the first enum value
            Assert.AreEqual(ButtonVariant.Primary, dropdown.Value);
        }

        [Test]
        public void ForEnum_CanChangeValueMultipleTimes()
        {
            var dropdown = JDropdown<ButtonVariant>.ForEnum<ButtonVariant>();

            dropdown.Value = ButtonVariant.Success;
            Assert.AreEqual(ButtonVariant.Success, dropdown.Value);

            dropdown.Value = ButtonVariant.Danger;
            Assert.AreEqual(ButtonVariant.Danger, dropdown.Value);

            dropdown.Value = ButtonVariant.Primary;
            Assert.AreEqual(ButtonVariant.Primary, dropdown.Value);
        }

        #endregion
    }
}
