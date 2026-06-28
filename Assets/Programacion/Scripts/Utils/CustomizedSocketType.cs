using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum PostSocketBehavior
{
    LockGrabAndRigid,
    RemoveComponents,
    KeepFree,
    LockGrab
}

public class CustomizedSocketType : XRSocketInteractor
{
    [Space(20)]
    [Header("Type Settings")]
    public string validType;

    [Header("Socket Behavior")]
    public PostSocketBehavior behavior = PostSocketBehavior.LockGrabAndRigid;

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        if (!base.CanHover(interactable))
            return false;

        var typePiece = interactable.transform.GetComponent<TypePiece>();
        return typePiece != null && typePiece.type == validType;
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        if (!base.CanSelect(interactable))
            return false;

        var typePiece = interactable.transform.GetComponent<TypePiece>();
        return typePiece != null && typePiece.type == validType;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        Transform target = args.interactableObject.transform;

        StartCoroutine(HandlePostAttach(target));
    }

    private IEnumerator HandlePostAttach(Transform target)
    {
        yield return new WaitForSeconds(0.3f);

        switch (behavior)
        {
            case PostSocketBehavior.LockGrabAndRigid:
                HandleLock(target);
                break;

            case PostSocketBehavior.RemoveComponents:
                HandleRemove(target);
                target.SetParent(this.transform, true);
                break;

            case PostSocketBehavior.KeepFree:
                break;

            case PostSocketBehavior.LockGrab:
                var grab = target.GetComponent<CustomizedGrab>();
                    grab.locked = true;
                break;
        }
    }

    private void HandleLock(Transform target)
    {
        var rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.isKinematic = true;
        }

        var grab = target.GetComponent<CustomizedGrab>();
        if (grab != null)
        {
            grab.locked = true;
        }
    }

    private void HandleRemove(Transform target)
    {
        var grab = target.GetComponent<CustomizedGrab>();
        if (grab != null)
        {
            Destroy(grab);
        }

        var rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }

        var col = target.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }
    }
}