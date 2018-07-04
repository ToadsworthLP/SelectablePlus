using UnityEngine;

namespace SelectablePlus {

    public abstract class SelectableCursorBase : MonoBehaviour {

        /// <summary>
        /// Make a cursor enter the specified group.
        /// </summary>
        /// <param name="group">The group that should be entered</param>
        /// <param name="selectOption">The option within the group to select. If this is null, the first option of the group will be used.</param>
        /// <param name="context">Use this to pass custom data to the cursor</param>
        public virtual void EnterGroup(SelectableGroup group, SelectableOptionBase selectOption = null, object context = null) { }

        /// <summary>
        /// Select an option inside the current group.
        /// </summary>
        /// <param name="option">The option within the current group to select</param>
        /// <param name="context">Use this to pass custom data to the cursor</param>
        public virtual void SelectOption(SelectableOptionBase option, object context = null) { }

        /// <summary>
        /// Use this to call a cursor response from the OkPressed method of an option
        /// </summary>
        /// <param name="context">Use this to pass custom data to the cursor</param>
        public virtual void AfterOkPressed(object context = null) { }

        /// <summary>
        /// Use this to call a cursor response from the CancelPressed method of an option
        /// </summary>
        /// <param name="context">Use this to pass custom data to the cursor</param>
        public virtual void AfterCancelPressed(object context = null) { }
    }

}