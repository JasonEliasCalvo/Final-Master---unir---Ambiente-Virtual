using UnityEngine.XR.Interaction.Toolkit;

public class CustomizedGrab : XRGrabInteractable
{
    public bool locked = false;
    public bool isInSocket = false;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (args.interactorObject is XRSocketInteractor)
        {
            isInSocket = true;
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (args.interactorObject is XRSocketInteractor)
        {
            isInSocket = false;
        }
    }

    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        if (locked)
        {
            if (interactor is XRSocketInteractor)
                return true;
            else
                return false;
        }

        return base.IsSelectableBy(interactor);
    }
}
