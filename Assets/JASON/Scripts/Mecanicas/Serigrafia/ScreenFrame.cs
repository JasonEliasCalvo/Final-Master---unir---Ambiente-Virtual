using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class ScreenFrame : MonoBehaviour
{
    private Ink currentInk;
    public Frame frameData;
    public GameObject _renderer;
    public Squeegee squeegeeObj;
    public CustomizedGrab xRGrab;
    public Rigidbody rb;

    public Ink GetInk() => currentInk;
    public event Action OnSqueegeePass;
    public event Action<Ink> OnInkApplied;

    [SerializeField] private float minMoveDistance = 0.005f;
    [SerializeField] private float maxYDeviation = 0.001f;
    [SerializeField] private UnityEvent UnityEvent;

    private Vector3 entryLocalPos;
    private float accumulatedXDistance;
    private float deviationY;
    private bool canPassSpatula = false;
    private bool isSqueegeeInside = false;
    public bool canInk = false;
    [SerializeField] private XRSocketInteractor inkSokect, squeegeeSokect;

    public void SetInk(Ink ink)
    {
        currentInk = ink;
        if (_renderer != null && ink != null && ink.color != null)
        {
            _renderer.GetComponent<MeshRenderer>().material.color = ink.color;
            canPassSpatula = true;
        }
    }

    public void CanInk()
    {
        canInk = true;

        if (inkSokect != null)
            inkSokect.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        var inkInstance = other.GetComponent<InkInstance>();
        if (inkInstance != null && inkInstance.inkData != null && canInk)
        {
            SetInk(inkInstance.inkData);
            Destroy(inkInstance.gameObject);
            inkSokect.gameObject.SetActive(false);

            if (squeegeeSokect != null)
                squeegeeSokect.gameObject.SetActive(true);

            OnInkApplied?.Invoke(currentInk);
        }

        var squeegee = other.GetComponent<Squeegee>();
        if (squeegee != null && canPassSpatula == true)
        {
            Debug.Log("Espátula entro");
            isSqueegeeInside = true;
            squeegeeObj = squeegee;
            Destroy(squeegeeObj.GetComponent<InteractableOptions>());
            entryLocalPos = transform.InverseTransformPoint(squeegee.transform.position);
            squeegee.rb.constraints |= RigidbodyConstraints.FreezePositionY;
            accumulatedXDistance = 0f;
            squeegeeSokect.gameObject.SetActive(false);
            UnityEvent?.Invoke();
        }
    }

    public void SetSqueegee(GameObject position)
    {
        squeegeeObj.gameObject.transform.SetParent(position.transform);
        squeegeeObj.gameObject.transform.rotation = new Quaternion(0, 0, 0, 0);
        squeegeeObj.gameObject.transform.position = position.transform.position;
        squeegeeObj.rb.constraints = RigidbodyConstraints.FreezeAll;
        entryLocalPos = transform.InverseTransformPoint(position.transform.position);

        StartCoroutine(MoveSqueegeeAlongX());
    }

    private IEnumerator MoveSqueegeeAlongX()
    {
        yield return new WaitForSeconds(0.2f);
        isSqueegeeInside = true;

        float duration = 2f; // Duración del movimiento
        float moveDistance = -1f; // Distancia a mover en eje X
        float elapsed = 0f;

        Vector3 startPos = squeegeeObj.transform.localPosition;
        Vector3 endPos = startPos + new Vector3(0, moveDistance, 0);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            if(squeegeeObj == null ) break;
            squeegeeObj.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (squeegeeObj != null)
            squeegeeObj.transform.localPosition = endPos;
    }


    private void OnTriggerStay(Collider other)
    {
        if (!isSqueegeeInside || !canPassSpatula) return;

        var squeegee = other.GetComponent<Squeegee>();
        if (squeegee == null) return;

        Vector3 currentLocalPos = transform.InverseTransformPoint(squeegee.transform.position);
        squeegee.rb.constraints |= RigidbodyConstraints.FreezePositionY;

        accumulatedXDistance = Mathf.Abs(currentLocalPos.x - entryLocalPos.x); 
        deviationY = Mathf.Abs(currentLocalPos.y - entryLocalPos.y);
        float deviationZ = Mathf.Abs(currentLocalPos.z - entryLocalPos.z);

        Debug.Log($"Espátula movida → X total: {accumulatedXDistance:F5}, Y desviación: {deviationY:F5}, Z desviación: {deviationY:F5}\"");

        if (deviationY > maxYDeviation)
        {
            Debug.Log("❌ Movimiento demasiado vertical, vuelva a intentarlo");
            ResetSqueegeeTracking();
            return;
        }

        if (accumulatedXDistance >= minMoveDistance)
        {
            Debug.Log("✅ Pasada de espátula válida → imprimiendo...");
            canPassSpatula = false;
            OnSqueegeePass?.Invoke();
            Destroy(squeegee.gameObject);
            ResetSqueegeeTracking();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var squeegee = other.GetComponent<Squeegee>();
        if (squeegee == null) return;
        Debug.Log("Salio la espatula");
    }

    private void ResetSqueegeeTracking()
    {
        isSqueegeeInside = false;
        accumulatedXDistance = 0f;
    }

    public void ClearInk()
    {
        Debug.Log("Se limpio la tinta...");
    }
}
