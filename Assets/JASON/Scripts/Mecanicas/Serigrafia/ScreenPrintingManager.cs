using Convai.Scripts.Runtime.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static Convai.Scripts.Runtime.Features.ConvaiInteractablesData;

public class ScreenPrintingManager : MonoBehaviour
{
    public static ScreenPrintingManager instance;

    [Header("Base de Datos")]
    [SerializeField] private ScreenPrintingDatabase database;
    [SerializeField] private GameObject inkSpawnPoint;
    [SerializeField] private GameObject printSurfaceSpawnPoint;
    [SerializeField] PrintDesign design;

    [Header("Pantallas UI")]
    [SerializeField] private ScreenGroups screens;

    [Header("Eventos")]
    [SerializeField] private ScreenPrintingEvents events;

    [Header("Tour Guide")]
    [SerializeField] private TourGuideController tourGuide;
    [SerializeField] private ConvaiInteractablesData convaiData;

    private GameObject grabbedObject;
    private GameObject instantiateSuperfice;
    private GameObject instantiateInk;
    private FabricMaterial pendingMaterial;
    private PrintSurface selectedPrintSurface;
    private PrintDesign selectedDesign;
    private Ink selectedInk;

    [Header("Mini Inventario")]
    private GameObject inventorySurfaceObj;
    private GameObject inventoryInkObj;

    #region Clases internas
    [Serializable]
    public class ScreenGroups
    {
        public List<MaterialScreen> inkScreens = new List<MaterialScreen>();
        public List<MaterialScreen> designScreens = new List<MaterialScreen>();
        public List<MaterialScreen> printSurfaceScreens = new List<MaterialScreen>();
    }

    [Serializable]
    public class ScreenPrintingEvents
    {
        public UnityEvent onSelectPrintSurface;
        public UnityEvent onSelectDesign;
        public UnityEvent onSelectInk;
        public UnityEvent onEndSimulation;
    }
    #endregion

    // -------------------------
    // Ciclo de vida
    // -------------------------
    public void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        if (database == null)
            Debug.LogError("ScreenPrintingManager: La base de datos no está asignada.");
        if (tourGuide == null)
            Debug.LogError("ScreenPrintingManager: El Tour Guide no está asignado.");

        if (convaiData == null)
        {
            Debug.LogError("ScreenPrintingManager: El Convai Data no está asignado.");
            Debug.Log("Se creo la lista base");
        }

        SetDataBase();

