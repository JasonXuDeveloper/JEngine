// EditorUIUtils.cs
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

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JEngine.Core.Editor.CustomEditor
{
    /// <summary>
    /// Common utility functions for Unity Editor UI Toolkit components
    /// Shared across all editor windows and inspectors
    /// </summary>
    public static class EditorUIUtils
    {
        public enum ButtonType
        {
            Primary, // Main actions (blue)
            Secondary, // Secondary actions (gray)
            Success, // Positive actions (green)
            Danger, // Destructive actions (red)
            Warning // Warning actions (orange)
        }

        public enum TextType
        {
            Normal, // Regular text (13px base)
            Header, // Group headers (16px base)
            Subtitle, // Subtitles (18px base)
            Title // Main titles (20px base)
        }

        /// <summary>
        /// Switches button color by removing all color classes and adding the specified type
        /// </summary>
        public static void SwitchButtonColor(Button button, ButtonType newType)
        {
            // Remove all button color classes
            button.RemoveFromClassList("btn-primary");
            button.RemoveFromClassList("btn-secondary");
            button.RemoveFromClassList("btn-success");
            button.RemoveFromClassList("btn-danger");
            button.RemoveFromClassList("btn-warning");

            // Add the new color class - CSS will handle all styling
            switch (newType)
            {
                case ButtonType.Primary:
                    button.AddToClassList("btn-primary");
                    break;
                case ButtonType.Secondary:
                    button.AddToClassList("btn-secondary");
                    break;
                case ButtonType.Success:
                    button.AddToClassList("btn-success");
                    break;
                case ButtonType.Danger:
                    button.AddToClassList("btn-danger");
                    break;
                case ButtonType.Warning:
                    button.AddToClassList("btn-warning");
                    break;
            }
        }

        /// <summary>
        /// Makes text responsive with dynamic font sizing based on container size
        /// </summary>
        public static void MakeTextResponsive(VisualElement element)
        {
            element.AddToClassList("responsive-text");
            element.RegisterCallback<GeometryChangedEvent, VisualElement>(
                static (_, ele) => { AdjustTextElementFontSize(ele); }, element);
        }

        /// <summary>
        /// Makes title text responsive with largest font size
        /// </summary>
        public static void MakeTitleTextResponsive(VisualElement element)
        {
            element.AddToClassList("responsive-title");
            // Apply initial font size
            element.style.fontSize = 24f;
            element.RegisterCallback<GeometryChangedEvent, VisualElement>(
                static (_, ele) => { AdjustTextElementFontSize(ele, TextType.Title); }, element);
        }

        /// <summary>
        /// Makes subtitle text responsive with medium-large font size
        /// </summary>
        public static void MakeSubtitleTextResponsive(VisualElement element)
        {
            element.AddToClassList("responsive-subtitle");
            // Apply initial font size
            element.style.fontSize = 18f;
            element.RegisterCallback<GeometryChangedEvent, VisualElement>(
                static (_, ele) => { AdjustTextElementFontSize(ele, TextType.Subtitle); }, element);
        }

        /// <summary>
        /// Makes header text responsive with larger base font size
        /// </summary>
        public static void MakeHeaderTextResponsive(VisualElement element)
        {
            element.AddToClassList("responsive-header");
            // Apply initial font size
            element.style.fontSize = 16f;
            element.RegisterCallback<GeometryChangedEvent, VisualElement>(
                static (_, ele) => { AdjustTextElementFontSize(ele, TextType.Header); }, element);
        }

        /// <summary>
        /// Applies responsive text sizing to all child elements recursively
        /// </summary>
        public static void MakeAllTextResponsive(VisualElement root)
        {
            MakeTextResponsive(root);

            // Apply to all children recursively
            for (int i = 0; i < root.childCount; i++)
            {
                MakeAllTextResponsive(root[i]);
            }
        }

        private static void AdjustTextElementFontSize(VisualElement element, TextType textType = TextType.Normal)
        {
            // Get the element's current width for responsive sizing
            var elementWidth = element.resolvedStyle.width;

            // Calculate responsive font size based on available space
            float baseFontSize;
            float scaleFactor = 1f;

            // Different base sizes based on text type
            switch (textType)
            {
                case TextType.Title:
                    baseFontSize = 20f;
                    if (Screen.width < 1200)
                        scaleFactor = 1.0f;
                    else if (Screen.width > 1600)
                        scaleFactor = 1.3f;
                    break;
                case TextType.Subtitle:
                    baseFontSize = 18f;
                    if (Screen.width < 1200)
                        scaleFactor = 1.0f;
                    else if (Screen.width > 1600)
                        scaleFactor = 1.25f;
                    break;
                case TextType.Header:
                    baseFontSize = 16f;
                    if (Screen.width < 1200)
                        scaleFactor = 1.0f;
                    else if (Screen.width > 1600)
                        scaleFactor = 1.2f;
                    break;
                default: // TextType.Normal
                    baseFontSize = 13f;
                    if (Screen.width < 1200)
                        scaleFactor *= 0.95f;
                    else if (Screen.width > 1600)
                        scaleFactor *= 1.1f;
                    break;
            }

            // Special handling for different element types
            if (element is Label)
            {
                // Labels still readable even in constrained spaces
                if (textType == TextType.Normal && elementWidth > 0 && elementWidth < 150)
                    scaleFactor *= 0.95f; // Less aggressive scaling for small labels
            }
            else if (element is TextField || element is PopupField<string> || element is EnumField)
            {
                // Form controls need very readable text (keep current base size)
            }

            float finalFontSize = baseFontSize * scaleFactor;

            // Apply different size constraints based on text type
            switch (textType)
            {
                case TextType.Title:
                    element.style.fontSize = Mathf.Clamp(finalFontSize, 18f, 32f); // Titles: 18px - 32px
                    break;
                case TextType.Subtitle:
                    element.style.fontSize = Mathf.Clamp(finalFontSize, 16f, 28f); // Subtitles: 16px - 28px
                    break;
                case TextType.Header:
                    element.style.fontSize = Mathf.Clamp(finalFontSize, 14f, 24f); // Headers: 14px - 24px
                    break;
                default: // TextType.Normal
                    element.style.fontSize = Mathf.Clamp(finalFontSize, 11f, 18f); // Normal text: 11px - 18px
                    break;
            }
        }

        /// <summary>
        /// Creates a standard form row with label
        /// </summary>
        public static VisualElement CreateFormRow(string labelText)
        {
            var row = new VisualElement();
            row.AddToClassList("form-row");

            // Always add a label element to maintain two-column layout
            var label = new Label(labelText ?? "");
            label.AddToClassList("form-label");
            if (!string.IsNullOrEmpty(labelText))
            {
                MakeTextResponsive(label);
            }

            row.Add(label);

            return row;
        }

        /// <summary>
        /// Creates a group container with header
        /// </summary>
        public static VisualElement CreateGroup(string title)
        {
            var group = new VisualElement();
            group.AddToClassList("group-box");

            var header = new Label(title);
            header.AddToClassList("group-header");
            MakeHeaderTextResponsive(header);
            group.Add(header);

            return group;
        }

        /// <summary>
        /// Creates a flexible button row that wraps buttons responsively
        /// </summary>
        public static VisualElement CreateFlexButtonRow(params Button[] buttons)
        {
            var container = new VisualElement();
            container.AddToClassList("button-row");

            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                // Apply flex button styling via CSS class
                button.AddToClassList("flex-button");

                // Add margin between buttons except for the last one
                if (i < buttons.Length - 1)
                {
                    button.style.marginRight = 4;
                }

                container.Add(button);
            }

            return container;
        }


        /// <summary>
        /// Makes a button responsive with proper text wrapping and styling
        /// </summary>
        public static void MakeButtonResponsive(Button button)
        {
            // Add CSS classes for all styling - no inline styles
            button.AddToClassList("button");
            button.AddToClassList("responsive-button");

            // Register for geometry change events to adjust font size dynamically
            button.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (evt.target is Button target)
                {
                    AdjustButtonFontSize(target);
                }
            });
        }

        /// <summary>
        /// Makes an action button with specific styling and color
        /// </summary>
        public static void MakeActionButtonResponsive(Button button, ButtonType buttonType = ButtonType.Primary)
        {
            // Apply standard responsive behavior
            MakeButtonResponsive(button);

            // Apply consistent action button styling via CSS class
            button.AddToClassList("action-button");

            // Apply color theme using CSS classes only
            SwitchButtonColor(button, buttonType);
        }

        /// <summary>
        /// Makes a form-width button that matches dropdown inputs
        /// </summary>
        public static void MakeFormWidthButton(Button button, ButtonType buttonType = ButtonType.Primary)
        {
            // Apply action button styling and behavior
            MakeActionButtonResponsive(button, buttonType);

            // Add CSS classes for form-specific styling - all properties handled by CSS
            button.AddToClassList("form-control"); // Use same class as dropdowns
            button.AddToClassList("flex-button"); // Same class as working action buttons
        }


        private static void AdjustButtonFontSize(Button button)
        {
            // Get the button's current width and parent width for responsive sizing
            var buttonWidth = button.resolvedStyle.width;
            var parentWidth = button.parent?.resolvedStyle.width ?? 300f;

            // Calculate responsive font size based on available space
            float baseFontSize = 13f;
            float scaleFactor = 1f;

            // Scale font based on button width relative to parent
            if (buttonWidth > 0 && parentWidth > 0)
            {
                float widthRatio = buttonWidth / parentWidth;

                // Smaller buttons get slightly smaller fonts, but still readable
                if (widthRatio < 0.2f)
                    scaleFactor = 0.85f;
                else if (widthRatio < 0.3f)
                    scaleFactor = 0.95f;
                else if (widthRatio > 0.6f)
                    scaleFactor = 1.1f;
                else if (widthRatio > 0.8f)
                    scaleFactor = 1.15f;
            }

            var finalFontSize = Mathf.RoundToInt(baseFontSize * scaleFactor);
            finalFontSize = Mathf.Clamp(finalFontSize, 10, 16);

            button.style.fontSize = finalFontSize;
        }

        /// <summary>
        /// Creates an editable byte array field with hex display
        /// </summary>
        public static VisualElement CreateByteArrayField(string label, SerializedProperty property)
        {
            var row = CreateFormRow(label);

            var container = new VisualElement();
            container.AddToClassList("byte-array-container");

            // Create property field for normal editing
            var propertyField = new UnityEditor.UIElements.PropertyField(property);
            propertyField.AddToClassList("byte-array-property");

            // Create a read-only text field to display the byte array as hex
            var hexField = new TextField("Hex View")
            {
                value = ByteArrayToHex(property),
                isReadOnly = true,
                multiline = true,
                userData = property // Store property reference for manual refresh
            };
            hexField.AddToClassList("byte-array-hex-field");

            // Update hex display when property field changes
            propertyField.RegisterValueChangeCallback(_ =>
            {
                EditorApplication.delayCall += () =>
                {
                    if (property.serializedObject.targetObject != null)
                    {
                        property.serializedObject.Update();
                        hexField.value = ByteArrayToHex(property);
                    }
                };
            });

            // Set up periodic update for hex field to catch all changes
            var lastHexValue = ByteArrayToHex(property);
            hexField.schedule.Execute(() =>
            {
                if (property.serializedObject.targetObject != null)
                {
                    var newHexValue = ByteArrayToHex(property);
                    if (newHexValue != lastHexValue)
                    {
                        hexField.value = newHexValue;
                        lastHexValue = newHexValue;
                    }
                }
            }).Every(100); // Check every 100ms

            container.Add(propertyField);
            container.Add(hexField);
            row.Add(container);

            return row;
        }

        /// <summary>
        /// Converts a byte array property to hex string representation
        /// </summary>
        public static string ByteArrayToHex(SerializedProperty property)
        {
            if (property.arraySize == 0)
                return "Empty";

            var hex = "";
            for (int i = 0; i < property.arraySize; i++)
            {
                if (i > 0 && i % 16 == 0)
                    hex += "\n";
                else if (i > 0)
                    hex += " ";

                hex += property.GetArrayElementAtIndex(i).intValue.ToString("X2");
            }
            return hex;
        }
    }
}