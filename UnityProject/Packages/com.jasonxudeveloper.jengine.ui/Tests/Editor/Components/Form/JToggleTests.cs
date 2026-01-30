// JToggleTests.cs
// EditMode unit tests for JToggle

using NUnit.Framework;
using UnityEngine.UIElements;
using JEngine.UI.Editor.Components.Form;

namespace JEngine.UI.Tests.Editor.Components.Form
{
    [TestFixture]
    public class JToggleTests
    {
        private JToggle _toggle;

        [SetUp]
        public void SetUp()
        {
            _toggle = new JToggle();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_Default_AddsBaseClass()
        {
            Assert.IsTrue(_toggle.ClassListContains("j-toggle"));
        }

        [Test]
        public void Constructor_Default_ValueIsFalse()
        {
            Assert.IsFalse(_toggle.Value);
        }

        [Test]
        public void Constructor_WithTrueValue_SetsValueToTrue()
        {
            var toggle = new JToggle(true);
            Assert.IsTrue(toggle.Value);
        }

        [Test]
        public void Constructor_WithFalseValue_SetsValueToFalse()
        {
            var toggle = new JToggle(false);
            Assert.IsFalse(toggle.Value);
        }

        [Test]
        public void Constructor_SetsWidth()
        {
            Assert.AreEqual(36f, _toggle.style.width.value.value);
        }

        [Test]
        public void Constructor_SetsHeight()
        {
            Assert.AreEqual(18f, _toggle.style.height.value.value);
        }

        [Test]
        public void Constructor_SetsFlexShrinkToZero()
        {
            Assert.AreEqual(0f, _toggle.style.flexShrink.value);
        }

        [Test]
        public void Constructor_SetsFlexGrowToZero()
        {
            Assert.AreEqual(0f, _toggle.style.flexGrow.value);
        }

        [Test]
        public void Constructor_SetsAlignSelfToCenter()
        {
            Assert.AreEqual(Align.Center, _toggle.style.alignSelf.value);
        }

        [Test]
        public void Constructor_CreatesTrackElement()
        {
            var track = _toggle.Q(className: "j-toggle__track");
            Assert.IsNotNull(track);
        }

        [Test]
        public void Constructor_CreatesThumbElement()
        {
            var thumb = _toggle.Q(className: "j-toggle__thumb");
            Assert.IsNotNull(thumb);
        }

        #endregion

        #region Value Property Tests

        [Test]
        public void Value_Get_ReturnsCurrentValue()
        {
            var toggle = new JToggle(true);
            Assert.IsTrue(toggle.Value);
        }

        [Test]
        public void Value_Set_True_UpdatesValue()
        {
            _toggle.Value = true;
            Assert.IsTrue(_toggle.Value);
        }

        [Test]
        public void Value_Set_False_UpdatesValue()
        {
            var toggle = new JToggle(true);
            toggle.Value = false;
            Assert.IsFalse(toggle.Value);
        }

        [Test]
        public void Value_Set_SameValue_DoesNotChangeValue()
        {
            _toggle.Value = false;

            // Setting same value shouldn't change anything
            _toggle.Value = false;

            Assert.IsFalse(_toggle.Value);
        }

        #endregion

        #region SetValueWithoutNotify Tests

        [Test]
        public void SetValueWithoutNotify_True_UpdatesValue()
        {
            _toggle.SetValueWithoutNotify(true);
            Assert.IsTrue(_toggle.Value);
        }

        [Test]
        public void SetValueWithoutNotify_False_UpdatesValue()
        {
            var toggle = new JToggle(true);
            toggle.SetValueWithoutNotify(false);
            Assert.IsFalse(toggle.Value);
        }

        [Test]
        public void SetValueWithoutNotify_DoesNotInvokeCallback()
        {
            bool callbackInvoked = false;
            _toggle.OnValueChanged(_ => callbackInvoked = true);

            _toggle.SetValueWithoutNotify(true);

            Assert.IsFalse(callbackInvoked);
        }

        #endregion

        #region OnValueChanged Tests

        [Test]
        public void OnValueChanged_ReturnsToggleForChaining()
        {
            var result = _toggle.OnValueChanged(_ => { });
            Assert.AreSame(_toggle, result);
        }

        [Test]
        public void OnValueChanged_CanRegisterCallback()
        {
            // Note: OnValueChanged callback is only invoked on click, not on Value setter.
            // We verify the callback can be registered without throwing.
            Assert.DoesNotThrow(() => _toggle.OnValueChanged(_ => { }));
        }

        [Test]
        public void OnValueChanged_ReplacesExistingCallback()
        {
            // Verify second call replaces first (no exception)
            Assert.DoesNotThrow(() =>
            {
                _toggle.OnValueChanged(_ => { });
                _toggle.OnValueChanged(_ => { });
            });
        }

        [Test]
        public void Value_Set_DoesNotInvokeCallback()
        {
            // By design, setting Value programmatically doesn't invoke callback.
            // Only click events invoke the callback.
            bool callbackInvoked = false;
            _toggle.OnValueChanged(_ => callbackInvoked = true);

            _toggle.Value = true;

            Assert.IsFalse(callbackInvoked);
        }

        #endregion

        #region WithClass Tests

        [Test]
        public void WithClass_AddsClassName()
        {
            _toggle.WithClass("custom-class");
            Assert.IsTrue(_toggle.ClassListContains("custom-class"));
        }

        [Test]
        public void WithClass_ReturnsToggleForChaining()
        {
            var result = _toggle.WithClass("test");
            Assert.AreSame(_toggle, result);
        }

        [Test]
        public void WithClass_PreservesBaseClass()
        {
            _toggle.WithClass("custom");
            Assert.IsTrue(_toggle.ClassListContains("j-toggle"));
        }

        [Test]
        public void WithClass_CanAddMultipleClasses()
        {
            _toggle.WithClass("class1").WithClass("class2");

            Assert.IsTrue(_toggle.ClassListContains("class1"));
            Assert.IsTrue(_toggle.ClassListContains("class2"));
        }

        #endregion

        #region Visual State Tests

        [Test]
        public void Track_InitiallyHasOffColor()
        {
            var track = _toggle.Q(className: "j-toggle__track");
            // Track exists and has background color set
            Assert.IsNotNull(track);
            Assert.IsTrue(track.style.backgroundColor.keyword != StyleKeyword.Null);
        }

        [Test]
        public void Thumb_InitiallyAtOffPosition()
        {
            var thumb = _toggle.Q(className: "j-toggle__thumb");
            Assert.AreEqual(2f, thumb.style.left.value.value);
        }

        [Test]
        public void Thumb_WhenOn_AtOnPosition()
        {
            var toggle = new JToggle(true);
            var thumb = toggle.Q(className: "j-toggle__thumb");
            Assert.AreEqual(20f, thumb.style.left.value.value);
        }

        [Test]
        public void Thumb_Width_IsCorrect()
        {
            var thumb = _toggle.Q(className: "j-toggle__thumb");
            Assert.AreEqual(14f, thumb.style.width.value.value);
        }

        [Test]
        public void Thumb_Height_IsCorrect()
        {
            var thumb = _toggle.Q(className: "j-toggle__thumb");
            Assert.AreEqual(14f, thumb.style.height.value.value);
        }

        [Test]
        public void Track_HasRoundedCorners()
        {
            var track = _toggle.Q(className: "j-toggle__track");
            Assert.AreEqual(9f, track.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(9f, track.style.borderTopRightRadius.value.value);
            Assert.AreEqual(9f, track.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(9f, track.style.borderBottomRightRadius.value.value);
        }

        [Test]
        public void Thumb_HasRoundedCorners()
        {
            var thumb = _toggle.Q(className: "j-toggle__thumb");
            Assert.AreEqual(7f, thumb.style.borderTopLeftRadius.value.value);
            Assert.AreEqual(7f, thumb.style.borderTopRightRadius.value.value);
            Assert.AreEqual(7f, thumb.style.borderBottomLeftRadius.value.value);
            Assert.AreEqual(7f, thumb.style.borderBottomRightRadius.value.value);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentApi_CanChainMultipleMethods()
        {
            var result = _toggle
                .WithClass("custom")
                .OnValueChanged(_ => { });

            Assert.AreSame(_toggle, result);
            Assert.IsTrue(_toggle.ClassListContains("custom"));
        }

        #endregion
    }
}
