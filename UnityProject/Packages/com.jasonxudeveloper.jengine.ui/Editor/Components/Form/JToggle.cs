// JToggle.cs
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

using System;
using System.Collections.Generic;
using JEngine.UI.Editor.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Components.Form
{
    /// <summary>
    /// A styled toggle switch that adapts to the editor theme.
    /// Dark mode: Cyan accent when on
    /// Light mode: Black/grey (shadcn style)
    /// </summary>
    public class JToggle : VisualElement
    {
        private readonly VisualElement _track;
        private readonly VisualElement _thumb;
        private bool _value;
        private Action<bool> _onValueChanged;

        // Track colors (same as button colors)
        private static Color TrackOffColor => Tokens.Colors.Secondary;
        private static Color TrackOnColor => Tokens.Colors.Primary;

        // Thumb colors (dynamic based on state and theme)
        private static Color ThumbOffColor => Tokens.Colors.ToggleThumbOff;
        private static Color ThumbOnColor => Tokens.Colors.ToggleThumbOn;

        /// <summary>
        /// Creates a new styled toggle.
        /// </summary>
        /// <param name="initialValue">Initial toggle state.</param>
        public JToggle(bool initialValue = false)
        {
            AddToClassList("j-toggle");

            _value = initialValue;

            // Container styling - compact toggle switch
            style.width = 36;
            style.height = 18;
            style.flexShrink = 0;
            style.flexGrow = 0;
            style.alignSelf = Align.Center;

            // Track (background)
            _track = new VisualElement();
            _track.AddToClassList("j-toggle__track");
            _track.style.position = Position.Absolute;
            _track.style.top = 0;
            _track.style.left = 0;
            _track.style.right = 0;
            _track.style.bottom = 0;
            _track.style.borderTopLeftRadius = 9;
            _track.style.borderTopRightRadius = 9;
            _track.style.borderBottomLeftRadius = 9;
            _track.style.borderBottomRightRadius = 9;
            // Smooth transition for track color
            _track.style.transitionProperty = new List<StylePropertyName> { new("background-color") };
            _track.style.transitionDuration = new List<TimeValue> { new(150, TimeUnit.Millisecond) };
            Add(_track);

            // Thumb (circle)
            _thumb = new VisualElement();
            _thumb.AddToClassList("j-toggle__thumb");
            _thumb.style.position = Position.Absolute;
            _thumb.style.width = 14;
            _thumb.style.height = 14;
            _thumb.style.top = 2;
            _thumb.style.borderTopLeftRadius = 7;
            _thumb.style.borderTopRightRadius = 7;
            _thumb.style.borderBottomLeftRadius = 7;
            _thumb.style.borderBottomRightRadius = 7;
            // Smooth transition for thumb movement and color
            _thumb.style.transitionProperty = new List<StylePropertyName> { new("left"), new("background-color") };
            _thumb.style.transitionDuration = new List<TimeValue> { new(150, TimeUnit.Millisecond) };
            _thumb.style.transitionTimingFunction = new List<EasingFunction> { new(EasingMode.EaseInOut) };
            Add(_thumb);

            // Pointer cursor on all elements
            JTheme.ApplyPointerCursor(this);
            JTheme.ApplyPointerCursor(_track);
            JTheme.ApplyPointerCursor(_thumb);

            // Apply initial state
            UpdateVisuals();

            // Click handler
            RegisterCallback<ClickEvent>(OnClick);

            // Hover effect - change track color
            RegisterCallback<MouseEnterEvent, JToggle>(static (_, toggle) =>
            {
                toggle._track.style.backgroundColor = toggle._value
                    ? Tokens.Colors.PrimaryHover
                    : Tokens.Colors.SecondaryHover;
            }, this);

            RegisterCallback<MouseLeaveEvent, JToggle>(static (_, toggle) =>
            {
                toggle.UpdateVisuals();
            }, this);
        }

        private void OnClick(ClickEvent evt)
        {
            _value = !_value;
            UpdateVisuals();
            _onValueChanged?.Invoke(_value);
        }

        private void UpdateVisuals()
        {
            _track.style.backgroundColor = _value ? TrackOnColor : TrackOffColor;
            _thumb.style.left = _value ? 20 : 2;
            _thumb.style.backgroundColor = _value ? ThumbOnColor : ThumbOffColor;
        }

        /// <summary>
        /// Gets or sets the toggle value.
        /// </summary>
        public bool Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    UpdateVisuals();
                }
            }
        }

        /// <summary>
        /// Sets the value without triggering the callback.
        /// </summary>
        public void SetValueWithoutNotify(bool value)
        {
            _value = value;
            UpdateVisuals();
        }

        /// <summary>
        /// Registers a callback for value changes.
        /// </summary>
        public JToggle OnValueChanged(Action<bool> callback)
        {
            _onValueChanged = callback;
            return this;
        }

        /// <summary>
        /// Adds a CSS class.
        /// </summary>
        public JToggle WithClass(string className)
        {
            AddToClassList(className);
            return this;
        }
    }
}
