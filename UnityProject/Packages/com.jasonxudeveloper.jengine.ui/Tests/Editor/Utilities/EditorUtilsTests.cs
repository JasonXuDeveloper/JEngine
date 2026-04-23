// EditorUtilsTests.cs
// EditMode unit tests for JEngine.Core.Editor.EditorUtils dropdown helpers.

using System.Collections.Generic;
using NUnit.Framework;
using JEngine.Core.Editor;

namespace JEngine.UI.Tests.Editor.Utilities
{
    [TestFixture]
    public class EditorUtilsTests
    {
        #region WithNoneOption Tests

        [Test]
        public void WithNoneOption_NullChoices_ReturnsSingletonNone()
        {
            var result = EditorUtils.WithNoneOption(null);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(EditorUtils.NoneDropdownOption, result[0]);
        }

        [Test]
        public void WithNoneOption_EmptyChoices_ReturnsSingletonNone()
        {
            var result = EditorUtils.WithNoneOption(new List<string>());

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(EditorUtils.NoneDropdownOption, result[0]);
        }

        [Test]
        public void WithNoneOption_PopulatedChoices_PrependsNoneInOrder()
        {
            var result = EditorUtils.WithNoneOption(new List<string> { "main", "addon1", "raw" });

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(EditorUtils.NoneDropdownOption, result[0]);
            Assert.AreEqual("main", result[1]);
            Assert.AreEqual("addon1", result[2]);
            Assert.AreEqual("raw", result[3]);
        }

        [Test]
        public void WithNoneOption_ReturnsNewList_DoesNotMutateInput()
        {
            var input = new List<string> { "a", "b" };
            var result = EditorUtils.WithNoneOption(input);

            Assert.AreEqual(2, input.Count, "Input list should not be modified");
            Assert.AreNotSame(input, result);
        }

        #endregion

        #region ResolveDropdownValue Tests

        [Test]
        public void ResolveDropdownValue_StoredValuePresent_ReturnsStoredValue()
        {
            var options = new List<string> { EditorUtils.NoneDropdownOption, "main", "addon1" };
            var result = EditorUtils.ResolveDropdownValue("main", options);

            Assert.AreEqual("main", result);
        }

        [Test]
        public void ResolveDropdownValue_StoredValueMissing_ReturnsNone()
        {
            var options = new List<string> { EditorUtils.NoneDropdownOption, "game" };
            var result = EditorUtils.ResolveDropdownValue("main", options);

            Assert.AreEqual(EditorUtils.NoneDropdownOption, result);
        }

        [Test]
        public void ResolveDropdownValue_StoredValueEmpty_ReturnsNone()
        {
            var options = new List<string> { EditorUtils.NoneDropdownOption, "main" };

            Assert.AreEqual(EditorUtils.NoneDropdownOption, EditorUtils.ResolveDropdownValue("", options));
            Assert.AreEqual(EditorUtils.NoneDropdownOption, EditorUtils.ResolveDropdownValue(null, options));
        }

        [Test]
        public void ResolveDropdownValue_OptionsNull_ReturnsNone()
        {
            var result = EditorUtils.ResolveDropdownValue("main", null);

            Assert.AreEqual(EditorUtils.NoneDropdownOption, result);
        }

        #endregion

        #region NormalizeDropdownSelection Tests

        [Test]
        public void NormalizeDropdownSelection_NoneSentinel_ReturnsEmpty()
        {
            var result = EditorUtils.NormalizeDropdownSelection(EditorUtils.NoneDropdownOption);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void NormalizeDropdownSelection_RealValue_ReturnsAsIs()
        {
            Assert.AreEqual("main", EditorUtils.NormalizeDropdownSelection("main"));
            Assert.AreEqual("HotUpdate.Code.dll", EditorUtils.NormalizeDropdownSelection("HotUpdate.Code.dll"));
        }

        [Test]
        public void NormalizeDropdownSelection_EmptyString_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, EditorUtils.NormalizeDropdownSelection(string.Empty));
        }

        #endregion

        #region Roundtrip Tests

        [Test]
        public void Roundtrip_StoredValuePresent_PreservesValue()
        {
            var options = EditorUtils.WithNoneOption(new List<string> { "main", "addon1" });
            var current = EditorUtils.ResolveDropdownValue("main", options);
            var normalized = EditorUtils.NormalizeDropdownSelection(current);

            Assert.AreEqual("main", normalized);
        }

        [Test]
        public void Roundtrip_StoredValueMissing_NormalizesToEmpty()
        {
            // Simulates the package-renamed case: "main" was stored but current choices only
            // contain "game" after the rename - the dropdown should display "None" and, if the
            // user confirms, the stored value should be cleared to empty.
            var options = EditorUtils.WithNoneOption(new List<string> { "game" });
            var current = EditorUtils.ResolveDropdownValue("main", options);
            var normalized = EditorUtils.NormalizeDropdownSelection(current);

            Assert.AreEqual(EditorUtils.NoneDropdownOption, current);
            Assert.AreEqual(string.Empty, normalized);
        }

        #endregion
    }
}
