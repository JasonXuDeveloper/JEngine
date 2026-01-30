// JThemeTests.cs
// EditMode unit tests for JTheme

using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Theming
{
    [TestFixture]
    public class JThemeTests
    {
        private VisualElement _element;

        [SetUp]
        public void SetUp()
        {
            _element = new VisualElement();
        }

        #region ApplyBase Tests

        [Test]
        public void ApplyBase_SetsBackgroundColor()
        {
            JTheme.ApplyBase(_element);
            Assert.AreEqual(Tokens.Colors.BgBase, _element.style.backgroundColor.value);
        }

        [Test]
        public void ApplyBase_SetsTextColor()
        {
            JTheme.ApplyBase(_element);
            Assert.AreEqual(Tokens.Colors.TextPrimary, _element.style.color.value);
        }

        #endregion

        #region ApplyElevated Tests

        [Test]
        public void ApplyElevated_SetsBackgroundColor()
        {
            JTheme.ApplyElevated(_element);
            Assert.AreEqual(Tokens.Colors.BgElevated, _element.style.backgroundColor.value);
        }

        [Test]
        public void ApplyElevated_SetsBorderColors()
        {
            JTheme.ApplyElevated(_element);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderTopColor.value);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderRightColor.value);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderBottomColor.value);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderLeftColor.value);
        }

        [Test]
        public void ApplyElevated_SetsBorderWidths()
        {
            JTheme.ApplyElevated(_element);
            Assert.AreEqual(1f, _element.style.borderTopWidth.value);
            Assert.AreEqual(1f, _element.style.borderRightWidth.value);
            Assert.AreEqual(1f, _element.style.borderBottomWidth.value);
            Assert.AreEqual(1f, _element.style.borderLeftWidth.value);
        }

        [Test]
        public void ApplyElevated_SetsBorderRadius()
        {
            JTheme.ApplyElevated(_element);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _element.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _element.style.borderTopRightRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _element.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _element.style.borderBottomRightRadius.value.value);
        }

        #endregion

        #region ApplyHeaderStyle Tests

        [Test]
        public void ApplyHeaderStyle_SetsTextColor()
        {
            var label = new Label();
            JTheme.ApplyHeaderStyle(label);
            Assert.AreEqual(Tokens.Colors.TextHeader, label.style.color.value);
        }

        [Test]
        public void ApplyHeaderStyle_SetsFontSize()
        {
            var label = new Label();
            JTheme.ApplyHeaderStyle(label);
            Assert.AreEqual(Tokens.FontSize.Lg, label.style.fontSize.value.value);
        }

        [Test]
        public void ApplyHeaderStyle_SetsBoldFont()
        {
            var label = new Label();
            JTheme.ApplyHeaderStyle(label);
            Assert.AreEqual(FontStyle.Bold, label.style.unityFontStyleAndWeight.value);
        }

        [Test]
        public void ApplyHeaderStyle_SetsMarginBottom()
        {
            var label = new Label();
            JTheme.ApplyHeaderStyle(label);
            Assert.AreEqual(Tokens.Spacing.MD, label.style.marginBottom.value.value);
        }

        #endregion

        #region ApplyMutedStyle Tests

        [Test]
        public void ApplyMutedStyle_SetsTextColor()
        {
            var label = new Label();
            JTheme.ApplyMutedStyle(label);
            Assert.AreEqual(Tokens.Colors.TextMuted, label.style.color.value);
        }

        [Test]
        public void ApplyMutedStyle_SetsFontSize()
        {
            var label = new Label();
            JTheme.ApplyMutedStyle(label);
            Assert.AreEqual(Tokens.FontSize.Sm, label.style.fontSize.value.value);
        }

        #endregion

        #region ApplySurface Tests

        [Test]
        public void ApplySurface_SetsBackgroundColor()
        {
            JTheme.ApplySurface(_element);
            Assert.AreEqual(Tokens.Colors.BgSurface, _element.style.backgroundColor.value);
        }

        #endregion

        #region ApplySubtle Tests

        [Test]
        public void ApplySubtle_SetsBackgroundColor()
        {
            JTheme.ApplySubtle(_element);
            Assert.AreEqual(Tokens.Colors.BgSubtle, _element.style.backgroundColor.value);
        }

        #endregion

        #region ApplyBorder Tests

        [Test]
        public void ApplyBorder_NotSubtle_SetsNormalBorderColors()
        {
            JTheme.ApplyBorder(_element, subtle: false);
            Assert.AreEqual(Tokens.Colors.BorderLight, _element.style.borderTopColor.value);
            Assert.AreEqual(Tokens.Colors.BorderLight, _element.style.borderLeftColor.value);
            Assert.AreEqual(Tokens.Colors.BorderDark, _element.style.borderBottomColor.value);
            Assert.AreEqual(Tokens.Colors.BorderDark, _element.style.borderRightColor.value);
        }

        [Test]
        public void ApplyBorder_Subtle_SetsSubtleBorderColors()
        {
            JTheme.ApplyBorder(_element, subtle: true);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderTopColor.value);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderRightColor.value);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderBottomColor.value);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderLeftColor.value);
        }

        [Test]
        public void ApplyBorder_SetsBorderWidths()
        {
            JTheme.ApplyBorder(_element);
            Assert.AreEqual(1f, _element.style.borderTopWidth.value);
            Assert.AreEqual(1f, _element.style.borderRightWidth.value);
            Assert.AreEqual(1f, _element.style.borderBottomWidth.value);
            Assert.AreEqual(1f, _element.style.borderLeftWidth.value);
        }

        [Test]
        public void ApplyBorder_DefaultIsNotSubtle()
        {
            JTheme.ApplyBorder(_element);
            // Default behavior should be non-subtle
            Assert.AreEqual(Tokens.Colors.BorderLight, _element.style.borderTopColor.value);
        }

        #endregion

        #region ApplyTransition Tests

        [Test]
        public void ApplyTransition_SetsTransitionDuration()
        {
            JTheme.ApplyTransition(_element);
            Assert.IsNotEmpty(_element.style.transitionDuration.value);
        }

        [Test]
        public void ApplyTransition_SetsTransitionProperty()
        {
            JTheme.ApplyTransition(_element);
            Assert.IsNotEmpty(_element.style.transitionProperty.value);
        }

        #endregion

        #region ApplyGlassCard Tests

        [Test]
        public void ApplyGlassCard_SetsSurfaceBackground()
        {
            JTheme.ApplyGlassCard(_element);
            Assert.AreEqual(Tokens.Colors.BgSurface, _element.style.backgroundColor.value);
        }

        [Test]
        public void ApplyGlassCard_SetsBorderRadius()
        {
            JTheme.ApplyGlassCard(_element);
            Assert.AreEqual(Tokens.BorderRadius.MD, _element.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _element.style.borderTopRightRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _element.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _element.style.borderBottomRightRadius.value.value);
        }

        [Test]
        public void ApplyGlassCard_SetsBorderWidths()
        {
            JTheme.ApplyGlassCard(_element);
            Assert.AreEqual(1f, _element.style.borderTopWidth.value);
            Assert.AreEqual(1f, _element.style.borderRightWidth.value);
            Assert.AreEqual(1f, _element.style.borderBottomWidth.value);
            Assert.AreEqual(1f, _element.style.borderLeftWidth.value);
        }

        #endregion

        #region GetButtonColor Tests

        [Test]
        public void GetButtonColor_Primary_ReturnsPrimaryColor()
        {
            var color = JTheme.GetButtonColor(ButtonVariant.Primary);
            Assert.AreEqual(Tokens.Colors.Primary, color);
        }

        [Test]
        public void GetButtonColor_Secondary_ReturnsSecondaryColor()
        {
            var color = JTheme.GetButtonColor(ButtonVariant.Secondary);
            Assert.AreEqual(Tokens.Colors.Secondary, color);
        }

        [Test]
        public void GetButtonColor_Success_ReturnsSuccessColor()
        {
            var color = JTheme.GetButtonColor(ButtonVariant.Success);
            Assert.AreEqual(Tokens.Colors.Success, color);
        }

        [Test]
        public void GetButtonColor_Danger_ReturnsDangerColor()
        {
            var color = JTheme.GetButtonColor(ButtonVariant.Danger);
            Assert.AreEqual(Tokens.Colors.Danger, color);
        }

        [Test]
        public void GetButtonColor_Warning_ReturnsWarningColor()
        {
            var color = JTheme.GetButtonColor(ButtonVariant.Warning);
            Assert.AreEqual(Tokens.Colors.Warning, color);
        }

        #endregion

        #region GetButtonHoverColor Tests

        [Test]
        public void GetButtonHoverColor_Primary_ReturnsPrimaryHoverColor()
        {
            var color = JTheme.GetButtonHoverColor(ButtonVariant.Primary);
            Assert.AreEqual(Tokens.Colors.PrimaryHover, color);
        }

        [Test]
        public void GetButtonHoverColor_Secondary_ReturnsSecondaryHoverColor()
        {
            var color = JTheme.GetButtonHoverColor(ButtonVariant.Secondary);
            Assert.AreEqual(Tokens.Colors.SecondaryHover, color);
        }

        [Test]
        public void GetButtonHoverColor_Success_ReturnsSuccessHoverColor()
        {
            var color = JTheme.GetButtonHoverColor(ButtonVariant.Success);
            Assert.AreEqual(Tokens.Colors.SuccessHover, color);
        }

        [Test]
        public void GetButtonHoverColor_Danger_ReturnsDangerHoverColor()
        {
            var color = JTheme.GetButtonHoverColor(ButtonVariant.Danger);
            Assert.AreEqual(Tokens.Colors.DangerHover, color);
        }

        [Test]
        public void GetButtonHoverColor_Warning_ReturnsWarningHoverColor()
        {
            var color = JTheme.GetButtonHoverColor(ButtonVariant.Warning);
            Assert.AreEqual(Tokens.Colors.WarningHover, color);
        }

        #endregion

        #region GetButtonActiveColor Tests

        [Test]
        public void GetButtonActiveColor_Primary_ReturnsPrimaryActiveColor()
        {
            var color = JTheme.GetButtonActiveColor(ButtonVariant.Primary);
            Assert.AreEqual(Tokens.Colors.PrimaryActive, color);
        }

        [Test]
        public void GetButtonActiveColor_Secondary_ReturnsSecondaryActiveColor()
        {
            var color = JTheme.GetButtonActiveColor(ButtonVariant.Secondary);
            Assert.AreEqual(Tokens.Colors.SecondaryActive, color);
        }

        [Test]
        public void GetButtonActiveColor_Success_ReturnsSuccessActiveColor()
        {
            var color = JTheme.GetButtonActiveColor(ButtonVariant.Success);
            Assert.AreEqual(Tokens.Colors.SuccessActive, color);
        }

        [Test]
        public void GetButtonActiveColor_Danger_ReturnsDangerActiveColor()
        {
            var color = JTheme.GetButtonActiveColor(ButtonVariant.Danger);
            Assert.AreEqual(Tokens.Colors.DangerActive, color);
        }

        [Test]
        public void GetButtonActiveColor_Warning_ReturnsWarningActiveColor()
        {
            var color = JTheme.GetButtonActiveColor(ButtonVariant.Warning);
            Assert.AreEqual(Tokens.Colors.WarningActive, color);
        }

        #endregion

        #region ApplyInputContainerStyle Tests

        [Test]
        public void ApplyInputContainerStyle_SetsFlexGrow()
        {
            JTheme.ApplyInputContainerStyle(_element);
            Assert.AreEqual(1f, _element.style.flexGrow.value);
        }

        [Test]
        public void ApplyInputContainerStyle_SetsFlexShrink()
        {
            JTheme.ApplyInputContainerStyle(_element);
            Assert.AreEqual(1f, _element.style.flexShrink.value);
        }

        [Test]
        public void ApplyInputContainerStyle_SetsMinWidth()
        {
            JTheme.ApplyInputContainerStyle(_element);
            Assert.AreEqual(50f, _element.style.minWidth.value.value);
        }

        [Test]
        public void ApplyInputContainerStyle_SetsSizeConstraints()
        {
            JTheme.ApplyInputContainerStyle(_element);
            Assert.AreEqual(20f, _element.style.minHeight.value.value);
            Assert.AreEqual(26f, _element.style.maxHeight.value.value);
        }

        [Test]
        public void ApplyInputContainerStyle_SetsAlignSelf()
        {
            JTheme.ApplyInputContainerStyle(_element);
            Assert.AreEqual(Align.Center, _element.style.alignSelf.value);
        }

        #endregion

        #region ApplyInputElementStyle Tests

        [Test]
        public void ApplyInputElementStyle_NullElement_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => JTheme.ApplyInputElementStyle(null));
        }

        [Test]
        public void ApplyInputElementStyle_SetsBackgroundColor()
        {
            JTheme.ApplyInputElementStyle(_element);
            Assert.AreEqual(Tokens.Colors.BgInput, _element.style.backgroundColor.value);
        }

        [Test]
        public void ApplyInputElementStyle_SetsBorderColors()
        {
            JTheme.ApplyInputElementStyle(_element);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderTopColor.value);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderRightColor.value);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderBottomColor.value);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderLeftColor.value);
        }

        [Test]
        public void ApplyInputElementStyle_SetsBorderRadius()
        {
            JTheme.ApplyInputElementStyle(_element);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _element.style.borderTopLeftRadius.value.value);
        }

        [Test]
        public void ApplyInputElementStyle_SetsPadding()
        {
            JTheme.ApplyInputElementStyle(_element);
            Assert.AreEqual(Tokens.Spacing.MD, _element.style.paddingLeft.value.value);
            Assert.AreEqual(Tokens.Spacing.MD, _element.style.paddingRight.value.value);
            Assert.AreEqual(Tokens.Spacing.Sm, _element.style.paddingTop.value.value);
            Assert.AreEqual(Tokens.Spacing.Sm, _element.style.paddingBottom.value.value);
        }

        [Test]
        public void ApplyInputElementStyle_SetsTextColor()
        {
            JTheme.ApplyInputElementStyle(_element);
            Assert.AreEqual(Tokens.Colors.TextPrimary, _element.style.color.value);
        }

        #endregion

        #region ApplyInputTextStyle Tests

        [Test]
        public void ApplyInputTextStyle_NullElement_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => JTheme.ApplyInputTextStyle(null));
        }

        [Test]
        public void ApplyInputTextStyle_SetsTextColor()
        {
            JTheme.ApplyInputTextStyle(_element);
            Assert.AreEqual(Tokens.Colors.TextPrimary, _element.style.color.value);
        }

        [Test]
        public void ApplyInputTextStyle_SetsFontSize()
        {
            JTheme.ApplyInputTextStyle(_element);
            Assert.AreEqual(Tokens.FontSize.Sm, _element.style.fontSize.value.value);
        }

        #endregion

        #region ApplyInputHoverState Tests

        [Test]
        public void ApplyInputHoverState_NullElement_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => JTheme.ApplyInputHoverState(null));
        }

        [Test]
        public void ApplyInputHoverState_SetsHoverBackgroundColor()
        {
            JTheme.ApplyInputHoverState(_element);
            Assert.AreEqual(Tokens.Colors.BgHover, _element.style.backgroundColor.value);
        }

        [Test]
        public void ApplyInputHoverState_SetsHoverBorderColors()
        {
            JTheme.ApplyInputHoverState(_element);
            Assert.AreEqual(Tokens.Colors.Border, _element.style.borderTopColor.value);
            Assert.AreEqual(Tokens.Colors.Border, _element.style.borderRightColor.value);
            Assert.AreEqual(Tokens.Colors.Border, _element.style.borderBottomColor.value);
            Assert.AreEqual(Tokens.Colors.Border, _element.style.borderLeftColor.value);
        }

        #endregion

        #region ApplyInputNormalState Tests

        [Test]
        public void ApplyInputNormalState_NullElement_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => JTheme.ApplyInputNormalState(null));
        }

        [Test]
        public void ApplyInputNormalState_SetsInputBackgroundColor()
        {
            JTheme.ApplyInputNormalState(_element);
            Assert.AreEqual(Tokens.Colors.BgInput, _element.style.backgroundColor.value);
        }

        [Test]
        public void ApplyInputNormalState_SetsSubtleBorderColors()
        {
            JTheme.ApplyInputNormalState(_element);
            Assert.AreEqual(Tokens.Colors.BorderSubtle, _element.style.borderTopColor.value);
        }

        #endregion

        #region ApplyInputFocusState Tests

        [Test]
        public void ApplyInputFocusState_NullElement_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => JTheme.ApplyInputFocusState(null));
        }

        [Test]
        public void ApplyInputFocusState_SetsFocusBorderColors()
        {
            JTheme.ApplyInputFocusState(_element);
            Assert.AreEqual(Tokens.Colors.BorderFocus, _element.style.borderTopColor.value);
            Assert.AreEqual(Tokens.Colors.BorderFocus, _element.style.borderRightColor.value);
            Assert.AreEqual(Tokens.Colors.BorderFocus, _element.style.borderBottomColor.value);
            Assert.AreEqual(Tokens.Colors.BorderFocus, _element.style.borderLeftColor.value);
        }

        #endregion

        #region HideFieldLabel Tests

        [Test]
        public void HideFieldLabel_WithLabel_HidesLabel()
        {
            var container = new VisualElement();
            var label = new Label();
            label.AddToClassList("unity-base-field__label");
            container.Add(label);

            JTheme.HideFieldLabel(container);

            Assert.AreEqual(DisplayStyle.None, label.style.display.value);
        }

        [Test]
        public void HideFieldLabel_WithoutLabel_DoesNotThrow()
        {
            var container = new VisualElement();
            Assert.DoesNotThrow(() => JTheme.HideFieldLabel(container));
        }

        #endregion

        #region ApplyTextCursor Tests

        [Test]
        public void ApplyTextCursor_NullElement_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => JTheme.ApplyTextCursor(null));
        }

        [Test]
        public void ApplyTextCursor_AddsTextCursorClass()
        {
            JTheme.ApplyTextCursor(_element);
            Assert.IsTrue(_element.ClassListContains("j-cursor-text"));
        }

        #endregion

        #region ApplyPointerCursor Tests

        [Test]
        public void ApplyPointerCursor_NullElement_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => JTheme.ApplyPointerCursor(null));
        }

        [Test]
        public void ApplyPointerCursor_AddsPointerCursorClass()
        {
            JTheme.ApplyPointerCursor(_element);
            Assert.IsTrue(_element.ClassListContains("j-cursor-pointer"));
        }

        #endregion
    }
}
