using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPrintDatabase", menuName = "ScreenPrinting/ScreenPrintingDatabase")]
public class ScreenPrintingDatabase : ScriptableObject
{
    public List<PrintSurface> printSurfaces = new List<PrintSurface>();
    public List<PrintDesign> designs = new List<PrintDesign>();
    public List<Ink> inks = new List<Ink>();

    public FabricMaterial GetMaterialByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogWarning("GetMaterialByName: name vacío o nulo.");
            return null;
        }

        foreach (PrintSurface surface in printSurfaces)
        {
            if (surface != null && surface.materialName.Equals(name, StringComparison.OrdinalIgnoreCase))
                return surface;
        }

        foreach (Ink ink in inks)
        {
            if (ink != null && ink.materialName.Equals(name, StringComparison.OrdinalIgnoreCase))
                return ink;
        }

        foreach (PrintDesign design in designs)
        {
            if (design != null && design.materialName.Equals(name, StringComparison.OrdinalIgnoreCase))
                return design;
        }

        Debug.LogWarning($"Material {name} no encontrado.");
        return null;
    }
} 
