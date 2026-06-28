using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void PlayButtonC(int index)
    {
        StartCoroutine(GameManager.instance.FadeAndLoad(index));
    }

    public void QuitButton()
    {
        Debug.Log("Salir del juego...");
        Application.Quit();
    }
}
