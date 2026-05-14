using UnityEngine;

public class FindMaterialUsers : MonoBehaviour
{
    [SerializeField] private Material targetMaterial;

    void Start()
    {
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();

        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat == targetMaterial)
                {
                    Debug.Log("Usado por: " + renderer.gameObject.name);
                }
            }
        }
    }
}

