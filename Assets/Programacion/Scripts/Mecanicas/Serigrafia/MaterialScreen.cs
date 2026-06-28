using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialScreen : MonoBehaviour
{
    public TextMeshProUGUI materialNameText;
    public TextMeshProUGUI materialDescriptionText;
    public Image materialImage;

    public void SetItem(Item item)
    {
        if (item == null)
        {
            materialImage.sprite = null;
            materialImage.enabled = false;
            if (materialNameText != null) materialNameText.text = "";
        }
        else
        {
            materialImage.sprite = item.materialIcon;
            materialImage.enabled = true;
            if (materialNameText != null) materialNameText.text = item.materialName;
        }
    }
}
