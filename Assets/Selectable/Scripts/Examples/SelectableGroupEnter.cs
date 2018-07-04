using UnityEngine;

namespace SelectablePlus.Examples {

    public class SelectableGroupEnter : SelectableOptionBase {

        public SelectableGroup targetGroup;
        [Tooltip("Specifies if the group change should be recorded in the cursor's group history")]
        public bool incognitoMode;

        public override void OkPressed(SelectableCursorBase cursor) {
            cursor.EnterGroup(targetGroup, null, incognitoMode);
        }

    }

}
