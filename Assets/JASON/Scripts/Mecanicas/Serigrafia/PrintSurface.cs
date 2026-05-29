using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPrintSurface", menuName = "ScreenPrinting/PrintSurface", order = 0)]
public class PrintSurface : FabricMaterial
{
    [SerializeField] private List<InkType> compatibleInks = new List<InkType>();
    [SerializeField] private List<Frame> compatibleFrames = new List<Frame>();
    private HashSet<InkType> _compatibleInksSet;
    private HashSet<Frame> _compatibleFramesSet;

    private void EnsureHashSet()
    {
        if(_compatibleInksSet == null)
            _compatibleInksSet = new HashSet<InkType>(compatibleInks ?? new List<InkType>());

        if (_compatibleFramesSet == null)
            _compatibleFramesSet = new HashSet<Frame>(compatibleFrames ?? new List<Frame>());
    }

    public bool IsCompatibleWith(Ink ink)
    {
        if (ink == null) return false;
        EnsureHashSet();
        return _compatibleInksSet.Contains(ink.inkType);
    }

    public bool IsFrameCompatibleWithSurface(Frame frame)
    {
        if (frame == null) return false;
        EnsureHashSet();

        // Validación directa por referencia O(1) basada estrictamente en la lista del inspector
        return _compatibleFramesSet.Contains(frame);
    }

    public string GetCompatibilityDescription()
    {
        EnsureHashSet();
        if (_compatibleInksSet == null || _compatibleInksSet.Count == 0)
            return "No tiene tintas compatibles definidas.";

        return "Tintas compatibles: " + string.Join(", ", _compatibleInksSet);
    }
}
