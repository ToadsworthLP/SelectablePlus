using UnityEditor;
using UnityEngine;

public class SelectableCustomMenuItems{

    [MenuItem("GameObject/Selectable/Selectable Group", false, 0)]
    private static void CreateSelectableGroup() {
        var selected = Selection.activeTransform;
        GameObject gameObject = new GameObject() {
            name = "New SelectableGroup"
        };
        gameObject.AddComponent<SelectableGroup>();

        if (selected != null)
            gameObject.transform.SetParent(selected);

        gameObject.transform.localPosition = Vector3.zero;
        Selection.SetActiveObjectWithContext(gameObject, null);
    }

    [MenuItem("GameObject/Selectable/Cursor", false, 0)]
    private static void CreateSelectableCursor() {
        var selected = Selection.activeTransform;
        GameObject gameObject = new GameObject() {
            name = "New Cursor"
        };
        gameObject.AddComponent<SelectableCursor>();

        if (selected != null)
            gameObject.transform.SetParent(selected);

        gameObject.transform.localPosition = Vector3.zero;
        Selection.SetActiveObjectWithContext(gameObject, null);
    }

}
