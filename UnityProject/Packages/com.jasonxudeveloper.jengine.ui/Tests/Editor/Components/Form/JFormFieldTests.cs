// JFormFieldTests.cs
// EditMode unit tests for JFormField

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Form;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Form
{
    [TestFixture]
    public class JFormFieldTests
    {
        private JFormField _formField;

        [SetUp]
        public void SetUp()
        {
            _formField = new JFormField("Test Label");
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_formField.ClassListContains("j-form-field"));
        }

        [Test]
        public void Constructor_SetsRowDirection()
        {
            Assert.AreEqual(FlexDirection.Row, _formField.style.flexDirection.value);
        }

        [Test]
        public void Constructor_SetsCenterAlignment()
        {
            Assert.AreEqual(Align.Center, _formField.style.alignItems.value);
        }

        [Test]
        public void Constructor_SetsNoWrap()
        {
            Assert.AreEqual(Wrap.NoWrap, _formField.style.flexWrap.value);
        }

        [Test]
        public void Constructor_SetsBottomMargin()
        {
            Assert.AreEqual(Tokens.Spacing.Sm, _formField.style.marginBottom.value.value);
        }

        [Test]
        public void Constructor_SetsMinAndMaxHeight()
        {
            Assert.AreEqual(22f, _formField.style.minHeight.value.value);
            Assert.AreEqual(28f, _formField.style.maxHeight.value.value);
        }

        [Test]
        public void Constructor_SetsOverflowHidden()
        {
            Assert.AreEqual(Overflow.Hidden, _formField.style.overflow.value);
        }

        [Test]
        public void Constructor_CreatesLabel()
        {
            Assert.IsNotNull(_formField.Label);
        }

        [Test]
        public void Constructor_SetsLabelText()
        {
            Assert.AreEqual("Test Label", _formField.Label.text);
        }

        [Test]
        public void Constructor_LabelHasCorrectClass()
        {
            Assert.IsTrue(_formField.Label.ClassListContains("j-form-field__label"));
        }

        [Test]
        public void Constructor_SetsLabelWidth()
        {
            Assert.AreEqual(Tokens.Layout.FormLabelWidth, _formField.Label.style.width.value.value);
        }

        [Test]
        public void Constructor_SetsLabelMinWidth()
        {
            Assert.AreEqual(Tokens.Layout.FormLabelMinWidth, _formField.Label.style.minWidth.value.value);
        }

        [Test]
        public void Constructor_CreatesControlContainer()
        {
            Assert.IsNotNull(_formField.ControlContainer);
        }

        [Test]
        public void Constructor_ControlContainerHasCorrectClass()
        {
            Assert.IsTrue(_formField.ControlContainer.ClassListContains("j-form-field__control"));
        }

        [Test]
        public void Constructor_ControlContainerHasFlexGrow()
        {
            Assert.AreEqual(1f, _formField.ControlContainer.style.flexGrow.value);
        }

        [Test]
        public void Constructor_WithControl_AddsControlToContainer()
        {
            var control = new TextField();
            var field = new JFormField("Label", control);

            Assert.AreEqual(1, field.ControlContainer.childCount);
        }

        [Test]
        public void Constructor_WithNullControl_DoesNotAddAnything()
        {
            var field = new JFormField("Label", null);
            Assert.AreEqual(0, field.ControlContainer.childCount);
        }

        #endregion

        #region Label Property Tests

        [Test]
        public void Label_ReturnsLabelElement()
        {
            Assert.IsInstanceOf<Label>(_formField.Label);
        }

        [Test]
        public void Label_IsSameInstanceOnMultipleCalls()
        {
            var label1 = _formField.Label;
            var label2 = _formField.Label;
            Assert.AreSame(label1, label2);
        }

        #endregion

        #region ControlContainer Property Tests

        [Test]
        public void ControlContainer_ReturnsVisualElement()
        {
            Assert.IsInstanceOf<VisualElement>(_formField.ControlContainer);
        }

        [Test]
        public void ControlContainer_IsSameInstanceOnMultipleCalls()
        {
            var container1 = _formField.ControlContainer;
            var container2 = _formField.ControlContainer;
            Assert.AreSame(container1, container2);
        }

        #endregion

        #region WithControl Tests

        [Test]
        public void WithControl_AddsControlToContainer()
        {
            var control = new TextField();
            _formField.WithControl(control);

            Assert.AreEqual(1, _formField.ControlContainer.childCount);
        }

        [Test]
        public void WithControl_ClearsPreviousControl()
        {
            _formField.WithControl(new TextField());
            _formField.WithControl(new TextField());

            Assert.AreEqual(1, _formField.ControlContainer.childCount);
        }

        [Test]
        public void WithControl_NullControl_ClearsContainer()
        {
            _formField.WithControl(new TextField());
            _formField.WithControl(null);

            Assert.AreEqual(0, _formField.ControlContainer.childCount);
        }

        [Test]
        public void WithControl_ReturnsFormFieldForChaining()
        {
            var result = _formField.WithControl(new TextField());
            Assert.AreSame(_formField, result);
        }

        #endregion

        #region WithLabelWidth Tests

        [Test]
        public void WithLabelWidth_SetsLabelWidth()
        {
            _formField.WithLabelWidth(200f);
            Assert.AreEqual(200f, _formField.Label.style.width.value.value);
        }

        [Test]
        public void WithLabelWidth_SetsLabelMinWidth()
        {
            _formField.WithLabelWidth(200f);
            Assert.AreEqual(200f, _formField.Label.style.minWidth.value.value);
        }

        [Test]
        public void WithLabelWidth_ReturnsFormFieldForChaining()
        {
            var result = _formField.WithLabelWidth(100f);
            Assert.AreSame(_formField, result);
        }

        #endregion

        #region NoLabel Tests

        [Test]
        public void NoLabel_HidesLabel()
        {
            _formField.NoLabel();
            Assert.AreEqual(DisplayStyle.None, _formField.Label.style.display.value);
        }

        [Test]
        public void NoLabel_ReturnsFormFieldForChaining()
        {
            var result = _formField.NoLabel();
            Assert.AreSame(_formField, result);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_SingleChild_AddsToControlContainer()
        {
            var child = new Label("test");
            _formField.Add(child);

            Assert.AreEqual(1, _formField.ControlContainer.childCount);
        }

        [Test]
        public void Add_MultipleChildren_AddsAllToContainer()
        {
            var child1 = new Label("test1");
            var child2 = new Label("test2");

            _formField.Add(child1, child2);

            Assert.AreEqual(2, _formField.ControlContainer.childCount);
        }

        [Test]
        public void Add_NullChild_IsIgnored()
        {
            _formField.Add((VisualElement)null);
            Assert.AreEqual(0, _formField.ControlContainer.childCount);
        }

        [Test]
        public void Add_ReturnsFormFieldForChaining()
        {
            var result = _formField.Add(new Label());
            Assert.AreSame(_formField, result);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            var control = new TextField();
            _formField
                .WithLabelWidth(160f)
                .WithControl(control)
                .Add(new Label("extra"));

            Assert.AreEqual(160f, _formField.Label.style.width.value.value);
            Assert.AreEqual(2, _formField.ControlContainer.childCount);
        }

        #endregion

        #region Inheritance Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _formField.WithClass("custom-class");
            Assert.IsTrue(_formField.ClassListContains("custom-class"));
        }

        [Test]
        public void WithName_SetsElementName()
        {
            _formField.WithName("test-field");
            Assert.AreEqual("test-field", _formField.name);
        }

        [Test]
        public void WithVisibility_False_HidesFormField()
        {
            _formField.WithVisibility(false);
            Assert.AreEqual(DisplayStyle.None, _formField.style.display.value);
        }

        #endregion
    }
}
