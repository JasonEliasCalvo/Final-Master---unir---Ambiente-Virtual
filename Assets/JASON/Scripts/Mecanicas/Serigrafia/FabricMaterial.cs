using UnityEngine;

public abstract class FabricMaterial : ScriptableObject
{
    public string materialName;
    [TextArea(1, 6)] public string materialDescription;
    public Sprite materialIcon;
    public GameObject materialPrefab;
}
