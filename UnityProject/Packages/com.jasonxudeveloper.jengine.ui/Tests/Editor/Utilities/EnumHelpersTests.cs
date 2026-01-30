// EnumHelpersTests.cs
// EditMode unit tests for EnumHelpers

using System;
using System.Collections.Generic;
using NUnit.Framework;
using JEngine.UI.Editor.Utilities;
using JEngine.UI.Editor.Theming;

namespace JEngine.UI.Tests.Editor.Utilities
{
    [TestFixture]
    public class EnumHelpersTests
    {
        // Test enum for testing
        private enum TestEnum
        {
            First,
            Second,
            Third
        }

        #region GetEnumNames Tests

        [Test]
        public void GetEnumNames_ReturnsListOfStrings()
        {
            var result = EnumHelpers.GetEnumNames<TestEnum>();
            Assert.IsInstanceOf<List<string>>(result);
        }

        [Test]
        public void GetEnumNames_ReturnsCorrectCount()
        {
            var result = EnumHelpers.GetEnumNames<TestEnum>();
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void GetEnumNames_ContainsAllEnumNames()
        {
            var result = EnumHelpers.GetEnumNames<TestEnum>();

            Assert.Contains("First", result);
            Assert.Contains("Second", result);
            Assert.Contains("Third", result);
        }

        [Test]
        public void GetEnumNames_WithButtonVariant_ReturnsAllVariants()
        {
            var result = EnumHelpers.GetEnumNames<ButtonVariant>();

            Assert.Contains("Primary", result);
            Assert.Contains("Secondary", result);
            Assert.Contains("Success", result);
            Assert.Contains("Danger", result);
            Assert.Contains("Warning", result);
        }

        [Test]
        public void GetEnumNames_WithGapSize_ReturnsAllSizes()
        {
            var result = EnumHelpers.GetEnumNames<GapSize>();

            Assert.Contains("Xs", result);
            Assert.Contains("Sm", result);
            Assert.Contains("MD", result);
            Assert.Contains("Lg", result);
            Assert.Contains("Xl", result);
        }

        [Test]
        public void GetEnumNames_WithStatusType_ReturnsAllTypes()
        {
            var result = EnumHelpers.GetEnumNames<StatusType>();

            Assert.Contains("Info", result);
            Assert.Contains("Success", result);
            Assert.Contains("Warning", result);
            Assert.Contains("Error", result);
        }

        [Test]
        public void GetEnumNames_PreservesOrder()
        {
            var result = EnumHelpers.GetEnumNames<TestEnum>();

            Assert.AreEqual("First", result[0]);
            Assert.AreEqual("Second", result[1]);
            Assert.AreEqual("Third", result[2]);
        }

        #endregion

        #region StringToEnum Tests

        [Test]
        public void StringToEnum_ValidString_ReturnsEnumValue()
        {
            var result = EnumHelpers.StringToEnum<TestEnum>("First");
            Assert.AreEqual(TestEnum.First, result);
        }

        [Test]
        public void StringToEnum_SecondValue_ReturnsCorrectEnum()
        {
            var result = EnumHelpers.StringToEnum<TestEnum>("Second");
            Assert.AreEqual(TestEnum.Second, result);
        }

        [Test]
        public void StringToEnum_ThirdValue_ReturnsCorrectEnum()
        {
            var result = EnumHelpers.StringToEnum<TestEnum>("Third");
            Assert.AreEqual(TestEnum.Third, result);
        }

        [Test]
        public void StringToEnum_WithButtonVariant_ParsesPrimary()
        {
            var result = EnumHelpers.StringToEnum<ButtonVariant>("Primary");
            Assert.AreEqual(ButtonVariant.Primary, result);
        }

        [Test]
        public void StringToEnum_WithButtonVariant_ParsesSecondary()
        {
            var result = EnumHelpers.StringToEnum<ButtonVariant>("Secondary");
            Assert.AreEqual(ButtonVariant.Secondary, result);
        }

        [Test]
        public void StringToEnum_WithButtonVariant_ParsesSuccess()
        {
            var result = EnumHelpers.StringToEnum<ButtonVariant>("Success");
            Assert.AreEqual(ButtonVariant.Success, result);
        }

        [Test]
        public void StringToEnum_WithButtonVariant_ParsesDanger()
        {
            var result = EnumHelpers.StringToEnum<ButtonVariant>("Danger");
            Assert.AreEqual(ButtonVariant.Danger, result);
        }

        [Test]
        public void StringToEnum_WithButtonVariant_ParsesWarning()
        {
            var result = EnumHelpers.StringToEnum<ButtonVariant>("Warning");
            Assert.AreEqual(ButtonVariant.Warning, result);
        }

        [Test]
        public void StringToEnum_InvalidString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EnumHelpers.StringToEnum<TestEnum>("Invalid"));
        }

        [Test]
        public void StringToEnum_EmptyString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EnumHelpers.StringToEnum<TestEnum>(""));
        }

        [Test]
        public void StringToEnum_CaseSensitive_ThrowsOnWrongCase()
        {
            Assert.Throws<ArgumentException>(() => EnumHelpers.StringToEnum<TestEnum>("first"));
        }

        [Test]
        public void StringToEnum_WithGapSize_ParsesMD()
        {
            var result = EnumHelpers.StringToEnum<GapSize>("MD");
            Assert.AreEqual(GapSize.MD, result);
        }

        [Test]
        public void StringToEnum_WithStatusType_ParsesError()
        {
            var result = EnumHelpers.StringToEnum<StatusType>("Error");
            Assert.AreEqual(StatusType.Error, result);
        }

        #endregion

        #region Roundtrip Tests

        [Test]
        public void Roundtrip_GetNames_ThenParse_ReturnsOriginalValues()
        {
            var names = EnumHelpers.GetEnumNames<TestEnum>();

            foreach (var name in names)
            {
                var parsed = EnumHelpers.StringToEnum<TestEnum>(name);
                Assert.AreEqual(name, parsed.ToString());
            }
        }

        [Test]
        public void Roundtrip_ButtonVariant_AllValuesRoundtrip()
        {
            var names = EnumHelpers.GetEnumNames<ButtonVariant>();

            foreach (var name in names)
            {
                var parsed = EnumHelpers.StringToEnum<ButtonVariant>(name);
                Assert.AreEqual(name, parsed.ToString());
            }
        }

        [Test]
        public void Roundtrip_GapSize_AllValuesRoundtrip()
        {
            var names = EnumHelpers.GetEnumNames<GapSize>();

            foreach (var name in names)
            {
                var parsed = EnumHelpers.StringToEnum<GapSize>(name);
                Assert.AreEqual(name, parsed.ToString());
            }
        }

        [Test]
        public void Roundtrip_StatusType_AllValuesRoundtrip()
        {
            var names = EnumHelpers.GetEnumNames<StatusType>();

            foreach (var name in names)
            {
                var parsed = EnumHelpers.StringToEnum<StatusType>(name);
                Assert.AreEqual(name, parsed.ToString());
            }
        }

        #endregion
    }
}
