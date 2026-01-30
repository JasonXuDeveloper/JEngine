// JBreadcrumbTests.cs
// EditMode unit tests for JBreadcrumb

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Navigation;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Navigation
{
    [TestFixture]
    public class JBreadcrumbTests
    {
        private JBreadcrumb _breadcrumb;

        [SetUp]
        public void SetUp()
        {
            _breadcrumb = new JBreadcrumb();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_breadcrumb.ClassListContains("j-breadcrumb"));
        }

        [Test]
        public void Constructor_SetsFlexGrow()
        {
            Assert.AreEqual(1f, _breadcrumb.style.flexGrow.value);
        }

        [Test]
        public void Constructor_SetsFlexShrink()
        {
            Assert.AreEqual(1f, _breadcrumb.style.flexShrink.value);
        }

        [Test]
        public void Constructor_CreatesContainer()
        {
            Assert.AreEqual(1, _breadcrumb.childCount);
        }

        [Test]
        public void Constructor_ContainerHasRowDirection()
        {
            var container = _breadcrumb[0];
            Assert.AreEqual(FlexDirection.Row, container.style.flexDirection.value);
        }

        [Test]
        public void Constructor_ContainerHasCenterAlignment()
        {
            var container = _breadcrumb[0];
            Assert.AreEqual(Align.Center, container.style.alignItems.value);
        }

        #endregion

        #region AddItem Tests

        [Test]
        public void AddItem_ReturnsBreadcrumbForChaining()
        {
            var result = _breadcrumb.AddItem("Test");
            Assert.AreSame(_breadcrumb, result);
        }

        [Test]
        public void AddItem_CanAddMultipleItems()
        {
            _breadcrumb.AddItem("First");
            _breadcrumb.AddItem("Second");
            _breadcrumb.AddItem("Third");

            // Items are added but not yet built
            // Build to see them
            _breadcrumb.Build();

            // Container has items plus separators
            var container = _breadcrumb[0];
            Assert.GreaterOrEqual(container.childCount, 3);
        }

        #endregion

        #region SetPath Tests

        [Test]
        public void SetPath_CreatesItemsFromArray()
        {
            _breadcrumb.SetPath("Package", "Scene");

            var container = _breadcrumb[0];
            // 2 labels + 1 separator = 3 children
            Assert.AreEqual(3, container.childCount);
        }

        [Test]
        public void SetPath_SingleItem_NoSeparator()
        {
            _breadcrumb.SetPath("OnlyOne");

            var container = _breadcrumb[0];
            Assert.AreEqual(1, container.childCount);
        }

        [Test]
        public void SetPath_ReturnsBreadcrumbForChaining()
        {
            var result = _breadcrumb.SetPath("Test");
            Assert.AreSame(_breadcrumb, result);
        }

        [Test]
        public void SetPath_ClearsPreviousItems()
        {
            _breadcrumb.SetPath("First", "Second");
            _breadcrumb.SetPath("New");

            var container = _breadcrumb[0];
            Assert.AreEqual(1, container.childCount);
        }

        [Test]
        public void SetPath_ThreeItems_HasTwoSeparators()
        {
            _breadcrumb.SetPath("Core", "Scripts", "Bootstrap.cs");

            var container = _breadcrumb[0];
            // 3 labels + 2 separators = 5 children
            Assert.AreEqual(5, container.childCount);
        }

        #endregion

        #region Clear Tests

        [Test]
        public void Clear_RemovesAllItems()
        {
            _breadcrumb.SetPath("Item1", "Item2");
            _breadcrumb.Clear();

            var container = _breadcrumb[0];
            Assert.AreEqual(0, container.childCount);
        }

        [Test]
        public void Clear_CanAddNewItemsAfter()
        {
            _breadcrumb.SetPath("Old");
            _breadcrumb.Clear();
            _breadcrumb.SetPath("New");

            var container = _breadcrumb[0];
            Assert.AreEqual(1, container.childCount);
        }

        #endregion

        #region Build Tests

        [Test]
        public void Build_CreatesLabelsForItems()
        {
            _breadcrumb.AddItem("First");
            _breadcrumb.AddItem("Second");
            _breadcrumb.Build();

            var container = _breadcrumb[0];
            // First label + separator + second label = 3
            Assert.AreEqual(3, container.childCount);
        }

        [Test]
        public void Build_LastItemHasPillStyle()
        {
            _breadcrumb.SetPath("First", "Last");

            var container = _breadcrumb[0];
            // Last item is at index 2 (First, separator, Last)
            var lastLabel = container[2] as Label;

            // Check pill styling indicators
            Assert.AreEqual(Tokens.Colors.BgElevated, lastLabel.style.backgroundColor.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, lastLabel.style.borderTopLeftRadius.value.value);
        }

        [Test]
        public void Build_NonLastItemsHaveMutedColor()
        {
            _breadcrumb.SetPath("First", "Last");

            var container = _breadcrumb[0];
            var firstLabel = container[0] as Label;

            Assert.AreEqual(Tokens.Colors.TextMuted, firstLabel.style.color.value);
        }

        [Test]
        public void Build_SeparatorsHaveChevron()
        {
            _breadcrumb.SetPath("First", "Last");

            var container = _breadcrumb[0];
            var separator = container[1] as Label;

            Assert.AreEqual("›", separator.text);
        }

        [Test]
        public void Build_SeparatorsHaveMutedColor()
        {
            _breadcrumb.SetPath("First", "Last");

            var container = _breadcrumb[0];
            var separator = container[1] as Label;

            Assert.AreEqual(Tokens.Colors.TextMuted, separator.style.color.value);
        }

        [Test]
        public void Build_SeparatorsDoNotShrink()
        {
            _breadcrumb.SetPath("First", "Last");

            var container = _breadcrumb[0];
            var separator = container[1] as Label;

            Assert.AreEqual(0f, separator.style.flexShrink.value);
        }

        [Test]
        public void Build_LastItemCanShrink()
        {
            _breadcrumb.SetPath("First", "Last");

            var container = _breadcrumb[0];
            var lastLabel = container[2] as Label;

            Assert.AreEqual(1f, lastLabel.style.flexShrink.value);
        }

        [Test]
        public void Build_NonLastItemsDoNotShrink()
        {
            _breadcrumb.SetPath("First", "Last");

            var container = _breadcrumb[0];
            var firstLabel = container[0] as Label;

            Assert.AreEqual(0f, firstLabel.style.flexShrink.value);
        }

        #endregion

        #region FromPath Static Method Tests

        [Test]
        public void FromPath_CreatesBreadcrumbWithPath()
        {
            var breadcrumb = JBreadcrumb.FromPath("Package", "Scene");

            var container = breadcrumb[0];
            Assert.AreEqual(3, container.childCount);
        }

        [Test]
        public void FromPath_ReturnsBreadcrumbInstance()
        {
            var breadcrumb = JBreadcrumb.FromPath("Test");
            Assert.IsInstanceOf<JBreadcrumb>(breadcrumb);
        }

        [Test]
        public void FromPath_SinglePath_NoSeparator()
        {
            var breadcrumb = JBreadcrumb.FromPath("Single");

            var container = breadcrumb[0];
            Assert.AreEqual(1, container.childCount);
        }

        #endregion

        #region BreadcrumbItem Tests

        [Test]
        public void BreadcrumbItem_CanSetLabel()
        {
            var item = new JBreadcrumb.BreadcrumbItem("Test");
            Assert.AreEqual("Test", item.Label);
        }

        [Test]
        public void BreadcrumbItem_IsLastDefaultFalse()
        {
            var item = new JBreadcrumb.BreadcrumbItem("Test");
            Assert.IsFalse(item.IsLast);
        }

        [Test]
        public void BreadcrumbItem_CanSetIsLast()
        {
            var item = new JBreadcrumb.BreadcrumbItem("Test");
            item.IsLast = true;
            Assert.IsTrue(item.IsLast);
        }

        #endregion

        #region Styling Tests

        [Test]
        public void Build_LastItemHasBorders()
        {
            _breadcrumb.SetPath("First", "Last");

            var container = _breadcrumb[0];
            var lastLabel = container[2] as Label;

            Assert.AreEqual(1f, lastLabel.style.borderTopWidth.value);
            Assert.AreEqual(1f, lastLabel.style.borderBottomWidth.value);
            Assert.AreEqual(1f, lastLabel.style.borderLeftWidth.value);
            Assert.AreEqual(1f, lastLabel.style.borderRightWidth.value);
        }

        [Test]
        public void Build_LastItemHasPadding()
        {
            _breadcrumb.SetPath("First", "Last");

            var container = _breadcrumb[0];
            var lastLabel = container[2] as Label;

            Assert.AreEqual(Tokens.Spacing.MD, lastLabel.style.paddingLeft.value.value);
            Assert.AreEqual(Tokens.Spacing.MD, lastLabel.style.paddingRight.value.value);
            Assert.AreEqual(Tokens.Spacing.Xs, lastLabel.style.paddingTop.value.value);
            Assert.AreEqual(Tokens.Spacing.Xs, lastLabel.style.paddingBottom.value.value);
        }

        [Test]
        public void Build_LabelsHaveSmallFontSize()
        {
            _breadcrumb.SetPath("First", "Last");

            var container = _breadcrumb[0];
            var firstLabel = container[0] as Label;

            Assert.AreEqual(Tokens.FontSize.Sm, firstLabel.style.fontSize.value.value);
        }

        #endregion
    }
}
