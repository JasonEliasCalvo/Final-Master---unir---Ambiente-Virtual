using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

[Flags]
public enum RotationAxis
{
    None = 0,
    X = 1 << 0,
    Y = 1 << 1,
    Z = 1 << 2
}

[Serializable]
public class RotatingObject
{
    [Header("Rotation Settings")]
    public RotationAxis rotationAxis = RotationAxis.X;
    public Transform targetObject;
    public float maxRotation = -45f;
    public XRSlider slider;
    public XRSocketInteractor sokect;

    [Space(5)]
    [Header("Events")]
    public UnityEvent onUp;
    public UnityEvent onDown;
    public UnityEvent<RotatingObject, MonoBehaviour> onDetected;

    [HideInInspector] public Quaternion startRotation;

    [Space(10)]
    [Header("Detection Settings")]
    public TriggerDetector triggerDetector;

    [HideInInspector] public bool hasDetected = false;
    [HideInInspector] public bool hasUP = false;
    [HideInInspector] public bool hasDown = false;
}

public class UpAndDownRotationWithDetection : MonoBehaviour
{
    public RotatingObject[] rotatingObjects = new RotatingObject[0];

    private const float UpThreshold = 0.95f;
    private const float ResetDetectedThreshold = 0.2f;
    private const float ResetUpThreshold = 0.90f;
    private const float ResetDownThreshold = 0.2f;

    private void Start()
    {
        foreach (var obj in rotatingObjects)
        {
            if (obj?.targetObject == null) continue;
            obj.startRotation = obj.targetObject.localRotation;
            if (obj.slider != null) obj.slider.value = 1f;
        }
    }

    private void LateUpdate()
    {
        foreach (var obj in rotatingObjects)
        {
            if (obj?.targetObject == null || obj.slider == null) continue;

            ApplyRotation(obj);
            HandleBoundariesAndDetection(obj);
        }
    }

    public void MoveSliderValue(int index, float delta)
    {
        if (index < 0 || index >= rotatingObjects.Length) return;

        var obj = rotatingObjects[index];
        if (obj?.slider == null) return;

        float newVal = Mathf.Clamp01(obj.slider.value + delta);
        obj.slider.value = newVal;
    }

    private void ApplyRotation(RotatingObject obj)
    {
        float rotationAmount = obj.slider.value * obj.maxRotation;
        Vector3 rotationEuler = obj.startRotation.eulerAngles;

        if ((obj.rotationAxis & RotationAxis.X) == RotationAxis.X) rotationEuler.x = rotationAmount;
        if ((obj.rotationAxis & RotationAxis.Y) == RotationAxis.Y) rotationEuler.y = rotationAmount;
        if ((obj.rotationAxis & RotationAxis.Z) == RotationAxis.Z) rotationEuler.z = rotationAmount;

        obj.targetObject.localRotation = Quaternion.Euler(rotationEuler);
    }

    private void HandleBoundariesAndDetection(RotatingObject obj)
    {
        float v = obj.slider.value;

        if (v >= UpThreshold && !obj.hasUP)
        {
            obj.hasUP = true;
            obj.hasDown = false; // reset del otro extremo
            obj.onUp?.Invoke();
        }

        // Bajada
        if (v <= 0.05f && !obj.hasDown) // pequeño margen
        {
            obj.hasDown = true;
            obj.hasUP = false; // reset del otro extremo
            obj.onDown?.Invoke();

            // Detección solo una vez por ciclo de bajada
            if (!obj.hasDetected && obj.triggerDetector != null)
            {
                StartCoroutine(DelayedDetection(obj, 0.3f));
            }
        }

        if (v < ResetUpThreshold)
            obj.hasUP = false;
        if (v > ResetDownThreshold)
            obj.hasDown = false;

        // Reset de detección cuando sube
        if (v > ResetDetectedThreshold)
            obj.hasDetected = false;
    }

    private IEnumerator DelayedDetection(RotatingObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj.slider.value <= 0.05f)
        {
            var detected = obj.triggerDetector.GetDetected<MonoBehaviour>();
            if (detected != null)
            {
                obj.hasDetected = true;
                obj.onDetected?.Invoke(obj, detected);
            }
        }
    }
}
