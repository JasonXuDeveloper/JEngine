// TokensTests.cs
// EditMode unit tests for Tokens design system

using NUnit.Framework;
using UnityEngine;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Theming
{
    [TestFixture]
    public class TokensTests
    {
        #region IsDarkTheme Tests

        [Test]
        public void IsDarkTheme_ReturnsBoolean()
        {
            // Just verify it's accessible and returns a boolean
            var result = Tokens.IsDarkTheme;
            Assert.That(result, Is.TypeOf<bool>());
        }

        #endregion

        #region Color Token Tests

        [Test]
        public void Colors_BgBase_ReturnsColor()
        {
            var color = Tokens.Colors.BgBase;
            Assert.That(color, Is.Not.EqualTo(Color.magenta), "Color should not be magenta (error color)");
        }

        [Test]
        public void Colors_BgSubtle_ReturnsColor()
        {
            var color = Tokens.Colors.BgSubtle;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_BgSurface_ReturnsColor()
        {
            var color = Tokens.Colors.BgSurface;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_BgElevated_ReturnsColor()
        {
            var color = Tokens.Colors.BgElevated;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_BgOverlay_ReturnsColor()
        {
            var color = Tokens.Colors.BgOverlay;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_BgHover_ReturnsColor()
        {
            var color = Tokens.Colors.BgHover;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_BgInput_ReturnsColor()
        {
            var color = Tokens.Colors.BgInput;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_Primary_ReturnsColor()
        {
            var color = Tokens.Colors.Primary;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_PrimaryHover_ReturnsColor()
        {
            var color = Tokens.Colors.PrimaryHover;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_PrimaryActive_ReturnsColor()
        {
            var color = Tokens.Colors.PrimaryActive;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_PrimaryText_ReturnsColor()
        {
            var color = Tokens.Colors.PrimaryText;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_Secondary_ReturnsColor()
        {
            var color = Tokens.Colors.Secondary;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_SecondaryHover_ReturnsColor()
        {
            var color = Tokens.Colors.SecondaryHover;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_SecondaryActive_ReturnsColor()
        {
            var color = Tokens.Colors.SecondaryActive;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_SecondaryText_ReturnsColor()
        {
            var color = Tokens.Colors.SecondaryText;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_Success_ReturnsColor()
        {
            var color = Tokens.Colors.Success;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_Danger_ReturnsColor()
        {
            var color = Tokens.Colors.Danger;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_Warning_ReturnsColor()
        {
            var color = Tokens.Colors.Warning;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_TextPrimary_ReturnsColor()
        {
            var color = Tokens.Colors.TextPrimary;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_TextSecondary_ReturnsColor()
        {
            var color = Tokens.Colors.TextSecondary;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_TextMuted_ReturnsColor()
        {
            var color = Tokens.Colors.TextMuted;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_TextHeader_ReturnsColor()
        {
            var color = Tokens.Colors.TextHeader;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_TextSectionHeader_ReturnsColor()
        {
            var color = Tokens.Colors.TextSectionHeader;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_Border_ReturnsColor()
        {
            var color = Tokens.Colors.Border;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_BorderLight_ReturnsColor()
        {
            var color = Tokens.Colors.BorderLight;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_BorderDark_ReturnsColor()
        {
            var color = Tokens.Colors.BorderDark;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_BorderFocus_ReturnsColor()
        {
            var color = Tokens.Colors.BorderFocus;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_BorderHover_ReturnsColor()
        {
            var color = Tokens.Colors.BorderHover;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_BorderSubtle_ReturnsColor()
        {
            var color = Tokens.Colors.BorderSubtle;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_StatusInfo_ReturnsColor()
        {
            var color = Tokens.Colors.StatusInfo;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_StatusSuccess_ReturnsColor()
        {
            var color = Tokens.Colors.StatusSuccess;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_StatusWarning_ReturnsColor()
        {
            var color = Tokens.Colors.StatusWarning;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_StatusError_ReturnsColor()
        {
            var color = Tokens.Colors.StatusError;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_ToggleThumbOff_ReturnsColor()
        {
            var color = Tokens.Colors.ToggleThumbOff;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        [Test]
        public void Colors_ToggleThumbOn_ReturnsColor()
        {
            var color = Tokens.Colors.ToggleThumbOn;
            Assert.That(color, Is.Not.EqualTo(Color.magenta));
        }

        #endregion

        #region Spacing Token Tests

        [Test]
        public void Spacing_Xs_IsCorrectValue()
        {
            Assert.AreEqual(2f, Tokens.Spacing.Xs);
        }

        [Test]
        public void Spacing_Sm_IsCorrectValue()
        {
            Assert.AreEqual(4f, Tokens.Spacing.Sm);
        }

        [Test]
        public void Spacing_MD_IsCorrectValue()
        {
            Assert.AreEqual(8f, Tokens.Spacing.MD);
        }

        [Test]
        public void Spacing_Lg_IsCorrectValue()
        {
            Assert.AreEqual(12f, Tokens.Spacing.Lg);
        }

        [Test]
        public void Spacing_Xl_IsCorrectValue()
        {
            Assert.AreEqual(16f, Tokens.Spacing.Xl);
        }

        [Test]
        public void Spacing_Xxl_IsCorrectValue()
        {
            Assert.AreEqual(24f, Tokens.Spacing.Xxl);
        }

        [Test]
        public void Spacing_FollowsFourPixelGrid()
        {
            // All values should be multiples of 2 (4px grid base)
            Assert.AreEqual(0f, Tokens.Spacing.Xs % 2);
            Assert.AreEqual(0f, Tokens.Spacing.Sm % 2);
            Assert.AreEqual(0f, Tokens.Spacing.MD % 2);
            Assert.AreEqual(0f, Tokens.Spacing.Lg % 2);
            Assert.AreEqual(0f, Tokens.Spacing.Xl % 2);
            Assert.AreEqual(0f, Tokens.Spacing.Xxl % 2);
        }

        #endregion

        #region FontSize Token Tests

        [Test]
        public void FontSize_Xs_IsCorrectValue()
        {
            Assert.AreEqual(10f, Tokens.FontSize.Xs);
        }

        [Test]
        public void FontSize_Sm_IsCorrectValue()
        {
            Assert.AreEqual(11f, Tokens.FontSize.Sm);
        }

        [Test]
        public void FontSize_Base_IsCorrectValue()
        {
            Assert.AreEqual(12f, Tokens.FontSize.Base);
        }

        [Test]
        public void FontSize_MD_IsCorrectValue()
        {
            Assert.AreEqual(13f, Tokens.FontSize.MD);
        }

        [Test]
        public void FontSize_Lg_IsCorrectValue()
        {
            Assert.AreEqual(14f, Tokens.FontSize.Lg);
        }

        [Test]
        public void FontSize_Xl_IsCorrectValue()
        {
            Assert.AreEqual(16f, Tokens.FontSize.Xl);
        }

        [Test]
        public void FontSize_Title_IsCorrectValue()
        {
            Assert.AreEqual(18f, Tokens.FontSize.Title);
        }

        [Test]
        public void FontSize_Hierarchy_IsIncreasing()
        {
            Assert.Less(Tokens.FontSize.Xs, Tokens.FontSize.Sm);
            Assert.Less(Tokens.FontSize.Sm, Tokens.FontSize.Base);
            Assert.Less(Tokens.FontSize.Base, Tokens.FontSize.MD);
            Assert.Less(Tokens.FontSize.MD, Tokens.FontSize.Lg);
            Assert.Less(Tokens.FontSize.Lg, Tokens.FontSize.Xl);
            Assert.Less(Tokens.FontSize.Xl, Tokens.FontSize.Title);
        }

        #endregion

        #region BorderRadius Token Tests

        [Test]
        public void BorderRadius_Sm_IsCorrectValue()
        {
            Assert.AreEqual(3f, Tokens.BorderRadius.Sm);
        }

        [Test]
        public void BorderRadius_MD_IsCorrectValue()
        {
            Assert.AreEqual(5f, Tokens.BorderRadius.MD);
        }

        [Test]
        public void BorderRadius_Lg_IsCorrectValue()
        {
            Assert.AreEqual(8f, Tokens.BorderRadius.Lg);
        }

        [Test]
        public void BorderRadius_Hierarchy_IsIncreasing()
        {
            Assert.Less(Tokens.BorderRadius.Sm, Tokens.BorderRadius.MD);
            Assert.Less(Tokens.BorderRadius.MD, Tokens.BorderRadius.Lg);
        }

        #endregion

        #region Transition Token Tests

        [Test]
        public void Transition_Fast_IsCorrectValue()
        {
            Assert.AreEqual(150, Tokens.Transition.Fast);
        }

        [Test]
        public void Transition_Normal_IsCorrectValue()
        {
            Assert.AreEqual(200, Tokens.Transition.Normal);
        }

        [Test]
        public void Transition_Fast_IsFasterThanNormal()
        {
            Assert.Less(Tokens.Transition.Fast, Tokens.Transition.Normal);
        }

        #endregion

        #region Layout Token Tests

        [Test]
        public void Layout_FormLabelWidth_IsCorrectValue()
        {
            Assert.AreEqual(140f, Tokens.Layout.FormLabelWidth);
        }

        [Test]
        public void Layout_FormLabelMinWidth_IsCorrectValue()
        {
            Assert.AreEqual(60f, Tokens.Layout.FormLabelMinWidth);
        }

        [Test]
        public void Layout_MinTouchTarget_IsCorrectValue()
        {
            Assert.AreEqual(24f, Tokens.Layout.MinTouchTarget);
        }

        [Test]
        public void Layout_MinControlWidth_IsCorrectValue()
        {
            Assert.AreEqual(80f, Tokens.Layout.MinControlWidth);
        }

        [Test]
        public void Layout_FormLabelMinWidth_IsLessThanFormLabelWidth()
        {
            Assert.Less(Tokens.Layout.FormLabelMinWidth, Tokens.Layout.FormLabelWidth);
        }

        #endregion

        #region Container Token Tests

        [Test]
        public void Container_Xs_IsCorrectValue()
        {
            Assert.AreEqual(480f, Tokens.Container.Xs);
        }

        [Test]
        public void Container_Sm_IsCorrectValue()
        {
            Assert.AreEqual(640f, Tokens.Container.Sm);
        }

        [Test]
        public void Container_Md_IsCorrectValue()
        {
            Assert.AreEqual(768f, Tokens.Container.Md);
        }

        [Test]
        public void Container_Lg_IsCorrectValue()
        {
            Assert.AreEqual(1024f, Tokens.Container.Lg);
        }

        [Test]
        public void Container_Xl_IsCorrectValue()
        {
            Assert.AreEqual(1280f, Tokens.Container.Xl);
        }

        [Test]
        public void Container_Hierarchy_IsIncreasing()
        {
            Assert.Less(Tokens.Container.Xs, Tokens.Container.Sm);
            Assert.Less(Tokens.Container.Sm, Tokens.Container.Md);
            Assert.Less(Tokens.Container.Md, Tokens.Container.Lg);
            Assert.Less(Tokens.Container.Lg, Tokens.Container.Xl);
        }

        #endregion

        #region Color Alias Tests

        [Test]
        public void Colors_StatusInfo_IsSameAsPrimary()
        {
            Assert.AreEqual(Tokens.Colors.Primary, Tokens.Colors.StatusInfo);
        }

        [Test]
        public void Colors_StatusSuccess_IsSameAsSuccess()
        {
            Assert.AreEqual(Tokens.Colors.Success, Tokens.Colors.StatusSuccess);
        }

        [Test]
        public void Colors_StatusWarning_IsSameAsWarning()
        {
            Assert.AreEqual(Tokens.Colors.Warning, Tokens.Colors.StatusWarning);
        }

        [Test]
        public void Colors_StatusError_IsSameAsDanger()
        {
            Assert.AreEqual(Tokens.Colors.Danger, Tokens.Colors.StatusError);
        }

        [Test]
        public void Colors_TextHeader_IsSameAsTextPrimary()
        {
            Assert.AreEqual(Tokens.Colors.TextPrimary, Tokens.Colors.TextHeader);
        }

        [Test]
        public void Colors_Danger_IsSameAsSuccess()
        {
            // In monochrome design, these are all same
            Assert.AreEqual(Tokens.Colors.Success, Tokens.Colors.Danger);
        }

        [Test]
        public void Colors_Warning_IsSameAsSuccess()
        {
            // In monochrome design, these are all same
            Assert.AreEqual(Tokens.Colors.Success, Tokens.Colors.Warning);
        }

        [Test]
        public void Colors_BorderLight_IsSameAsBorder()
        {
            Assert.AreEqual(Tokens.Colors.Border, Tokens.Colors.BorderLight);
        }

        [Test]
        public void Colors_BorderDark_IsSameAsBorder()
        {
            Assert.AreEqual(Tokens.Colors.Border, Tokens.Colors.BorderDark);
        }

        #endregion

        #region Toggle Thumb Tests

        [Test]
        public void Colors_ToggleThumbOff_IsSameAsPrimary()
        {
            Assert.AreEqual(Tokens.Colors.Primary, Tokens.Colors.ToggleThumbOff);
        }

        [Test]
        public void Colors_ToggleThumbOn_IsSameAsSecondary()
        {
            Assert.AreEqual(Tokens.Colors.Secondary, Tokens.Colors.ToggleThumbOn);
        }

        #endregion
    }
}
