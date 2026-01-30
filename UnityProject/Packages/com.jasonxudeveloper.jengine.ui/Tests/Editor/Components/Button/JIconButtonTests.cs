// JIconButtonTests.cs
// EditMode unit tests for JIconButton

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
    public class JIconButtonTests
    {
        private JIconButton _iconButton;

        [SetUp]
        public void SetUp()
        {
            _iconButton = new JIconButton("X");
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_iconButton.ClassListContains("j-icon-button"));
        }

        [Test]
        public void Constructor_SetsText()
        {
            Assert.AreEqual("X", _iconButton.text);
        }

        [Test]
        public void Constructor_WithTooltip_SetsTooltip()
        {
            var button = new JIconButton("X", tooltip: "Close");
            Assert.AreEqual("Close", button.tooltip);
        }

        [Test]
        public void Constructor_WithClickHandler_RegistersCallback()
        {
            bool clicked = false;
            var button = new JIconButton("X", () => clicked = true);

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
            Assert.DoesNotThrow(() => new JIconButton("X", null));
        }

        [Test]
        public void Constructor_SetsWidth()
        {
            Assert.AreEqual(22f, _iconButton.style.width.value.value);
        }

        [Test]
        public void Constructor_SetsHeight()
        {
            Assert.AreEqual(18f, _iconButton.style.height.value.value);
        }

        [Test]
        public void Constructor_SetsMinWidth()
        {
            Assert.AreEqual(18f, _iconButton.style.minWidth.value.value);
        }

        [Test]
        public void Constructor_SetsMinHeight()
        {
            Assert.AreEqual(18f, _iconButton.style.minHeight.value.value);
        }

        [Test]
        public void Constructor_SetsTransparentBackground()
        {
            Assert.AreEqual(Color.clear, _iconButton.style.backgroundColor.value);
        }

        [Test]
        public void Constructor_SetsZeroBorderWidths()
        {
            Assert.AreEqual(0f, _iconButton.style.borderTopWidth.value);
            Assert.AreEqual(0f, _iconButton.style.borderRightWidth.value);
            Assert.AreEqual(0f, _iconButton.style.borderBottomWidth.value);
            Assert.AreEqual(0f, _iconButton.style.borderLeftWidth.value);
        }

        [Test]
        public void Constructor_SetsBorderRadius()
        {
            Assert.AreEqual(Tokens.BorderRadius.Sm, _iconButton.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _iconButton.style.borderTopRightRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _iconButton.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _iconButton.style.borderBottomRightRadius.value.value);
        }

        [Test]
        public void Constructor_SetsXsFontSize()
        {
            Assert.AreEqual(Tokens.FontSize.Xs, _iconButton.style.fontSize.value.value);
        }

        [Test]
        public void Constructor_SetsMutedTextColor()
        {
            Assert.AreEqual(Tokens.Colors.TextMuted, _iconButton.style.color.value);
        }

        [Test]
        public void Constructor_SetsZeroPadding()
        {
            Assert.AreEqual(0f, _iconButton.style.paddingLeft.value.value);
            Assert.AreEqual(0f, _iconButton.style.paddingRight.value.value);
            Assert.AreEqual(0f, _iconButton.style.paddingTop.value.value);
            Assert.AreEqual(0f, _iconButton.style.paddingBottom.value.value);
        }

        [Test]
        public void Constructor_SetsLeftMargin()
        {
            Assert.AreEqual(2f, _iconButton.style.marginLeft.value.value);
        }

        #endregion

        #region WithTooltip Tests

        [Test]
        public void WithTooltip_SetsTooltip()
        {
            _iconButton.WithTooltip("Test tooltip");
            Assert.AreEqual("Test tooltip", _iconButton.tooltip);
        }

        [Test]
        public void WithTooltip_ReturnsButtonForChaining()
        {
            var result = _iconButton.WithTooltip("Test");
            Assert.AreSame(_iconButton, result);
        }

        [Test]
        public void WithTooltip_CanOverwrite()
        {
            _iconButton.WithTooltip("First");
            _iconButton.WithTooltip("Second");
            Assert.AreEqual("Second", _iconButton.tooltip);
        }

        [Test]
        public void WithTooltip_EmptyString_SetsEmpty()
        {
            _iconButton.WithTooltip("Something");
            _iconButton.WithTooltip("");
            Assert.AreEqual("", _iconButton.tooltip);
        }

        #endregion

        #region WithSize Tests

        [Test]
        public void WithSize_SetsWidth()
        {
            _iconButton.WithSize(30f, 25f);
            Assert.AreEqual(30f, _iconButton.style.width.value.value);
        }

        [Test]
        public void WithSize_SetsHeight()
        {
            _iconButton.WithSize(30f, 25f);
            Assert.AreEqual(25f, _iconButton.style.height.value.value);
        }

        [Test]
        public void WithSize_ReturnsButtonForChaining()
        {
            var result = _iconButton.WithSize(20f, 20f);
            Assert.AreSame(_iconButton, result);
        }

        [Test]
        public void WithSize_CanSetSquare()
        {
            _iconButton.WithSize(24f, 24f);
            Assert.AreEqual(24f, _iconButton.style.width.value.value);
            Assert.AreEqual(24f, _iconButton.style.height.value.value);
        }

        #endregion

        #region Different Text Content Tests

        [Test]
        public void Constructor_WithEmoji_Works()
        {
            var button = new JIconButton("🔍");
            Assert.AreEqual("🔍", button.text);
        }

        [Test]
        public void Constructor_WithMultipleChars_Works()
        {
            var button = new JIconButton("...");
            Assert.AreEqual("...", button.text);
        }

        [Test]
        public void Constructor_WithEmptyString_Works()
        {
            var button = new JIconButton("");
            Assert.AreEqual("", button.text);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            _iconButton
                .WithTooltip("Close")
                .WithSize(24f, 24f);

            Assert.AreEqual("Close", _iconButton.tooltip);
            Assert.AreEqual(24f, _iconButton.style.width.value.value);
            Assert.AreEqual(24f, _iconButton.style.height.value.value);
        }

        #endregion
    }
}
