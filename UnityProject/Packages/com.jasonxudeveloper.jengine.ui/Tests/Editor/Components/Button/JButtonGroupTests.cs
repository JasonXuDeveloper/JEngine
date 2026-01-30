// JButtonGroupTests.cs
// EditMode unit tests for JButtonGroup

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Button;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Button
{
    [TestFixture]
    public class JButtonGroupTests
    {
        private JButtonGroup _buttonGroup;

        [SetUp]
        public void SetUp()
        {
            _buttonGroup = new JButtonGroup();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_Empty_AddsBaseClass()
        {
            Assert.IsTrue(_buttonGroup.ClassListContains("j-button-group"));
        }

        [Test]
        public void Constructor_Empty_HasNoChildren()
        {
            Assert.AreEqual(0, _buttonGroup.childCount);
        }

        [Test]
        public void Constructor_SetsRowDirection()
        {
            Assert.AreEqual(FlexDirection.Row, _buttonGroup.style.flexDirection.value);
        }

        [Test]
        public void Constructor_SetsFlexWrap()
        {
            Assert.AreEqual(Wrap.Wrap, _buttonGroup.style.flexWrap.value);
        }

        [Test]
        public void Constructor_SetsCenterAlignment()
        {
            Assert.AreEqual(Align.Center, _buttonGroup.style.alignItems.value);
        }

        [Test]
        public void Constructor_WithButtons_AddsAllButtons()
        {
            var btn1 = new JButton("Button 1");
            var btn2 = new JButton("Button 2");
            var group = new JButtonGroup(btn1, btn2);

            Assert.AreEqual(2, group.childCount);
        }

        [Test]
        public void Constructor_WithButtons_SetsRightMargin()
        {
            var btn1 = new JButton("Button 1");
            var btn2 = new JButton("Button 2");
            var group = new JButtonGroup(btn1, btn2);

            Assert.AreEqual(Tokens.Spacing.Sm, group[0].style.marginRight.value.value);
        }

        [Test]
        public void Constructor_WithButtons_LastButtonHasNoRightMargin()
        {
            var btn1 = new JButton("Button 1");
            var btn2 = new JButton("Button 2");
            var group = new JButtonGroup(btn1, btn2);

            Assert.AreEqual(0f, group[1].style.marginRight.value.value);
        }

        [Test]
        public void Constructor_WithButtons_SetsBottomMargin()
        {
            var btn1 = new JButton("Button 1");
            var group = new JButtonGroup(btn1);

            Assert.AreEqual(Tokens.Spacing.Xs, group[0].style.marginBottom.value.value);
        }

        [Test]
        public void Constructor_WithButtons_SetsFlexGrow()
        {
            var btn1 = new JButton("Button 1");
            var group = new JButtonGroup(btn1);

            Assert.AreEqual(1f, group[0].style.flexGrow.value);
        }

        [Test]
        public void Constructor_WithButtons_SetsFlexShrinkZero()
        {
            var btn1 = new JButton("Button 1");
            var group = new JButtonGroup(btn1);

            Assert.AreEqual(0f, group[0].style.flexShrink.value);
        }

        [Test]
        public void Constructor_WithButtons_SetsMinWidth()
        {
            var btn1 = new JButton("Button 1");
            var group = new JButtonGroup(btn1);

            Assert.AreEqual(100f, group[0].style.minWidth.value.value);
        }

        [Test]
        public void Constructor_WithNullButton_IgnoresNull()
        {
            var btn1 = new JButton("Button 1");
            var group = new JButtonGroup(btn1, null);

            Assert.AreEqual(1, group.childCount);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_SingleButton_AddsToGroup()
        {
            var btn = new JButton("Test");
            _buttonGroup.Add(btn);

            Assert.AreEqual(1, _buttonGroup.childCount);
        }

        [Test]
        public void Add_MultipleButtons_AddsAllToGroup()
        {
            var btn1 = new JButton("Button 1");
            var btn2 = new JButton("Button 2");

            _buttonGroup.Add(btn1, btn2);

            Assert.AreEqual(2, _buttonGroup.childCount);
        }

        [Test]
        public void Add_ReturnsGroupForChaining()
        {
            var result = _buttonGroup.Add(new JButton("Test"));
            Assert.AreSame(_buttonGroup, result);
        }

        [Test]
        public void Add_NullButton_IsIgnored()
        {
            _buttonGroup.Add((VisualElement)null);
            Assert.AreEqual(0, _buttonGroup.childCount);
        }

        [Test]
        public void Add_SetsMargins()
        {
            var btn1 = new JButton("Button 1");
            var btn2 = new JButton("Button 2");

            _buttonGroup.Add(btn1, btn2);

            Assert.AreEqual(Tokens.Spacing.Sm, _buttonGroup[0].style.marginRight.value.value);
            Assert.AreEqual(0f, _buttonGroup[1].style.marginRight.value.value);
        }

        [Test]
        public void Add_UpdatesMargins_WhenAddingMore()
        {
            var btn1 = new JButton("Button 1");
            _buttonGroup.Add(btn1);
            Assert.AreEqual(0f, _buttonGroup[0].style.marginRight.value.value);

            var btn2 = new JButton("Button 2");
            _buttonGroup.Add(btn2);

            Assert.AreEqual(Tokens.Spacing.Sm, _buttonGroup[0].style.marginRight.value.value);
            Assert.AreEqual(0f, _buttonGroup[1].style.marginRight.value.value);
        }

        [Test]
        public void Add_CanChainMultipleAddCalls()
        {
            _buttonGroup
                .Add(new JButton("1"))
                .Add(new JButton("2"))
                .Add(new JButton("3"));

            Assert.AreEqual(3, _buttonGroup.childCount);
        }

        #endregion

        #region NoWrap Tests

        [Test]
        public void NoWrap_SetsNoWrap()
        {
            _buttonGroup.NoWrap();
            Assert.AreEqual(Wrap.NoWrap, _buttonGroup.style.flexWrap.value);
        }

        [Test]
        public void NoWrap_ReturnsGroupForChaining()
        {
            var result = _buttonGroup.NoWrap();
            Assert.AreSame(_buttonGroup, result);
        }

        #endregion

        #region FixedWidth Tests

        [Test]
        public void FixedWidth_SetsFlexGrowToZero()
        {
            var btn = new JButton("Test");
            _buttonGroup.Add(btn);

            _buttonGroup.FixedWidth();

            Assert.AreEqual(0f, _buttonGroup[0].style.flexGrow.value);
        }

        [Test]
        public void FixedWidth_SetsFlexBasisAuto()
        {
            var btn = new JButton("Test");
            _buttonGroup.Add(btn);

            _buttonGroup.FixedWidth();

            Assert.AreEqual(StyleKeyword.Auto, _buttonGroup[0].style.flexBasis.keyword);
        }

        [Test]
        public void FixedWidth_AffectsAllChildren()
        {
            var btn1 = new JButton("Button 1");
            var btn2 = new JButton("Button 2");
            _buttonGroup.Add(btn1, btn2);

            _buttonGroup.FixedWidth();

            Assert.AreEqual(0f, _buttonGroup[0].style.flexGrow.value);
            Assert.AreEqual(0f, _buttonGroup[1].style.flexGrow.value);
        }

        [Test]
        public void FixedWidth_ReturnsGroupForChaining()
        {
            var result = _buttonGroup.FixedWidth();
            Assert.AreSame(_buttonGroup, result);
        }

        #endregion

        #region Inherited JComponent Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _buttonGroup.WithClass("custom-class");
            Assert.IsTrue(_buttonGroup.ClassListContains("custom-class"));
        }

        [Test]
        public void WithClass_PreservesBaseClass()
        {
            _buttonGroup.WithClass("custom");
            Assert.IsTrue(_buttonGroup.ClassListContains("j-button-group"));
        }

        [Test]
        public void WithName_SetsElementName()
        {
            _buttonGroup.WithName("test-group");
            Assert.AreEqual("test-group", _buttonGroup.name);
        }

        [Test]
        public void WithVisibility_False_HidesGroup()
        {
            _buttonGroup.WithVisibility(false);
            Assert.AreEqual(DisplayStyle.None, _buttonGroup.style.display.value);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            // JButtonGroup-specific methods chain together
            _buttonGroup
                .Add(new JButton("1"))
                .Add(new JButton("2"))
                .NoWrap()
                .FixedWidth();

            // JComponent methods called separately (they return JComponent)
            _buttonGroup.WithName("my-group");
            _buttonGroup.WithClass("custom");

            Assert.AreEqual("my-group", _buttonGroup.name);
            Assert.IsTrue(_buttonGroup.ClassListContains("custom"));
            Assert.AreEqual(2, _buttonGroup.childCount);
            Assert.AreEqual(Wrap.NoWrap, _buttonGroup.style.flexWrap.value);
            Assert.AreEqual(0f, _buttonGroup[0].style.flexGrow.value);
        }

        #endregion
    }
}
