using UnityEngine;

public enum InkType { Water, UV, Plastisol, Solvent, Oil }

[CreateAssetMenu(fileName = "NewInk", menuName = "ScreenPrinting/Ink", order = 1)]
public class Ink : FabricMaterial
{
    public InkType inkType;
    public Color color;
}
