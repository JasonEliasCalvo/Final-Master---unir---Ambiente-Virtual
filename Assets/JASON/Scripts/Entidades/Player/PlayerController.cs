using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    [SerializeField] private Vector2 dropForce = new Vector2(50, 1);
    [SerializeField] private Transform handParent;
    [SerializeField] private Transform dropPoint;
    [HideInInspector] public bool isGrabbed = false;
    private GameObject currentGrab;
    private Rigidbody rbGrab;

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

    public GameObject CurrentGrab { get => currentGrab; set => currentGrab = value; }

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
        inputActions.Player.Drop.performed += OnDropInput;

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

    public void ClearCurrentGrabReference()
    {
        isGrabbed = false;

        if (currentGrab != null)
        {
            CurrentGrab = null;
        }

        if (rbGrab != null)
        {
            rbGrab = null;
        }

        UIManager.instance.ShowDropPanel(false);
        Debug.Log("Referencia de objeto agarrado limpiada.");
    }

    private void CheckGrabObject()
    {
        if (CurrentGrab == null) return;

        var customGrab = CurrentGrab.GetComponent<CustomizedGrab>();
        if (customGrab != null && (customGrab.locked || customGrab.isInSocket))
        {
            isGrabbed = false;
            CurrentGrab.transform.SetParent(null);
            ScreenPrintingManager.instance.OnObjectDropped(CurrentGrab);
            rbGrab = null;
            CurrentGrab = null;
            UIManager.instance.ShowDropPanel(false);
        }
        else
        {
            UIManager.instance.ShowDropPanel(true);
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

        CheckGrabObject();
    }

    private void HandleMovement()
    {
        if(Input.GetKey(runKey))
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
    // Interacciones (inputs)
    // -------------------------

    private void OnDropInput(InputAction.CallbackContext ctx)
    {
        Drop();
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

        if (isGrabbed)
            StartCoroutine(DropAndGrabNext(_object));
        else
            Grab(_object);
    }

    private void Grab(GameObject obj)
    {
        var cg = obj.GetComponent<CustomizedGrab>();
        if (cg != null)
        {
            cg.enabled = false;
        }

        var socket = FindAnyObjectByType<Selected>().currentSocket;
        if (socket != null)
        {
            Debug.Log("Objeto estaba en un socket. Forzando salida del socket.");
            var interactable = cg.GetComponent<IXRSelectInteractable>();
            socket.interactionManager.SelectExit(socket, interactable);
            socket.enabled = false;
        }

        ScreenPrintingManager.instance.OnObjectPickedUp(obj);

        isGrabbed = true;
        CurrentGrab = obj;
        rbGrab = CurrentGrab.GetComponent<Rigidbody>();

        rbGrab.isKinematic = true;
        CurrentGrab.transform.SetParent(handParent);
        CurrentGrab.transform.localPosition = Vector3.zero;
        CurrentGrab.transform.localRotation = Quaternion.identity;
        StartCoroutine(ReenableGrabAndSocket(cg, socket));
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

    private void Drop()
    {
        if (CurrentGrab == null) return;

        ScreenPrintingManager.instance.OnObjectDropped(CurrentGrab);

        isGrabbed = false;
        CurrentGrab.transform.SetParent(null);

        rbGrab.isKinematic = false;
        Vector3 dropDirection = cam.transform.forward + cam.transform.up * dropForce.y;
        rbGrab.AddForce(dropDirection.normalized * dropForce.x, ForceMode.Impulse);

        CurrentGrab = null;
        rbGrab = null;

        UIManager.instance.ShowDropPanel(false);
    }

    private IEnumerator DropAndGrabNext(GameObject newObject)
    {
        isGrabbed = false;

        if (currentGrab != null)
        {
            currentGrab.transform.SetParent(null);
        }

        if (rbGrab != null)
        {
            rbGrab.isKinematic = false;
            ScreenPrintingManager.instance.OnObjectDropped(CurrentGrab);
            CurrentGrab.transform.position = dropPoint.position;
            CurrentGrab.transform.position += transform.right * Random.Range(-0.2f, 0.2f);
            rbGrab.AddForce((transform.forward + Vector3.up * 0.2f) * 2f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(0.1f);

        Grab(newObject);
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
