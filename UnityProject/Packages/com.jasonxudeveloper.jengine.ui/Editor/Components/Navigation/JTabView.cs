// JTabView.cs
//
//  Author:
//        JasonXuDeveloper <jason@xgamedev.net>
//
//  Copyright (c) 2025 JEngine
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.

using System.Collections.Generic;
using JEngine.UI.Editor.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Components.Navigation
{
    /// <summary>
    /// A tabbed container that shows one content panel at a time.
    /// Each tab has a button in the tab bar and an associated content element.
    /// </summary>
    public class JTabView : VisualElement
    {
        private readonly VisualElement _tabBar;
        private readonly VisualElement _contentArea;
        private readonly List<Label> _tabButtons = new();
        private readonly List<VisualElement> _contentPanels = new();
        private int _selectedIndex = -1;
        private int _maxTabsPerRow;

        /// <summary>
        /// Creates a new tab view with a tab bar and content area.
        /// </summary>
        /// <param name="maxTabsPerRow">Maximum number of tabs per row. 0 means no limit (auto-wrap).</param>
        public JTabView(int maxTabsPerRow = 0)
        {
            _maxTabsPerRow = maxTabsPerRow;

            AddToClassList("j-tab-view");

            // Tab bar - horizontal row of tab buttons
            _tabBar = new VisualElement();
            _tabBar.AddToClassList("j-tab-view__bar");
            _tabBar.style.flexDirection = FlexDirection.Row;
            _tabBar.style.flexWrap = Wrap.Wrap;
            _tabBar.style.backgroundColor = Tokens.Colors.BgSurface;
            _tabBar.style.borderBottomWidth = 1;
            _tabBar.style.borderBottomColor = Tokens.Colors.BorderSubtle;
            _tabBar.style.borderTopLeftRadius = Tokens.BorderRadius.MD;
            _tabBar.style.borderTopRightRadius = Tokens.BorderRadius.MD;
            hierarchy.Add(_tabBar);

            // Content area
            _contentArea = new VisualElement();
            _contentArea.AddToClassList("j-tab-view__content");
            _contentArea.style.paddingTop = Tokens.Spacing.Lg;
            _contentArea.style.paddingBottom = Tokens.Spacing.Lg;
            _contentArea.style.paddingLeft = Tokens.Spacing.Lg;
            _contentArea.style.paddingRight = Tokens.Spacing.Lg;
            hierarchy.Add(_contentArea);
        }

        /// <summary>
        /// Gets the currently selected tab index, or -1 if no tabs exist.
        /// </summary>
        public int SelectedIndex => _selectedIndex;

        /// <summary>
        /// Gets the number of tabs.
        /// </summary>
        public int TabCount => _tabButtons.Count;

        /// <summary>
        /// Gets the maximum number of tabs per row. 0 means no limit (auto-wrap).
        /// </summary>
        public int MaxTabsPerRow => _maxTabsPerRow;

        /// <summary>
        /// Adds a tab with the given label and content.
        /// The first tab added is automatically selected.
        /// </summary>
        /// <param name="label">The tab button text.</param>
        /// <param name="content">The content element shown when this tab is active.</param>
        /// <returns>This tab view for fluent chaining.</returns>
        public JTabView AddTab(string label, VisualElement content)
        {
            var index = _tabButtons.Count;

            // Create tab button
            var tabButton = new Label(label);
            tabButton.AddToClassList("j-tab-view__tab");
            tabButton.style.fontSize = Tokens.FontSize.Sm;
            tabButton.style.paddingTop = Tokens.Spacing.Sm;
            tabButton.style.paddingBottom = Tokens.Spacing.Sm;
            tabButton.style.paddingLeft = Tokens.Spacing.Lg;
            tabButton.style.paddingRight = Tokens.Spacing.Lg;
            tabButton.style.unityTextAlign = TextAnchor.MiddleCenter;

            // Rounded corners like buttons
            tabButton.style.borderTopLeftRadius = Tokens.BorderRadius.MD;
            tabButton.style.borderTopRightRadius = Tokens.BorderRadius.MD;
            tabButton.style.borderBottomLeftRadius = Tokens.BorderRadius.MD;
            tabButton.style.borderBottomRightRadius = Tokens.BorderRadius.MD;
            tabButton.style.marginTop = Tokens.Spacing.Xs;
            tabButton.style.marginBottom = Tokens.Spacing.Xs;
            tabButton.style.marginLeft = Tokens.Spacing.Xs;
            tabButton.style.marginRight = Tokens.Spacing.Xs;

            // Constrain tabs per row via percentage width
            if (_maxTabsPerRow > 0)
            {
                // Reduce basis to account for per-tab margins; flexGrow fills remaining space
                var percent = (100f - (_maxTabsPerRow * 2f)) / _maxTabsPerRow;
                tabButton.style.flexBasis = new StyleLength(new Length(percent, LengthUnit.Percent));
                tabButton.style.flexGrow = 1;
                tabButton.style.flexShrink = 1;
            }

            JTheme.ApplyTransition(tabButton);
            JTheme.ApplyPointerCursor(tabButton);

            // Click handler using closure-free pattern via userData
            tabButton.userData = index;
            tabButton.RegisterCallback<MouseDownEvent>(OnTabClicked);

            // Hover handlers
            tabButton.RegisterCallback<MouseEnterEvent>(OnTabMouseEnter);
            tabButton.RegisterCallback<MouseLeaveEvent>(OnTabMouseLeave);

            _tabButtons.Add(tabButton);
            _tabBar.Add(tabButton);

            // Add content panel (hidden by default)
            content.style.display = DisplayStyle.None;
            _contentPanels.Add(content);
            _contentArea.Add(content);

            // Auto-select first tab
            if (_tabButtons.Count == 1)
            {
                SelectTab(0);
            }
            else
            {
                ApplyInactiveStyle(tabButton);
            }

            return this;
        }

        /// <summary>
        /// Selects the tab at the given index.
        /// </summary>
        /// <param name="index">The zero-based tab index.</param>
        public void SelectTab(int index)
        {
            if (index < 0 || index >= _tabButtons.Count)
                return;

            // Deselect previous tab
            if (_selectedIndex >= 0 && _selectedIndex < _tabButtons.Count)
            {
                ApplyInactiveStyle(_tabButtons[_selectedIndex]);
                _contentPanels[_selectedIndex].style.display = DisplayStyle.None;
            }

            _selectedIndex = index;

            // Select new tab
            ApplyActiveStyle(_tabButtons[index]);
            _contentPanels[index].style.display = DisplayStyle.Flex;
        }

        private static void ApplyActiveStyle(Label tab)
        {
            tab.style.backgroundColor = Tokens.Colors.Primary;
            tab.style.color = Tokens.Colors.PrimaryText;
            tab.style.unityFontStyleAndWeight = FontStyle.Bold;
        }

        private static void ApplyInactiveStyle(Label tab)
        {
            tab.style.backgroundColor = StyleKeyword.None;
            tab.style.color = Tokens.Colors.TextSecondary;
            tab.style.unityFontStyleAndWeight = FontStyle.Normal;
        }

        private static void OnTabClicked(MouseDownEvent evt)
        {
            if (evt.target is Label tab && tab.userData is int index)
            {
                // Walk up to find the JTabView parent
                var parent = tab.parent?.parent;
                if (parent is JTabView tabView)
                {
                    tabView.SelectTab(index);
                }
            }
        }

        private void OnTabMouseEnter(MouseEnterEvent evt)
        {
            if (evt.target is Label tab && tab.userData is int index && index != _selectedIndex)
            {
                tab.style.backgroundColor = Tokens.Colors.BgHover;
            }
        }

        private void OnTabMouseLeave(MouseLeaveEvent evt)
        {
            if (evt.target is Label tab && tab.userData is int index && index != _selectedIndex)
            {
                tab.style.backgroundColor = StyleKeyword.None;
            }
        }
    }
}
