using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Content.Interaction;

public class OctopusController : MonoBehaviour
{
    private ScreenFrame currentFrame;
    [SerializeField] private ScreenFrame LookFrame;
    private PrintSurfaceInstance currentSurface;
    private RotatingObject currentArm;
    public UpAndDownRotationWithDetection upAndDown;
    public RotationWheel canRotate;

    private bool readyToPrint = false;
    private bool readyToInk = false;

    [Header("Camera Follow")]
    public Camera mainCamera;
    public float lookSpeed = 15f;

    public RotatingObject CurrentArm { get => currentArm; set => currentArm = value; }

    // -------------------------
    // Ciclo de vida
    // -------------------------
    private void OnEnable()
    {
        if (canRotate != null)
            canRotate.onSnapStateChanged.AddListener(HandleSnapState);

        foreach (var obj in upAndDown.rotatingObjects)
        {
            obj.onDetected.AddListener(HandleMaterialDetected);
            obj.onDown.AddListener(() => HandleArmDown(obj));
            obj.onUp.AddListener(() => HandleArmUp(obj));
        }
    }

    private void OnDisable()
    {
        if (canRotate != null)
            canRotate.onSnapStateChanged.RemoveListener(HandleSnapState);

        foreach (var obj in upAndDown.rotatingObjects)
        {
            obj.onDetected.RemoveListener(HandleMaterialDetected);
            obj.onDown.RemoveListener(() => HandleArmDown(obj));
            obj.onUp.RemoveListener(() => HandleArmUp(obj));
        }
    }

    private void HandleSnapState(bool isInSnap)
    {
        var allOpts = GetComponentsInChildren<InteractableOptions>(true);

        foreach (var opts in allOpts)
        {
            if (opts == null) continue;

            // Solo los que tengan StartMoving
            if ((opts.InteractionTypes & InteractionType.StartMoving) == 0)
                continue;

            if (opts.selectecObject != null)
            {
                var slider = opts.selectecObject.GetComponent<XRSlider>();
                if (slider != null)
                    opts.gameObject.SetActive(isInSnap);
            }
        }

        Debug.Log("Estado Snap (solo hijos): " + isInSnap);
    }

    public void Start()
    {
        StartCoroutine(OffArms());
    }

    // -------------------------
    // API 
    // -------------------------

    public IEnumerator OffArms()
    {
        yield return new WaitForSeconds(1f);
        foreach (var arm in upAndDown.rotatingObjects)
        {
            if (arm.slider != null)
                arm.slider.gameObject.SetActive(false);
        }
    }

    public void HandleArmState(bool state)
    {
        var allOpts = gameObject.GetComponentsInChildren<InteractableOptions>(true);
        foreach (var opts in allOpts)
        {
            if (opts == null) continue;

            if ((opts.InteractionTypes & InteractionType.StartMoving) != 0)
            {
                var wheel = opts.selectecObject?.GetComponent<XRKnob>();
                if (wheel != null)
                    opts.gameObject.SetActive(state);
            }
        }
    }

    private void HandleArmDown(RotatingObject arm)
    {
        var detected = arm.triggerDetector.GetCurrentSurface();
        HandleArmState(false);
    }

    private void HandleArmUp(RotatingObject arm)
    {
        var socket = arm.sokect;
        OctopusReactoivate();

        if (socket != null)
        {
            socket.gameObject.SetActive(true);
            Debug.Log("Socket reactivado al subir el brazo.");
        }
        HandleArmState(true);
    }

    public void AdjustCurrentArm(float delta, XRSlider slider)
    {
        var CurrentSlider = slider;
        if (CurrentSlider == null) return;

        float newVal = Mathf.Clamp01(CurrentSlider.value + delta);
        slider.value = newVal;
    }

    public void AdjustEhweel(float delta, XRKnob wheel)
    {
        if (readyToPrint || readyToInk) return;

        var tempWheel = wheel;
        if (tempWheel == null) return;

        float newVal = tempWheel.value + delta;
        wheel.value = newVal;
    }

    public void HandleMaterialDetected(RotatingObject arm, MonoBehaviour receiver)
    {
        CurrentArm = arm;
        Debug.Log("Manejando detección de material en brazo: " + arm.targetObject.name);

        GetCurrentSuperfice(currentArm);
        GetCurrentFrame(currentArm);

        if (currentSurface == null || currentFrame == null) return;

        currentFrame.CanInk();
        Debug.Log("Marco listo para recibir tinta.");

        // Suscribirse para detectar cuando se aplique tinta
        currentFrame.OnInkApplied += HandleInkApplied;

        // Desactivar slider solo del brazo actual
        if (CurrentArm.slider != null)
            CurrentArm.slider.gameObject.SetActive(false);

        // Desactivar rotacion
        if (canRotate != null)
            canRotate.enabled = false;

        // Quitar XRGrabInteractable para que no se saque
        if (currentSurface != null)
        {
            var grab = currentSurface.customizedGrab;
            if (grab != null) grab.locked = true;
        }

        Debug.Log("Material detectado, esperando tinta...");
        readyToInk = true;
    }

