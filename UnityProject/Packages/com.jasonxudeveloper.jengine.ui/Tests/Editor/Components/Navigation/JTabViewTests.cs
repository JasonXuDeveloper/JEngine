// JTabViewTests.cs
// EditMode unit tests for JTabView

using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Navigation;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Navigation
{
    [TestFixture]
    public class JTabViewTests
    {
        private JTabView _tabView;

        [SetUp]
        public void SetUp()
        {
            _tabView = new JTabView();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_tabView.ClassListContains("j-tab-view"));
        }

        [Test]
        public void Constructor_CreatesTwoChildren()
        {
            // Tab bar + content area
            Assert.AreEqual(2, _tabView.hierarchy.childCount);
        }

        [Test]
        public void Constructor_TabBarHasCorrectClass()
        {
            var bar = _tabView.hierarchy[0];
            Assert.IsTrue(bar.ClassListContains("j-tab-view__bar"));
        }

        [Test]
        public void Constructor_ContentAreaHasCorrectClass()
        {
            var content = _tabView.hierarchy[1];
            Assert.IsTrue(content.ClassListContains("j-tab-view__content"));
        }

        [Test]
        public void Constructor_TabBarHasRowDirection()
        {
            var bar = _tabView.hierarchy[0];
            Assert.AreEqual(FlexDirection.Row, bar.style.flexDirection.value);
        }

        [Test]
        public void Constructor_TabBarHasWrap()
        {
            var bar = _tabView.hierarchy[0];
            Assert.AreEqual(Wrap.Wrap, bar.style.flexWrap.value);
        }

        [Test]
        public void Constructor_TabBarHasSurfaceBackground()
        {
            var bar = _tabView.hierarchy[0];
            Assert.AreEqual(Tokens.Colors.BgSurface, bar.style.backgroundColor.value);
        }

        [Test]
        public void Constructor_TabBarHasBottomBorder()
        {
            var bar = _tabView.hierarchy[0];
            Assert.AreEqual(1f, bar.style.borderBottomWidth.value);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, bar.style.borderBottomColor.value);
        }

        [Test]
        public void Constructor_TabBarHasTopBorderRadius()
        {
            var bar = _tabView.hierarchy[0];
            Assert.AreEqual(Tokens.BorderRadius.MD, bar.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, bar.style.borderTopRightRadius.value.value);
        }

        [Test]
        public void Constructor_ContentAreaHasPadding()
        {
            var content = _tabView.hierarchy[1];
            Assert.AreEqual(Tokens.Spacing.Lg, content.style.paddingTop.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, content.style.paddingBottom.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, content.style.paddingLeft.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, content.style.paddingRight.value.value);
        }

        [Test]
        public void Constructor_SelectedIndexIsNegativeOne()
        {
            Assert.AreEqual(-1, _tabView.SelectedIndex);
        }

        [Test]
        public void Constructor_TabCountIsZero()
        {
            Assert.AreEqual(0, _tabView.TabCount);
        }

        #endregion

        #region AddTab Tests

        [Test]
        public void AddTab_ReturnsTabViewForChaining()
        {
            var result = _tabView.AddTab("Tab 1", new VisualElement());
            Assert.AreSame(_tabView, result);
        }

        [Test]
        public void AddTab_IncrementsTabCount()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            Assert.AreEqual(1, _tabView.TabCount);

            _tabView.AddTab("Tab 2", new VisualElement());
            Assert.AreEqual(2, _tabView.TabCount);
        }

        [Test]
        public void AddTab_AddsButtonToBar()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var bar = _tabView.hierarchy[0];
            Assert.AreEqual(1, bar.childCount);
        }

        [Test]
        public void AddTab_AddsContentToContentArea()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var contentArea = _tabView.hierarchy[1];
            Assert.AreEqual(1, contentArea.childCount);
        }

        [Test]
        public void AddTab_TabButtonHasCorrectClass()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var tabButton = bar[0];
            Assert.IsTrue(tabButton.ClassListContains("j-tab-view__tab"));
        }

        [Test]
        public void AddTab_TabButtonHasCorrectText()
        {
            _tabView.AddTab("My Tab", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var tabButton = bar[0] as Label;
            Assert.AreEqual("My Tab", tabButton.text);
        }

        [Test]
        public void AddTab_FirstTab_AutoSelects()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            Assert.AreEqual(0, _tabView.SelectedIndex);
        }

        [Test]
        public void AddTab_SecondTab_DoesNotAutoSelect()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());

            Assert.AreEqual(0, _tabView.SelectedIndex);
        }

        [Test]
        public void AddTab_FirstTabContent_IsVisible()
        {
            var content = new VisualElement();
            _tabView.AddTab("Tab 1", content);

            Assert.AreEqual(DisplayStyle.Flex, content.style.display.value);
        }

        [Test]
        public void AddTab_SecondTabContent_IsHidden()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            var content2 = new VisualElement();
            _tabView.AddTab("Tab 2", content2);

            Assert.AreEqual(DisplayStyle.None, content2.style.display.value);
        }

        [Test]
        public void AddTab_TabButtonHasSmallFontSize()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var tabButton = bar[0] as Label;
            Assert.AreEqual(Tokens.FontSize.Sm, tabButton.style.fontSize.value.value);
        }

        [Test]
        public void AddTab_TabButtonHasPadding()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var tabButton = bar[0] as Label;
            Assert.AreEqual(Tokens.Spacing.Sm, tabButton.style.paddingTop.value.value);
            Assert.AreEqual(Tokens.Spacing.Sm, tabButton.style.paddingBottom.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, tabButton.style.paddingLeft.value.value);
            Assert.AreEqual(Tokens.Spacing.Lg, tabButton.style.paddingRight.value.value);
        }

        [Test]
        public void AddTab_MultipleTabButtons_AddedToBar()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());
            _tabView.AddTab("Tab 3", new VisualElement());

            var bar = _tabView.hierarchy[0];
            Assert.AreEqual(3, bar.childCount);
        }

        [Test]
        public void AddTab_MultipleContentPanels_AddedToContentArea()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());
            _tabView.AddTab("Tab 3", new VisualElement());

            var contentArea = _tabView.hierarchy[1];
            Assert.AreEqual(3, contentArea.childCount);
        }

        #endregion

        #region SelectTab Tests

        [Test]
        public void SelectTab_UpdatesSelectedIndex()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());

            _tabView.SelectTab(1);

            Assert.AreEqual(1, _tabView.SelectedIndex);
        }

        [Test]
        public void SelectTab_ShowsSelectedContent()
        {
            var content1 = new VisualElement();
            var content2 = new VisualElement();
            _tabView.AddTab("Tab 1", content1);
            _tabView.AddTab("Tab 2", content2);

            _tabView.SelectTab(1);

            Assert.AreEqual(DisplayStyle.Flex, content2.style.display.value);
        }

        [Test]
        public void SelectTab_HidesPreviousContent()
        {
            var content1 = new VisualElement();
            var content2 = new VisualElement();
            _tabView.AddTab("Tab 1", content1);
            _tabView.AddTab("Tab 2", content2);

            _tabView.SelectTab(1);

            Assert.AreEqual(DisplayStyle.None, content1.style.display.value);
        }

        [Test]
        public void SelectTab_NegativeIndex_DoesNothing()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            _tabView.SelectTab(-1);

            Assert.AreEqual(0, _tabView.SelectedIndex);
        }

        [Test]
        public void SelectTab_OutOfRangeIndex_DoesNothing()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            _tabView.SelectTab(5);

            Assert.AreEqual(0, _tabView.SelectedIndex);
        }

        [Test]
        public void SelectTab_SwitchBackAndForth()
        {
            var content1 = new VisualElement();
            var content2 = new VisualElement();
            var content3 = new VisualElement();
            _tabView.AddTab("Tab 1", content1);
            _tabView.AddTab("Tab 2", content2);
            _tabView.AddTab("Tab 3", content3);

            _tabView.SelectTab(2);
            Assert.AreEqual(DisplayStyle.Flex, content3.style.display.value);
            Assert.AreEqual(DisplayStyle.None, content1.style.display.value);

            _tabView.SelectTab(0);
            Assert.AreEqual(DisplayStyle.Flex, content1.style.display.value);
            Assert.AreEqual(DisplayStyle.None, content3.style.display.value);
        }

        #endregion

        #region Styling Tests

        [Test]
        public void ActiveTab_HasPrimaryBackground()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var tabButton = bar[0] as Label;
            Assert.AreEqual(Tokens.Colors.Primary, tabButton.style.backgroundColor.value);
        }

        [Test]
        public void ActiveTab_HasPrimaryTextColor()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var tabButton = bar[0] as Label;
            Assert.AreEqual(Tokens.Colors.PrimaryText, tabButton.style.color.value);
        }

        [Test]
        public void ActiveTab_HasBoldFont()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var tabButton = bar[0] as Label;
            Assert.AreEqual(FontStyle.Bold, tabButton.style.unityFontStyleAndWeight.value);
        }

        [Test]
        public void InactiveTab_HasSecondaryTextColor()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var inactiveTab = bar[1] as Label;
            Assert.AreEqual(Tokens.Colors.TextSecondary, inactiveTab.style.color.value);
        }

        [Test]
        public void InactiveTab_HasNormalFont()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var inactiveTab = bar[1] as Label;
            Assert.AreEqual(FontStyle.Normal, inactiveTab.style.unityFontStyleAndWeight.value);
        }

        [Test]
        public void SelectTab_PreviousTab_BecomesInactive()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());

            _tabView.SelectTab(1);

            var bar = _tabView.hierarchy[0];
            var previousTab = bar[0] as Label;
            Assert.AreEqual(Tokens.Colors.TextSecondary, previousTab.style.color.value);
            Assert.AreEqual(FontStyle.Normal, previousTab.style.unityFontStyleAndWeight.value);
        }

        [Test]
        public void SelectTab_NewTab_BecomesActive()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());

            _tabView.SelectTab(1);

            var bar = _tabView.hierarchy[0];
            var activeTab = bar[1] as Label;
            Assert.AreEqual(Tokens.Colors.Primary, activeTab.style.backgroundColor.value);
            Assert.AreEqual(Tokens.Colors.PrimaryText, activeTab.style.color.value);
            Assert.AreEqual(FontStyle.Bold, activeTab.style.unityFontStyleAndWeight.value);
        }

        #endregion

        #region Border Radius Tests

        [Test]
        public void AddTab_TabButtonHasBorderRadius()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var tabButton = bar[0] as Label;
            Assert.AreEqual(Tokens.BorderRadius.MD, tabButton.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, tabButton.style.borderTopRightRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, tabButton.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, tabButton.style.borderBottomRightRadius.value.value);
        }

        [Test]
        public void AddTab_TabButtonHasMargin()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var tabButton = bar[0] as Label;
            Assert.AreEqual(Tokens.Spacing.Xs, tabButton.style.marginTop.value.value);
            Assert.AreEqual(Tokens.Spacing.Xs, tabButton.style.marginBottom.value.value);
            Assert.AreEqual(Tokens.Spacing.Xs, tabButton.style.marginLeft.value.value);
            Assert.AreEqual(Tokens.Spacing.Xs, tabButton.style.marginRight.value.value);
        }

        [Test]
        public void AddTab_TabButtonHasCenterTextAlign()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var tabButton = bar[0] as Label;
            Assert.AreEqual(TextAnchor.MiddleCenter, tabButton.style.unityTextAlign.value);
        }

        #endregion

        #region MaxTabsPerRow Tests

        [Test]
        public void Constructor_DefaultMaxTabsPerRow_IsZero()
        {
            Assert.AreEqual(0, _tabView.MaxTabsPerRow);
        }

        [Test]
        public void Constructor_WithMaxTabsPerRow_StoresValue()
        {
            var tabView = new JTabView(maxTabsPerRow: 3);
            Assert.AreEqual(3, tabView.MaxTabsPerRow);
        }

        [Test]
        public void AddTab_WithMaxTabsPerRow_SetsFlexBasisPercent()
        {
            var tabView = new JTabView(maxTabsPerRow: 3);
            tabView.AddTab("Tab 1", new VisualElement());

            var bar = tabView.hierarchy[0];
            var tabButton = bar[0] as Label;

            // (100 - 3*2) / 3 = 31.33...%
            var basis = tabButton.style.flexBasis; // StyleLength
            Assert.AreEqual(LengthUnit.Percent, basis.value.unit);
            Assert.AreEqual((100f - (3f * 2f)) / 3f, basis.value.value, 0.01f);
        }

        [Test]
        public void AddTab_WithMaxTabsPerRow_SetsFlexGrow()
        {
            var tabView = new JTabView(maxTabsPerRow: 3);
            tabView.AddTab("Tab 1", new VisualElement());

            var bar = tabView.hierarchy[0];
            var tabButton = bar[0] as Label;
            Assert.AreEqual(1f, tabButton.style.flexGrow.value);
        }

        [Test]
        public void AddTab_WithoutMaxTabsPerRow_NoFlexBasis()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var tabButton = bar[0] as Label;

            // Default (no maxTabsPerRow) should not set flex-basis
            Assert.AreEqual(StyleKeyword.Null, tabButton.style.flexBasis.keyword);
        }

        [Test]
        public void AddTab_MaxTabsPerRow2_SetsCorrectBasis()
        {
            var tabView = new JTabView(maxTabsPerRow: 2);
            tabView.AddTab("Tab 1", new VisualElement());

            var bar = tabView.hierarchy[0];
            var tabButton = bar[0] as Label;

            // (100 - 2*2) / 2 = 48%
            Assert.AreEqual((100f - (2f * 2f)) / 2f, tabButton.style.flexBasis.value.value, 0.01f);
        }

        #endregion

        #region Multiple Tabs Content Visibility Tests

        [Test]
        public void SixTabs_OnlySelectedContentVisible()
        {
            var contents = new VisualElement[6];
            for (int i = 0; i < 6; i++)
            {
                contents[i] = new VisualElement();
                _tabView.AddTab($"Tab {i + 1}", contents[i]);
            }

            // First tab should be selected by default
            Assert.AreEqual(DisplayStyle.Flex, contents[0].style.display.value);
            for (int i = 1; i < 6; i++)
            {
                Assert.AreEqual(DisplayStyle.None, contents[i].style.display.value, $"Content {i} should be hidden");
            }

            // Select tab 3
            _tabView.SelectTab(3);
            Assert.AreEqual(DisplayStyle.Flex, contents[3].style.display.value);
            Assert.AreEqual(DisplayStyle.None, contents[0].style.display.value);
        }

        #endregion

        #region Event Handler Tests

        [Test]
        public void OnTabClicked_SelectsCorrectTab()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());

            Assert.AreEqual(0, _tabView.SelectedIndex);

            var bar = _tabView.hierarchy[0];
            var secondTab = bar[1] as Label;

            var method = typeof(JTabView).GetMethod("OnTabClicked",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "OnTabClicked method should exist");

            var evt = (MouseDownEvent)Activator.CreateInstance(typeof(MouseDownEvent), true);
            evt.currentTarget = secondTab;
            method.Invoke(null, new object[] { evt });

            Assert.AreEqual(1, _tabView.SelectedIndex);
        }

        [Test]
        public void OnTabClicked_WithNonLabelTarget_DoesNothing()
        {
            _tabView.AddTab("Tab 1", new VisualElement());

            var method = typeof(JTabView).GetMethod("OnTabClicked",
                BindingFlags.NonPublic | BindingFlags.Static);

            var evt = (MouseDownEvent)Activator.CreateInstance(typeof(MouseDownEvent), true);
            evt.currentTarget = new VisualElement();
            method.Invoke(null, new object[] { evt });

            Assert.AreEqual(0, _tabView.SelectedIndex);
        }

        [Test]
        public void OnTabMouseEnter_InactiveTab_SetsHoverBackground()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var inactiveTab = bar[1] as Label;

            var method = typeof(JTabView).GetMethod("OnTabMouseEnter",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "OnTabMouseEnter method should exist");

            var evt = (MouseEnterEvent)Activator.CreateInstance(typeof(MouseEnterEvent), true);
            evt.currentTarget = inactiveTab;
            method.Invoke(_tabView, new object[] { evt });

            Assert.AreEqual(Tokens.Colors.BgHover, inactiveTab.style.backgroundColor.value);
        }

        [Test]
        public void OnTabMouseEnter_ActiveTab_DoesNotChangeBackground()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var activeTab = bar[0] as Label;
            var expectedBg = activeTab.style.backgroundColor.value;

            var method = typeof(JTabView).GetMethod("OnTabMouseEnter",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var evt = (MouseEnterEvent)Activator.CreateInstance(typeof(MouseEnterEvent), true);
            evt.currentTarget = activeTab;
            method.Invoke(_tabView, new object[] { evt });

            Assert.AreEqual(expectedBg, activeTab.style.backgroundColor.value);
        }

        [Test]
        public void OnTabMouseLeave_InactiveTab_ClearsBackground()
        {
            _tabView.AddTab("Tab 1", new VisualElement());
            _tabView.AddTab("Tab 2", new VisualElement());

            var bar = _tabView.hierarchy[0];
            var inactiveTab = bar[1] as Label;

            // First hover
            var enterMethod = typeof(JTabView).GetMethod("OnTabMouseEnter",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var enterEvt = (MouseEnterEvent)Activator.CreateInstance(typeof(MouseEnterEvent), true);
            enterEvt.target = inactiveTab;
            enterMethod.Invoke(_tabView, new object[] { enterEvt });

            // Then leave
            var leaveMethod = typeof(JTabView).GetMethod("OnTabMouseLeave",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var leaveEvt = (MouseLeaveEvent)Activator.CreateInstance(typeof(MouseLeaveEvent), true);
            leaveEvt.target = inactiveTab;
            leaveMethod.Invoke(_tabView, new object[] { leaveEvt });

            Assert.AreEqual(StyleKeyword.None, inactiveTab.style.backgroundColor.keyword);
        }

        #endregion
    }
}
