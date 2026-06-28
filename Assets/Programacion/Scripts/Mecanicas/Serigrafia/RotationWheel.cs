using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Content.Interaction;

public class RotationWheel : MonoBehaviour
{
    [Header("Knob Reference")]
    public XRKnob knob;

    [Header("Rotation Settings")]
    [Tooltip("Distancia entre puntos de snap (por ejemplo 0.25 para cada 1/4 de vuelta)")]
    public float snapStep = 0.25f;
    public float rotationMultiplier = 360f;
    public float snapThreshold = 0.05f;
    public float snapLockTime = 0.2f;

    private float _lastKnobValue = float.NaN;
    private bool isSnapping = false;
    private bool insideSnap = false;

    public UnityEvent<bool> onSnapStateChanged;

    private void LateUpdate()
    {
        if (knob == null) return;

        float currentValue = knob.value;

        if (!float.IsNaN(_lastKnobValue) && Mathf.Approximately(currentValue, _lastKnobValue))
            return;

        _lastKnobValue = currentValue;

        float nearestSnap = Mathf.Round(currentValue / snapStep) * snapStep;
        float diff = Mathf.Abs(currentValue - nearestSnap);

        if (!insideSnap && diff < snapThreshold)
        {
            insideSnap = true;
            onSnapStateChanged?.Invoke(true);
            StartCoroutine(DoSnap(nearestSnap));
        }
        else if (insideSnap && diff >= snapThreshold)
        {
            insideSnap = false;
            onSnapStateChanged?.Invoke(false);
        }

        if (!isSnapping) // mientras no está bloqueado
        {
            float rotation = currentValue * rotationMultiplier;
            transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }
    }

    private IEnumerator DoSnap(float targetValue)
    {
        isSnapping = true;

        knob.value = targetValue; // lo encaja al snap
        float rotation = targetValue * rotationMultiplier;
        transform.localRotation = Quaternion.Euler(0f, 0f, rotation);

        yield return new WaitForSeconds(snapLockTime);

        isSnapping = false;
    }

    public void ForceSnap()
    {
        if (knob == null) return;

        float currentValue = knob.value;
        float nearestSnap = Mathf.Round(currentValue / snapStep) * snapStep;
        float diff = Mathf.Abs(currentValue - nearestSnap);

        if (insideSnap)
        {
            StartCoroutine(DoSnap(nearestSnap));
            Debug.Log("Forzando snap a: " + nearestSnap);
            onSnapStateChanged?.Invoke(true);
        }
        else
        {
            Debug.Log("No está cerca de un punto de snap.");
        }
    }
}
