using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SelectableOptionBase), true)]
[CanEditMultipleObjects]
public class SelectableOptionEditor : Editor
{

    private SelectableOptionBase option;
    private bool isEditorShown = true;

    private void OnEnable() {
        option = (SelectableOptionBase)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        isEditorShown = EditorGUILayout.Foldout(isEditorShown, "Navigation");
        if (isEditorShown) {
            DrawCustomInspector();
        }
    }

    private void DrawCustomInspector() {
        SerializedProperty navArrayProperty = serializedObject.FindProperty("navigationArray");

        EditorGUILayout.PropertyField(navArrayProperty.GetArrayElementAtIndex(0), new GUIContent("Up"));
        EditorGUILayout.PropertyField(navArrayProperty.GetArrayElementAtIndex(1), new GUIContent("Right"));
        EditorGUILayout.PropertyField(navArrayProperty.GetArrayElementAtIndex(2), new GUIContent("Down"));
        EditorGUILayout.PropertyField(navArrayProperty.GetArrayElementAtIndex(3), new GUIContent("Left"));

        serializedObject.ApplyModifiedProperties();
    }

}

[CustomEditor(typeof(SelectableGroup), true)]
public class SelectableGroupEditor : Editor
{

    private SelectableGroup group;

    private float[] maxSearchDistances;

    private void OnEnable() {
        group = (SelectableGroup)target;
        group.options = group.GetComponentsInChildren<SelectableOptionBase>().ToList();
        maxSearchDistances = new float[4];
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        DrawCustomInspector();
    }

    private void DrawCustomInspector() {
        if (group.options.Count < 2) {
            EditorGUILayout.HelpBox("This group contains less than 2 options. Please add options to it by creating child objects with an attached script inheriting from SelectableOptionBase!", MessageType.Error);
            return;
        } else {
            EditorGUILayout.HelpBox("This group contains " + group.options.Count + " options.", MessageType.Info);
        }

        if (group.navigationType == SelectableGroup.NavigationBuildType.SMART) {
            EditorGUILayout.HelpBox("Please set the maximum distances to search for Selectable UI elements for each direction.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Up distance");
            maxSearchDistances[0] = EditorGUILayout.FloatField(maxSearchDistances[0]);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Right distance");
            maxSearchDistances[1] = EditorGUILayout.FloatField(maxSearchDistances[1]);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Down distance");
            maxSearchDistances[2] = EditorGUILayout.FloatField(maxSearchDistances[2]);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Left distance");
            maxSearchDistances[3] = EditorGUILayout.FloatField(maxSearchDistances[3]);
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Build navigation")) {
            foreach (SelectableOptionBase option in group.options) {
                option.ResetOptions();
            }

            switch (group.navigationType) {
                case SelectableGroup.NavigationBuildType.HORIZONTAL:
                    SelectableNavigationBuilder.BuildNavigationByXCoord(group);
                    break;
                case SelectableGroup.NavigationBuildType.VERTICAL:
                    SelectableNavigationBuilder.BuildNavigationByYCoord(group);
                    break;
                case SelectableGroup.NavigationBuildType.SMART:
                    SelectableNavigationBuilder.BuildSmartNavigation(group, maxSearchDistances);
                    break;
            }
        }
    }

    
}