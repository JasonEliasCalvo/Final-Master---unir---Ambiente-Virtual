using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldUIInteractor : MonoBehaviour
{
    [Header("Configuración")]
    public Camera playerCamera;
    public float maxDistance = 4f;
    public LayerMask uiLayer;

    [Header("Raycast screen pos (center)")]
    public Vector2 screenPositionOverride = Vector2.zero;

    private PointerEventData pointerData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    private GameObject currentHover;
    private GameObject lastHover;        
    private GameObject rawPointerPress;   
    private bool isPressing = false;

    private void OnEnable()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (EventSystem.current == null)
        {
            Debug.LogWarning("No hay EventSystem en la escena. Agrega uno para que el UI funcione.");
            enabled = false;
            return;
        }

        pointerData = new PointerEventData(EventSystem.current);

        if (screenPositionOverride == Vector2.zero)
            screenPositionOverride = new Vector2(Screen.width / 2f, Screen.height / 2f);

        GameInputManager.OnInteractStarted += StartHandleInteractInput;
        GameInputManager.OnInteractCanceled += EndHandleInteractInput;
    }

    private void OnDisable()
    {
        GameInputManager.OnInteractStarted -= StartHandleInteractInput;
        GameInputManager.OnInteractCanceled -= EndHandleInteractInput;
    }

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        bool hitPhysical = Physics.Raycast(ray, out RaycastHit physHit, maxDistance, uiLayer);

        if (!hitPhysical)
        {
            ClearHover();
            return;
        }

        Vector3 hitScreenPos = playerCamera.WorldToScreenPoint(physHit.point);
        pointerData.position = new Vector2(hitScreenPos.x, hitScreenPos.y);
        raycastResults.Clear();

        EventSystem.current.RaycastAll(pointerData, raycastResults);

        // Filtrar solo la UI del uiLayer
        raycastResults.RemoveAll(r => ((1 << r.gameObject.layer) & uiLayer) == 0);

        //Verificar si hay resultados
        GameObject hitUI = null;
        if (raycastResults.Count > 0)
            hitUI = raycastResults[0].gameObject;

        //Hover enter/exit
        if (currentHover != hitUI)
        {
            if (currentHover != null)
                ExecuteEvents.Execute(currentHover, pointerData, ExecuteEvents.pointerExitHandler);

            currentHover = hitUI;
            ExecuteEvents.Execute(currentHover, pointerData, ExecuteEvents.pointerEnterHandler);
        }

        // Ejecutar pointerDown solo si hay hover válido
        if (isPressing && currentHover != null)
            ExecuteEvents.Execute(currentHover, pointerData, ExecuteEvents.pointerDownHandler);
    }

    private void ClearHover()
    {
        if (currentHover != null)
        {
            ExecuteEvents.Execute(currentHover, pointerData, ExecuteEvents.pointerExitHandler);
            currentHover = null;
        }
    }

    private void StartHandleInteractInput()
    {
        if (currentHover == null) return;

        isPressing = true;
        rawPointerPress = currentHover;
        lastHover = currentHover;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit uiHit, maxDistance, uiLayer))
        {
            Vector3 hitScreenPos = playerCamera.WorldToScreenPoint(uiHit.point);
            pointerData.position = new Vector2(hitScreenPos.x, hitScreenPos.y);
        }

        ExecuteEvents.ExecuteHierarchy(rawPointerPress, pointerData, ExecuteEvents.pointerDownHandler);
    }

    private void EndHandleInteractInput()
    {
        if (!isPressing)
        {
            isPressing = false;
            return;
        }

        GameObject pointerUpTarget = lastHover != null ? lastHover : (currentHover != null ? currentHover : rawPointerPress);

        if (pointerUpTarget != null)
        {
            ExecuteEvents.ExecuteHierarchy(pointerUpTarget, pointerData, ExecuteEvents.pointerUpHandler);

            // Ejecutar click si el objeto tiene IPointerClickHandler (ExecuteHierarchy lo maneja)
            ExecuteEvents.ExecuteHierarchy(pointerUpTarget, pointerData, ExecuteEvents.pointerClickHandler);
        }

        isPressing = false;
        rawPointerPress = null;
        lastHover = null;
    }
}


