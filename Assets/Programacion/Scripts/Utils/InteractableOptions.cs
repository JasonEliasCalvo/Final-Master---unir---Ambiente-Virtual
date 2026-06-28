using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Content.Interaction;

[Flags]
public enum InteractionType
{
    None = 0,
    InvokeEvent = 1 << 0,
    ShowBook = 1 << 1,
    SelectMaterial = 1 << 2,
    GrabAndRelease = 1 << 3,
    StartMoving = 1 << 4,
    ShowDescription = 1 << 5,
}

public class InteractableOptions : MonoBehaviour
{
    [SerializeField] private InteractionType interactionTypes;
    [SerializeField] private bool justOneInteraction = false;
    [SerializeField] private bool canInteract = true;

    private ScreenPrintingManager simulator;
    private PlayerController player;
    public UnityEvent onInteract;
    public UnityEvent endInteract;

    [Header("Configuración de Interacción")]
    public GameObject selectecObject;
    public string itemName;
    public int ID;

    [Tooltip("Si está en TRUE, el objeto no será destruido al seleccionar material")]
    public bool dontDestroyOnSelect = false;

    public InteractionType InteractionTypes => interactionTypes;
    private void Start()
    {
        simulator = FindObjectOfType<ScreenPrintingManager>();
        player = FindObjectOfType<PlayerController>();
        if (simulator == null)
        {
            Debug.LogWarning("No se encontró un ScreenPrintingSimulator en la escena.");
        }
    }

    public void TryInteract()
    {
        if (!canInteract) return;

        if (justOneInteraction)
        {
            canInteract = false;
            ExecuteInteraction();
        }
        else
        {
            ExecuteInteraction();
        }
    }

    public void EndInteract()
    {
        EndInteraction();
    }

    public IEnumerable<InteractionType> GetActiveFlags()
    {
        foreach (InteractionType flag in System.Enum.GetValues(typeof(InteractionType)))
        {
            if (flag != InteractionType.None && interactionTypes.HasFlag(flag))
                yield return flag;
        }
    }

    private void ExecuteInteraction()
    {
        foreach (var type in GetActiveFlags())
        {
            switch (type)
            {
                case InteractionType.InvokeEvent:
                    onInteract?.Invoke();
                    break;

                case InteractionType.SelectMaterial:
                    simulator.SelectMaterialByName(itemName);
                    break;

                case InteractionType.GrabAndRelease:

                    var customGrab = gameObject.GetComponent<CustomizedGrab>();
                    if (customGrab != null && customGrab.locked)
                    {
                        UIManager.instance.ShowWarningPanel(true, "Objeto bloqueado");
                        return;
                    }

                    player.HandleGrab(gameObject);
                    break;

                case InteractionType.StartMoving:
                    var slider = selectecObject.GetComponent<XRSlider>();
                    if (slider != null)
                    {
                        player.StartSliderControl(slider);

                        var octopus = selectecObject.GetComponentInParent<OctopusController>();
                        if (octopus == null)
                            octopus = selectecObject.GetComponentInChildren<OctopusController>();

                        if (octopus != null)
                            octopus.AssignFrameByIndex(ID);
                    }

                    var wheel = selectecObject.GetComponent<XRKnob>();
                    if (wheel != null)
                    {
                        player.StartWheelControl(wheel);
                    }
                    Debug.Log("Iniciar movimiento del objeto " + selectecObject);
                    break;
            }
        }
    }

    private void EndInteraction()
    {
        foreach (var type in GetActiveFlags())
        {
            switch (type)
            {
                case InteractionType.InvokeEvent:
                    endInteract?.Invoke();
                    break;
                case InteractionType.StartMoving:
                    Debug.Log("Finalizar movimiento del objeto " + selectecObject);
                    break;
            }
        }
    }

    public void EnableInteraction() => canInteract = true;
    public void DisableInteraction() => canInteract = false;
}
