// JTextFieldTests.cs
// EditMode unit tests for JTextField

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Form;

namespace JEngine.UI.Tests.Editor.Components.Form
{
    [TestFixture]
    public class JTextFieldTests
    {
        private JTextField _textField;

        [SetUp]
        public void SetUp()
        {
            _textField = new JTextField();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_Default_AddsBaseClass()
        {
            Assert.IsTrue(_textField.ClassListContains("j-text-field"));
        }

        [Test]
        public void Constructor_Default_ValueIsEmpty()
        {
            Assert.AreEqual("", _textField.Value);
        }

        [Test]
        public void Constructor_WithInitialValue_SetsValue()
        {
            var field = new JTextField("initial");
            Assert.AreEqual("initial", field.Value);
        }

        [Test]
        public void Constructor_WithEmptyString_SetsEmptyValue()
        {
            var field = new JTextField("");
            Assert.AreEqual("", field.Value);
        }

        [Test]
        public void Constructor_WithPlaceholder_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new JTextField("", "Enter text"));
        }

        [Test]
        public void Constructor_CreatesInternalTextField()
        {
            Assert.IsNotNull(_textField.TextField);
        }

        [Test]
        public void Constructor_SetsFlexGrow()
        {
            Assert.AreEqual(1f, _textField.style.flexGrow.value);
        }

        [Test]
        public void Constructor_SetsFlexShrink()
        {
            Assert.AreEqual(1f, _textField.style.flexShrink.value);
        }

        #endregion

        #region Value Property Tests

        [Test]
        public void Value_Get_ReturnsCurrentValue()
        {
            var field = new JTextField("test");
            Assert.AreEqual("test", field.Value);
        }

        [Test]
        public void Value_Set_UpdatesValue()
        {
            _textField.Value = "new value";
            Assert.AreEqual("new value", _textField.Value);
        }

        [Test]
        public void Value_Set_CanSetMultipleTimes()
        {
            _textField.Value = "first";
            _textField.Value = "second";
            _textField.Value = "third";

            Assert.AreEqual("third", _textField.Value);
        }

        [Test]
        public void Value_Set_EmptyString_SetsEmpty()
        {
            _textField.Value = "something";
            _textField.Value = "";

            Assert.AreEqual("", _textField.Value);
        }

        [Test]
        public void Value_Set_Null_SetsNull()
        {
            _textField.Value = "something";
            _textField.Value = null;

            Assert.IsNull(_textField.Value);
        }

        #endregion

        #region TextField Property Tests

        [Test]
        public void TextField_ReturnsInternalTextField()
        {
            Assert.IsInstanceOf<TextField>(_textField.TextField);
        }

        [Test]
        public void TextField_IsSameInstanceOnMultipleCalls()
        {
            var field1 = _textField.TextField;
            var field2 = _textField.TextField;

            Assert.AreSame(field1, field2);
        }

        [Test]
        public void TextField_ValueSyncsWithWrapper()
        {
            _textField.TextField.value = "synced";
            Assert.AreEqual("synced", _textField.Value);
        }

        #endregion

        #region RegisterValueChangedCallback Tests

        [Test]
        public void RegisterValueChangedCallback_CanRegister()
        {
            // Note: Value changed callbacks may not fire in EditMode when setting value programmatically.
            // We verify the callback can be registered without throwing.
            Assert.DoesNotThrow(() => _textField.RegisterValueChangedCallback(_ => { }));
        }

        [Test]
        public void RegisterValueChangedCallback_CanRegisterWithLambda()
        {
            // Verify registration with a lambda that captures state works
            string captured = null;
            Assert.DoesNotThrow(() => _textField.RegisterValueChangedCallback(evt => captured = evt.newValue));
        }

        [Test]
        public void RegisterValueChangedCallback_MultipleRegistrations_DoNotThrow()
        {
            // Verify multiple registrations work
            Assert.DoesNotThrow(() =>
            {
                _textField.RegisterValueChangedCallback(_ => { });
                _textField.RegisterValueChangedCallback(_ => { });
            });
        }

        #endregion

        #region SetReadOnly Tests

        [Test]
        public void SetReadOnly_True_MakesFieldReadOnly()
        {
            _textField.SetReadOnly(true);
            Assert.IsTrue(_textField.TextField.isReadOnly);
        }

        [Test]
        public void SetReadOnly_False_MakesFieldEditable()
        {
            _textField.SetReadOnly(true);
            _textField.SetReadOnly(false);

            Assert.IsFalse(_textField.TextField.isReadOnly);
        }

        [Test]
        public void SetReadOnly_ReturnsTextFieldForChaining()
        {
            var result = _textField.SetReadOnly(true);
            Assert.AreSame(_textField, result);
        }

        #endregion

        #region SetMultiline Tests

        [Test]
        public void SetMultiline_True_EnablesMultiline()
        {
            _textField.SetMultiline(true);
            Assert.IsTrue(_textField.TextField.multiline);
        }

        [Test]
        public void SetMultiline_False_DisablesMultiline()
        {
            _textField.SetMultiline(true);
            _textField.SetMultiline(false);

            Assert.IsFalse(_textField.TextField.multiline);
        }

        [Test]
        public void SetMultiline_ReturnsTextFieldForChaining()
        {
            var result = _textField.SetMultiline(true);
            Assert.AreSame(_textField, result);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            var result = _textField
                .SetReadOnly(true)
                .SetMultiline(true);

            Assert.AreSame(_textField, result);
            Assert.IsTrue(_textField.TextField.isReadOnly);
            Assert.IsTrue(_textField.TextField.multiline);
        }

        #endregion

        #region Style Tests

        [Test]
        public void Constructor_InternalTextFieldHasFlexGrow()
        {
            Assert.AreEqual(1f, _textField.TextField.style.flexGrow.value);
        }

        [Test]
        public void Constructor_InternalTextFieldHasZeroMargins()
        {
            Assert.AreEqual(0f, _textField.TextField.style.marginLeft.value.value);
            Assert.AreEqual(0f, _textField.TextField.style.marginRight.value.value);
            Assert.AreEqual(0f, _textField.TextField.style.marginTop.value.value);
            Assert.AreEqual(0f, _textField.TextField.style.marginBottom.value.value);
        }

        #endregion
    }
}
