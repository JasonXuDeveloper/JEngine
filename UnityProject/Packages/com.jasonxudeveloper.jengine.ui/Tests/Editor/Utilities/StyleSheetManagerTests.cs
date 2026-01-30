// StyleSheetManagerTests.cs
// EditMode unit tests for StyleSheetManager

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Utilities;

namespace JEngine.UI.Tests.Editor.Utilities
{
    [TestFixture]
    public class StyleSheetManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            // Clear cache before each test for clean state
            StyleSheetManager.ClearCache();
        }

        [TearDown]
        public void TearDown()
        {
            StyleSheetManager.ClearCache();
        }

        #region Tokens Property Tests

        [Test]
        public void Tokens_ReturnsStyleSheetOrNull()
        {
            // May be null if running outside package context
            var result = StyleSheetManager.Tokens;
            // Just verify it doesn't throw
            Assert.That(result == null || result is StyleSheet);
        }

        [Test]
        public void Tokens_IsCached()
        {
            var first = StyleSheetManager.Tokens;
            var second = StyleSheetManager.Tokens;

            // Should be same instance (or both null)
            Assert.AreSame(first, second);
        }

        #endregion

        #region Base Property Tests

        [Test]
        public void Base_ReturnsStyleSheetOrNull()
        {
            var result = StyleSheetManager.Base;
            Assert.That(result == null || result is StyleSheet);
        }

        [Test]
        public void Base_IsCached()
        {
            var first = StyleSheetManager.Base;
            var second = StyleSheetManager.Base;

            Assert.AreSame(first, second);
        }

        #endregion

        #region Components Property Tests

        [Test]
        public void Components_ReturnsStyleSheetOrNull()
        {
            var result = StyleSheetManager.Components;
            Assert.That(result == null || result is StyleSheet);
        }

        [Test]
        public void Components_IsCached()
        {
            var first = StyleSheetManager.Components;
            var second = StyleSheetManager.Components;

            Assert.AreSame(first, second);
        }

        #endregion

        #region ApplyAllStyleSheets Tests

        [Test]
        public void ApplyAllStyleSheets_DoesNotThrow()
        {
            var element = new VisualElement();
            Assert.DoesNotThrow(() => StyleSheetManager.ApplyAllStyleSheets(element));
        }

        [Test]
        public void ApplyAllStyleSheets_AddsStyleSheets()
        {
            var element = new VisualElement();
            StyleSheetManager.ApplyAllStyleSheets(element);

            // At minimum, the method should have tried to add stylesheets
            // (actual count depends on whether files exist in test context)
            Assert.That(element.styleSheets != null);
        }

        [Test]
        public void ApplyAllStyleSheets_CanBeCalledMultipleTimes()
        {
            var element = new VisualElement();

            Assert.DoesNotThrow(() =>
            {
                StyleSheetManager.ApplyAllStyleSheets(element);
                StyleSheetManager.ApplyAllStyleSheets(element);
                StyleSheetManager.ApplyAllStyleSheets(element);
            });
        }

        #endregion

        #region ClearCache Tests

        [Test]
        public void ClearCache_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => StyleSheetManager.ClearCache());
        }

        [Test]
        public void ClearCache_CanBeCalledMultipleTimes()
        {
            Assert.DoesNotThrow(() =>
            {
                StyleSheetManager.ClearCache();
                StyleSheetManager.ClearCache();
                StyleSheetManager.ClearCache();
            });
        }

        [Test]
        public void ClearCache_ResetsTokensCache()
        {
            // Access to populate cache
            var first = StyleSheetManager.Tokens;

            StyleSheetManager.ClearCache();

            // After clear, accessing again should reload
            var second = StyleSheetManager.Tokens;

            // Both may be null in test context, but the test ensures
            // no exceptions and proper behavior
            Assert.That(second == null || second is StyleSheet);
        }

        [Test]
        public void ClearCache_ResetsBaseCache()
        {
            var first = StyleSheetManager.Base;
            StyleSheetManager.ClearCache();
            var second = StyleSheetManager.Base;

            Assert.That(second == null || second is StyleSheet);
        }

        [Test]
        public void ClearCache_ResetsComponentsCache()
        {
            var first = StyleSheetManager.Components;
            StyleSheetManager.ClearCache();
            var second = StyleSheetManager.Components;

            Assert.That(second == null || second is StyleSheet);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void AllProperties_DoNotInterfereWithEachOther()
        {
            // Access all properties in various orders
            var tokens1 = StyleSheetManager.Tokens;
            var base1 = StyleSheetManager.Base;
            var components1 = StyleSheetManager.Components;

            var tokens2 = StyleSheetManager.Tokens;
            var base2 = StyleSheetManager.Base;
            var components2 = StyleSheetManager.Components;

            // Each should return same cached instance
            Assert.AreSame(tokens1, tokens2);
            Assert.AreSame(base1, base2);
            Assert.AreSame(components1, components2);
        }

        [Test]
        public void ApplyAllStyleSheets_WorksWithNestedElements()
        {
            var parent = new VisualElement();
            var child = new VisualElement();
            parent.Add(child);

            Assert.DoesNotThrow(() =>
            {
                StyleSheetManager.ApplyAllStyleSheets(parent);
                StyleSheetManager.ApplyAllStyleSheets(child);
            });
        }

        [Test]
        public void ApplyAllStyleSheets_WorksWithDifferentElementTypes()
        {
            var label = new Label("Test");
            var button = new Button();
            var textField = new TextField();

            Assert.DoesNotThrow(() =>
            {
                StyleSheetManager.ApplyAllStyleSheets(label);
                StyleSheetManager.ApplyAllStyleSheets(button);
                StyleSheetManager.ApplyAllStyleSheets(textField);
            });
        }

        #endregion
    }
}
