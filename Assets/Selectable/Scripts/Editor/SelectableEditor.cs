using System.Linq;
using UnityEditor;
using UnityEngine;
using SelectablePlus.Navigation;
using SelectablePlus;

[CustomEditor(typeof(SelectableOptionBase), true)]
[CanEditMultipleObjects]
public class SelectableOptionEditor : Editor {

    private SelectableOptionBase option;
    private static bool isNavigationEditorShown = true;
    private static bool showNavigation = true;

    private void OnEnable() {
        option = (SelectableOptionBase)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        isNavigationEditorShown = EditorGUILayout.Foldout(isNavigationEditorShown, "Navigation");
        if (isNavigationEditorShown) {
            DrawCustomInspector();
        }
    }

    private void OnSceneGUI() {
        if (!showNavigation)
            return;

        SelectableEditorUtils.DrawNavigationForSelectable(option);
    }

    private void DrawCustomInspector() {
        SerializedProperty navArrayProperty = serializedObject.FindProperty("navigationArray");

        EditorGUILayout.PropertyField(navArrayProperty.GetArrayElementAtIndex(0), new GUIContent("Up"));
        EditorGUILayout.PropertyField(navArrayProperty.GetArrayElementAtIndex(1), new GUIContent("Right"));
        EditorGUILayout.PropertyField(navArrayProperty.GetArrayElementAtIndex(2), new GUIContent("Down"));
        EditorGUILayout.PropertyField(navArrayProperty.GetArrayElementAtIndex(3), new GUIContent("Left"));

        serializedObject.ApplyModifiedProperties();

        showNavigation = GUILayout.Toggle(showNavigation, "Show Navigation");
    }

}

[CustomEditor(typeof(SelectableGroup), true)]
public class SelectableGroupEditor : Editor {

    private SelectableGroup group;
    private static bool showNavigation;

    private float[] maxRaycastDistances;

    private float maxDistance = -1;
    private float minScore = -1;

    private void OnEnable() {
        group = (SelectableGroup)target;
        group.options = group.GetComponentsInChildren<SelectableOptionBase>().ToList();
        maxRaycastDistances = new float[4];
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

        if (group.navigationType == SelectableGroup.NavigationBuildType.RAYCAST) {
            EditorGUILayout.HelpBox("Please set the maximum distances to search for Selectable UI elements for each direction.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Up distance");
            maxRaycastDistances[0] = EditorGUILayout.FloatField(maxRaycastDistances[0]);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Right distance");
            maxRaycastDistances[1] = EditorGUILayout.FloatField(maxRaycastDistances[1]);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Down distance");
            maxRaycastDistances[2] = EditorGUILayout.FloatField(maxRaycastDistances[2]);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Left distance");
            maxRaycastDistances[3] = EditorGUILayout.FloatField(maxRaycastDistances[3]);
            EditorGUILayout.EndHorizontal();
        }

        if (group.navigationType == SelectableGroup.NavigationBuildType.UNITY) {
            EditorGUILayout.HelpBox("Set the maximum allowed distance between options and the minimum score for an option to be considered valid. If negative values are set, the check will be skipped.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Max Distance");
            maxDistance = EditorGUILayout.FloatField(maxDistance);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Minimum Score");
            minScore = EditorGUILayout.FloatField(minScore);
            EditorGUILayout.EndHorizontal();
        }

        if (group.navigationType == SelectableGroup.NavigationBuildType.EXPLICIT) {
            EditorGUILayout.HelpBox("You can edit the target SelectableOption for each direction on the SelectableOption objects themselves.", MessageType.Info);
        }

        //Hide build button when navigation mode is set to "Explicit"
        if (group.navigationType != SelectableGroup.NavigationBuildType.EXPLICIT) {
            if (GUILayout.Button("Build navigation")) {
                foreach (SelectableOptionBase option in group.options) {
                    option.ResetNavigation();
                }

                ISelectableNavigationBuilder selectableNavigationBuilder;

                switch (group.navigationType) {
                    case SelectableGroup.NavigationBuildType.HORIZONTAL:
                        selectableNavigationBuilder = new AxisNavigationBuilder(AxisNavigationBuilder.SORTING_AXIS.X);
                        break;
                    case SelectableGroup.NavigationBuildType.VERTICAL:
                        selectableNavigationBuilder = new AxisNavigationBuilder(AxisNavigationBuilder.SORTING_AXIS.Y);
                        break;
                    case SelectableGroup.NavigationBuildType.RAYCAST:
                        selectableNavigationBuilder = new RaycastNavigationBuilder(maxRaycastDistances);
                        break;
                    case SelectableGroup.NavigationBuildType.UNITY:
                        if (minScore <= 0 && maxDistance <= 0) {
                            selectableNavigationBuilder = new UnityNavigationBuilder();
                        } else {
                            selectableNavigationBuilder = new AdvancedUnityNavigationBuilder(maxDistance, minScore);
                        }
                        break;
                    default:
                        selectableNavigationBuilder = new SortedListNavigationBuilder();
                        break;
                }

                selectableNavigationBuilder.BuildNavigation(group);
            }
        }

        showNavigation = GUILayout.Toggle(showNavigation, "Show Navigation");
    }

