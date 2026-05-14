using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialScreen : MonoBehaviour
{
    public TextMeshProUGUI materialNameText;
    public TextMeshProUGUI materialDescriptionText;
    public Image materialImage;

    public void SetMaterial(FabricMaterial material)
    {
        Debug.Log($"Setting material: {material?.materialName}");
        materialImage.sprite = material?.materialIcon;
    }
}
