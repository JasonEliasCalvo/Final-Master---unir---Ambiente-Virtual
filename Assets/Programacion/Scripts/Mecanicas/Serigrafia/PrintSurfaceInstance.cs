using System.Collections;
using UnityEngine;

public class PrintSurfaceInstance : Item
{
    public GameObject currentMaterial;
    public PrintSurface surfaceData;
    public CustomizedGrab customizedGrab;
    public bool isPrinted = false;
    public Rigidbody rb;

    public void SetMaterial(GameObject materialObj, PrintSurface data = null)
    {
        if (currentMaterial == null)
            currentMaterial = gameObject;
        else
            currentMaterial = materialObj;

        surfaceData = data;
    }

    public void ClearMaterial()
    {
        Debug.Log("ClearMaterial llamado. currentMaterial era: " + (currentMaterial ? currentMaterial.name : "null"));
    }

    public void TriggerPrint(Ink ink, PrintDesign design)
    {
        Print(ink, design);
    }

    public void Print(Ink ink, PrintDesign design)
    {
        var renderer = currentMaterial?.GetComponent<Renderer>();
        if (renderer != null && ink != null)
        {
            var mat = renderer.material;

            // Pasar color del tinte
            mat.SetColor("_InkColor", ink.color);

            // Pasar el logo/diseño
            if (design != null)
            mat.SetTexture("_DesignTex", design.designTexture);

            mat.SetFloat("_BlendAlpha", 1f);

            isPrinted = true;
            ScreenPrintingManager.instance.EndSimulation();
        }
        else
        {
            Debug.LogWarning("No se pudo aplicar la impresión. Asegúrate de que currentMaterial, ink y design no sean nulos.");
        }

        ClearMaterial();
        Debug.Log("Proceso de impresión completado.");
    }
}