    private void HandleInkApplied(Ink ink)
    {
        readyToPrint = true;
        readyToInk = false;

        Debug.Log("Tinta aplicada al marco: " + (ink != null ? ink.materialName : "null"));

        if (ink == null)
        {
            Debug.LogWarning("La tinta aplicada es null. No se puede proceder a imprimir.");
            return;
        }

        currentFrame.OnSqueegeePass += OnSqueegeePass; // Suscribirse al evento de pasada de espátula
        currentFrame.OnInkApplied -= HandleInkApplied; // Desuscribirse después de aplicar la tinta
    }

    public void OnSqueegeePass()
    {
        if (!readyToPrint)
        {
            Debug.LogWarning("No se puede imprimir: falta tinta en el marco.");
            return;
        }

        if (currentSurface != null && currentFrame != null)
        {
            currentFrame.OnSqueegeePass -= OnSqueegeePass;
            currentSurface.TriggerPrint(currentFrame.GetInk(), ScreenPrintingManager.instance.GetSelectedDesign());
            currentFrame.ClearInk();
            readyToPrint = false;

            // Reactivar slider
            CurrentArm.slider.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (mainCamera != null && LookFrame != null)
        {
            Debug.Log("Ajustando cámara hacia el marco...");

            Quaternion targetRot = Quaternion.LookRotation(
                LookFrame.transform.position - mainCamera.transform.position
            );

            Vector3 euler = targetRot.eulerAngles;
            euler.y = 0;

            // Reconstruir la rotación con esos valores
            targetRot = Quaternion.Euler(euler);

            mainCamera.transform.localRotation = Quaternion.Slerp(
                mainCamera.transform.localRotation,
                targetRot,
                Time.deltaTime * lookSpeed
            );
        }
    }


    // -------------------------
    // Helpers
    // -------------------------
    private void GetCurrentSuperfice(RotatingObject arm)
    {
        currentSurface = arm.triggerDetector.GetCurrentSurface();
        Debug.Log("Superficie detectada: " + (currentSurface != null ? currentSurface.name : "null"));

        if (currentSurface == null)
        {
            Debug.LogWarning("No se detectó superficie. Coloca la superficie antes de bajar.");
            var socket = arm.sokect;
            if (socket != null)
            {
                socket.gameObject.SetActive(false);
                Debug.Log("Socket desactivado");
            }
            return;
        }

        if (currentSurface.isPrinted)
        {
            Debug.LogWarning("La superficie ya está impresa. No se puede aplicar tinta.");
            currentSurface = null;
            return;
        }

        var rbSurface = currentSurface.rb;
        if (rbSurface != null)
        {
            rbSurface.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void GetCurrentFrame(RotatingObject arm)
    {
        currentFrame = arm.triggerDetector.GetCurrentScreenFrame();
        Debug.Log("Marco detectado: " + (currentFrame != null ? currentFrame.name : "null"));

        if (currentFrame == null)
        {
            Debug.LogWarning("No se detectó Marco.");
            return;
        }

        var rbFrame = currentFrame.rb;
        if (rbFrame != null)
        {
            rbFrame.constraints = RigidbodyConstraints.FreezeAll;
        }

        var grabFrame = currentFrame.xRGrab;
        if (grabFrame != null) grabFrame.locked = true;
    }

    private void OctopusReactoivate()
    {
        if (canRotate != null)
            canRotate.enabled = true;

        if (currentSurface != null)
        {
            var grabR = currentSurface.customizedGrab;
            if (grabR != null)
                grabR.locked = false;

            var rbSurface = currentSurface.rb;
            if (rbSurface != null)
            {
                rbSurface.isKinematic = false;
                rbSurface.constraints = RigidbodyConstraints.None;
            }
        }

        currentSurface = null;
        currentFrame = null;
        CurrentArm = null;
    }

    public void AssignFrameByIndex(int armIndex)
    {
        Debug.Log($"Asignando marco desde brazo con índice: {armIndex}");

        if (upAndDown == null || upAndDown.rotatingObjects == null)
        {
            Debug.LogWarning("UpAndDownRotationWithDetection no está asignado.");
            return;
        }

        if (armIndex < 0 || armIndex >= upAndDown.rotatingObjects.Length)
        {
            Debug.LogWarning($"Índice {armIndex} inválido. La lista de brazos tiene {upAndDown.rotatingObjects.Length} elementos.");
            return;
        }

        var arm = upAndDown.rotatingObjects[armIndex];
        AssignFrameFromArm(arm);
    }

    public void AssignFrameFromArm(RotatingObject arm)
    {
        LookFrame = arm.triggerDetector.GetCurrentScreenFrame();

        Debug.Log("Marco detectado: " + (LookFrame != null ? LookFrame.name : "null"));

        Debug.Log("Marco asignado desde StartMoving: " + (LookFrame != null ? LookFrame.name : "null"));
    }

    public void ClearFrame()
    {
        if (LookFrame == null)
        {
            Debug.Log("No hay marco para limpiar.");
            return;
        }

        LookFrame = null;
        PlayerController.instance.SetCam();
    }
}
