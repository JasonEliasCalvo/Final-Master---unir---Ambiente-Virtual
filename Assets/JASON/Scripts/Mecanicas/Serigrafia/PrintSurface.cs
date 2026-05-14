using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPrintSurface", menuName = "ScreenPrinting/PrintSurface", order = 0)]
public class PrintSurface : FabricMaterial
{
    [SerializeField] private List<InkType> compatibleInks = new List<InkType>();
    private HashSet<InkType> _compatibleSet;

    private void EnsureHashSet()
    {
        if (_compatibleSet == null)
            _compatibleSet = new HashSet<InkType>(compatibleInks ?? new List<InkType>());
    }

    public bool IsCompatibleWith(Ink ink)
    {
        if (ink == null) return false;
        EnsureHashSet();
        return _compatibleSet.Contains(ink.inkType);
    }

    public string GetCompatibilityDescription()
    {
        EnsureHashSet();
        if (_compatibleSet == null || _compatibleSet.Count == 0)
            return "No tiene tintas compatibles definidas.";

        return "Tintas compatibles: " + string.Join(", ", _compatibleSet);
    }
}
