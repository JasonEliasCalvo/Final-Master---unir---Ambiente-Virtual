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
        if (material == null)
        {
            materialImage.sprite = null;
            materialImage.enabled = false; // Opcional: apaga el componente imagen
            if (materialNameText != null) materialNameText.text = "";
        }
        else
        {
            materialImage.sprite = material.materialIcon;
            materialImage.enabled = true;
            if (materialNameText != null) materialNameText.text = material.materialName;
        }
    }
}
