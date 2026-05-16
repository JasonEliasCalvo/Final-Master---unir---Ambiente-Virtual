using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Selected : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    [SerializeField] private LayerMask interactableMask = ~0;
    [SerializeField] private string interactableTag = "Interactable";
    [SerializeField] private float distance = 2f;
    [SerializeField] private InteractableOptions currentInteractable;
    [SerializeField] private InteractableOptions enableInteractable;

    [Header("Fuentes de Rayo (Cámara y/o Control)")]
    [SerializeField] private Transform[] raySources;
    [SerializeField] private XRRayInteractor leftInteractor;
    [SerializeField] private XRRayInteractor rightInteractor;

    [Header("Preview dinámico")]
    [SerializeField] private Material previewColor;
    private GameObject socketPreviewInstance;
    private GameObject socketPreviewSourceRef;

    private GameObject lastSelectedObject;
    private Transform hightlight;
    public bool hitDetected = false;
    public XRSocketInteractor currentSocket;

    // -------------------------
    // Ciclo de vida
    // -------------------------
    private void OnEnable()
    {
        GameInputManager.OnInteractStarted += StartdHandleInteractInput;
        GameInputManager.OnInteractCanceled += EndHandleInteractInput;
    }

    private void OnDisable()
    {
        GameInputManager.OnInteractStarted -= StartdHandleInteractInput;
        GameInputManager.OnInteractCanceled -= EndHandleInteractInput;
    }

    void Update()
    {
        hitDetected = false;

        if (UIManager.instance.IsPanelActive())
        {
            DeselectLastObject();
            currentInteractable = null;

            UIManager.instance.ShowInteractPanel(false);

            if (hightlight != null)
                hightlight.gameObject.SetActive(false);

            return;
        }

        hitDetected |= DetectFromInteractor(leftInteractor);
        hitDetected |= DetectFromInteractor(rightInteractor);

        if (!hitDetected)
        {
            foreach (Transform source in raySources)
            {
                if (DetectFromInteractable(source))
                {
                    hitDetected = true;
                    break;
                }
            }
        }

        if (!hitDetected)
        {
            DeselectLastObject();
            currentInteractable = null;
        }
    }

    // -------------------------
    // Detección
    // -------------------------
    private bool DetectFromInteractor(XRRayInteractor interactor)
    {
        if (interactor != null && interactor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.collider.CompareTag(interactableTag))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject != lastSelectedObject)
                {
                    DeselectLastObject();
                    SelectObject(hitObject);
                }

                currentInteractable = hitObject.GetComponent<InteractableOptions>();
                return true;
            }
        }
        return false;
    }

    private bool DetectFromInteractable(Transform source)
    {
        if (source == null) return false;

        Debug.DrawRay(source.position, source.forward * distance, Color.red, 0.1f);

        Ray ray = new Ray(source.position, source.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, interactableMask);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            var xrSocket = hit.collider.GetComponentInParent<XRSocketInteractor>();
            if (xrSocket != null)
            {
                PlayerController player = GetPlayer();

                if (!xrSocket.hasSelection && player.CurrentGrab != null && CanPlaceOnXRSocket(player.CurrentGrab, xrSocket))
                {
                    HandleSocketHover(xrSocket, hit);
                    currentSocket = xrSocket;
                    return true;
                }
                continue;
            }
            else
            {
                currentSocket = null;
            }

            if (hit.collider.CompareTag(interactableTag))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject != lastSelectedObject)
                {
                    DeselectLastObject();
                    SelectObject(hitObject);
                }

                currentInteractable = hitObject.GetComponent<InteractableOptions>();
                Debug.DrawRay(source.position, source.forward * distance, Color.green);
                return true;
            }
        }

        DeselectLastObject();
        currentInteractable = null;
        HideSocketPreview();

        return false;
    }

    private void HandleSocketHover(XRSocketInteractor xrSocket, RaycastHit hit)
    {
        if (xrSocket == null) return;

        GameObject held = GetPlayer().CurrentGrab;
        bool canPlace = false;

        if (held != null)
            canPlace = CanPlaceOnXRSocket(held, xrSocket);

        if (canPlace)
        {
            currentSocket = xrSocket;
            Transform attachPoint = xrSocket.GetAttachTransform(null);
            ShowSocketPreviewAt(attachPoint);
        }
        else
        {
            currentSocket = null;
            HideSocketPreview();
        }

        currentInteractable = null;
    }

    private bool CanPlaceOnXRSocket(GameObject held, XRSocketInteractor socket)
    {
        if (held == null || socket == null) return false;

        var xrInteractable = held.GetComponent<XRBaseInteractable>();
        if (xrInteractable != null)
        {
            return (socket.interactionLayers & xrInteractable.interactionLayers) != 0;
        }
        return false;
    }

    private PlayerController GetPlayer()
    {
        return PlayerController.instance;
    }

    // -------------------------
    // Interacción (inputs)
    // -------------------------
    private void StartdHandleInteractInput()
    {
        var player = GetPlayer();

        if (currentInteractable != null)
        {
            currentInteractable.TryInteract();
            enableInteractable = currentInteractable;

            if (!currentInteractable.InteractionTypes.HasFlag(InteractionType.ShowDescription))
            {
                hightlight?.gameObject.SetActive(false);
                UIManager.instance.ShowInteractPanel(false);
            }
            return;
        }

        if (currentSocket != null)
        {
            if (player == null) return;

            var held = player.CurrentGrab;
            if (held == null) return;

            if (!CanPlaceOnXRSocket(held, currentSocket))
            {
                UIManager.instance.ShowWarningPanel(true, "No es compatible con este socket.");
                UIManager.instance.UpdateScore(5);
                return;
            }

            ScreenPrintingManager.instance.OnObjectDropped(held);
            PlaceHeldInSocket(currentSocket, held);
            player.ClearCurrentGrabReference();
            HideSocketPreview();
            return;
        }
    }

    private void EndHandleInteractInput()
    {
        var octopusC = FindFirstObjectByType<OctopusController>();
        var player = GetPlayer();

        if (enableInteractable != null)
        {
            enableInteractable.EndInteract();

            foreach (var type in enableInteractable.GetActiveFlags())
            {
                if (type == InteractionType.StartMoving)
                {
                    hightlight?.gameObject.SetActive(false);
                    UIManager.instance.ShowInteractPanel(false);

                    if (player != null)
                    {
                        if (player.CurrentSliderTarget != null)
                            player.CurrentSliderTarget = null;


                        if (player.CurrentWheelTarget != null)
                        {
                            if (octopusC != null)
                            {
                                var wheelScript = octopusC.canRotate;
                                wheelScript.ForceSnap();
                            }

                            player.CurrentWheelTarget = null;
                        }
                    }
                    octopusC?.ClearFrame();
                }
            }
            enableInteractable = null;
        }

        PlayerController.instance.ActiveMovement();
    }

    // -------------------------
    // Selección y deselección
    // -------------------------
    private void SelectObject(GameObject obj)
    {
        hightlight = obj.transform.Find("Highlight");

        var customGrab = obj.GetComponent<CustomizedGrab>();
        if (customGrab != null && customGrab.locked)
        {
            UIManager.instance.ShowInteractPanel(false);

            if (hightlight != null)
                hightlight.gameObject.SetActive(false);

            lastSelectedObject = null;
            return;
        }

        UIManager.instance.ShowInteractPanel(true);

        if (hightlight != null && !hightlight.gameObject.activeSelf)
            hightlight.gameObject.SetActive(true);
        else
            Debug.LogWarning("No se encontró el hijo del objeto " + obj.name);

        lastSelectedObject = obj;
    }

    private void DeselectLastObject()
    {
        if (lastSelectedObject != null)
        {
            UIManager.instance.ShowInteractPanel(false);

            if (hightlight != null)
                hightlight.gameObject.SetActive(false);

            lastSelectedObject = null;
        }
    }

    // -------------------------
    // Sockets
    // -------------------------
    private void PlaceHeldInSocket(XRSocketInteractor socket, GameObject held)
    {
        if (socket == null || held == null) return;

        Transform attach = socket.GetAttachTransform(null) ?? socket.transform;
        socket.enabled = true; 
        held.transform.position = attach.position;
        held.transform.rotation = attach.rotation;
    }


    private void ShowSocketPreviewAt(Transform attach)
    {
        var held = GetPlayer().CurrentGrab;
        if (held == null)
        {
            HideSocketPreview();
            return;
        }

        if (socketPreviewInstance == null || socketPreviewSourceRef != held)
        {
            CreatePreviewFromHeld(held);
            socketPreviewSourceRef = held;
        }

        if (socketPreviewInstance == null) return;

        socketPreviewInstance.SetActive(true);
        socketPreviewInstance.transform.position = attach != null ? attach.position : Vector3.zero;
        socketPreviewInstance.transform.rotation = attach != null ? attach.rotation : Quaternion.identity;
    }

    private void HideSocketPreview()
    {
        if (socketPreviewInstance == null) return;

        socketPreviewInstance.SetActive(false);
        currentSocket = null;
    }

    private void CreatePreviewFromHeld(GameObject held)
    {
        // limpiar preview anterior
        if (socketPreviewInstance != null)
        {
            Destroy(socketPreviewInstance);
            socketPreviewInstance = null;
        }

        if (held == null) return;

        // Parent container del preview
        socketPreviewInstance = Instantiate(held);
        socketPreviewInstance.transform.SetParent(transform, true); // lo ponemos como hijo del Selected para organización
        socketPreviewInstance.SetActive(false);

        ApplyPreviewMaterial(socketPreviewInstance, previewColor);

        // Evita colisiones y componentes extra en el preview: quitar colliders y scripts si hubiera (deben ser solo meshes)
        var colliders = socketPreviewInstance.GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders) Destroy(c);
    }

    void ApplyPreviewMaterial(GameObject socketPreviewInstance, Material previewColor)
    {
        MeshRenderer[] meshRenderers = socketPreviewInstance.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer mr in meshRenderers)
        {
            Material[] newMaterials = new Material[mr.sharedMaterials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = previewColor;
            }

            mr.sharedMaterials = newMaterials;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }
    }

    // -------------------------
    // API
    // -------------------------
    public InteractableOptions GetCurrentInteractable()
    {
        return currentInteractable;
    }
}