        if (design != null)
            selectedDesign = design;
        else
            Debug.Log("No hay diseño asignado al inicializar");
    }

    // -------------------------
    // API - Lógica de Inventario Mejorada
    // -------------------------

    public void ToggleMaterialInHand()
    {
        if (instantiateSuperfice == null) return;

        bool currentlyActive = instantiateSuperfice.activeSelf;

        if (!currentlyActive)
        {
            // REGLA: Si ya tengo algo en la mano (que no sea este objeto), no permitir aparecer otro
            if (PlayerController.instance.CurrentGrab != null)
            {
                Debug.Log("Mano ocupada, no puedes sacar la superficie.");
                return;
            }

            // Aparecer y posicionar
            instantiateSuperfice.SetActive(true);
            instantiateSuperfice.transform.position = printSurfaceSpawnPoint.transform.position;
            instantiateSuperfice.transform.rotation = printSurfaceSpawnPoint.transform.rotation;

            // ACTIVAR EVENTO GRAB: Forzamos al PlayerController a agarrarlo
            PlayerController.instance.HandleGrab(instantiateSuperfice);
        }
        else
        {
            // Si el objeto está activo y es lo que tenemos en la mano, lo soltamos y desaparecemos
            if (PlayerController.instance.CurrentGrab == instantiateSuperfice)
            {
                // Forzamos el release en el player (debes tener un método similar o limpiar la ref)
                PlayerController.instance.ClearCurrentGrabReference();
            }

            instantiateSuperfice.SetActive(false);
        }
    }

    public void ToggleInkInHand()
    {
        if (instantiateInk == null) return;

        bool currentlyActive = instantiateInk.activeSelf;

        if (!currentlyActive)
        {
            // REGLA: No permitir si la mano está ocupada
            if (PlayerController.instance.CurrentGrab != null)
            {
                Debug.Log("Mano ocupada, no puedes sacar la tinta.");
                return;
            }

            instantiateInk.SetActive(true);
            instantiateInk.transform.position = inkSpawnPoint.transform.position;
            instantiateInk.transform.rotation = inkSpawnPoint.transform.rotation;

            // ACTIVAR EVENTO GRAB
            PlayerController.instance.HandleGrab(instantiateInk);
        }
        else
        {
            if (PlayerController.instance.CurrentGrab == instantiateInk)
            {
                PlayerController.instance.ClearCurrentGrabReference();
            }

            instantiateInk.SetActive(false);
        }
    }

    // -------------------------
    // API 
    // -------------------------

    public void SelectMaterialByName(string materialName)
    {
        var material = database?.GetMaterialByName(materialName);

        if (material == null)
        {
            Debug.LogWarning($"No se encontró material con nombre: {materialName}");
            return;
        }

        if (material is Ink inkToCheck)
        {
            if (selectedPrintSurface == null)
            {
                UIManager.instance.ShowWarningPanel(true, "Selecciona primero un material");
                tourGuide.TriggerNPCEvent("Missing_Printing_Surface");
                UIManager.instance.UpdateScore(5);
                return;
            }

            if (!selectedPrintSurface.IsCompatibleWith(inkToCheck))
            {
                UIManager.instance.ShowWarningPanel(true, $"La tinta {inkToCheck.materialName} no es compatible con {selectedPrintSurface.materialName}.");
                tourGuide.TriggerNPCEvent("Ink_Error");
                string context = $"Intento de selección de tinta: {inkToCheck.materialName}. Superficie actual: {(selectedPrintSurface != null ? selectedPrintSurface.materialName : "ninguna")}";
                tourGuide.SendContext(context);
                UIManager.instance.UpdateScore(5);
                return;
            }
        }

        pendingMaterial = material;

        if (ShouldShowReplaceConfirmation(material))
            return;

        UIManager.instance.ShowConfirmPanel(true, $"¿Estás seguro de seleccionar {material.materialName}?");
    }

    public void SetGrabbedObject(GameObject obj)
    {
        grabbedObject = obj;
    }

    public void ConfirmSelection()
    {
        if (pendingMaterial == null)
        {
            UIManager.instance.ShowConfirmPanel(false);
            return;
        }

        if (pendingMaterial is PrintSurface ps)
            SetSelectedMaterial(ps, printSurfaceSpawnPoint.transform, screens.printSurfaceScreens, events.onSelectPrintSurface);

        else if (pendingMaterial is Ink ink)
            SetSelectedMaterial(ink, inkSpawnPoint.transform, screens.inkScreens, events.onSelectInk);

        else if (pendingMaterial is PrintDesign design)
            SetSelectedMaterial(design, null, screens.designScreens, events.onSelectDesign);

        CurrectSelectedMaterials();

        pendingMaterial = null;
        UIManager.instance.ShowConfirmPanel(false);
    }

    public void CancelSelection()
    {
        pendingMaterial = null;
        UIManager.instance.ShowConfirmPanel(false);
    }

    public void EndSimulation()
    {
        if (selectedDesign == null || selectedInk == null || selectedPrintSurface == null) return;

        string context = $"Simulación Inicianda con {selectedPrintSurface.materialName}, tinta {selectedInk.materialName}, diseño {selectedDesign.materialName}";
        Debug.Log(context);
        tourGuide.SendContext(context);
        InvokeEvents(events.onEndSimulation);
    }

    internal PrintDesign GetSelectedDesign()
    {
        return selectedDesign;
    }

    // -------------------------
    // Helpers - Selección y reemplazo
    // -------------------------
    private bool ShouldShowReplaceConfirmation(FabricMaterial material)
    {
        // Avisar reemplazo
        if (material is PrintSurface newSurface && selectedPrintSurface != null)
        {
            UIManager.instance.ShowConfirmPanel(true, $"¿Estás seguro de seleccionar {material.materialName}?");
            return true;
        }

        if (material is Ink newInk && selectedInk != null)
        {
            UIManager.instance.ShowConfirmPanel(true, $"¿Estás seguro de seleccionar {material.materialName}?");
            return true;
        }

        if (material is PrintDesign newDesign && selectedDesign != null)
        {
            UIManager.instance.ShowConfirmPanel(true, $"¿Estás seguro de seleccionar {material.materialName}?");
            return true;
        }

        // Si no hay reemplazo, continuar normalmente
        return false;
    }

    private void ClearGrabbedObject()
    {
        if (grabbedObject != null)
        {
            Destroy(grabbedObject);
            grabbedObject = null;
        }
    }

    private void ClearInstancie(FabricMaterial material)
    {
        if (material is PrintSurface && instantiateSuperfice != null)
        {
            Destroy(instantiateSuperfice);
            instantiateSuperfice = null;
        }
        else if (material is Ink && instantiateInk != null)
        {
            Destroy(instantiateInk);
            instantiateInk = null;
        }
    }

    private void SetSelectedMaterial<T>(T material, Transform spawnParent, List<MaterialScreen> screensToUpdate, UnityEvent unityEvents) where T : FabricMaterial
    {
        if (material == null) return;

        // 1. Guardar la referencia lógica del ScriptableObject
        if (material is PrintSurface ps) selectedPrintSurface = ps;
        if (material is Ink ink) selectedInk = ink;
        if (material is PrintDesign d) selectedDesign = d;

        // 2. Limpiar lo que había antes
        ClearGrabbedObject();
        ClearInstancie(material);

        // 3. Instanciar si tiene prefab
        if (material.materialPrefab != null && spawnParent != null)
        {
            GameObject newObj = Instantiate(material.materialPrefab, spawnParent);
            newObj.SetActive(false);

            var inkComp = newObj.GetComponentInChildren<InkInstance>();
            if (inkComp != null && material is Ink selectedInkSO)
            {
                instantiateInk = newObj; // ¡Ahora sí se guarda!
                inkComp.SetInkData(selectedInkSO);
                Debug.Log($"<color=green>Éxito:</color> Tinta {selectedInkSO.materialName} guardada en instancia.");
            }

            var surfaceComp = newObj.GetComponentInChildren<PrintSurfaceInstance>();
            if (surfaceComp != null && material is PrintSurface selectedSurfaceSO)
            {
                instantiateSuperfice = newObj; // ¡Ahora sí se guarda!
                surfaceComp.SetMaterial(surfaceComp.currentMaterial, selectedSurfaceSO);
                Debug.Log($"<color=green>Éxito:</color> Superficie {selectedSurfaceSO.materialName} guardada en instancia.");
            }
        }

        InvokeEvents(unityEvents);
        UpdateScreens(screensToUpdate, material);
    }

    private void UpdateScreens(List<MaterialScreen> screensToUpdate, FabricMaterial material)
    {
        if (screensToUpdate == null) return;
        foreach (var screen in screensToUpdate)
            screen?.SetMaterial(material);
    }

    // -------------------------
    // Helpers - Convai
    // -------------------------

    private void CurrectSelectedMaterials()
    {
        UpdateOrCreateConvaiObject("Superficie actual",
            selectedPrintSurface != null
                ? $"{selectedPrintSurface.materialName}. {selectedPrintSurface.materialDescription}"
                : "ninguna");

        UpdateOrCreateConvaiObject("Tinta actual",
            selectedInk != null
                ? $"{selectedInk.materialName}. {selectedInk.materialDescription}. Color: {selectedInk.color}"
                : "ninguna");

        UpdateOrCreateConvaiObject("Diseño actual",
            selectedDesign != null
                ? $"{selectedDesign.materialName}. {selectedDesign.materialDescription}"
                : "ninguno");

        UpdateOrCreateConvaiObject("Compatibilidad actual",
            selectedPrintSurface != null ? selectedPrintSurface.GetCompatibilityDescription() : "ninguna");

        var handler = FindAnyObjectByType<ConvaiActionsHandler>();
        if (handler != null)
            handler.RefreshObjectsFromConvaiData();

        UpdateConvaiWithSelection();
    }

    private void UpdateOrCreateConvaiObject(string name, string description)
    {
        var obj = convaiData.Objects.FirstOrDefault(o => o.Name == name);
        if (obj != null)
        {
            obj.Description = description;
            obj.gameObject = GameObject.Find("PlayerStateManager");
        }
        else
        {
            var list = convaiData.Objects.ToList();
            list.Add(new ConvaiInteractablesData.Object
            {
                Name = name,
                Description = description,
                gameObject = GameObject.Find("PlayerStateManager")
            });
            convaiData.Objects = list.ToArray();
        }
        Debug.Log($"Convai actualizado: {name} -> {description}");
    }

    public void UpdateConvaiWithSelection()
    {
        if (tourGuide == null) return;

        // Construye el texto del estado actual
        string context =
            $"Superficie: {(selectedPrintSurface != null ? selectedPrintSurface.materialName : "ninguna")}\n" +
            $"Tinta: {(selectedInk != null ? selectedInk.materialName : "ninguna")}\n" +
            $"Diseño: {(selectedDesign != null ? selectedDesign.materialName : "ninguno")}\n" +
            $"Compatibilidad: {(selectedPrintSurface != null ? selectedPrintSurface.GetCompatibilityDescription() : "ninguna")}";

        // Solo enviamos texto al backend de Convai, no interrumpimos al NPC
        tourGuide.SendContext(context);
    }


    private void SetDataBase()
    {
        if (convaiData == null) return;

        var dbHall = new Hall
        {
            Name = "Database",
            Description = "Base de datos con todos los elemntos disponibles en el salon de serigrafia superficies, tintas y diseños.",
            Objects = new List<ConvaiInteractablesData.Object>(),
            gameObject = this.gameObject,
        };

        // Superficies
        AddObjectsToHall(database.printSurfaces, dbHall.Objects, surface => new ConvaiInteractablesData.Object
        {
            Name = surface.materialName,
            Description = $"Superficie de impresión: {surface.materialDescription}. {surface.GetCompatibilityDescription()}",
            gameObject = surface.materialPrefab
        });

        // Tintas
        AddObjectsToHall(database.inks, dbHall.Objects, ink => new ConvaiInteractablesData.Object
        {
            Name = ink.materialName,
            Description = $"Tinta ({ink.inkType}): {ink.materialDescription}.",
            gameObject = ink.materialPrefab
        });

        // Diseños
        AddObjectsToHall(database.designs, dbHall.Objects, design => new ConvaiInteractablesData.Object
        {
            Name = design.materialName,
            Description = $"Diseño: {design.materialDescription}.",
            gameObject = design.materialPrefab
        });

        var hallsList = convaiData.Halls != null ? new List<Hall>(convaiData.Halls) : new List<Hall>();
        hallsList.RemoveAll(h => h.Name == dbHall.Name);
        hallsList.Add(dbHall);
        convaiData.Halls = hallsList.ToArray();

        // Refrescar en Convai
        FindAnyObjectByType<ConvaiActionsHandler>()?.RefreshObjectsFromConvaiData();

        Debug.Log("Base de datos cargada en Convai dentro del Hall 'Database'");
    }

    private void AddObjectsToHall<T>(IEnumerable<T> items, List<ConvaiInteractablesData.Object> target, Func<T, ConvaiInteractablesData.Object> mapper)
    {
        foreach (var item in items)
        {
            if (item == null) continue;
            target.Add(mapper(item));
        }
    }

    // -------------------------
    // Helpers - Generales
    // -------------------------
    private void InvokeEvents(UnityEvent unityEvents)
    {
        if (unityEvents == null) return;
        unityEvents?.Invoke();
    }
}
