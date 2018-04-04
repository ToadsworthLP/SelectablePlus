using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableGroupEnter : SelectableOptionBase {

    public SelectableGroup targetGroup;

    public override void OkPressed(SelectableCursor cursor) {
        cursor.EnterGroup(targetGroup);
    }

}
