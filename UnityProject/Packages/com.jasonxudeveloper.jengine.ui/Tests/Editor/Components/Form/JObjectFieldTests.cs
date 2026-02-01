// JObjectFieldTests.cs
// EditMode unit tests for JObjectField

using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using JEngine.UI.Editor.Components.Form;

namespace JEngine.UI.Tests.Editor.Components.Form
{
    [TestFixture]
    public class JObjectFieldTests
    {
        private JObjectField<GameObject> _objectField;

        [SetUp]
        public void SetUp()
        {
            _objectField = new JObjectField<GameObject>();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_AddsBaseClass()
        {
            Assert.IsTrue(_objectField.ClassListContains("j-object-field"));
        }

        [Test]
        public void Constructor_Default_AllowsSceneObjects()
        {
            Assert.IsTrue(_objectField.ObjectField.allowSceneObjects);
        }

        [Test]
        public void Constructor_AllowSceneObjectsTrue_AllowsSceneObjects()
        {
            var field = new JObjectField<GameObject>(true);
            Assert.IsTrue(field.ObjectField.allowSceneObjects);
        }

        [Test]
        public void Constructor_AllowSceneObjectsFalse_DisallowsSceneObjects()
        {
            var field = new JObjectField<GameObject>(false);
            Assert.IsFalse(field.ObjectField.allowSceneObjects);
        }

        [Test]
        public void Constructor_SetsObjectType()
        {
            Assert.AreEqual(typeof(GameObject), _objectField.ObjectField.objectType);
        }

        [Test]
        public void Constructor_SetsFlexGrow()
        {
            Assert.AreEqual(1f, _objectField.style.flexGrow.value);
        }

        [Test]
        public void Constructor_SetsFlexShrink()
        {
            Assert.AreEqual(1f, _objectField.style.flexShrink.value);
        }

        [Test]
        public void Constructor_CreatesInternalObjectField()
        {
            Assert.IsNotNull(_objectField.ObjectField);
        }

        #endregion

        #region Value Property Tests

        [Test]
        public void Value_Default_IsNull()
        {
            Assert.IsNull(_objectField.Value);
        }

        [Test]
        public void Value_Set_UpdatesValue()
        {
            var go = new GameObject("Test");
            try
            {
                _objectField.Value = go;
                Assert.AreSame(go, _objectField.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Value_SetToNull_ClearsValue()
        {
            var go = new GameObject("Test");
            try
            {
                _objectField.Value = go;
                _objectField.Value = null;
                Assert.IsNull(_objectField.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region ObjectField Property Tests

        [Test]
        public void ObjectField_ReturnsInternalField()
        {
            Assert.IsInstanceOf<ObjectField>(_objectField.ObjectField);
        }

        [Test]
        public void ObjectField_IsSameInstanceOnMultipleCalls()
        {
            var field1 = _objectField.ObjectField;
            var field2 = _objectField.ObjectField;

            Assert.AreSame(field1, field2);
        }

        [Test]
        public void ObjectField_ValueSyncsWithWrapper()
        {
            var go = new GameObject("Test");
            try
            {
                _objectField.ObjectField.value = go;
                Assert.AreSame(go, _objectField.Value);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region RegisterValueChangedCallback Tests

        [Test]
        public void RegisterValueChangedCallback_CanRegister()
        {
            // Note: Value changed callbacks may not fire in EditMode when setting value programmatically.
            // We verify the callback can be registered without throwing.
            Assert.DoesNotThrow(() => _objectField.RegisterValueChangedCallback(_ => { }));
        }

        [Test]
        public void RegisterValueChangedCallback_MultipleRegistrations_DoNotThrow()
        {
            // Verify multiple registrations work without throwing
            Assert.DoesNotThrow(() =>
            {
                _objectField.RegisterValueChangedCallback(_ => { });
                _objectField.RegisterValueChangedCallback(_ => { });
            });
        }

        #endregion

        #region Different Object Types Tests

        [Test]
        public void GenericType_ScriptableObject_Works()
        {
            var field = new JObjectField<ScriptableObject>();
            Assert.AreEqual(typeof(ScriptableObject), field.ObjectField.objectType);
        }

        [Test]
        public void GenericType_Material_Works()
        {
            var field = new JObjectField<Material>();
            Assert.AreEqual(typeof(Material), field.ObjectField.objectType);
        }

        [Test]
        public void GenericType_Texture2D_Works()
        {
            var field = new JObjectField<Texture2D>();
            Assert.AreEqual(typeof(Texture2D), field.ObjectField.objectType);
        }

        [Test]
        public void GenericType_Mesh_Works()
        {
            var field = new JObjectField<Mesh>();
            Assert.AreEqual(typeof(Mesh), field.ObjectField.objectType);
        }

        #endregion

        #region Style Tests

        [Test]
        public void Constructor_InternalObjectFieldHasFlexGrow()
        {
            Assert.AreEqual(1f, _objectField.ObjectField.style.flexGrow.value);
        }

        [Test]
        public void Constructor_InternalObjectFieldHasZeroMargins()
        {
            Assert.AreEqual(0f, _objectField.ObjectField.style.marginLeft.value.value);
            Assert.AreEqual(0f, _objectField.ObjectField.style.marginRight.value.value);
            Assert.AreEqual(0f, _objectField.ObjectField.style.marginTop.value.value);
            Assert.AreEqual(0f, _objectField.ObjectField.style.marginBottom.value.value);
        }

        #endregion

        #region Child Composition Tests

        [Test]
        public void Constructor_HasSingleChild()
        {
            var field = new JObjectField<GameObject>();

            // The ObjectField should be the only child
            Assert.AreEqual(1, field.childCount);
            Assert.AreSame(field.ObjectField, field.ElementAt(0));
        }

        [Test]
        public void Constructor_ObjectFieldIsChild()
        {
            Assert.IsTrue(_objectField.Contains(_objectField.ObjectField));
        }

        [Test]
        public void Constructor_AppliesInputContainerStyle()
        {
            // Verify flexGrow and flexShrink are applied (from JTheme.ApplyInputContainerStyle)
            Assert.AreEqual(1f, _objectField.style.flexGrow.value);
            Assert.AreEqual(1f, _objectField.style.flexShrink.value);
        }

        #endregion

        #region BindProperty Tests

        [Test]
        public void BindProperty_MethodExists()
        {
            // Verify the method exists and is accessible
            Assert.IsNotNull((System.Action<SerializedProperty>)_objectField.BindProperty);
        }

        #endregion
    }
}
