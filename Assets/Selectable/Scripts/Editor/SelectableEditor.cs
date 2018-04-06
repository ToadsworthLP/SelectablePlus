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
                    BuildNavigationFromSortedArray(SortByXPosFirst(group.options), group.navigationType);
                    break;
                case SelectableGroup.NavigationBuildType.VERTICAL:
                    BuildNavigationFromSortedArray(SortByYPosFirst(group.options), group.navigationType);
                    break;
                case SelectableGroup.NavigationBuildType.SMART:
                    BuildSmartNavigation(SortByYPosFirst(group.options), maxSearchDistances);
                    break;
            }
        }
    }

    private void BuildSmartNavigation(List<SelectableOptionBase> options, float[] maxSearchDistances) {
        List<BoxCollider2D> cachedColliders = new List<BoxCollider2D>();
        List<RectTransform> cachedTransforms = new List<RectTransform>();
        List<int> cachedLayerIndices = new List<int>();

        //Attach BoxCollider2Ds to every option and set to to default layer to allow Raycasts to work
        foreach (SelectableOptionBase option in options) {
            BoxCollider2D collider = option.gameObject.AddComponent<BoxCollider2D>();
            RectTransform optionRectTransform = option.GetComponent<RectTransform>();

            collider.size = optionRectTransform.sizeDelta;
            cachedColliders.Add(collider);
            cachedTransforms.Add(optionRectTransform);
            cachedLayerIndices.Add(option.gameObject.layer);

            option.gameObject.layer = 0;
        }

        //Raycast from each control border to a direction for the given distance for the direction
        for (int i = 0; i < options.Count; i++){
            for (int j = 0; j < maxSearchDistances.Length; j++) {
                SelectableNavigationDirection direction = (SelectableNavigationDirection)j;
                RectTransform optionRectTransform = cachedTransforms[i];

                RaycastHit2D hit = Physics2D.Raycast(GetRaycastOrigin(direction, optionRectTransform), GetRaycastVector(direction), maxSearchDistances[j]);

                if(hit.transform != null){
                    SelectableOptionBase hitOption = hit.transform.GetComponent<SelectableOptionBase>();
                    if (hitOption != null) {
                        options[i].SetNextOption(direction, hitOption);
                    }
                }
            }
        }

        //Destroy the previously created BoxColliders and restore previous layer
        for (int i = 0; i < options.Count; i++) {
            DestroyImmediate(cachedColliders[i]);
            options[i].gameObject.layer = cachedLayerIndices[i];
        }
    }

    private Vector2 GetRaycastOrigin(SelectableNavigationDirection raycastDirection, RectTransform optionTransform){
        Vector2 center = optionTransform.position;
        switch (raycastDirection) {
            case SelectableNavigationDirection.UP:
                return center + new Vector2(0, optionTransform.sizeDelta.y / 2 + 1);
            case SelectableNavigationDirection.RIGHT:
                return center + new Vector2(optionTransform.sizeDelta.x / 2 + 1, 0);
            case SelectableNavigationDirection.DOWN:
                return center - new Vector2(0, optionTransform.sizeDelta.y / 2 + 1);
            case SelectableNavigationDirection.LEFT:
                return center - new Vector2(optionTransform.sizeDelta.x / 2 + 1, 0);
        }

        return center;
    }

    private Vector2 GetRaycastVector(SelectableNavigationDirection direction) {
        switch (direction) {
            case SelectableNavigationDirection.UP:
                return new Vector2(0, 1);
            case SelectableNavigationDirection.RIGHT:
                return new Vector2(1, 0);
            case SelectableNavigationDirection.DOWN:
                return new Vector2(0, -1);
            case SelectableNavigationDirection.LEFT:
                return new Vector2(-1, 0);
        }

        return Vector2.zero;
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
        }
    }

    private List<SelectableOptionBase> SortByXPosFirst(List<SelectableOptionBase> list) {
        list.Sort(delegate (SelectableOptionBase b, SelectableOptionBase a) {
            Transform aTransform = a.GetComponent<Transform>();
            Transform bTransform = b.GetComponent<Transform>();

            int x = aTransform.position.x.CompareTo(bTransform.position.x);

            if (x == 0)
                return bTransform.position.y.CompareTo(aTransform.position.y);

            return x;
        });

        return list;
    }

    private List<SelectableOptionBase> SortByYPosFirst(List<SelectableOptionBase> list) {
        list.Sort(delegate (SelectableOptionBase b, SelectableOptionBase a) {
            Transform aTransform = a.GetComponent<Transform>();
            Transform bTransform = b.GetComponent<Transform>();

            int x = aTransform.position.y.CompareTo(bTransform.position.y);

            if (x == 0)
                return bTransform.position.x.CompareTo(aTransform.position.x);

            return x;
        });

        return list;
    }
}