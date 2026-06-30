using Convai.Scripts.Runtime.Features;
using GLTFast.Schema;
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

    [Header("Eventos")]
    public ScreenPrintingEvents events;

    [Header("Tour Guide")]
    [SerializeField] private TourGuideController tourGuide;
    [SerializeField] private ConvaiInteractablesData convaiData;

    private GameObject instantiateSuperfice;
    private GameObject instantiateInk;
    public GameObject instantiateFrame;
    private FabricMaterial pendingMaterial;

    public List<MaterialScreen> inventoryScreens = new();
    [HideInInspector] public PrintSurface selectedPrintSurface;
    [HideInInspector] public PrintDesign selectedDesign;
    [HideInInspector] public Ink selectedInk;
    [HideInInspector] public Frame selectedFrame;

    #region Clases internas

    [Serializable]
    public class ScreenPrintingEvents
    {
        public UnityEvent onSelectPrintSurface;
        public UnityEvent onSelectDesign;
        public UnityEvent onSelectInk;
        public UnityEvent onSelectFrame;
        public UnityEvent onInventoryComplet;
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

        if (material is Frame frameToCheck)
        {
            if (selectedPrintSurface == null)
            {
                UIManager.instance.ShowWarningPanel(true, "Selecciona primero un sustrato/superficie.");
                return;
            }

            if (!selectedPrintSurface.IsFrameCompatibleWithSurface(frameToCheck))
            {
                UIManager.instance.ShowWarningPanel(true, $"El marco de {frameToCheck.threadCount} hilos no es compatible con {selectedPrintSurface.materialName}.");
                string context = $"Intento de selección de marco: {frameToCheck.materialName}. Superficie actual: {(selectedPrintSurface != null ? selectedPrintSurface.materialName : "ninguna")}";
                tourGuide.SendContext(context);
                return;
            }
        }

        if (material is Ink inkToCheck)
        {
            if (selectedPrintSurface == null)
            {
                UIManager.instance.ShowWarningPanel(true, "Selecciona primero un material");
                UIManager.instance.UpdateScore(5);
                return;
            }

            if (!selectedPrintSurface.IsCompatibleWith(inkToCheck))
            {
                UIManager.instance.ShowWarningPanel(true, $"La tinta {inkToCheck.materialName} no es compatible con {selectedPrintSurface.materialName}.");
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

    public void ConfirmSelection()
    {
        if (pendingMaterial == null)
        {
            UIManager.instance.ShowConfirmPanel(false);
            return;
        }

        if (pendingMaterial is PrintSurface ps)
            SetSelectedMaterial(ps, printSurfaceSpawnPoint.transform, events.onSelectPrintSurface);

        else if (pendingMaterial is Ink ink)
            SetSelectedMaterial(ink, inkSpawnPoint.transform, events.onSelectInk);

        else if (pendingMaterial is PrintDesign design)
            SetSelectedMaterial(design, null, events.onSelectDesign);

        else if (pendingMaterial is Frame sf)
            SetSelectedMaterial(sf, inkSpawnPoint.transform, events.onSelectFrame);

        if (instantiateSuperfice != null || instantiateInk != null)
        {
            UIManager.instance.ShowInventoryTutorial();
        }

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
        tourGuide.TriggerNPCEvent("Final_Trigger");
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

    private void SetSelectedMaterial<T>(T material, Transform spawnParent, UnityEvent unityEvents) where T : FabricMaterial
    {
        if (material == null) return;

        //  Limpiar lo que había antes
        if (selectedPrintSurface != null && material is PrintSurface)
        {
            ResetPrintingSetup();
        }

        // Guardar la referencia lógica del ScriptableObject
        if (material is PrintSurface ps) selectedPrintSurface = ps;
        if (material is Ink ink) selectedInk = ink;
        if (material is PrintDesign d) selectedDesign = d;
        if (material is Frame f) selectedFrame = f;

        // 3. Instanciar si tiene prefab
        if (material.materialPrefab != null && spawnParent != null)
        {
            GameObject newObj = Instantiate(material.materialPrefab, spawnParent);
            newObj.SetActive(true);

            var inkComp = newObj.GetComponentInChildren<InkInstance>();
            if (inkComp != null && material is Ink selectedInkSO)
            {
                instantiateInk = newObj;
                inkComp.SetInkData(selectedInkSO);
                Debug.Log($"<color=green>Éxito:</color> Tinta {selectedInkSO.materialName} guardada en instancia.");
            }

            var surfaceComp = newObj.GetComponentInChildren<PrintSurfaceInstance>();
            if (surfaceComp != null && material is PrintSurface selectedSurfaceSO)
            {
                instantiateSuperfice = newObj;
                surfaceComp.SetMaterial(surfaceComp.currentMaterial, selectedSurfaceSO);
                Debug.Log($"<color=green>Éxito:</color> Superficie {selectedSurfaceSO.materialName} guardada en instancia.");
            }

            var frameComp = newObj.GetComponentInChildren<ScreenFrame>();
            if (frameComp != null && material is Frame selectedFrameSO)
            {
                instantiateFrame = newObj;
                Debug.Log($"<color=green>Éxito:</color> Marco físico de {selectedFrameSO.threadCount} hilos guardado en instancia.");
            }

            PlayerController.instance.AddToInventory(newObj);
        }

        InvokeEvents(unityEvents);
    }

    private void ResetPrintingSetup()
    {
        PlayerController.instance.RemoveFromInventory(instantiateSuperfice);
        PlayerController.instance.RemoveFromInventory(instantiateInk);
        PlayerController.instance.RemoveFromInventory(instantiateFrame);

        instantiateSuperfice = null;
        instantiateInk = null;
        instantiateFrame = null;

        selectedPrintSurface = null;
        selectedInk = null;
        selectedFrame = null;

        PlayerController.instance.RefreshInventoryUI();
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
    public void InvokeEvents(UnityEvent unityEvents)
    {
        if (unityEvents == null) return;
        unityEvents?.Invoke();
    }
}
