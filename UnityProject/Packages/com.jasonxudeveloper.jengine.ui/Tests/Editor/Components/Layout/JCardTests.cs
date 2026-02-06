// JCardTests.cs
// EditMode unit tests for JCard

using System.Reflection;
using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Layout;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Layout
{
    [TestFixture]
    public class JCardTests
    {
        private JCard _card;

        [SetUp]
        public void SetUp()
        {
            _card = new JCard();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_card.ClassListContains("j-card"));
        }

        [Test]
        public void Constructor_SetsSurfaceBackgroundColor()
        {
            Assert.AreEqual(Tokens.Colors.BgSurface, _card.style.backgroundColor.value);
        }

        [Test]
        public void Constructor_SetsBorderRadius()
        {
            Assert.AreEqual(Tokens.BorderRadius.MD, _card.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _card.style.borderTopRightRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _card.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _card.style.borderBottomRightRadius.value.value);
        }

        [Test]
        public void Constructor_SetsPadding()
        {
            Assert.AreEqual(Tokens.Spacing.Lg, _card.style.paddingTop.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, _card.style.paddingRight.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, _card.style.paddingBottom.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, _card.style.paddingLeft.value.value);
        }

        [Test]
        public void Constructor_SetsBottomMargin()
        {
            Assert.AreEqual(Tokens.Spacing.Lg, _card.style.marginBottom.value.value);
        }

        [Test]
        public void Constructor_SetsBorderWidths()
        {
            Assert.AreEqual(1f, _card.style.borderTopWidth.value);
            Assert.AreEqual(1f, _card.style.borderRightWidth.value);
            Assert.AreEqual(1f, _card.style.borderBottomWidth.value);
            Assert.AreEqual(1f, _card.style.borderLeftWidth.value);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_SingleChild_AddsToCard()
        {
            var child = new Label("test");
            _card.Add(child);

            Assert.AreEqual(1, _card.childCount);
            Assert.AreSame(child, _card[0]);
        }

        [Test]
        public void Add_MultipleChildren_AddsAllToCard()
        {
            var child1 = new Label("test1");
            var child2 = new Label("test2");

            _card.Add(child1, child2);

            Assert.AreEqual(2, _card.childCount);
        }

        [Test]
        public void Add_ReturnsCardForChaining()
        {
            var result = _card.Add(new Label());
            Assert.AreSame(_card, result);
        }

        [Test]
        public void Add_CanChainMultipleAddCalls()
        {
            _card
                .Add(new Label("1"))
                .Add(new Label("2"))
                .Add(new Label("3"));

            Assert.AreEqual(3, _card.childCount);
        }

        #endregion

        #region NoMargin Tests

        [Test]
        public void NoMargin_RemovesBottomMargin()
        {
            _card.NoMargin();
            Assert.AreEqual(0f, _card.style.marginBottom.value.value);
        }

        [Test]
        public void NoMargin_ReturnsCardForChaining()
        {
            var result = _card.NoMargin();
            Assert.AreSame(_card, result);
        }

        #endregion

        #region Compact Tests

        [Test]
        public void Compact_SetsCompactPadding()
        {
            _card.Compact();

            Assert.AreEqual(Tokens.Spacing.MD, _card.style.paddingTop.value.value);
            Assert.AreEqual(Tokens.Spacing.MD, _card.style.paddingRight.value.value);
            Assert.AreEqual(Tokens.Spacing.MD, _card.style.paddingBottom.value.value);
            Assert.AreEqual(Tokens.Spacing.MD, _card.style.paddingLeft.value.value);
        }

        [Test]
        public void Compact_ReturnsCardForChaining()
        {
            var result = _card.Compact();
            Assert.AreSame(_card, result);
        }

        #endregion

        #region Inherited JComponent Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _card.WithClass("custom-class");
            Assert.IsTrue(_card.ClassListContains("custom-class"));
        }

        [Test]
        public void WithName_SetsElementName()
        {
            _card.WithName("test-card");
            Assert.AreEqual("test-card", _card.name);
        }

        [Test]
        public void WithVisibility_False_HidesCard()
        {
            _card.WithVisibility(false);
            Assert.AreEqual(DisplayStyle.None, _card.style.display.value);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            // JCard-specific methods chain together
            _card
                .Compact()
                .NoMargin()
                .Add(new Label("content"));

            // JComponent methods called separately (they return JComponent)
            _card.WithName("my-card");
            _card.WithClass("custom");

            Assert.AreEqual("my-card", _card.name);
            Assert.IsTrue(_card.ClassListContains("custom"));
            Assert.AreEqual(Tokens.Spacing.MD, _card.style.paddingTop.value.value);
            Assert.AreEqual(0f, _card.style.marginBottom.value.value);
            Assert.AreEqual(1, _card.childCount);
        }

        #endregion

        #region Hover Event Tests

        [Test]
        public void OnMouseEnter_SetsElevatedBackground()
        {
            var method = typeof(JCard).GetMethod("OnMouseEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "OnMouseEnter method should exist");

            method.Invoke(_card, new object[] { null });

            Assert.AreEqual(Tokens.Colors.BgElevated, _card.style.backgroundColor.value);
        }

        [Test]
        public void OnMouseEnter_SetsHoverBorderColor()
        {
            var method = typeof(JCard).GetMethod("OnMouseEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_card, new object[] { null });

            Assert.AreEqual(Tokens.Colors.BorderHover, _card.style.borderTopColor.value);
            Assert.AreEqual(Tokens.Colors.BorderHover, _card.style.borderLeftColor.value);
        }

        [Test]
        public void OnMouseLeave_RestoresSurfaceBackground()
        {
            var enter = typeof(JCard).GetMethod("OnMouseEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            enter.Invoke(_card, new object[] { null });

            var leave = typeof(JCard).GetMethod("OnMouseLeave", BindingFlags.NonPublic | BindingFlags.Instance);
            leave.Invoke(_card, new object[] { null });

            Assert.AreEqual(Tokens.Colors.BgSurface, _card.style.backgroundColor.value);
        }

        [Test]
        public void OnMouseLeave_RestoresLightBorderColor()
        {
            var enter = typeof(JCard).GetMethod("OnMouseEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            enter.Invoke(_card, new object[] { null });

            var leave = typeof(JCard).GetMethod("OnMouseLeave", BindingFlags.NonPublic | BindingFlags.Instance);
            leave.Invoke(_card, new object[] { null });

            Assert.AreEqual(Tokens.Colors.BorderLight, _card.style.borderTopColor.value);
            Assert.AreEqual(Tokens.Colors.BorderLight, _card.style.borderLeftColor.value);
        }

        #endregion
    }
}
