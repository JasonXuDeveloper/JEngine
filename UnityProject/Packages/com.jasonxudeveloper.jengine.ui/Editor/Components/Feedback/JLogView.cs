// JLogView.cs
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

using JEngine.UI.Editor.Theming;
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Components.Feedback
{
    /// <summary>
    /// A scrollable log view with optional max lines limit.
    /// </summary>
    public class JLogView : JComponent
    {
        private readonly ScrollView _scrollView;
        private int _maxLines;
        private int _currentLineCount;

        /// <summary>
        /// Creates a new log view with monochrome background.
        /// </summary>
        /// <param name="maxLines">Maximum number of lines to keep (0 = unlimited).</param>
        public JLogView(int maxLines = 100) : base("j-log-view")
        {
            _maxLines = maxLines;
            _currentLineCount = 0;

            // Apply theme-aware background
            // Use input background for consistent control styling
            style.backgroundColor = Tokens.Colors.BgInput;

            // Standard borders
            style.borderTopColor = Tokens.Colors.Border;
            style.borderRightColor = Tokens.Colors.Border;
            style.borderBottomColor = Tokens.Colors.Border;
            style.borderLeftColor = Tokens.Colors.Border;
            style.borderTopWidth = 1;
            style.borderRightWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;

            // Border radius (8px)
            style.borderTopLeftRadius = Tokens.BorderRadius.MD;
            style.borderTopRightRadius = Tokens.BorderRadius.MD;
            style.borderBottomLeftRadius = Tokens.BorderRadius.MD;
            style.borderBottomRightRadius = Tokens.BorderRadius.MD;

            style.minHeight = 100;
            style.maxHeight = 300;
            style.paddingTop = Tokens.Spacing.MD;
            style.paddingRight = Tokens.Spacing.MD;
            style.paddingBottom = Tokens.Spacing.MD;
            style.paddingLeft = Tokens.Spacing.MD;

            // Create scroll view
            _scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            _scrollView.AddToClassList("j-log-view__scroll");
            _scrollView.style.flexGrow = 1;
            _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            Add(_scrollView);
        }

        /// <summary>
        /// Gets the scroll view container.
        /// </summary>
        public ScrollView ScrollView => _scrollView;

        /// <summary>
        /// Gets or sets the maximum number of lines.
        /// </summary>
        public int MaxLines
        {
            get => _maxLines;
            set => _maxLines = value;
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <returns>This log view for chaining.</returns>
        public JLogView LogInfo(string message)
        {
            return Log(message);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <returns>This log view for chaining.</returns>
        public JLogView LogError(string message)
        {
            return Log(message, true);
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="isError">Whether this is an error message.</param>
        /// <returns>This log view for chaining.</returns>
        public JLogView Log(string message, bool isError = false)
        {
            var entry = new Label(message);
            entry.AddToClassList("j-log-view__entry");
            entry.AddToClassList(isError ? "j-log-view__entry--error" : "j-log-view__entry--info");

            entry.style.fontSize = Tokens.FontSize.Sm;
            entry.style.paddingTop = Tokens.Spacing.Xs;
            entry.style.paddingBottom = Tokens.Spacing.Xs;
            entry.style.borderBottomColor = Tokens.Colors.BorderSubtle;
            entry.style.borderBottomWidth = 1;
            entry.style.color = isError ? Tokens.Colors.StatusError : Tokens.Colors.TextSecondary;
            entry.style.whiteSpace = WhiteSpace.Normal;

            _scrollView.Add(entry);
            _currentLineCount++;

            // Remove oldest entries if over limit
            if (_maxLines > 0 && _currentLineCount > _maxLines)
            {
                while (_scrollView.childCount > _maxLines)
                {
                    _scrollView.RemoveAt(0);
                    _currentLineCount--;
                }
            }

            // Scroll to bottom
            _scrollView.ScrollTo(entry);

            return this;
        }

        /// <summary>
        /// Clears all log entries.
        /// </summary>
        /// <returns>This log view for chaining.</returns>
        public new JLogView Clear()
        {
            _scrollView.Clear();
            _currentLineCount = 0;
            return this;
        }

        /// <summary>
        /// Sets the minimum height.
        /// </summary>
        /// <param name="height">The minimum height.</param>
        /// <returns>This log view for chaining.</returns>
        public JLogView WithMinHeight(float height)
        {
            style.minHeight = height;
            return this;
        }

        /// <summary>
        /// Sets the maximum height.
        /// </summary>
        /// <param name="height">The maximum height.</param>
        /// <returns>This log view for chaining.</returns>
        public JLogView WithMaxHeight(float height)
        {
            style.maxHeight = height;
            return this;
        }
    }
}
