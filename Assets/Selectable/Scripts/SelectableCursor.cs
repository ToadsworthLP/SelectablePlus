using SelectablePlus;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SelectablePlus {

    public class SelectableCursor : SelectableCursorBase {

        //Controls
        public string upKeyName = "w";
        public string rightKeyName = "d";
        public string downKeyName = "s";
        public string leftKeyName = "a";
        public string okKeyName = "space";
        public string cancelKeyName = "backspace";

        //Default group
        public SelectableGroup firstGroup;

        //Current group the cursor is operating in
        private SelectableGroup currentGroup;

        //Holds the currently selected option and its transform
        private SelectableOptionBase currentlySelectedOption;

        //Holds the group history and the last selected option before leaving the group
        private Stack<SelectableGroup> groupHistory;
        private Stack<SelectableOptionBase> optionHistory;

        //Movement animation settings
        public float smoothingTime = 0.1f;

        //Holds the current cursor animation movement speed, used for smoothdamping positions
        private Vector3 currentAnimationVelocity;

        //Holds the currently presed button
        private SelectableNavigationDirection currentDirection;

        //Stores the GraphicsRaycaster component of the canvas this option is on
        [HideInInspector]
        public GraphicRaycaster raycaster;

        //Cached PointerEventData
        private PointerEventData pointerEventData;

        //Previous mouse position, if it didn't change we don't have to do another raycast
        private Vector2 previousMousePosition;

        public bool mouseControls = true;

        /// <summary>
        /// Enters the given SelectableGroup.
        /// </summary>
        /// <param name="group">The SelectableGroup to enter</param>
        /// <param name="selectOption">The option to select after entering the group (instead of the group's first option)</param>
        /// <param name="context">If set to true, the group change won't be recorded in the group history and option history stacks</param>
        public override void EnterGroup(SelectableGroup group, SelectableOptionBase selectOption = null, object context = null) {
            if (group != null) {
                if (context == null || (context is bool && (bool)context == false)) {
                    groupHistory.Push(currentGroup);
                    optionHistory.Push(currentlySelectedOption);
                }

                foreach (SelectableOptionBase option in currentGroup.options) {
                    option.GroupLeft(this);
                }

                currentGroup = group;

                foreach (SelectableOptionBase option in group.options) {
                    option.GroupEntered(this);
                }

                if (selectOption == null) {
                    SelectOption(group.GetFirstOption());
                } else {
                    SelectOption(selectOption);
                }
            } else {
                Debug.LogWarning("A null group was almost entered, which shouldn't happen. Returning to the last one!");
            }
        }

        /// <summary>
        /// Selects the given option.
        /// </summary>
        /// <param name="option">The object deriving from SelectableOptionBase to select.</param>
        public override void SelectOption(SelectableOptionBase option, object context = null) {
            if (option != null && currentGroup.options.Contains(option)) {
                currentlySelectedOption.Deselect(this);
                currentlySelectedOption = option;
                currentlySelectedOption.Select(this);
            } else {
                Debug.LogWarning("A null option or an option outside of the current group was almost selected, which shouldn't happen. Returning to the last one!");
            }
        }

        /// <summary>
        /// Returns to the previously selected group and option before the current group was entered, as recorded in the group history.
        /// </summary>
        public void ReturnToPreviousGroup() {
            if (groupHistory.Count < 1) {
                return;
            }

            EnterGroup(groupHistory.Pop(), optionHistory.Pop());
            groupHistory.Pop();
            optionHistory.Pop();
        }

        private void Awake() {
            if (firstGroup == null) {
                Debug.LogError("A cursor without a valid first group was activated! Please set a valid first group!");
                gameObject.SetActive(false);
                return;
            }

            groupHistory = new Stack<SelectableGroup>();
            optionHistory = new Stack<SelectableOptionBase>();

            currentGroup = firstGroup;
            currentlySelectedOption = firstGroup.GetFirstOption();

            if (mouseControls) pointerEventData = new PointerEventData(null);
        }

        private void Update() {
            currentDirection = GetPressedDirection();
            if (currentDirection != SelectableNavigationDirection.NONE && currentlySelectedOption.GetNextOption(currentDirection) != null) {
                SelectOption(currentlySelectedOption.GetNextOption(currentDirection));
            }

            UpdatePosition(currentlySelectedOption.GetOptionPosition());

            if (Input.GetKeyDown(okKeyName))
                currentlySelectedOption.OkPressed(this);

            if (Input.GetKeyDown(cancelKeyName))
                currentlySelectedOption.CancelPressed(this);

            if (mouseControls)
                HandleMouseControls();
        }

        private void HandleMouseControls() {
            if (raycaster == null) {
                Debug.LogError("No graphic raycaster was assigned! Please set one in the inspector as it is required for mouse controls to work!");
                mouseControls = false;
                return;
            }

            if (Input.GetMouseButtonDown(0)) {
                currentlySelectedOption.OkPressed(this);
            }

            if (Input.GetMouseButtonDown(1)) {
                currentlySelectedOption.CancelPressed(this);
            }

            Vector2 mousePosition = Input.mousePosition;
            if (mousePosition == previousMousePosition) return;

            previousMousePosition = mousePosition;
            pointerEventData.position = mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();

            raycaster.Raycast(pointerEventData, results);

            if (results.Count < 1) return;

            SelectableOptionBase hitOption = results[0].gameObject.GetComponent<SelectableOptionBase>();
            if (hitOption != null && currentGroup.options.Contains(hitOption)) {
                SelectOption(hitOption);
            }
        }

        private void UpdatePosition(Vector3 targetPosition) {
            if(transform.position != targetPosition)
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentAnimationVelocity, smoothingTime);
        }

        private SelectableNavigationDirection GetPressedDirection() {
            if (Input.GetKeyDown(upKeyName)) { return SelectableNavigationDirection.UP; }
            if (Input.GetKeyDown(rightKeyName)) { return SelectableNavigationDirection.RIGHT; }
            if (Input.GetKeyDown(downKeyName)) { return SelectableNavigationDirection.DOWN; }
            if (Input.GetKeyDown(leftKeyName)) { return SelectableNavigationDirection.LEFT; }
            return SelectableNavigationDirection.NONE;
        }

        public override void AfterCancelPressed(object context = null) {
            ReturnToPreviousGroup();
        }

    }

    public enum SelectableNavigationDirection { UP, RIGHT, DOWN, LEFT, NONE };

}

#if UNITY_EDITOR
[CustomEditor(typeof(SelectableCursor), true)]
[CanEditMultipleObjects]
public class SelectableCursorEditor : Editor {

    private SelectableCursor cursor;
    private SerializedProperty raycasterProperty;

    private void OnEnable() {
        cursor = (SelectableCursor)target;
        raycasterProperty = serializedObject.FindProperty("raycaster");
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (cursor.mouseControls) {
            if (cursor.raycaster == null) {
                EditorGUILayout.HelpBox("Please set a graphic raycaster! Without it, mouse controls don't work.", MessageType.Error);
            }

            EditorGUILayout.PropertyField(raycasterProperty, new GUIContent("Graphic Raycaster"));
            serializedObject.ApplyModifiedProperties();
        }
    }

}
#endif