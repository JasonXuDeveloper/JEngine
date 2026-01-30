// JProgressBarTests.cs
// EditMode unit tests for JProgressBar

using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Feedback;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Feedback
{
    [TestFixture]
    public class JProgressBarTests
    {
        private JProgressBar _progressBar;

        [SetUp]
        public void SetUp()
        {
            _progressBar = new JProgressBar();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_progressBar.ClassListContains("j-progress-bar"));
        }

        [Test]
        public void Constructor_Default_ProgressIsZero()
        {
            Assert.AreEqual(0f, _progressBar.Progress);
        }

        [Test]
        public void Constructor_WithInitialProgress_SetsProgress()
        {
            var bar = new JProgressBar(0.5f);
            Assert.AreEqual(0.5f, bar.Progress);
        }

        [Test]
        public void Constructor_SetsHeight()
        {
            Assert.AreEqual(8f, _progressBar.style.height.value.value);
        }

        [Test]
        public void Constructor_SetsSurfaceBackgroundColor()
        {
            Assert.AreEqual(Tokens.Colors.BgSurface, _progressBar.style.backgroundColor.value);
        }

        [Test]
        public void Constructor_SetsBorderRadius()
        {
            Assert.AreEqual(Tokens.BorderRadius.Sm, _progressBar.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _progressBar.style.borderTopRightRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _progressBar.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.Sm, _progressBar.style.borderBottomRightRadius.value.value);
        }

        [Test]
        public void Constructor_SetsOverflowHidden()
        {
            Assert.AreEqual(Overflow.Hidden, _progressBar.style.overflow.value);
        }

        [Test]
        public void Constructor_CreatesFillElement()
        {
            Assert.IsNotNull(_progressBar.Fill);
        }

        [Test]
        public void Constructor_FillHasCorrectClass()
        {
            Assert.IsTrue(_progressBar.Fill.ClassListContains("j-progress-bar__fill"));
        }

        [Test]
        public void Constructor_FillHasPrimaryColor()
        {
            Assert.AreEqual(Tokens.Colors.Primary, _progressBar.Fill.style.backgroundColor.value);
        }

        #endregion

        #region Progress Property Tests

        [Test]
        public void Progress_Get_ReturnsCurrentProgress()
        {
            var bar = new JProgressBar(0.75f);
            Assert.AreEqual(0.75f, bar.Progress);
        }

        [Test]
        public void Progress_Set_UpdatesProgress()
        {
            _progressBar.Progress = 0.5f;
            Assert.AreEqual(0.5f, _progressBar.Progress);
        }

        [Test]
        public void Progress_Set_Clamps_ToZero()
        {
            _progressBar.Progress = -0.5f;
            Assert.AreEqual(0f, _progressBar.Progress);
        }

        [Test]
        public void Progress_Set_Clamps_ToOne()
        {
            _progressBar.Progress = 1.5f;
            Assert.AreEqual(1f, _progressBar.Progress);
        }

        [Test]
        public void Progress_Set_UpdatesFillWidth()
        {
            _progressBar.Progress = 0.5f;
            Assert.AreEqual(50f, _progressBar.Fill.style.width.value.value);
        }

        [Test]
        public void Progress_Set_Zero_FillWidthIsZero()
        {
            _progressBar.Progress = 0f;
            Assert.AreEqual(0f, _progressBar.Fill.style.width.value.value);
        }

        [Test]
        public void Progress_Set_Full_FillWidthIs100Percent()
        {
            _progressBar.Progress = 1f;
            Assert.AreEqual(100f, _progressBar.Fill.style.width.value.value);
        }

        #endregion

        #region SetProgress Tests

        [Test]
        public void SetProgress_UpdatesProgress()
        {
            _progressBar.SetProgress(0.75f);
            Assert.AreEqual(0.75f, _progressBar.Progress);
        }

        [Test]
        public void SetProgress_ReturnsProgressBarForChaining()
        {
            var result = _progressBar.SetProgress(0.5f);
            Assert.AreSame(_progressBar, result);
        }

        [Test]
        public void SetProgress_ClampsBelowZero()
        {
            _progressBar.SetProgress(-1f);
            Assert.AreEqual(0f, _progressBar.Progress);
        }

        [Test]
        public void SetProgress_ClampsAboveOne()
        {
            _progressBar.SetProgress(2f);
            Assert.AreEqual(1f, _progressBar.Progress);
        }

        #endregion

        #region WithSuccessOnComplete Tests

        [Test]
        public void WithSuccessOnComplete_WhenNotComplete_KeepsPrimaryColor()
        {
            _progressBar.Progress = 0.5f;
            _progressBar.WithSuccessOnComplete(true);

            Assert.AreEqual(Tokens.Colors.Primary, _progressBar.Fill.style.backgroundColor.value);
        }

        [Test]
        public void WithSuccessOnComplete_WhenComplete_UsesSuccessColor()
        {
            _progressBar.Progress = 1f;
            _progressBar.WithSuccessOnComplete(true);
            // Need to set progress again to trigger color update
            _progressBar.SetProgress(1f);

            Assert.AreEqual(Tokens.Colors.Success, _progressBar.Fill.style.backgroundColor.value);
        }

        [Test]
        public void WithSuccessOnComplete_ReturnsProgressBarForChaining()
        {
            var result = _progressBar.WithSuccessOnComplete(true);
            Assert.AreSame(_progressBar, result);
        }

        [Test]
        public void WithSuccessOnComplete_False_UsesPrimaryColor()
        {
            _progressBar.Progress = 1f;
            _progressBar.WithSuccessOnComplete(false);

            Assert.AreEqual(Tokens.Colors.Primary, _progressBar.Fill.style.backgroundColor.value);
        }

        #endregion

        #region WithHeight Tests

        [Test]
        public void WithHeight_SetsHeight()
        {
            _progressBar.WithHeight(16f);
            Assert.AreEqual(16f, _progressBar.style.height.value.value);
        }

        [Test]
        public void WithHeight_ReturnsProgressBarForChaining()
        {
            var result = _progressBar.WithHeight(12f);
            Assert.AreSame(_progressBar, result);
        }

        #endregion

        #region WithColor Tests

        [Test]
        public void WithColor_SetsFillColor()
        {
            _progressBar.WithColor(Color.red);
            Assert.AreEqual(Color.red, _progressBar.Fill.style.backgroundColor.value);
        }

        [Test]
        public void WithColor_ReturnsProgressBarForChaining()
        {
            var result = _progressBar.WithColor(Color.blue);
            Assert.AreSame(_progressBar, result);
        }

        #endregion

        #region WithVariant Tests

        [Test]
        public void WithVariant_Primary_SetsPrimaryColor()
        {
            _progressBar.WithVariant(ButtonVariant.Primary);
            Assert.AreEqual(JTheme.GetButtonColor(ButtonVariant.Primary), _progressBar.Fill.style.backgroundColor.value);
        }

        [Test]
        public void WithVariant_Secondary_SetsSecondaryColor()
        {
            _progressBar.WithVariant(ButtonVariant.Secondary);
            Assert.AreEqual(JTheme.GetButtonColor(ButtonVariant.Secondary), _progressBar.Fill.style.backgroundColor.value);
        }

        [Test]
        public void WithVariant_Success_SetsSuccessColor()
        {
            _progressBar.WithVariant(ButtonVariant.Success);
            Assert.AreEqual(JTheme.GetButtonColor(ButtonVariant.Success), _progressBar.Fill.style.backgroundColor.value);
        }

        [Test]
        public void WithVariant_Danger_SetsDangerColor()
        {
            _progressBar.WithVariant(ButtonVariant.Danger);
            Assert.AreEqual(JTheme.GetButtonColor(ButtonVariant.Danger), _progressBar.Fill.style.backgroundColor.value);
        }

        [Test]
        public void WithVariant_Warning_SetsWarningColor()
        {
            _progressBar.WithVariant(ButtonVariant.Warning);
            Assert.AreEqual(JTheme.GetButtonColor(ButtonVariant.Warning), _progressBar.Fill.style.backgroundColor.value);
        }

        [Test]
        public void WithVariant_ReturnsProgressBarForChaining()
        {
            var result = _progressBar.WithVariant(ButtonVariant.Success);
            Assert.AreSame(_progressBar, result);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            _progressBar
                .SetProgress(0.75f)
                .WithHeight(12f)
                .WithVariant(ButtonVariant.Success)
                .WithSuccessOnComplete(true);

            Assert.AreEqual(0.75f, _progressBar.Progress);
            Assert.AreEqual(12f, _progressBar.style.height.value.value);
        }

        #endregion
    }
}
