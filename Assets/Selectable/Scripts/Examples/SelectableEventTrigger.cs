using UnityEngine.Events;

public class SelectableEventTrigger : SelectableOptionBase
{

    public UnityEvent OnOkPressed;

    public override void OkPressed(SelectableCursor cursor) {
        OnOkPressed.Invoke();
    }
}
