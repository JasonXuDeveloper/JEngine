using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Malee.List {

	[CustomPropertyDrawer(typeof(ReorderableAttribute))]
	public class ReorderableDrawer : PropertyDrawer {

		public const string ARRAY_PROPERTY_NAME = "array";

		private static Dictionary<int, ReorderableList> lists = new Dictionary<int, ReorderableList>();

		public override bool CanCacheInspectorGUI(SerializedProperty property) {

			return false;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			ReorderableList list = GetList(property, attribute as ReorderableAttribute, ARRAY_PROPERTY_NAME);

			return list != null ? list.GetHeight() : EditorGUIUtility.singleLineHeight;
		}		

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			ReorderableList list = GetList(property, attribute as ReorderableAttribute, ARRAY_PROPERTY_NAME);

			if (list != null) {

				list.DoList(EditorGUI.IndentedRect(position), label);
			}
			else {

				GUI.Label(position, "Array must extend from ReorderableArray", EditorStyles.label);
			}
		}

		public static int GetListId(SerializedProperty property) {

			if (property != null) {

				int h1 = property.serializedObject.targetObject.GetHashCode();
				int h2 = property.propertyPath.GetHashCode();

				return (((h1 << 5) + h1) ^ h2);
			}

			return 0;
		}

		public static ReorderableList GetList(SerializedProperty property, string arrayPropertyName) {

			return GetList(property, null, GetListId(property), arrayPropertyName);
		}

		public static ReorderableList GetList(SerializedProperty property, ReorderableAttribute attrib, string arrayPropertyName) {

			return GetList(property, attrib, GetListId(property), arrayPropertyName);
		}

		public static ReorderableList GetList(SerializedProperty property, int id, string arrayPropertyName) {

			return GetList(property, null, id, arrayPropertyName);
		}

		public static ReorderableList GetList(SerializedProperty property, ReorderableAttribute attrib, int id, string arrayPropertyName) {

			if (property == null) {

				return null;
			}

			ReorderableList list = null;
			SerializedProperty array = property.FindPropertyRelative(arrayPropertyName);

			if (array != null && array.isArray) {

				if (!lists.TryGetValue(id, out list)) {

					if (attrib != null) {

						Texture icon = !string.IsNullOrEmpty(attrib.elementIconPath) ? AssetDatabase.GetCachedIcon(attrib.elementIconPath) : null;

						ReorderableList.ElementDisplayType displayType = attrib.singleLine ? ReorderableList.ElementDisplayType.SingleLine : ReorderableList.ElementDisplayType.Auto;

						list = new ReorderableList(array, attrib.add, attrib.remove, attrib.draggable, displayType, attrib.elementNameProperty, attrib.elementNameOverride, icon);
						list.paginate = attrib.paginate;
						list.pageSize = attrib.pageSize;
						list.sortable = attrib.sortable;
						list.elementLabels = attrib.labels;

						//handle surrogate if any

						if (attrib.surrogateType != null) {

							SurrogateCallback callback = new SurrogateCallback(attrib.surrogateProperty);

							list.surrogate = new ReorderableList.Surrogate(attrib.surrogateType, callback.SetReference);
						}
					}
					else {

						list = new ReorderableList(array, true, true, true);
					}

					lists.Add(id, list);
				}
				else {

					list.List = array;
				}
			}

			return list;
		}

		private struct SurrogateCallback {

			private string property;

			internal SurrogateCallback(string property) {

				this.property = property;
			}

			internal void SetReference(SerializedProperty element, Object objectReference, ReorderableList list) {

				SerializedProperty prop = !string.IsNullOrEmpty(property) ? element.FindPropertyRelative(property) : null;

				if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference) {

					prop.objectReferenceValue = objectReference;
				}
			}
		}
	}
}
