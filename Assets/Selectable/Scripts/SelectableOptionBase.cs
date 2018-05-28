using UnityEditor;
using UnityEngine;

namespace SelectablePlus {

    public abstract class SelectableOptionBase : MonoBehaviour {

        [HideInInspector]
        public SelectableOptionBase[] navigationArray = new SelectableOptionBase[4];
        public Vector3 optionPositionOffset;

        /// <summary>
        /// Called when the cursor selects this option.
        /// </summary>
        /// <param name="cursor">The cursor triggering the event</param>
        public virtual void Select(SelectableCursor cursor) { }

        /// <summary>
        /// Called when the cursor leaves this option.
        /// </summary>
        /// <param name="cursor">The cursor triggering the event</param>
        public virtual void Deselect(SelectableCursor cursor) { }

        /// <summary>
        /// Called when the OK key specified in the cursor's properties is pressed while this option is selected.
        /// </summary>
        /// <param name="cursor">The cursor triggering the event</param>
        public virtual void OkPressed(SelectableCursor cursor) { }

        /// <summary>
        /// Called when the Cancel key specified in the cursor's properties is pressed while this option is selected.
        /// </summary>
        /// <param name="cursor">The cursor triggering the event</param>
        public virtual void CancelPressed(SelectableCursor cursor) {
            cursor.ReturnToPreviousGroup();
        }

        private Transform optionTransform;
        private Vector3 staticOptionPosition;

        private void Start() {
            optionTransform = GetComponent<Transform>();

            if (navigationArray != null && navigationArray.Length == 4)
                return;

            if (gameObject.isStatic)
                staticOptionPosition = optionTransform.position + optionPositionOffset;

            Debug.LogWarning("An uninitialized SelectableOption was detected on GameObject " + gameObject.name + ", it has been disabled! Please build the navigation data first in the SelectableGroup script!");
            gameObject.SetActive(false);
        }

        private void OnDrawGizmos() {
            if (optionTransform == null)
                optionTransform = GetComponent<Transform>();

            Gizmos.DrawWireSphere(GetOptionPosition(), 10);
        }

        /// <summary>
        /// Gets the next option specified in the navigation data for the given direction.
        /// </summary>
        /// <param name="direction">The direction to look in</param>
        /// <returns></returns>
        public SelectableOptionBase GetNextOption(SelectableNavigationDirection direction) {
            return navigationArray[(int)direction];
        }


        /// <summary>
        /// Overrides the navigation data for a given direction with a custom object deriving from the SelectableOptionBase class.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="option"></param>
        public void SetNextOption(SelectableNavigationDirection direction, SelectableOptionBase option) {
            if (!direction.Equals(SelectableNavigationDirection.NONE))
                navigationArray[(int)direction] = option;
        }

        public void ResetOptions() {
            navigationArray = new SelectableOptionBase[4];
        }

        public Vector3 GetOptionPosition() {
            if (gameObject.isStatic)
                return staticOptionPosition;

            return optionTransform.position + optionPositionOffset;
        }

    }

}