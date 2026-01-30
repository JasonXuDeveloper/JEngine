// JLogViewTests.cs
// EditMode unit tests for JLogView

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Feedback;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Components.Feedback
{
    [TestFixture]
    public class JLogViewTests
    {
        private JLogView _logView;

        [SetUp]
        public void SetUp()
        {
            _logView = new JLogView();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_logView.ClassListContains("j-log-view"));
        }

        [Test]
        public void Constructor_Default_MaxLinesIs100()
        {
            Assert.AreEqual(100, _logView.MaxLines);
        }

        [Test]
        public void Constructor_WithMaxLines_SetsMaxLines()
        {
            var log = new JLogView(50);
            Assert.AreEqual(50, log.MaxLines);
        }

        [Test]
        public void Constructor_SetsInputBackgroundColor()
        {
            Assert.AreEqual(Tokens.Colors.BgInput, _logView.style.backgroundColor.value);
        }

        [Test]
        public void Constructor_SetsBorderColors()
        {
            Assert.AreEqual(Tokens.Colors.Border, _logView.style.borderTopColor.value);
            Assert.AreEqual(Tokens.Colors.Border, _logView.style.borderRightColor.value);
            Assert.AreEqual(Tokens.Colors.Border, _logView.style.borderBottomColor.value);
            Assert.AreEqual(Tokens.Colors.Border, _logView.style.borderLeftColor.value);
        }

        [Test]
        public void Constructor_SetsBorderWidths()
        {
            Assert.AreEqual(1f, _logView.style.borderTopWidth.value);
            Assert.AreEqual(1f, _logView.style.borderRightWidth.value);
            Assert.AreEqual(1f, _logView.style.borderBottomWidth.value);
            Assert.AreEqual(1f, _logView.style.borderLeftWidth.value);
        }

        [Test]
        public void Constructor_SetsBorderRadius()
        {
            Assert.AreEqual(Tokens.BorderRadius.MD, _logView.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _logView.style.borderTopRightRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _logView.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(Tokens.BorderRadius.MD, _logView.style.borderBottomRightRadius.value.value);
        }

        [Test]
        public void Constructor_SetsMinHeight()
        {
            Assert.AreEqual(100f, _logView.style.minHeight.value.value);
        }

        [Test]
        public void Constructor_SetsMaxHeight()
        {
            Assert.AreEqual(300f, _logView.style.maxHeight.value.value);
        }

        [Test]
        public void Constructor_SetsPadding()
        {
            Assert.AreEqual(Tokens.Spacing.MD, _logView.style.paddingTop.value.value);
            Assert.AreEqual(Tokens.Spacing.MD, _logView.style.paddingRight.value.value);
            Assert.AreEqual(Tokens.Spacing.MD, _logView.style.paddingBottom.value.value);
            Assert.AreEqual(Tokens.Spacing.MD, _logView.style.paddingLeft.value.value);
        }

        [Test]
        public void Constructor_CreatesScrollView()
        {
            Assert.IsNotNull(_logView.ScrollView);
        }

        [Test]
        public void Constructor_ScrollViewHasCorrectClass()
        {
            Assert.IsTrue(_logView.ScrollView.ClassListContains("j-log-view__scroll"));
        }

        [Test]
        public void Constructor_ScrollViewHasFlexGrow()
        {
            Assert.AreEqual(1f, _logView.ScrollView.style.flexGrow.value);
        }

        #endregion

        #region MaxLines Property Tests

        [Test]
        public void MaxLines_Get_ReturnsCurrentMaxLines()
        {
            var log = new JLogView(75);
            Assert.AreEqual(75, log.MaxLines);
        }

        [Test]
        public void MaxLines_Set_UpdatesMaxLines()
        {
            _logView.MaxLines = 200;
            Assert.AreEqual(200, _logView.MaxLines);
        }

        [Test]
        public void MaxLines_SetToZero_UnlimitedLines()
        {
            _logView.MaxLines = 0;
            Assert.AreEqual(0, _logView.MaxLines);
        }

        #endregion

        #region Log Tests

        [Test]
        public void Log_AddsEntryToScrollView()
        {
            _logView.Log("Test message");
            Assert.AreEqual(1, _logView.ScrollView.childCount);
        }

        [Test]
        public void Log_MultipleMessages_AddsAllEntries()
        {
            _logView.Log("Message 1");
            _logView.Log("Message 2");
            _logView.Log("Message 3");

            Assert.AreEqual(3, _logView.ScrollView.childCount);
        }

        [Test]
        public void Log_ReturnsLogViewForChaining()
        {
            var result = _logView.Log("Test");
            Assert.AreSame(_logView, result);
        }

        [Test]
        public void Log_DefaultNotError_AddsInfoClass()
        {
            _logView.Log("Info message");
            var entry = _logView.ScrollView[0];
            Assert.IsTrue(entry.ClassListContains("j-log-view__entry--info"));
        }

        [Test]
        public void Log_IsError_AddsErrorClass()
        {
            _logView.Log("Error message", isError: true);
            var entry = _logView.ScrollView[0];
            Assert.IsTrue(entry.ClassListContains("j-log-view__entry--error"));
        }

        [Test]
        public void Log_EntryHasCorrectBaseClass()
        {
            _logView.Log("Test");
            var entry = _logView.ScrollView[0];
            Assert.IsTrue(entry.ClassListContains("j-log-view__entry"));
        }

        [Test]
        public void Log_ExceedsMaxLines_RemovesOldestEntries()
        {
            var log = new JLogView(3);

            log.Log("Message 1");
            log.Log("Message 2");
            log.Log("Message 3");
            log.Log("Message 4");

            Assert.AreEqual(3, log.ScrollView.childCount);
        }

        [Test]
        public void Log_UnlimitedMaxLines_DoesNotRemoveEntries()
        {
            var log = new JLogView(0); // Unlimited

            for (int i = 0; i < 150; i++)
            {
                log.Log($"Message {i}");
            }

            Assert.AreEqual(150, log.ScrollView.childCount);
        }

        #endregion

        #region LogInfo Tests

        [Test]
        public void LogInfo_AddsEntry()
        {
            _logView.LogInfo("Info message");
            Assert.AreEqual(1, _logView.ScrollView.childCount);
        }

        [Test]
        public void LogInfo_AddsInfoClass()
        {
            _logView.LogInfo("Info message");
            var entry = _logView.ScrollView[0];
            Assert.IsTrue(entry.ClassListContains("j-log-view__entry--info"));
        }

        [Test]
        public void LogInfo_ReturnsLogViewForChaining()
        {
            var result = _logView.LogInfo("Test");
            Assert.AreSame(_logView, result);
        }

        #endregion

        #region LogError Tests

        [Test]
        public void LogError_AddsEntry()
        {
            _logView.LogError("Error message");
            Assert.AreEqual(1, _logView.ScrollView.childCount);
        }

        [Test]
        public void LogError_AddsErrorClass()
        {
            _logView.LogError("Error message");
            var entry = _logView.ScrollView[0];
            Assert.IsTrue(entry.ClassListContains("j-log-view__entry--error"));
        }

        [Test]
        public void LogError_ReturnsLogViewForChaining()
        {
            var result = _logView.LogError("Test");
            Assert.AreSame(_logView, result);
        }

        #endregion

        #region Clear Tests

        [Test]
        public void Clear_RemovesAllEntries()
        {
            _logView.Log("Message 1");
            _logView.Log("Message 2");
            _logView.Log("Message 3");

            _logView.Clear();

            Assert.AreEqual(0, _logView.ScrollView.childCount);
        }

        [Test]
        public void Clear_ReturnsLogViewForChaining()
        {
            var result = _logView.Clear();
            Assert.AreSame(_logView, result);
        }

        [Test]
        public void Clear_ResetsLineCount()
        {
            var log = new JLogView(3);
            log.Log("1");
            log.Log("2");
            log.Log("3");

            log.Clear();

            // After clearing, we should be able to add 3 more without removal
            log.Log("A");
            log.Log("B");
            log.Log("C");

            Assert.AreEqual(3, log.ScrollView.childCount);
        }

        #endregion

        #region WithMinHeight Tests

        [Test]
        public void WithMinHeight_SetsMinHeight()
        {
            _logView.WithMinHeight(150f);
            Assert.AreEqual(150f, _logView.style.minHeight.value.value);
        }

        [Test]
        public void WithMinHeight_ReturnsLogViewForChaining()
        {
            var result = _logView.WithMinHeight(200f);
            Assert.AreSame(_logView, result);
        }

        #endregion

        #region WithMaxHeight Tests

        [Test]
        public void WithMaxHeight_SetsMaxHeight()
        {
            _logView.WithMaxHeight(500f);
            Assert.AreEqual(500f, _logView.style.maxHeight.value.value);
        }

        [Test]
        public void WithMaxHeight_ReturnsLogViewForChaining()
        {
            var result = _logView.WithMaxHeight(400f);
            Assert.AreSame(_logView, result);
        }

        #endregion

        #region ScrollView Property Tests

        [Test]
        public void ScrollView_ReturnsScrollViewElement()
        {
            Assert.IsInstanceOf<ScrollView>(_logView.ScrollView);
        }

        [Test]
        public void ScrollView_IsSameInstanceOnMultipleCalls()
        {
            var sv1 = _logView.ScrollView;
            var sv2 = _logView.ScrollView;
            Assert.AreSame(sv1, sv2);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            _logView
                .WithMinHeight(150f)
                .WithMaxHeight(400f)
                .LogInfo("Info")
                .LogError("Error")
                .Log("Normal");

            Assert.AreEqual(150f, _logView.style.minHeight.value.value);
            Assert.AreEqual(400f, _logView.style.maxHeight.value.value);
            Assert.AreEqual(3, _logView.ScrollView.childCount);
        }

        #endregion
    }
}
