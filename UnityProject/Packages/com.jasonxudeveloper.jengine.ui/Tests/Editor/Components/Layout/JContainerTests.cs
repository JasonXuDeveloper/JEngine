// JContainerTests.cs
// EditMode unit tests for JContainer layout component

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Layout;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Layout
{
    [TestFixture]
    public class JContainerTests
    {
        private JContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = new JContainer();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_Default_AddsBaseClass()
        {
            Assert.IsTrue(_container.ClassListContains("j-container"));
        }

        [Test]
        public void Constructor_Default_SetsWidth100Percent()
        {
            Assert.AreEqual(100f, _container.style.width.value.value);
            Assert.AreEqual(LengthUnit.Percent, _container.style.width.value.unit);
        }

        [Test]
        public void Constructor_Default_SetsAutoMargins()
        {
            Assert.AreEqual(StyleKeyword.Auto, _container.style.marginLeft.keyword);
            Assert.AreEqual(StyleKeyword.Auto, _container.style.marginRight.keyword);
        }

        [Test]
        public void Constructor_Default_UsesDefaultLgSize()
        {
            Assert.IsTrue(_container.ClassListContains("j-container--lg"));
            Assert.AreEqual(ContainerSize.Lg, _container.Size);
        }

        [Test]
        public void Constructor_Default_SetsLgMaxWidth()
        {
            Assert.AreEqual(Tokens.Container.Lg, _container.style.maxWidth.value.value);
        }

        [Test]
        public void Constructor_WithXsSize_SetsCorrectClass()
        {
            var container = new JContainer(ContainerSize.Xs);
            Assert.IsTrue(container.ClassListContains("j-container--xs"));
        }

        [Test]
        public void Constructor_WithXsSize_SetsCorrectMaxWidth()
        {
            var container = new JContainer(ContainerSize.Xs);
            Assert.AreEqual(Tokens.Container.Xs, container.style.maxWidth.value.value);
        }

        [Test]
        public void Constructor_WithSmSize_SetsCorrectClass()
        {
            var container = new JContainer(ContainerSize.Sm);
            Assert.IsTrue(container.ClassListContains("j-container--sm"));
        }

        [Test]
        public void Constructor_WithSmSize_SetsCorrectMaxWidth()
        {
            var container = new JContainer(ContainerSize.Sm);
            Assert.AreEqual(Tokens.Container.Sm, container.style.maxWidth.value.value);
        }

        [Test]
        public void Constructor_WithMdSize_SetsCorrectClass()
        {
            var container = new JContainer(ContainerSize.Md);
            Assert.IsTrue(container.ClassListContains("j-container--md"));
        }

        [Test]
        public void Constructor_WithMdSize_SetsCorrectMaxWidth()
        {
            var container = new JContainer(ContainerSize.Md);
            Assert.AreEqual(Tokens.Container.Md, container.style.maxWidth.value.value);
        }

        [Test]
        public void Constructor_WithLgSize_SetsCorrectClass()
        {
            var container = new JContainer(ContainerSize.Lg);
            Assert.IsTrue(container.ClassListContains("j-container--lg"));
        }

        [Test]
        public void Constructor_WithLgSize_SetsCorrectMaxWidth()
        {
            var container = new JContainer(ContainerSize.Lg);
            Assert.AreEqual(Tokens.Container.Lg, container.style.maxWidth.value.value);
        }

        [Test]
        public void Constructor_WithXlSize_SetsCorrectClass()
        {
            var container = new JContainer(ContainerSize.Xl);
            Assert.IsTrue(container.ClassListContains("j-container--xl"));
        }

        [Test]
        public void Constructor_WithXlSize_SetsCorrectMaxWidth()
        {
            var container = new JContainer(ContainerSize.Xl);
            Assert.AreEqual(Tokens.Container.Xl, container.style.maxWidth.value.value);
        }

        [Test]
        public void Constructor_WithFullSize_SetsCorrectClass()
        {
            var container = new JContainer(ContainerSize.Full);
            Assert.IsTrue(container.ClassListContains("j-container--full"));
        }

        [Test]
        public void Constructor_WithFullSize_SetsNoMaxWidth()
        {
            var container = new JContainer(ContainerSize.Full);
            Assert.AreEqual(StyleKeyword.None, container.style.maxWidth.keyword);
        }

        #endregion

        #region Size Property Tests

        [Test]
        public void Size_Get_ReturnsCurrentSize()
        {
            var container = new JContainer(ContainerSize.Sm);
            Assert.AreEqual(ContainerSize.Sm, container.Size);
        }

        [Test]
        public void Size_Set_UpdatesSizeAndClass()
        {
            _container.Size = ContainerSize.Xs;
            Assert.AreEqual(ContainerSize.Xs, _container.Size);
            Assert.IsTrue(_container.ClassListContains("j-container--xs"));
        }

        [Test]
        public void Size_Set_RemovesPreviousClass()
        {
            _container.Size = ContainerSize.Xs;
            Assert.IsFalse(_container.ClassListContains("j-container--lg"));
        }

        #endregion

        #region WithSize Tests

        [Test]
        public void WithSize_Xs_SetsCorrectClass()
        {
            _container.WithSize(ContainerSize.Xs);
            Assert.IsTrue(_container.ClassListContains("j-container--xs"));
        }

        [Test]
        public void WithSize_Sm_SetsCorrectClass()
        {
            _container.WithSize(ContainerSize.Sm);
            Assert.IsTrue(_container.ClassListContains("j-container--sm"));
        }

        [Test]
        public void WithSize_Md_SetsCorrectClass()
        {
            _container.WithSize(ContainerSize.Md);
            Assert.IsTrue(_container.ClassListContains("j-container--md"));
        }

        [Test]
        public void WithSize_Xl_SetsCorrectClass()
        {
            _container.WithSize(ContainerSize.Xl);
            Assert.IsTrue(_container.ClassListContains("j-container--xl"));
        }

        [Test]
        public void WithSize_Full_SetsCorrectClass()
        {
            _container.WithSize(ContainerSize.Full);
            Assert.IsTrue(_container.ClassListContains("j-container--full"));
        }

        [Test]
        public void WithSize_RemovesPreviousSizeClass()
        {
            _container.WithSize(ContainerSize.Xs);
            _container.WithSize(ContainerSize.Xl);

            Assert.IsFalse(_container.ClassListContains("j-container--xs"));
            Assert.IsTrue(_container.ClassListContains("j-container--xl"));
        }

        [Test]
        public void WithSize_ReturnsContainerForChaining()
        {
            var result = _container.WithSize(ContainerSize.Sm);
            Assert.AreSame(_container, result);
        }

        #endregion

        #region WithHorizontalPadding Tests

        [Test]
        public void WithHorizontalPadding_SetsLeftPadding()
        {
            _container.WithHorizontalPadding(20f);
            Assert.AreEqual(20f, _container.style.paddingLeft.value.value);
        }

        [Test]
        public void WithHorizontalPadding_SetsRightPadding()
        {
            _container.WithHorizontalPadding(20f);
            Assert.AreEqual(20f, _container.style.paddingRight.value.value);
        }

        [Test]
        public void WithHorizontalPadding_ReturnsContainerForChaining()
        {
            var result = _container.WithHorizontalPadding(10f);
            Assert.AreSame(_container, result);
        }

        #endregion

        #region WithResponsivePadding Tests

        [Test]
        public void WithResponsivePadding_Xs_SetsMdPadding()
        {
            var container = new JContainer(ContainerSize.Xs);
            container.WithResponsivePadding();
            Assert.AreEqual(Tokens.Spacing.MD, container.style.paddingLeft.value.value);
        }

        [Test]
        public void WithResponsivePadding_Sm_SetsLgPadding()
        {
            var container = new JContainer(ContainerSize.Sm);
            container.WithResponsivePadding();
            Assert.AreEqual(Tokens.Spacing.Lg, container.style.paddingLeft.value.value);
        }

        [Test]
        public void WithResponsivePadding_Md_SetsXlPadding()
        {
            var container = new JContainer(ContainerSize.Md);
            container.WithResponsivePadding();
            Assert.AreEqual(Tokens.Spacing.Xl, container.style.paddingLeft.value.value);
        }

        [Test]
        public void WithResponsivePadding_Lg_SetsXxlPadding()
        {
            var container = new JContainer(ContainerSize.Lg);
            container.WithResponsivePadding();
            Assert.AreEqual(Tokens.Spacing.Xxl, container.style.paddingLeft.value.value);
        }

        [Test]
        public void WithResponsivePadding_Xl_SetsXxlPadding()
        {
            var container = new JContainer(ContainerSize.Xl);
            container.WithResponsivePadding();
            Assert.AreEqual(Tokens.Spacing.Xxl, container.style.paddingLeft.value.value);
        }

        [Test]
        public void WithResponsivePadding_Full_SetsLgPadding()
        {
            var container = new JContainer(ContainerSize.Full);
            container.WithResponsivePadding();
            Assert.AreEqual(Tokens.Spacing.Lg, container.style.paddingLeft.value.value);
        }

        [Test]
        public void WithResponsivePadding_ReturnsContainerForChaining()
        {
            var result = _container.WithResponsivePadding();
            Assert.AreSame(_container, result);
        }

        #endregion

        #region Fluid Tests

        [Test]
        public void Fluid_SetsFullSize()
        {
            _container.Fluid();
            Assert.AreEqual(ContainerSize.Full, _container.Size);
        }

        [Test]
        public void Fluid_AddsFullClass()
        {
            _container.Fluid();
            Assert.IsTrue(_container.ClassListContains("j-container--full"));
        }

        [Test]
        public void Fluid_RemovesMaxWidth()
        {
            _container.Fluid();
            Assert.AreEqual(StyleKeyword.None, _container.style.maxWidth.keyword);
        }

        [Test]
        public void Fluid_ReturnsContainerForChaining()
        {
            var result = _container.Fluid();
            Assert.AreSame(_container, result);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_SingleChild_AddsToContainer()
        {
            var child = new Label("test");
            _container.Add(child);

            Assert.AreEqual(1, _container.childCount);
            Assert.AreSame(child, _container[0]);
        }

        [Test]
        public void Add_MultipleChildren_AddsAllToContainer()
        {
            var child1 = new Label("test1");
            var child2 = new Label("test2");
            var child3 = new Label("test3");

            _container.Add(child1, child2, child3);

            Assert.AreEqual(3, _container.childCount);
        }

        [Test]
        public void Add_ReturnsContainerForChaining()
        {
            var result = _container.Add(new Label());
            Assert.AreSame(_container, result);
        }

        [Test]
        public void Add_CanChainMultipleAddCalls()
        {
            _container
                .Add(new Label("1"))
                .Add(new Label("2"))
                .Add(new Label("3"));

            Assert.AreEqual(3, _container.childCount);
        }

        #endregion

        #region Inherited JComponent Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _container.WithClass("custom-class");
            Assert.IsTrue(_container.ClassListContains("custom-class"));
        }

        [Test]
        public void WithClass_PreservesBaseClass()
        {
            _container.WithClass("custom");
            Assert.IsTrue(_container.ClassListContains("j-container"));
        }

        [Test]
        public void WithName_SetsElementName()
        {
            _container.WithName("test-container");
            Assert.AreEqual("test-container", _container.name);
        }

        [Test]
        public void WithMargin_SetsAllMargins()
        {
            _container.WithMargin(10f);
            Assert.AreEqual(10f, _container.style.marginTop.value.value);
            Assert.AreEqual(10f, _container.style.marginBottom.value.value);
        }

        [Test]
        public void WithPadding_SetsAllPadding()
        {
            _container.WithPadding(10f);
            Assert.AreEqual(10f, _container.style.paddingTop.value.value);
            Assert.AreEqual(10f, _container.style.paddingBottom.value.value);
        }

        [Test]
        public void WithFlexGrow_SetsFlexGrow()
        {
            _container.WithFlexGrow(1f);
            Assert.AreEqual(1f, _container.style.flexGrow.value);
        }

        [Test]
        public void WithVisibility_False_HidesContainer()
        {
            _container.WithVisibility(false);
            Assert.AreEqual(DisplayStyle.None, _container.style.display.value);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            // JContainer-specific methods chain together
            _container
                .WithSize(ContainerSize.Md)
                .WithHorizontalPadding(16f)
                .Add(new Label("item1"))
                .Add(new Label("item2"));

            // JComponent methods called separately (they return JComponent)
            _container.WithName("my-container");
            _container.WithClass("custom");

            Assert.AreEqual("my-container", _container.name);
            Assert.IsTrue(_container.ClassListContains("custom"));
            Assert.IsTrue(_container.ClassListContains("j-container--md"));
            Assert.AreEqual(16f, _container.style.paddingLeft.value.value);
            Assert.AreEqual(2, _container.childCount);
        }

        #endregion
    }
}
