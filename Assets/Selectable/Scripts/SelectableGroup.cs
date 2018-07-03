using System.Collections.Generic;
using UnityEngine;

namespace SelectablePlus {

    public class SelectableGroup : MonoBehaviour {

        [SerializeField]
        private SelectableOptionBase firstOption;

        [HideInInspector]
        public List<SelectableOptionBase> options;

        public enum NavigationBuildType { HORIZONTAL, VERTICAL, RAYCAST, UNITY }
        public NavigationBuildType navigationType = NavigationBuildType.UNITY;

        /// <summary>
        /// Returns the first option of this group, as set in the inspector.
        /// </summary>
        /// <returns></returns>
        public SelectableOptionBase GetFirstOption() {
            if (firstOption == null) {
                Debug.LogWarning("The SelectableGroup on object " + gameObject.name + " doesn't have a valid first option set! The one with the smallest x or y coordinates, depending on the set navigation build type, will be used as a default!");
                return options[0];
            }

            return firstOption;
        }

    }


}