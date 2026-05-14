using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldButton : MonoBehaviour
{
    public List<Image> images = new List<Image>();
    public Color selected;
    public Color normal;

    public void SetSelectedColor(int index)
    {
        if (images == null || images.Count == 0) return;

        for (int i = 0; i < images.Count; i++)
        {
            if (images[i] == null) continue;

            images[i].color = (i == index) ? selected : normal;

            Color c = images[i].color;
            c.a = 1f;
            images[i].color = c;
        }
    }

    private void SetNormalColor(int index)
    {
        if (index < 0 || index >= images.Count) return;
        if (images[index] == null) return;

        images[index].color = normal;
        Color c = images[index].color; 
        c.a = 1f;
        images[index].color = c;
    }
}
