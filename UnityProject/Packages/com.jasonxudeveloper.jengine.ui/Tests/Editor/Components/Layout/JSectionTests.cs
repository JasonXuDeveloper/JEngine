// JSectionTests.cs
// EditMode unit tests for JSection

using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Layout;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Layout
{
    [TestFixture]
    public class JSectionTests
    {
        private JSection _section;

        [SetUp]
        public void SetUp()
        {
            _section = new JSection("Test Section");
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_section.ClassListContains("j-section"));
        }

        [Test]
        public void Constructor_CreatesHeader()
        {
            Assert.IsNotNull(_section.Header);
        }

        [Test]
        public void Constructor_SetsHeaderText()
        {
            Assert.AreEqual("Test Section", _section.Header.text);
        }

        [Test]
        public void Constructor_HeaderHasCorrectClass()
        {
            Assert.IsTrue(_section.Header.ClassListContains("j-section__header"));
        }

        [Test]
        public void Constructor_CreatesContent()
        {
            Assert.IsNotNull(_section.Content);
        }

        [Test]
        public void Constructor_ContentHasCorrectClass()
        {
            Assert.IsTrue(_section.Content.ClassListContains("j-section__content"));
        }

        [Test]
        public void Constructor_SetsSurfaceBackgroundColor()
        {
            Assert.AreEqual(Tokens.Colors.BgSurface, _section.style.backgroundColor.value);
        }

        [Test]
        public void Constructor_SetsBorderRadius()
        {
            Assert.AreEqual(Tokens.BorderRadius.MD, _section.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _section.style.borderTopRightRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _section.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _section.style.borderBottomRightRadius.value.value);
        }

        [Test]
        public void Constructor_SetsPadding()
        {
            Assert.AreEqual(Tokens.Spacing.Lg, _section.style.paddingTop.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, _section.style.paddingRight.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, _section.style.paddingBottom.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, _section.style.paddingLeft.value.value);
        }

        [Test]
        public void Constructor_SetsBottomMargin()
        {
            Assert.AreEqual(Tokens.Spacing.Lg, _section.style.marginBottom.value.value);
        }

        [Test]
        public void Constructor_SetsOverflowHidden()
        {
            Assert.AreEqual(Overflow.Hidden, _section.style.overflow.value);
        }

        [Test]
        public void Constructor_HeaderHasCorrectFontSize()
        {
            Assert.AreEqual(Tokens.FontSize.Xl, _section.Header.style.fontSize.value.value);
        }

        [Test]
        public void Constructor_HeaderHasBoldFont()
        {
            Assert.AreEqual(FontStyle.Bold, _section.Header.style.unityFontStyleAndWeight.value);
        }

        [Test]
        public void Constructor_HeaderHasBottomBorder()
        {
            Assert.AreEqual(1f, _section.Header.style.borderBottomWidth.value);
        }

        [Test]
        public void Constructor_ContentHasColumnDirection()
        {
            Assert.AreEqual(FlexDirection.Column, _section.Content.style.flexDirection.value);
        }

        #endregion

        #region Header Property Tests

        [Test]
        public void Header_ReturnsLabelElement()
        {
            Assert.IsInstanceOf<Label>(_section.Header);
        }

        [Test]
        public void Header_IsSameInstanceOnMultipleCalls()
        {
            var header1 = _section.Header;
            var header2 = _section.Header;
            Assert.AreSame(header1, header2);
        }

        #endregion

        #region Content Property Tests

        [Test]
        public void Content_ReturnsVisualElement()
        {
            Assert.IsInstanceOf<VisualElement>(_section.Content);
        }

        [Test]
        public void Content_IsSameInstanceOnMultipleCalls()
        {
            var content1 = _section.Content;
            var content2 = _section.Content;
            Assert.AreSame(content1, content2);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_SingleChild_AddsToContent()
        {
            var child = new Label("test");
            _section.Add(child);

            Assert.AreEqual(1, _section.Content.childCount);
            Assert.AreSame(child, _section.Content[0]);
        }

        [Test]
        public void Add_MultipleChildren_AddsAllToContent()
        {
            var child1 = new Label("test1");
            var child2 = new Label("test2");

            _section.Add(child1, child2);

            Assert.AreEqual(2, _section.Content.childCount);
        }

        [Test]
        public void Add_NullChild_IsIgnored()
        {
            _section.Add((VisualElement)null);
            Assert.AreEqual(0, _section.Content.childCount);
        }

        [Test]
        public void Add_ReturnsSectionForChaining()
        {
            var result = _section.Add(new Label());
            Assert.AreSame(_section, result);
        }

        [Test]
        public void Add_DoesNotAffectDirectChildren()
        {
            // Section has 2 direct children (header + content)
            var initialCount = _section.childCount;
            _section.Add(new Label("test"));

            // Direct children remain unchanged
            Assert.AreEqual(initialCount, _section.childCount);
        }

        #endregion

        #region WithTitle Tests

        [Test]
        public void WithTitle_UpdatesHeaderText()
        {
            _section.WithTitle("New Title");
            Assert.AreEqual("New Title", _section.Header.text);
        }

        [Test]
        public void WithTitle_ReturnsSectionForChaining()
        {
            var result = _section.WithTitle("New");
            Assert.AreSame(_section, result);
        }

        [Test]
        public void WithTitle_CanSetEmptyString()
        {
            _section.WithTitle("");
            Assert.AreEqual("", _section.Header.text);
        }

        #endregion

        #region NoHeader Tests

        [Test]
        public void NoHeader_HidesHeader()
        {
            _section.NoHeader();
            Assert.AreEqual(DisplayStyle.None, _section.Header.style.display.value);
        }

        [Test]
        public void NoHeader_ReturnsSectionForChaining()
        {
            var result = _section.NoHeader();
            Assert.AreSame(_section, result);
        }

        #endregion

        #region NoMargin Tests

        [Test]
        public void NoMargin_RemovesBottomMargin()
        {
            _section.NoMargin();
            Assert.AreEqual(0f, _section.style.marginBottom.value.value);
        }

        [Test]
        public void NoMargin_ReturnsSectionForChaining()
        {
            var result = _section.NoMargin();
            Assert.AreSame(_section, result);
        }

        #endregion

        #region Inherited JComponent Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _section.WithClass("custom-class");
            Assert.IsTrue(_section.ClassListContains("custom-class"));
        }

        [Test]
        public void WithName_SetsElementName()
        {
            _section.WithName("test-section");
            Assert.AreEqual("test-section", _section.name);
        }

        [Test]
        public void WithVisibility_False_HidesSection()
        {
            _section.WithVisibility(false);
            Assert.AreEqual(DisplayStyle.None, _section.style.display.value);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            // JSection-specific methods chain together
            _section
                .WithTitle("Updated Title")
                .NoMargin()
                .Add(new Label("content"));

            // JComponent methods called separately (they return JComponent)
            _section.WithName("my-section");
            _section.WithClass("custom");

            Assert.AreEqual("my-section", _section.name);
            Assert.IsTrue(_section.ClassListContains("custom"));
            Assert.AreEqual("Updated Title", _section.Header.text);
            Assert.AreEqual(0f, _section.style.marginBottom.value.value);
            Assert.AreEqual(1, _section.Content.childCount);
        }

        #endregion
    }
}
