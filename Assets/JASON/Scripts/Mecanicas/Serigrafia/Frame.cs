using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FrameMaterialType
{
    Wood,
    Aluminum
}

[CreateAssetMenu(fileName = "NewScreenFrame", menuName = "Serigrafia/Marco")]
public class Frame : FabricMaterial
{
    public int threadCount;
    public FrameMaterialType type;
}