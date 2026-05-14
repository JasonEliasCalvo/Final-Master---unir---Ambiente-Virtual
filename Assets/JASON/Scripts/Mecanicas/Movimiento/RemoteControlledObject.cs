using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class RemoteControlledObject : MonoBehaviour
{
    public XRJoystick joystick;
    public float speed = 0.1f;
    public AssemblyChecker assemblyChecker;
    private bool hasAssembled = false;
    public float scaleFactor = 0.4f;

    void Start()
    {
        if (joystick == null)
        {
            joystick = FindAnyObjectByType<XRJoystick>();

            joystick.deadZoneAngle = 2f;
            joystick.maxAngle = 45f;
            joystick.joystickMotion = XRJoystick.JoystickType.BothCircle;
        }
    }

    void Update()
    {
        if (!hasAssembled && assemblyChecker != null && assemblyChecker.IsAssembled())
        {
            hasAssembled = true;

            Rigidbody rb = GetComponent<Rigidbody>();
            CustomizedGrab grab = GetComponent<CustomizedGrab>();

            if (rb == null || grab == null) return;
            grab.enabled = false;

            grab.interactionLayers = LayerMask.GetMask("Default");
            rb.AddForce(Vector3.up * 2.5f, ForceMode.Impulse);

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOShakeScale(0.25f, 0.15f, 10, 80f, true));
            seq.Append(transform.DOScale(Vector3.one * scaleFactor, scaleFactor).SetEase(Ease.OutBack));
            grab.enabled = true;
        }

        if (hasAssembled)
        {
            Vector3 movement = new Vector3(joystick.value.x, 0, joystick.value.y) * speed * Time.deltaTime;
            Debug.Log($"Joystick Input: {joystick.value}, Movement Vector: {movement}");
            transform.Translate(movement, Space.World);
        }
    }
}