    private void OnSceneGUI() {
        if (!showNavigation)
            return;

        foreach (SelectableOptionBase option in group.options) {
            SelectableEditorUtils.DrawNavigationForSelectable(option);
        }
    }

    

}

public class SelectableEditorUtils {

    public static void DrawNavigationForSelectable(SelectableOptionBase option) {
        if (option == null)
            return;

        Transform transform = option.transform;
        bool active = Selection.transforms.Any(e => e == transform);
        Handles.color = new Color(1.0f, 0.9f, 0.1f, active ? 1.0f : 0.4f);
        DrawNavigationArrow(-Vector2.right, option, option.GetNextOption(SelectableNavigationDirection.LEFT));
        DrawNavigationArrow(Vector2.right, option, option.GetNextOption(SelectableNavigationDirection.RIGHT));
        DrawNavigationArrow(Vector2.up, option, option.GetNextOption(SelectableNavigationDirection.UP));
        DrawNavigationArrow(-Vector2.up, option, option.GetNextOption(SelectableNavigationDirection.DOWN));
    }

    const float kArrowThickness = 2.5f;
    const float kArrowHeadSize = 1.2f;

    private static void DrawNavigationArrow(Vector2 direction, SelectableOptionBase fromObj, SelectableOptionBase toObj) {
        if (fromObj == null || toObj == null)
            return;
        Transform fromTransform = fromObj.transform;
        Transform toTransform = toObj.transform;

        Vector2 sideDir = new Vector2(direction.y, -direction.x);
        Vector3 fromPoint = fromTransform.TransformPoint(SelectableNavigationUtils.GetPointOnRectEdge(fromTransform as RectTransform, direction));
        Vector3 toPoint = toTransform.TransformPoint(SelectableNavigationUtils.GetPointOnRectEdge(toTransform as RectTransform, -direction));
        float fromSize = HandleUtility.GetHandleSize(fromPoint) * 0.05f;
        float toSize = HandleUtility.GetHandleSize(toPoint) * 0.05f;
        fromPoint += fromTransform.TransformDirection(sideDir) * fromSize;
        toPoint += toTransform.TransformDirection(sideDir) * toSize;
        float length = Vector3.Distance(fromPoint, toPoint);
        Vector3 fromTangent = fromTransform.rotation * direction * length * 0.3f;
        Vector3 toTangent = toTransform.rotation * -direction * length * 0.3f;

        Handles.DrawBezier(fromPoint, toPoint, fromPoint + fromTangent, toPoint + toTangent, Handles.color, null, kArrowThickness);
        Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + toTransform.rotation * (-direction - sideDir) * toSize * kArrowHeadSize);
        Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + toTransform.rotation * (-direction + sideDir) * toSize * kArrowHeadSize);
    }

}