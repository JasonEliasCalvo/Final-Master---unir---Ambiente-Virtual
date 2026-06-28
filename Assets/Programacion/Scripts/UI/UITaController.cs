using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Tab
{
    public Button button;
    public GameObject content;
}

public class UITaController : MonoBehaviour
{
    [Header("Configura tus Tabs")]
    public List<Tab> tabs;
    public Color normalColor = Color.gray;
    public Color selectedColor = Color.green;

    private int currentIndex = -1;

    private void Start()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            tabs[i].button.onClick.AddListener(() => ShowTab(index));
        }

        if (tabs.Count > 0)
            ShowTab(0);

        Debug.Log("UITabController initialized with " + tabs.Count + " tabs.");
    }

    public void ShowTab(int index)
    {
        if (index == currentIndex) return;

        for (int i = 0; i < tabs.Count; i++)
        {
            bool active = (i == index);

            tabs[i].content.SetActive(active);

            var colors = tabs[i].button.colors; // ColorBlock
            colors.normalColor = active ? selectedColor : normalColor;

            // Reasignar el bloque modificado al botón
            tabs[i].button.colors = colors;
            tabs[i].button.OnDeselect(null);
        }

        currentIndex = index;
    }
}
