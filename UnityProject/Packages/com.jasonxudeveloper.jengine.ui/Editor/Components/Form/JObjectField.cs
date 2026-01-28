// JObjectField.cs
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
using JEngine.UI.Editor.Theming;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace JEngine.UI.Editor.Components.Form
{
    /// <summary>
    /// A styled object field matching the JEngine dark theme.
    /// </summary>
    public class JObjectField : VisualElement
    {
        private readonly ObjectField _objectField;

        /// <summary>
        /// Creates a new styled object field.
        /// </summary>
        /// <param name="objectType">The type of object to accept.</param>
        /// <param name="allowSceneObjects">Whether to allow scene objects.</param>
        public JObjectField(Type objectType, bool allowSceneObjects = true)
        {
            AddToClassList("j-object-field");

            _objectField = new ObjectField
            {
                objectType = objectType,
                allowSceneObjects = allowSceneObjects
            };

            // Style the container
            style.flexGrow = 1;
            style.flexShrink = 1;
            style.minWidth = 80;
            style.minHeight = 20;
            style.maxHeight = 24;
            style.alignSelf = Align.Center;

            _objectField.style.flexGrow = 1;
            _objectField.style.marginLeft = 0;
            _objectField.style.marginRight = 0;
            _objectField.style.marginTop = 0;
            _objectField.style.marginBottom = 0;
            _objectField.style.height = 22;

            _objectField.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            Add(_objectField);
        }

        private void ApplyInternalStyles()
        {
            // Style the input container
            var input = _objectField.Q(className: "unity-object-field__input");
            if (input != null)
            {
                input.style.backgroundColor = Tokens.Colors.BgSurface;
                input.style.borderTopColor = Tokens.Colors.BorderSubtle;
                input.style.borderRightColor = Tokens.Colors.BorderSubtle;
                input.style.borderBottomColor = Tokens.Colors.BorderSubtle;
                input.style.borderLeftColor = Tokens.Colors.BorderSubtle;
                input.style.borderTopWidth = 1;
                input.style.borderRightWidth = 1;
                input.style.borderBottomWidth = 1;
                input.style.borderLeftWidth = 1;
                input.style.borderTopLeftRadius = Tokens.BorderRadius.Sm;
                input.style.borderTopRightRadius = Tokens.BorderRadius.Sm;
                input.style.borderBottomLeftRadius = Tokens.BorderRadius.Sm;
                input.style.borderBottomRightRadius = Tokens.BorderRadius.Sm;
                input.style.paddingLeft = Tokens.Spacing.Sm;
                input.style.paddingRight = Tokens.Spacing.Sm;
                input.style.minHeight = 22;
                input.style.height = 22;
                input.style.alignItems = Align.Center;

                // Hover effect
                input.RegisterCallback<MouseEnterEvent, VisualElement>(static (_, element) =>
                {
                    element.style.backgroundColor = Tokens.Colors.BgHover;
                }, input);

                input.RegisterCallback<MouseLeaveEvent, VisualElement>(static (_, element) =>
                {
                    element.style.backgroundColor = Tokens.Colors.BgSurface;
                }, input);
            }

            // Style the object display container (icon + label)
            var display = _objectField.Q(className: "unity-object-field__display");
            if (display != null)
            {
                display.style.alignItems = Align.Center;
                display.style.height = 20;
            }

            // Style the object label - ensure vertical centering
            var objectLabel = _objectField.Q(className: "unity-object-field-display__label");
            if (objectLabel != null)
            {
                objectLabel.style.color = Tokens.Colors.TextPrimary;
                objectLabel.style.fontSize = Tokens.FontSize.Sm;
                objectLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                objectLabel.style.alignSelf = Align.Center;
            }

            // Style the icon
            var icon = _objectField.Q(className: "unity-object-field-display__icon");
            if (icon != null)
            {
                icon.style.alignSelf = Align.Center;
            }

            // Hide the field label if present
            var label = _objectField.Q<Label>(className: "unity-base-field__label");
            if (label != null)
            {
                label.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Gets or sets the object value.
        /// </summary>
        public Object Value
        {
            get => _objectField.value;
            set => _objectField.value = value;
        }

        /// <summary>
        /// Gets the internal ObjectField.
        /// </summary>
        public ObjectField ObjectField => _objectField;

        /// <summary>
        /// Binds to a serialized property.
        /// </summary>
        public void BindProperty(SerializedProperty property)
        {
            _objectField.BindProperty(property);
        }

        /// <summary>
        /// Registers a callback for value changes.
        /// </summary>
        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<Object>> callback)
        {
            _objectField.RegisterValueChangedCallback(callback);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            ApplyInternalStyles();
        }
    }

    /// <summary>
    /// A generic styled object field matching the JEngine dark theme.
    /// </summary>
    public class JObjectField<T> : VisualElement where T : Object
    {
        private readonly ObjectField _objectField;

        /// <summary>
        /// Creates a new styled object field.
        /// </summary>
        /// <param name="allowSceneObjects">Whether to allow scene objects.</param>
        public JObjectField(bool allowSceneObjects = true)
        {
            AddToClassList("j-object-field");

            _objectField = new ObjectField
            {
                objectType = typeof(T),
                allowSceneObjects = allowSceneObjects
            };

            // Style the container
            style.flexGrow = 1;
            style.flexShrink = 1;
            style.minWidth = 80;
            style.minHeight = 20;
            style.maxHeight = 24;
            style.alignSelf = Align.Center;

            _objectField.style.flexGrow = 1;
            _objectField.style.marginLeft = 0;
            _objectField.style.marginRight = 0;
            _objectField.style.marginTop = 0;
            _objectField.style.marginBottom = 0;
            _objectField.style.height = 22;

            _objectField.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            Add(_objectField);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            ApplyInternalStyles();
        }

        private void ApplyInternalStyles()
        {
            // Style the input container
            var input = _objectField.Q(className: "unity-object-field__input");
            if (input != null)
            {
                input.style.backgroundColor = Tokens.Colors.BgSurface;
                input.style.borderTopColor = Tokens.Colors.BorderSubtle;
                input.style.borderRightColor = Tokens.Colors.BorderSubtle;
                input.style.borderBottomColor = Tokens.Colors.BorderSubtle;
                input.style.borderLeftColor = Tokens.Colors.BorderSubtle;
                input.style.borderTopWidth = 1;
                input.style.borderRightWidth = 1;
                input.style.borderBottomWidth = 1;
                input.style.borderLeftWidth = 1;
                input.style.borderTopLeftRadius = Tokens.BorderRadius.Sm;
                input.style.borderTopRightRadius = Tokens.BorderRadius.Sm;
                input.style.borderBottomLeftRadius = Tokens.BorderRadius.Sm;
                input.style.borderBottomRightRadius = Tokens.BorderRadius.Sm;
                input.style.paddingLeft = Tokens.Spacing.Sm;
                input.style.paddingRight = Tokens.Spacing.Sm;
                input.style.minHeight = 22;
                input.style.height = 22;
                input.style.alignItems = Align.Center;

                // Hover effect
                input.RegisterCallback<MouseEnterEvent, VisualElement>(static (_, element) =>
                {
                    element.style.backgroundColor = Tokens.Colors.BgHover;
                }, input);

                input.RegisterCallback<MouseLeaveEvent, VisualElement>(static (_, element) =>
                {
                    element.style.backgroundColor = Tokens.Colors.BgSurface;
                }, input);
            }

            // Style the object display container (icon + label)
            var display = _objectField.Q(className: "unity-object-field__display");
            if (display != null)
            {
                display.style.alignItems = Align.Center;
                display.style.height = 20;
            }

            // Style the object label - ensure vertical centering
            var objectLabel = _objectField.Q(className: "unity-object-field-display__label");
            if (objectLabel != null)
            {
                objectLabel.style.color = Tokens.Colors.TextPrimary;
                objectLabel.style.fontSize = Tokens.FontSize.Sm;
                objectLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                objectLabel.style.alignSelf = Align.Center;
            }

            // Style the icon
            var icon = _objectField.Q(className: "unity-object-field-display__icon");
            if (icon != null)
            {
                icon.style.alignSelf = Align.Center;
            }

            // Hide the field label if present
            var label = _objectField.Q<Label>(className: "unity-base-field__label");
            if (label != null)
            {
                label.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Gets or sets the object value.
        /// </summary>
        public T Value
        {
            get => _objectField.value as T;
            set => _objectField.value = value;
        }

        /// <summary>
        /// Gets the internal ObjectField.
        /// </summary>
        public ObjectField ObjectField => _objectField;

        /// <summary>
        /// Binds to a serialized property.
        /// </summary>
        public void BindProperty(SerializedProperty property)
        {
            _objectField.BindProperty(property);
        }

        /// <summary>
        /// Registers a callback for value changes.
        /// </summary>
        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<Object>> callback)
        {
            _objectField.RegisterValueChangedCallback(callback);
        }
    }
}
