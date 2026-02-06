// JStackTests.cs
// EditMode unit tests for JStack layout component

using System.Reflection;
using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Layout;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Layout
{
    [TestFixture]
    public class JStackTests
    {
        private JStack _stack;

        [SetUp]
        public void SetUp()
        {
            _stack = new JStack();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_Default_AddsBaseClass()
        {
            Assert.IsTrue(_stack.ClassListContains("j-stack"));
        }

        [Test]
        public void Constructor_Default_SetsColumnDirection()
        {
            Assert.AreEqual(FlexDirection.Column, _stack.style.flexDirection.value);
        }

        [Test]
        public void Constructor_Default_UsesDefaultMediumGap()
        {
            Assert.IsTrue(_stack.ClassListContains("j-stack--gap-md"));
        }

        [Test]
        public void Constructor_WithXsGap_SetsCorrectClass()
        {
            var stack = new JStack(GapSize.Xs);
            Assert.IsTrue(stack.ClassListContains("j-stack--gap-xs"));
        }

        [Test]
        public void Constructor_WithSmGap_SetsCorrectClass()
        {
            var stack = new JStack(GapSize.Sm);
            Assert.IsTrue(stack.ClassListContains("j-stack--gap-sm"));
        }

        [Test]
        public void Constructor_WithMdGap_SetsCorrectClass()
        {
            var stack = new JStack(GapSize.MD);
            Assert.IsTrue(stack.ClassListContains("j-stack--gap-md"));
        }

        [Test]
        public void Constructor_WithLgGap_SetsCorrectClass()
        {
            var stack = new JStack(GapSize.Lg);
            Assert.IsTrue(stack.ClassListContains("j-stack--gap-lg"));
        }

        [Test]
        public void Constructor_WithXlGap_SetsCorrectClass()
        {
            var stack = new JStack(GapSize.Xl);
            Assert.IsTrue(stack.ClassListContains("j-stack--gap-xl"));
        }

        #endregion

        #region WithGap Tests

        [Test]
        public void WithGap_Xs_SetsCorrectClass()
        {
            _stack.WithGap(GapSize.Xs);
            Assert.IsTrue(_stack.ClassListContains("j-stack--gap-xs"));
        }

        [Test]
        public void WithGap_Sm_SetsCorrectClass()
        {
            _stack.WithGap(GapSize.Sm);
            Assert.IsTrue(_stack.ClassListContains("j-stack--gap-sm"));
        }

        [Test]
        public void WithGap_Lg_SetsCorrectClass()
        {
            _stack.WithGap(GapSize.Lg);
            Assert.IsTrue(_stack.ClassListContains("j-stack--gap-lg"));
        }

        [Test]
        public void WithGap_Xl_SetsCorrectClass()
        {
            _stack.WithGap(GapSize.Xl);
            Assert.IsTrue(_stack.ClassListContains("j-stack--gap-xl"));
        }

        [Test]
        public void WithGap_RemovesPreviousGapClass()
        {
            _stack.WithGap(GapSize.Xs);
            _stack.WithGap(GapSize.Xl);

            Assert.IsFalse(_stack.ClassListContains("j-stack--gap-xs"));
            Assert.IsTrue(_stack.ClassListContains("j-stack--gap-xl"));
        }

        [Test]
        public void WithGap_ReturnsStackForChaining()
        {
            var result = _stack.WithGap(GapSize.Sm);
            Assert.AreSame(_stack, result);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_SingleChild_AddsToStack()
        {
            var child = new Label("test");
            _stack.Add(child);

            Assert.AreEqual(1, _stack.childCount);
            Assert.AreSame(child, _stack[0]);
        }

        [Test]
        public void Add_MultipleChildren_AddsAllToStack()
        {
            var child1 = new Label("test1");
            var child2 = new Label("test2");
            var child3 = new Label("test3");

            _stack.Add(child1, child2, child3);

            Assert.AreEqual(3, _stack.childCount);
        }

        [Test]
        public void Add_ReturnsStackForChaining()
        {
            var result = _stack.Add(new Label());
            Assert.AreSame(_stack, result);
        }

        [Test]
        public void Add_CanChainMultipleAddCalls()
        {
            _stack
                .Add(new Label("1"))
                .Add(new Label("2"))
                .Add(new Label("3"));

            Assert.AreEqual(3, _stack.childCount);
        }

        #endregion

        #region Inherited JComponent Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _stack.WithClass("custom-class");
            Assert.IsTrue(_stack.ClassListContains("custom-class"));
        }

        [Test]
        public void WithClass_PreservesBaseClass()
        {
            _stack.WithClass("custom");
            Assert.IsTrue(_stack.ClassListContains("j-stack"));
        }

        [Test]
        public void WithName_SetsElementName()
        {
            _stack.WithName("test-stack");
            Assert.AreEqual("test-stack", _stack.name);
        }

        [Test]
        public void WithMargin_SetsAllMargins()
        {
            _stack.WithMargin(10f);
            Assert.AreEqual(10f, _stack.style.marginTop.value.value);
            Assert.AreEqual(10f, _stack.style.marginBottom.value.value);
        }

        [Test]
        public void WithPadding_SetsAllPadding()
        {
            _stack.WithPadding(10f);
            Assert.AreEqual(10f, _stack.style.paddingTop.value.value);
            Assert.AreEqual(10f, _stack.style.paddingBottom.value.value);
        }

        [Test]
        public void WithFlexGrow_SetsFlexGrow()
        {
            _stack.WithFlexGrow(1f);
            Assert.AreEqual(1f, _stack.style.flexGrow.value);
        }

        [Test]
        public void WithVisibility_False_HidesStack()
        {
            _stack.WithVisibility(false);
            Assert.AreEqual(DisplayStyle.None, _stack.style.display.value);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            // JStack-specific methods chain together
            _stack
                .WithGap(GapSize.Lg)
                .Add(new Label("item1"))
                .Add(new Label("item2"));

            // JComponent methods called separately (they return JComponent)
            _stack.WithName("my-stack");
            _stack.WithClass("custom");
            _stack.WithMargin(5f);

            Assert.AreEqual("my-stack", _stack.name);
            Assert.IsTrue(_stack.ClassListContains("custom"));
            Assert.IsTrue(_stack.ClassListContains("j-stack--gap-lg"));
            Assert.AreEqual(5f, _stack.style.marginTop.value.value);
            Assert.AreEqual(2, _stack.childCount);
        }

        #endregion

        #region ApplyChildGaps Tests

        [Test]
        public void ApplyChildGaps_SetsMarginOnChildren()
        {
            _stack.Add(new Label("1"), new Label("2"), new Label("3"));

            var method = typeof(JStack).GetMethod("ApplyChildGaps",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "ApplyChildGaps method should exist");

            method.Invoke(_stack, new object[] { 10f });

            Assert.AreEqual(10f, _stack[0].style.marginBottom.value.value);
            Assert.AreEqual(10f, _stack[1].style.marginBottom.value.value);
            Assert.AreEqual(0f, _stack[2].style.marginBottom.value.value);
        }

        [Test]
        public void ApplyChildGaps_SingleChild_ZeroMargin()
        {
            _stack.Add(new Label("1"));

            var method = typeof(JStack).GetMethod("ApplyChildGaps",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_stack, new object[] { 10f });

            Assert.AreEqual(0f, _stack[0].style.marginBottom.value.value);
        }

        [Test]
        public void ApplyChildGaps_NoChildren_DoesNotThrow()
        {
            var method = typeof(JStack).GetMethod("ApplyChildGaps",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.DoesNotThrow(() => method.Invoke(_stack, new object[] { 10f }));
        }

        [Test]
        public void ApplyChildGaps_WithZeroGap_AllMarginsZero()
        {
            _stack.Add(new Label("1"), new Label("2"));

            var method = typeof(JStack).GetMethod("ApplyChildGaps",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_stack, new object[] { 0f });

            Assert.AreEqual(0f, _stack[0].style.marginBottom.value.value);
            Assert.AreEqual(0f, _stack[1].style.marginBottom.value.value);
        }

        #endregion
    }
}
