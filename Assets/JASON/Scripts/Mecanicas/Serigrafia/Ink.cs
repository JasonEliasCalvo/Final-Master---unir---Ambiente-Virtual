using UnityEngine;

public enum InkType
{
    WaterBased,
    PVC,        
    Vitrifiable   
}

[CreateAssetMenu(fileName = "NewInk", menuName = "ScreenPrinting/Ink", order = 1)]
public class Ink : FabricMaterial
{
    public InkType inkType;
    public Color color;
}
