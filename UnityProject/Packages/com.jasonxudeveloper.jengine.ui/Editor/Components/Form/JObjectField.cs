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

using JEngine.UI.Editor.Theming;
using JEngine.UI.Editor.Utilities;
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
    /// <typeparam name="T">The type of object to accept.</typeparam>
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

            // Style the container using shared input styles
            JTheme.ApplyInputContainerStyle(this);

            _objectField.style.flexGrow = 1;
            _objectField.style.marginLeft = 0;
            _objectField.style.marginRight = 0;
            _objectField.style.marginTop = 0;
            _objectField.style.marginBottom = 0;

            _objectField.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            Add(_objectField);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            // Apply stylesheets to enable USS cursor classes
            StyleSheetManager.ApplyAllStyleSheets(this);

            ApplyInternalStyles();

            // ObjectField may have elements added after initial attach
            // Schedule a delayed re-application to catch any late additions
            schedule.Execute(() => ApplyCursorToAllDescendants(_objectField)).ExecuteLater(50);
        }

        private void ApplyInternalStyles()
        {
            // Style the input container using shared styles
            var input = _objectField.Q(className: "unity-object-field__input");
            if (input != null)
            {
                JTheme.ApplyInputElementStyle(input);
                input.style.alignItems = Align.Center;
                input.style.justifyContent = Justify.FlexStart;

                // Hover effect
                input.RegisterCallback<MouseEnterEvent, VisualElement>(
                    static (_, element) => JTheme.ApplyInputHoverState(element), input);
                input.RegisterCallback<MouseLeaveEvent, VisualElement>(
                    static (_, element) => JTheme.ApplyInputNormalState(element), input);
            }

            // Style the object display container (icon + label) - ensure vertical centering
            var display = _objectField.Q(className: "unity-object-field__display");
            if (display != null)
            {
                display.style.flexDirection = FlexDirection.Row;
                display.style.alignItems = Align.Center;
                display.style.flexGrow = 1;
                display.style.overflow = Overflow.Hidden;
            }

            // Style the object label using shared text styles
            var objectLabel = _objectField.Q(className: "unity-object-field-display__label");
            if (objectLabel != null)
            {
                JTheme.ApplyInputTextStyle(objectLabel);
                objectLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                objectLabel.style.overflow = Overflow.Hidden;
                objectLabel.style.textOverflow = TextOverflow.Ellipsis;
            }

            // Style the icon - ensure proper vertical alignment
            var icon = _objectField.Q(className: "unity-object-field-display__icon");
            if (icon != null)
            {
                icon.style.alignSelf = Align.Center;
                icon.style.flexShrink = 0;
            }

            // Apply pointer cursor to ALL descendants of ObjectField
            // This ensures we don't miss any internal Unity elements
            ApplyCursorToAllDescendants(_objectField);

            // Also apply to wrapper
            JTheme.ApplyPointerCursor(this);

            // Hide the field label if present
            JTheme.HideFieldLabel(_objectField);
        }

        private void ApplyCursorToAllDescendants(VisualElement root)
        {
            JTheme.ApplyPointerCursor(root);
            foreach (var child in root.Children())
            {
                ApplyCursorToAllDescendants(child);
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
