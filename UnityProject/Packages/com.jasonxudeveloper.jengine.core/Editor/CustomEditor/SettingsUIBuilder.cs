// SettingsUIBuilder.cs
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
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace JEngine.Core.Editor.CustomEditor
{
    /// <summary>
    /// Shared utility class for building Settings UI components
    /// Used by both Panel and SettingsEditor to avoid code duplication
    /// </summary>
    public static class SettingsUIBuilder
    {




        private static void AdjustTextElementFontSize(VisualElement element, bool isHeader = false)
        {
            // Get the element's current width for responsive sizing
            var elementWidth = element.resolvedStyle.width;

            // Calculate responsive font size based on available space
            float baseFontSize;
            float scaleFactor = 1f;

            // Different base sizes for headers vs normal text
            if (isHeader)
            {
                baseFontSize = 16f;  // Larger base size for headers
                // Headers can be more prominent on larger screens
                if (Screen.width < 1200)
                    scaleFactor = 1.0f;   // Keep headers readable on small screens
                else if (Screen.width > 1600)
                    scaleFactor = 1.2f;   // Make headers more prominent on large screens
            }
            else
            {
                baseFontSize = 13f;  // Normal text base size
                // Scale font based on screen width for global scaling
                if (Screen.width < 1200)
                    scaleFactor *= 0.95f;  // Less aggressive scaling down
                else if (Screen.width > 1600)
                    scaleFactor *= 1.1f;
            }

            // Special handling for different element types
            if (element is Label)
            {
                // Labels still readable even in constrained spaces
                if (!isHeader && elementWidth > 0 && elementWidth < 150)
                    scaleFactor *= 0.95f;  // Less aggressive scaling for small labels
            }
            else if (element is TextField || element is PopupField<string> || element is EnumField)
            {
                // Form controls need very readable text
                baseFontSize = isHeader ? 16f : 13f;
            }

            float finalFontSize = baseFontSize * scaleFactor;

            if (isHeader)
            {
                element.style.fontSize = Mathf.Clamp(finalFontSize, 14f, 24f);  // Headers: 14px - 24px
            }
            else
            {
                element.style.fontSize = Mathf.Clamp(finalFontSize, 11f, 18f);  // Normal text: 11px - 18px
            }
        }

        public static void MakeButtonResponsive(Button button)
        {
            // Add common button classes for consistent styling
            button.AddToClassList("button");
            button.AddToClassList("responsive-button");

            // Enable text wrapping and proper overflow handling
            button.style.whiteSpace = WhiteSpace.Normal;
            button.style.textOverflow = TextOverflow.Clip;
            button.style.overflow = Overflow.Visible;  // Allow content to be visible

            // Apply bold text styling
            button.style.unityFontStyleAndWeight = FontStyle.Bold;

            // Enable flexible sizing for buttons with proper text wrapping
            button.style.height = StyleKeyword.Auto;     // Auto height to accommodate wrapped text
            button.style.minHeight = 24;                 // Minimum height for usability
            button.style.maxHeight = StyleKeyword.None;  // No maximum height limit for text wrapping
            button.style.flexShrink = 0;

            // Remove fixed width constraints to allow natural sizing
            button.style.width = StyleKeyword.Auto;
            button.style.minWidth = StyleKeyword.Auto;
            button.style.maxWidth = StyleKeyword.None;

            // Ensure proper padding scales with content
            button.style.paddingTop = 6;
            button.style.paddingBottom = 6;
            button.style.paddingLeft = 12;
            button.style.paddingRight = 12;

            // Text alignment for better wrapping
            button.style.unityTextAlign = TextAnchor.MiddleCenter;

            // Register for geometry change events to adjust font size dynamically
            button.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var target = evt.target as Button;
                if (target != null)
                {
                    AdjustButtonFontSize(target);
                }
            });
        }


        public static void MakeActionButtonResponsive(Button button, EditorUIUtils.ButtonType buttonType = EditorUIUtils.ButtonType.Primary)
        {
            // Apply standard responsive behavior
            MakeButtonResponsive(button);

            // Apply consistent action button styling
            button.AddToClassList("action-button");

            // Apply uniform color theme using CSS classes + inline colors as backup
            switch (buttonType)
            {
                case EditorUIUtils.ButtonType.Primary:
                    button.AddToClassList("btn-primary");
                    button.style.backgroundColor = new Color(0.2f, 0.6f, 0.9f, 1f); // Blue
                    break;
                case EditorUIUtils.ButtonType.Secondary:
                    button.AddToClassList("btn-secondary");
                    button.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
                    break;
                case EditorUIUtils.ButtonType.Success:
                    button.AddToClassList("btn-success");
                    button.style.backgroundColor = new Color(0.2f, 0.7f, 0.2f, 1f); // Green
                    break;
                case EditorUIUtils.ButtonType.Danger:
                    button.AddToClassList("btn-danger");
                    button.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Red
                    break;
                case EditorUIUtils.ButtonType.Warning:
                    button.AddToClassList("btn-warning");
                    button.style.backgroundColor = new Color(0.9f, 0.6f, 0.1f, 1f); // Orange
                    break;
            }

            button.style.color = Color.white;
        }



        public static VisualElement CreateResponsiveContainer()
        {
            var container = new VisualElement();
            container.AddToClassList("responsive-container");
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexWrap = Wrap.NoWrap;
            container.style.alignItems = Align.Center;

            // Register for geometry changes to handle responsive layout
            container.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var target = evt.target as VisualElement;
                if (target != null)
                {
                    HandleResponsiveLayout(target);
                }
            });

            return container;
        }

        public static VisualElement CreateFlexButtonRow(params Button[] buttons)
        {
            var container = new VisualElement();
            container.AddToClassList("button-row");
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexWrap = Wrap.Wrap;
            container.style.alignItems = Align.Center;
            container.style.width = Length.Percent(100);

            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                // Allow buttons to shrink and grow responsively
                button.style.flexGrow = 1;
                button.style.flexShrink = 1;
                button.style.minWidth = 80; // Minimum width before wrapping
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

        public static VisualElement CreateFixedWidthButtonRow(params (Button button, float widthPercent)[] buttons)
        {
            var container = new VisualElement();
            container.AddToClassList("fixed-button-row");
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexWrap = Wrap.Wrap;
            container.style.alignItems = Align.Center;
            container.style.width = Length.Percent(100);

            for (int i = 0; i < buttons.Length; i++)
            {
                var (button, widthPercent) = buttons[i];

                // Use flex-basis for responsive behavior instead of fixed width
                button.style.flexBasis = Length.Percent(widthPercent);
                button.style.flexGrow = 0;
                button.style.flexShrink = 1;
                button.style.minWidth = 60; // Minimum width before wrapping

                // Add margin between buttons except for the last one
                if (i < buttons.Length - 1)
                {
                    button.style.marginRight = 4;
                }

                container.Add(button);
            }

            return container;
        }

        private static void HandleResponsiveLayout(VisualElement container)
        {
            if (container.resolvedStyle.width <= 0) return;

            var availableWidth = container.resolvedStyle.width;
            var childCount = container.childCount;

            // Estimate if items will fit horizontally (rough calculation)
            var estimatedItemWidth = childCount > 0 ? availableWidth / childCount : 0;

            // Switch to vertical layout if items are too cramped
            if (estimatedItemWidth < 120 && childCount > 2) // Threshold for cramped layout
            {
                container.style.flexDirection = FlexDirection.Column;
                container.style.alignItems = Align.Stretch;

                // Add spacing between items in vertical layout
                for (int i = 0; i < container.childCount; i++)
                {
                    var child = container[i];
                    child.style.marginBottom = i < container.childCount - 1 ? 4 : 0;
                    child.style.marginRight = 0;
                }
            }
            else
            {
                container.style.flexDirection = FlexDirection.Row;
                container.style.alignItems = Align.Center;

                // Reset margins for horizontal layout
                for (int i = 0; i < container.childCount; i++)
                {
                    var child = container[i];
                    child.style.marginBottom = 0;
                    child.style.marginRight = i < container.childCount - 1 ? 4 : 0;
                }
            }
        }

        private static void AdjustButtonFontSize(Button button)
        {
            // Get the button's current width and parent width for responsive sizing
            var buttonWidth = button.resolvedStyle.width;
            var parentWidth = button.parent?.resolvedStyle.width ?? 300f;

            // Calculate responsive font size based on available space - INCREASED BASE SIZE
            float baseFontSize = 13f;  // Increased from 11f to 13f
            float scaleFactor = 1f;

            // Scale font based on button width relative to parent
            if (buttonWidth > 0 && parentWidth > 0)
            {
                float widthRatio = buttonWidth / parentWidth;

                // Smaller buttons get slightly smaller fonts, but still readable
                if (widthRatio < 0.2f)
                    scaleFactor = 0.85f;  // Less aggressive scaling
                else if (widthRatio < 0.3f)
                    scaleFactor = 0.95f;  // Less aggressive scaling
                else if (widthRatio > 0.6f)
                    scaleFactor = 1.1f;
                else if (widthRatio > 0.8f)
                    scaleFactor = 1.15f;  // Slightly less aggressive scaling up
            }

            // Also consider screen width for global scaling
            if (Screen.width < 1200)
                scaleFactor *= 0.95f;  // Less aggressive scaling down
            else if (Screen.width > 1600)
                scaleFactor *= 1.1f;

            float finalFontSize = baseFontSize * scaleFactor;
            button.style.fontSize = Mathf.Clamp(finalFontSize, 11f, 18f);  // Increased min from 8f to 11f, max from 16f to 18f
        }

        public static VisualElement CreatePackageSettingsGroup(Settings settings, bool useInspectorBinding = false)
        {
            var packageGroup = EditorUIUtils.CreateGroup("Package Settings");

            // Package Name Dropdown
            var packageNameRow = EditorUIUtils.CreateFormRow("Package Name");
            var packageChoices = EditorUtils.GetAvailableYooAssetPackages();
            var packageNameField = new PopupField<string>()
            {
                choices = packageChoices.Any() ? packageChoices : new List<string> { settings.packageName },
                value = settings.packageName
            };
            packageNameField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(packageNameField);
            packageNameField.RegisterValueChangedCallback(evt =>
            {
                settings.packageName = evt.newValue;
                settings.Save();
            });
            packageNameRow.Add(packageNameField);
            packageGroup.Add(packageNameRow);

            // Build Target field
            var buildTargetRow = EditorUIUtils.CreateFormRow("Build Target");
            var buildTargetField = new EnumField(settings.buildTarget);
            buildTargetField.RegisterValueChangedCallback(evt =>
            {
                settings.buildTarget = (BuildTarget)evt.newValue;
                settings.Save();
            });
            buildTargetField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(buildTargetField);
            buildTargetRow.Add(buildTargetField);
            packageGroup.Add(buildTargetRow);

            // Set to Active button (same width as dropdown)
            var setActiveContainer = EditorUIUtils.CreateFormRow("");
            var setActiveButton = new Button(() =>
            {
                settings.buildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildTargetField.value = settings.buildTarget;
                settings.Save();
            })
            {
                text = "Set to Current Active Target"
            };
            setActiveButton.AddToClassList("form-control");
            EditorUIUtils.MakeActionButtonResponsive(setActiveButton, EditorUIUtils.ButtonType.Primary);
            setActiveContainer.Add(setActiveButton);
            packageGroup.Add(setActiveContainer);

            return packageGroup;
        }

        public static VisualElement CreateBuildOptionsGroup(Settings settings, bool useInspectorBinding = false)
        {
            var buildGroup = EditorUIUtils.CreateGroup("Build Options");

            // Clear Build Cache Toggle
            var clearCacheRow = EditorUIUtils.CreateFormRow("Clear Build Cache");
            var clearCacheToggle = new Toggle()
            {
                value = settings.clearBuildCache
            };
            clearCacheToggle.tooltip = "Clear build cache before building. Uncheck to enable incremental builds (faster)";
            clearCacheToggle.RegisterValueChangedCallback(evt =>
            {
                settings.clearBuildCache = evt.newValue;
                settings.Save();
            });
            clearCacheToggle.AddToClassList("form-control");
            clearCacheRow.Add(clearCacheToggle);
            buildGroup.Add(clearCacheRow);

            // Use Asset Dependency DB Toggle
            var useAssetDBRow = EditorUIUtils.CreateFormRow("Use Asset Dependency DB");
            var useAssetDBToggle = new Toggle()
            {
                value = settings.useAssetDependDB
            };
            useAssetDBToggle.tooltip = "Use asset dependency database to improve build speed";
            useAssetDBToggle.RegisterValueChangedCallback(evt =>
            {
                settings.useAssetDependDB = evt.newValue;
                settings.Save();
            });
            useAssetDBToggle.AddToClassList("form-control");
            useAssetDBRow.Add(useAssetDBToggle);
            buildGroup.Add(useAssetDBRow);

            return buildGroup;
        }

        public static VisualElement CreateJEngineSettingsGroup(Settings settings, bool useInspectorBinding = false)
        {
            var jengineGroup = EditorUIUtils.CreateGroup("JEngine Settings");

            // Language Selection
            var languageRow = EditorUIUtils.CreateFormRow("Display Language");
            var languageField = new EnumField(settings.language);
            languageField.RegisterValueChangedCallback(evt =>
            {
                settings.language = (JEngineLanguage)evt.newValue;
                settings.Save();
            });
            languageField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(languageField);
            languageRow.Add(languageField);
            jengineGroup.Add(languageRow);

            // Encrypt Password
            var passwordRow = EditorUIUtils.CreateFormRow("Encrypt DLL Password");
            var passwordField = new TextField()
            {
                value = settings.encryptPassword,
                isPasswordField = true
            };
            passwordField.RegisterValueChangedCallback(evt =>
            {
                settings.encryptPassword = evt.newValue;
                settings.Save();
            });
            passwordField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(passwordField);
            passwordRow.Add(passwordField);
            jengineGroup.Add(passwordRow);

            // Startup Scene
            var sceneRow = EditorUIUtils.CreateFormRow("Startup Scene");
            var currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(settings.startUpScenePath);
            var sceneField = new ObjectField()
            {
                objectType = typeof(SceneAsset),
                value = currentScene
            };
            sceneField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(evt.newValue);
                    if (assetPath.EndsWith(".unity"))
                    {
                        settings.startUpScenePath = assetPath;
                        settings.Save();
                    }
                }
            });
            sceneField.AddToClassList("form-control");
            EditorUIUtils.MakeTextResponsive(sceneField);
            sceneRow.Add(sceneField);
            jengineGroup.Add(sceneRow);

            // Jump to Startup Scene
            var jumpRow = EditorUIUtils.CreateFormRow("Jump to Startup Scene");
            var jumpToggle = new Toggle()
            {
                value = settings.jumpStartUp
            };
            jumpToggle.tooltip = "Jump to startup scene when launch";
            jumpToggle.RegisterValueChangedCallback(evt =>
            {
                settings.jumpStartUp = evt.newValue;
                settings.Save();
            });
            jumpToggle.AddToClassList("form-control");
            jumpRow.Add(jumpToggle);
            jengineGroup.Add(jumpRow);

            return jengineGroup;
        }
    }
}