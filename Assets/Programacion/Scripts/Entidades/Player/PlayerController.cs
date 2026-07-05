using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("Ajustes de Movimiento")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float acceleration = 15f;
    public float deceleration = 20f;
    public float gravity = 8;

    public KeyCode runKey = KeyCode.LeftShift;
    private bool isRunning = false;

    [SerializeField] private bool movementState;

    [Header("Ajustes de Cámara")]
    public Vector2 sensitivity = new Vector2(1f, 0.7f);

    [Header("Ajustes de GrabAndRelease")]
    [SerializeField] private Transform handParent;
    [HideInInspector] public bool isGrabbed = false;

    public List<GameObject> Inventory = new List<GameObject>();

    private Camera cam;
    private CharacterController chController;
    private PlayerInputActions inputActions;
    private Vector3 moveInput;
    private Vector3 lookInput;
    private float armInput;
    private float wheelInput;

    private Vector3 velocity;
    private float currentCameraY;

    public XRSlider CurrentSliderTarget { get; set; }
    public XRKnob CurrentWheelTarget { get; set; }

    // -------------------------
    // Ciclo de vida
    // -------------------------
    private void Awake()
    {
        inputActions = new PlayerInputActions();
        chController = GetComponent<CharacterController>();
        instance = this;
    }

    private void Start()
    {
        cam = Camera.main;
        UIManager.instance.ShowCursor(false);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx =>
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            moveInput = new Vector3(input.x, 0, input.y);
        };

        inputActions.Player.Move.canceled += ctx => moveInput = Vector3.zero;

        inputActions.Player.Look.performed += ctx =>
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            lookInput = new Vector3(input.x, input.y);
        };

        inputActions.Player.Look.canceled += ctx => lookInput = Vector3.zero;

        inputActions.Player.UpDown.performed += ctx =>
        {
            armInput = ctx.ReadValue<Vector2>().y;
        };
        inputActions.Player.UpDown.canceled += ctx => armInput = 0f;

        inputActions.Player.UpDown.performed += ctx =>
        {
            wheelInput = ctx.ReadValue<Vector2>().x;
        };
        inputActions.Player.UpDown.canceled += ctx => wheelInput = 0f;

        GameManager.instance.eventGameStart += ActiveMovement;
        GameManager.instance.eventGameEnd += DeactivateMovement;
    }

    private void OnDisable()
    {
        GameManager.instance.eventGameStart -= ActiveMovement;
        GameManager.instance.eventGameEnd -= DeactivateMovement;
    }

    // -------------------------
    // Estados
    // -------------------------
    public void ActiveMovement() => movementState = true;
    public void DeactivateMovement() => movementState = false;

    public void StartSliderControl(XRSlider options)
    {
        CurrentSliderTarget = options;
        DeactivateMovement();
    }

    public void StartWheelControl(XRKnob options)
    {
        CurrentWheelTarget = options;
        DeactivateMovement();
    }

    public void AddToInventory(GameObject obj)
    {
        if (obj == null)
            return;

        RemoveSameType(obj);

        Inventory.Add(obj);

        RefreshInventoryUI();
    }

    private void RemoveSameType(GameObject obj)
    {
        for (int i = Inventory.Count - 1; i >= 0; i--)
        {
            GameObject current = Inventory[i];

            if (current == null)
                continue;

            bool sameType =
                (FindComponent<PrintSurfaceInstance>(current) && FindComponent<PrintSurfaceInstance>(obj)) ||
                (FindComponent<InkInstance>(current) && FindComponent<InkInstance>(obj)) ||
                (FindComponent<ScreenFrame>(current) && FindComponent<ScreenFrame>(obj)) ||
                (FindComponent<Squeegee>(current) && FindComponent<Squeegee>(obj));

            if (sameType)
            {
                Destroy(current);
                Inventory.RemoveAt(i);
            }
        }
    }

    public void RemoveFromInventory(GameObject obj, bool destroy = true)
    {
        if (obj == null)
            return;

        if (Inventory.Remove(obj))
        {
            if (destroy)
                Destroy(obj);

            RefreshInventoryUI();
        }
    }

    public void ClearInventory(bool destroyObjects = true)
    {
        foreach (GameObject item in Inventory)
        {
            if (item == null)
                continue;

            if (destroyObjects)
                Destroy(item);
        }

        Inventory.Clear();

        RefreshInventoryUI();
    }

    private T FindComponent<T>(GameObject obj) where T : Component
    {
        if (obj == null)
            return null;

        if (obj.TryGetComponent(out T component))
            return component;

        return obj.GetComponentInChildren<T>(true);
    }

    public void RefreshInventoryUI()
    {
        var screen = ScreenPrintingManager.instance.inventoryScreens;

        foreach (var materialScreen in screen)
            materialScreen.SetItem(null);

        PrintSurfaceInstance surface = null;
        InkInstance ink = null;
        ScreenFrame frame = null;
        Squeegee squeegee = null;

        Debug.Log("Pantallas" + screen.Count);
        Debug.Log("Inventario" + Inventory.Count);

        foreach (var obj in Inventory)
        {
            if (surface == null)
                surface = FindComponent<PrintSurfaceInstance>(obj);

            if (ink == null)
                ink = FindComponent<InkInstance>(obj);

            if (frame == null)
                frame = FindComponent<ScreenFrame>(obj);

            if (squeegee == null)
                squeegee = FindComponent<Squeegee>(obj);
        }

        screen[0].SetItem(surface);
        screen[1].SetItem(ink);
        screen[2].SetItem(frame);
        screen[3].SetItem(squeegee);

        if(surface != null && ink != null && frame != null && squeegee != null)
        {
            ScreenPrintingManager.instance.InvokeEvents(ScreenPrintingManager.instance.events.onInventoryComplet);
        }
    }

    // -------------------------
    // Loop
    // -------------------------
    private void Update()
    {
        if (IsTypingInInputField())
            return;

        if (movementState)
        {
            HandleMovement();
            HandleCameraLook();
        }
        else
        {
            HandleSliderControl();
            HandleWheelContol();
        }
    }

    private void HandleMovement()
    {
        if (Input.GetKey(runKey))
            isRunning = true;
        else
            isRunning = false;

        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        if (moveInput.magnitude > 0.1f)
        {
            Vector3 moveDir = GetWorldMovementDirection(moveInput);
            velocity = Vector3.MoveTowards(velocity, moveDir * targetSpeed, acceleration * Time.deltaTime);
        }
        else
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration * Time.deltaTime);

        if (!chController.isGrounded)
            velocity.y += Physics.gravity.y * gravity * Time.deltaTime;
        else
            velocity.y = -1f;

        chController.Move(velocity * Time.deltaTime);
    }

    private void HandleCameraLook()
    {
        if (lookInput.magnitude < 0.1f || UIManager.instance.IsPanelActive())
            return;

        float mouseX = lookInput.x * sensitivity.x;
        float mouseY = lookInput.y * sensitivity.y;

        // Vertical camera rotation
        currentCameraY -= mouseY;
        currentCameraY = Mathf.Clamp(currentCameraY, -80f, 50f);
        cam.transform.localRotation = Quaternion.Euler(currentCameraY, 0f, 0f).normalized;

        // Horizontal player rotation
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleSliderControl()
    {
        if (CurrentSliderTarget == null) return;

        float dir = armInput; // valor de -1, 0, +1
        if (Mathf.Abs(dir) > 0.01f)
        {
            OctopusController octopusController = FindFirstObjectByType<OctopusController>();
            if (octopusController != null)
            {
                float speed = 0.5f; // ajusta sensibilidad
                float delta = dir * speed * Time.deltaTime;
                octopusController.AdjustCurrentArm(delta, CurrentSliderTarget);
            }
        }
    }

    private void HandleWheelContol()
    {
        if (CurrentWheelTarget == null) return;

        float dir = wheelInput;
        if (Mathf.Abs(dir) > 0.01f)
        {
            OctopusController octopusController = FindFirstObjectByType<OctopusController>();
            if (octopusController != null)
            {
                float speed = 0.25f; // ajusta sensibilidad
                float delta = dir * speed * Time.deltaTime;
                octopusController.AdjustEhweel(delta, CurrentWheelTarget);
            }
        }
    }

    private bool IsTypingInInputField()
    {
        if (EventSystem.current == null) return false;

        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return false;

        return selected.GetComponent<UnityEngine.UI.InputField>() != null
            || selected.GetComponent<TMPro.TMP_InputField>() != null;
    }

    // -------------------------
    // Grab & Release
    // -------------------------
    public void HandleGrab(GameObject _object)
    {
        if (_object == null) return;

        var customGrab = _object.GetComponent<CustomizedGrab>();
        if (customGrab != null && customGrab.locked)
        {
            Debug.Log("Objeto bloqueado. No se puede agarrar.");
            return;
        }

        var socket = FindAnyObjectByType<Selected>().currentSocket;
        if (socket != null)
        {
            Debug.Log("Objeto estaba en un socket. Forzando salida del socket.");
            return;
        }

        AddToInventory(_object);

        if (customGrab != null)
        {
            customGrab.enabled = false;
        }

        _object.transform.SetParent(handParent);
        _object.transform.localPosition = Vector3.zero;

        StartCoroutine(ReenableGrabAndSocket(customGrab, socket));
    }

    private IEnumerator ReenableGrabAndSocket(CustomizedGrab cg, XRSocketInteractor socket)
    {
        Debug.Log("Rehabilitando grab y socket después de un breve retraso.");
        yield return new WaitForSeconds(0.1f);

        if (cg != null)
            cg.enabled = true;
        if (socket != null)
            socket.enabled = true;
    }

    // -------------------------
    // Helpers
    // -------------------------
    private Vector3 GetWorldMovementDirection(Vector3 inputDirection)
    {
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0;
        right.y = 0;

        return (forward * inputDirection.z + right * inputDirection.x).normalized;
    }

    public void SetCam()
    {
        var octopusC = FindFirstObjectByType<OctopusController>();

        if (octopusC == null || octopusC.mainCamera == null) return;

        cam.transform.rotation = octopusC.mainCamera.transform.rotation;

        // --- Pitch (vertical) ---
        Vector3 camEuler = cam.transform.localEulerAngles;
        if (camEuler.x > 180) camEuler.x -= 360;
        currentCameraY = camEuler.x;
    }
}
