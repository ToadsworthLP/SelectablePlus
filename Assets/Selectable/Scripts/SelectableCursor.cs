using System.Collections.Generic;
using UnityEngine;

public class SelectableCursor : MonoBehaviour
{

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


    /// <summary>
    /// Enters the given SelectableGroup.
    /// </summary>
    /// <param name="group">The SelectableGroup to enter</param>
    /// <param name="selectOption">The option to select after entering the group (instead of the group's first option)</param>
    /// <param name="incognitoMode">If set to true, the group change won't be recorded in the group history and option history stacks</param>
    public void EnterGroup(SelectableGroup group, SelectableOptionBase selectOption = null, bool incognitoMode = false) {
        if (group != null) {
            if(!incognitoMode){
                groupHistory.Push(currentGroup);
                optionHistory.Push(currentlySelectedOption);
            }

            currentGroup = group;

            if (selectOption == null){
                SelectOption(group.GetFirstOption());
            }else{
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
    public void SelectOption(SelectableOptionBase option) {
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
        if(groupHistory.Count < 1){
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
    }

    private void Update() {
        SelectableNavigationDirection direction = GetPressedDirection();
        if (!direction.Equals(SelectableNavigationDirection.NONE) && currentlySelectedOption.GetNextOption(direction) != null) {
            SelectOption(currentlySelectedOption.GetNextOption(direction));
        }

        UpdatePosition(currentlySelectedOption.GetOptionPosition());

        if (Input.GetKeyDown(okKeyName))
            currentlySelectedOption.OkPressed(this);

        if (Input.GetKeyDown(cancelKeyName))
            currentlySelectedOption.CancelPressed(this);
    }

    private void UpdatePosition(Vector3 targetPosition) {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentAnimationVelocity, smoothingTime);
    }

    private SelectableNavigationDirection GetPressedDirection() {
        if (Input.GetKeyDown(upKeyName)) { return SelectableNavigationDirection.UP; }
        if (Input.GetKeyDown(rightKeyName)) { return SelectableNavigationDirection.RIGHT; }
        if (Input.GetKeyDown(downKeyName)) { return SelectableNavigationDirection.DOWN; }
        if (Input.GetKeyDown(leftKeyName)) { return SelectableNavigationDirection.LEFT; }

        return SelectableNavigationDirection.NONE;
    }

}

public enum SelectableNavigationDirection { UP, RIGHT, DOWN, LEFT, NONE };
