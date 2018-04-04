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

    private void OnEnable() {
        group = (SelectableGroup)target;
        group.options = group.GetComponentsInChildren<SelectableOptionBase>().ToList();
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        DrawCustomInspector();
    }

    private void DrawCustomInspector() {
        if (group.options.Count < 2) {
            EditorGUILayout.HelpBox("This group is empty. Please add options to it by creating child objects with an attached script inheriting from SelectableOptionBase!", MessageType.Error);
            return;
        } else {
            EditorGUILayout.HelpBox("This group contains " + group.options.Count + " options.", MessageType.Info);
        }

        if (GUILayout.Button("Build navigation")) {
            foreach (SelectableOptionBase option in group.options) {
                option.ResetOptions();
            }

            switch (group.navigationType) {
                case SelectableGroup.NavigationBuildType.HORIZONTAL:
                    BuildNavigationFromSortedArray(SortByXPos(group.options), group.navigationType);
                    break;
                case SelectableGroup.NavigationBuildType.VERTICAL:
                    BuildNavigationFromSortedArray(SortByYPos(group.options), group.navigationType);
                    break;
                case SelectableGroup.NavigationBuildType.BOTH:
                    Debug.Log(group.navigationType.ToString());
                    break;
            }
        }
    }

    private void BuildNavigationFromSortedArray(List<SelectableOptionBase> sortedOptions, SelectableGroup.NavigationBuildType direction) {
        switch (direction) {
            case SelectableGroup.NavigationBuildType.HORIZONTAL:
                sortedOptions[0].SetNextOption(SelectableNavigationDirection.RIGHT, sortedOptions[1]);
                for (int i = 1; i <= sortedOptions.Count - 2; i++) {
                    sortedOptions[i].SetNextOption(SelectableNavigationDirection.LEFT, sortedOptions[i - 1]);
                    sortedOptions[i].SetNextOption(SelectableNavigationDirection.RIGHT, sortedOptions[i + 1]);
                }
                sortedOptions[sortedOptions.Count - 1].SetNextOption(SelectableNavigationDirection.LEFT, sortedOptions[sortedOptions.Count - 2]);
                break;

            case SelectableGroup.NavigationBuildType.VERTICAL:
                sortedOptions[0].SetNextOption(SelectableNavigationDirection.DOWN, sortedOptions[1]);
                for (int i = 1; i <= sortedOptions.Count - 2; i++) {
                    sortedOptions[i].SetNextOption(SelectableNavigationDirection.UP, sortedOptions[i - 1]);
                    sortedOptions[i].SetNextOption(SelectableNavigationDirection.DOWN, sortedOptions[i + 1]);
                }
                sortedOptions[sortedOptions.Count - 1].SetNextOption(SelectableNavigationDirection.UP, sortedOptions[sortedOptions.Count - 2]);
                break;

            case SelectableGroup.NavigationBuildType.BOTH:
                //TODO Implement this
                break;
        }
    }

    private List<SelectableOptionBase> SortByXPos(List<SelectableOptionBase> list) {
        list.Sort(delegate (SelectableOptionBase b, SelectableOptionBase a) {
            return a.GetComponent<Transform>().position.x.CompareTo(b.GetComponent<Transform>().position.x);
        });

        return list;
    }

    private List<SelectableOptionBase> SortByYPos(List<SelectableOptionBase> list) {
        list.Sort(delegate (SelectableOptionBase b, SelectableOptionBase a) {
            return a.GetComponent<Transform>().position.y.CompareTo(b.GetComponent<Transform>().position.y);
        });

        return list;
    }

}