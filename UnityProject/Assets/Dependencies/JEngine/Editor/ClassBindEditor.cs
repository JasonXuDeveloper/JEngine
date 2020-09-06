using UnityEngine;
using UnityEditor;
using System.Collections;
using Malee.List;
using System;
using JEngine.Core;

[CanEditMultipleObjects]
[CustomEditor(typeof(ClassBind))]
public class ClassBindEditor : Editor {
	
    private ReorderableList list1;

    void OnEnable() {

        list1 = new ReorderableList(serializedObject.FindProperty("ScriptsToBind"));
        list1.elementNameProperty = "Class";
    }

    public override void OnInspectorGUI() {

        serializedObject.Update();

        //draw the list using GUILayout, you can of course specify your own position and label
        list1.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}