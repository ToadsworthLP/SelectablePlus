using SelectablePlus;
using UnityEngine.Events;

namespace SelectablePlus.Examples {

    public class SelectableEventTrigger : SelectableOptionBase {

        public UnityEvent OnOkPressed;

        public override void OkPressed(SelectableCursorBase cursor) {
            OnOkPressed.Invoke();
        }
    }

}
