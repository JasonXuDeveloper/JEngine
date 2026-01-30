// JRowTests.cs
// EditMode unit tests for JRow

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Layout;

namespace JEngine.UI.Tests.Editor.Components.Layout
{
    [TestFixture]
    public class JRowTests
    {
        private JRow _row;

        [SetUp]
        public void SetUp()
        {
            _row = new JRow();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_row.ClassListContains("j-row"));
        }

        [Test]
        public void Constructor_SetsRowDirection()
        {
            Assert.AreEqual(FlexDirection.Row, _row.style.flexDirection.value);
        }

        [Test]
        public void Constructor_SetsFlexWrap()
        {
            Assert.AreEqual(Wrap.Wrap, _row.style.flexWrap.value);
        }

        [Test]
        public void Constructor_SetsCenterAlignment()
        {
            Assert.AreEqual(Align.Center, _row.style.alignItems.value);
        }

        #endregion

        #region WithJustify Tests

        [Test]
        public void WithJustify_Start_SetsFlexStart()
        {
            _row.WithJustify(JustifyContent.Start);
            Assert.AreEqual(Justify.FlexStart, _row.style.justifyContent.value);
        }

        [Test]
        public void WithJustify_Center_SetsCenter()
        {
            _row.WithJustify(JustifyContent.Center);
            Assert.AreEqual(Justify.Center, _row.style.justifyContent.value);
        }

        [Test]
        public void WithJustify_End_SetsFlexEnd()
        {
            _row.WithJustify(JustifyContent.End);
            Assert.AreEqual(Justify.FlexEnd, _row.style.justifyContent.value);
        }

        [Test]
        public void WithJustify_SpaceBetween_SetsSpaceBetween()
        {
            _row.WithJustify(JustifyContent.SpaceBetween);
            Assert.AreEqual(Justify.SpaceBetween, _row.style.justifyContent.value);
        }

        [Test]
        public void WithJustify_ReturnsRowForChaining()
        {
            var result = _row.WithJustify(JustifyContent.Center);
            Assert.AreSame(_row, result);
        }

        [Test]
        public void WithJustify_RemovesPreviousJustifyClasses()
        {
            _row.WithJustify(JustifyContent.Start);
            _row.WithJustify(JustifyContent.End);

            // Check that both start and end styles work (class cleanup)
            Assert.IsFalse(_row.ClassListContains("j-row--justify-start"));
        }

        #endregion

        #region WithAlign Tests

        [Test]
        public void WithAlign_Start_SetsFlexStart()
        {
            _row.WithAlign(AlignItems.Start);
            Assert.AreEqual(Align.FlexStart, _row.style.alignItems.value);
        }

        [Test]
        public void WithAlign_Center_SetsCenter()
        {
            _row.WithAlign(AlignItems.Center);
            Assert.AreEqual(Align.Center, _row.style.alignItems.value);
        }

        [Test]
        public void WithAlign_End_SetsFlexEnd()
        {
            _row.WithAlign(AlignItems.End);
            Assert.AreEqual(Align.FlexEnd, _row.style.alignItems.value);
        }

        [Test]
        public void WithAlign_Stretch_SetsStretch()
        {
            _row.WithAlign(AlignItems.Stretch);
            Assert.AreEqual(Align.Stretch, _row.style.alignItems.value);
        }

        [Test]
        public void WithAlign_ReturnsRowForChaining()
        {
            var result = _row.WithAlign(AlignItems.Center);
            Assert.AreSame(_row, result);
        }

        #endregion

        #region NoWrap Tests

        [Test]
        public void NoWrap_SetsNoWrap()
        {
            _row.NoWrap();
            Assert.AreEqual(Wrap.NoWrap, _row.style.flexWrap.value);
        }

        [Test]
        public void NoWrap_AddsNoWrapClass()
        {
            _row.NoWrap();
            Assert.IsTrue(_row.ClassListContains("j-row--nowrap"));
        }

        [Test]
        public void NoWrap_ReturnsRowForChaining()
        {
            var result = _row.NoWrap();
            Assert.AreSame(_row, result);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_SingleChild_AddsToRow()
        {
            var child = new Label("test");
            _row.Add(child);

            Assert.AreEqual(1, _row.childCount);
            Assert.AreSame(child, _row[0]);
        }

        [Test]
        public void Add_MultipleChildren_AddsAllToRow()
        {
            var child1 = new Label("test1");
            var child2 = new Label("test2");
            var child3 = new Label("test3");

            _row.Add(child1, child2, child3);

            Assert.AreEqual(3, _row.childCount);
        }

        [Test]
        public void Add_ReturnsRowForChaining()
        {
            var result = _row.Add(new Label());
            Assert.AreSame(_row, result);
        }

        [Test]
        public void Add_CanChainMultipleAddCalls()
        {
            _row
                .Add(new Label("1"))
                .Add(new Label("2"))
                .Add(new Label("3"));

            Assert.AreEqual(3, _row.childCount);
        }

        #endregion

        #region Inherited JComponent Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _row.WithClass("custom-class");
            Assert.IsTrue(_row.ClassListContains("custom-class"));
        }

        [Test]
        public void WithClass_PreservesBaseClass()
        {
            _row.WithClass("custom");
            Assert.IsTrue(_row.ClassListContains("j-row"));
        }

        [Test]
        public void WithName_SetsElementName()
        {
            _row.WithName("test-row");
            Assert.AreEqual("test-row", _row.name);
        }

        [Test]
        public void WithMargin_SetsAllMargins()
        {
            _row.WithMargin(10f);
            Assert.AreEqual(10f, _row.style.marginTop.value.value);
            Assert.AreEqual(10f, _row.style.marginRight.value.value);
        }

        [Test]
        public void WithPadding_SetsAllPadding()
        {
            _row.WithPadding(10f);
            Assert.AreEqual(10f, _row.style.paddingTop.value.value);
            Assert.AreEqual(10f, _row.style.paddingLeft.value.value);
        }

        [Test]
        public void WithVisibility_False_HidesRow()
        {
            _row.WithVisibility(false);
            Assert.AreEqual(DisplayStyle.None, _row.style.display.value);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            // JRow-specific methods chain together
            _row
                .WithJustify(JustifyContent.SpaceBetween)
                .WithAlign(AlignItems.Stretch)
                .NoWrap()
                .Add(new Label("item1"))
                .Add(new Label("item2"));

            // JComponent methods called separately (they return JComponent)
            _row.WithName("my-row");
            _row.WithClass("custom");

            Assert.AreEqual("my-row", _row.name);
            Assert.IsTrue(_row.ClassListContains("custom"));
            Assert.AreEqual(Justify.SpaceBetween, _row.style.justifyContent.value);
            Assert.AreEqual(Align.Stretch, _row.style.alignItems.value);
            Assert.AreEqual(Wrap.NoWrap, _row.style.flexWrap.value);
            Assert.AreEqual(2, _row.childCount);
        }

        #endregion
    }
}
