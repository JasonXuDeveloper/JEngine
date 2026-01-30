// JComponentTests.cs
// EditMode unit tests for JComponent base class

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components;
using JEngine.UI.Editor.Components.Layout;

namespace JEngine.UI.Tests.Editor.Components.Base
{
    [TestFixture]
    public class JComponentTests
    {
        // Using JStack as concrete implementation of JComponent
        private JStack _component;

        [SetUp]
        public void SetUp()
        {
            _component = new JStack();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithBaseClassName_AddsClass()
        {
            // JStack inherits from JComponent with "j-stack" as base class
            Assert.IsTrue(_component.ClassListContains("j-stack"));
        }

        #endregion

        #region WithClass Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _component.WithClass("custom-class");
            Assert.IsTrue(_component.ClassListContains("custom-class"));
        }

        [Test]
        public void WithClass_ReturnsComponentForChaining()
        {
            var result = _component.WithClass("test");
            Assert.AreSame(_component, result);
        }

        [Test]
        public void WithClass_CanAddMultipleClasses()
        {
            _component.WithClass("class1");
            _component.WithClass("class2");

            Assert.IsTrue(_component.ClassListContains("class1"));
            Assert.IsTrue(_component.ClassListContains("class2"));
        }

        #endregion

        #region WithName Tests

        [Test]
        public void WithName_SetsElementName()
        {
            _component.WithName("test-element");
            Assert.AreEqual("test-element", _component.name);
        }

        [Test]
        public void WithName_ReturnsComponentForChaining()
        {
            var result = _component.WithName("test");
            Assert.AreSame(_component, result);
        }

        [Test]
        public void WithName_CanOverwritePreviousName()
        {
            _component.WithName("first");
            _component.WithName("second");
            Assert.AreEqual("second", _component.name);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_SingleChild_AddsToComponent()
        {
            var child = new Label("test");
            _component.Add(child);

            Assert.AreEqual(1, _component.childCount);
            Assert.AreSame(child, _component[0]);
        }

        [Test]
        public void Add_MultipleChildren_AddsAllToComponent()
        {
            var child1 = new Label("test1");
            var child2 = new Label("test2");
            var child3 = new Label("test3");

            _component.Add(child1, child2, child3);

            Assert.AreEqual(3, _component.childCount);
        }

        [Test]
        public void Add_NullChild_IsIgnored()
        {
            _component.Add((VisualElement)null);
            Assert.AreEqual(0, _component.childCount);
        }

        [Test]
        public void Add_MixedNullAndValid_AddsOnlyValidChildren()
        {
            var child1 = new Label("test1");
            var child2 = new Label("test2");

            _component.Add(child1, null, child2);

            Assert.AreEqual(2, _component.childCount);
        }

        [Test]
        public void Add_ReturnsComponentForChaining()
        {
            var result = _component.Add(new Label());
            Assert.AreSame(_component, result);
        }

        #endregion

        #region WithFlexGrow Tests

        [Test]
        public void WithFlexGrow_SetsFlexGrowValue()
        {
            _component.WithFlexGrow(2f);
            Assert.AreEqual(2f, _component.style.flexGrow.value);
        }

        [Test]
        public void WithFlexGrow_ReturnsComponentForChaining()
        {
            var result = _component.WithFlexGrow(1f);
            Assert.AreSame(_component, result);
        }

        [Test]
        public void WithFlexGrow_ZeroValue_SetsToZero()
        {
            _component.WithFlexGrow(0f);
            Assert.AreEqual(0f, _component.style.flexGrow.value);
        }

        #endregion

        #region WithFlexShrink Tests

        [Test]
        public void WithFlexShrink_SetsFlexShrinkValue()
        {
            _component.WithFlexShrink(2f);
            Assert.AreEqual(2f, _component.style.flexShrink.value);
        }

        [Test]
        public void WithFlexShrink_ReturnsComponentForChaining()
        {
            var result = _component.WithFlexShrink(1f);
            Assert.AreSame(_component, result);
        }

        #endregion

        #region WithMargin Tests

        [Test]
        public void WithMargin_SetsAllMargins()
        {
            _component.WithMargin(10f);

            Assert.AreEqual(10f, _component.style.marginTop.value.value);
            Assert.AreEqual(10f, _component.style.marginRight.value.value);
            Assert.AreEqual(10f, _component.style.marginBottom.value.value);
            Assert.AreEqual(10f, _component.style.marginLeft.value.value);
        }

        [Test]
        public void WithMargin_ReturnsComponentForChaining()
        {
            var result = _component.WithMargin(5f);
            Assert.AreSame(_component, result);
        }

        #endregion

        #region WithPadding Tests

        [Test]
        public void WithPadding_SetsAllPadding()
        {
            _component.WithPadding(10f);

            Assert.AreEqual(10f, _component.style.paddingTop.value.value);
            Assert.AreEqual(10f, _component.style.paddingRight.value.value);
            Assert.AreEqual(10f, _component.style.paddingBottom.value.value);
            Assert.AreEqual(10f, _component.style.paddingLeft.value.value);
        }

        [Test]
        public void WithPadding_ReturnsComponentForChaining()
        {
            var result = _component.WithPadding(5f);
            Assert.AreSame(_component, result);
        }

        #endregion

        #region WithVisibility Tests

        [Test]
        public void WithVisibility_True_SetsDisplayFlex()
        {
            _component.WithVisibility(true);
            Assert.AreEqual(DisplayStyle.Flex, _component.style.display.value);
        }

        [Test]
        public void WithVisibility_False_SetsDisplayNone()
        {
            _component.WithVisibility(false);
            Assert.AreEqual(DisplayStyle.None, _component.style.display.value);
        }

        [Test]
        public void WithVisibility_ReturnsComponentForChaining()
        {
            var result = _component.WithVisibility(true);
            Assert.AreSame(_component, result);
        }

        [Test]
        public void WithVisibility_CanToggle()
        {
            _component.WithVisibility(false);
            Assert.AreEqual(DisplayStyle.None, _component.style.display.value);

            _component.WithVisibility(true);
            Assert.AreEqual(DisplayStyle.Flex, _component.style.display.value);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            _component
                .WithName("test")
                .WithClass("custom")
                .WithMargin(5f)
                .WithPadding(10f)
                .WithFlexGrow(1f)
                .WithVisibility(true);

            Assert.AreEqual("test", _component.name);
            Assert.IsTrue(_component.ClassListContains("custom"));
            Assert.AreEqual(5f, _component.style.marginTop.value.value);
            Assert.AreEqual(10f, _component.style.paddingTop.value.value);
            Assert.AreEqual(1f, _component.style.flexGrow.value);
            Assert.AreEqual(DisplayStyle.Flex, _component.style.display.value);
        }

        #endregion
    }
}
