using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public delegate void DelegatedGameStates();
    public DelegatedGameStates eventGameStart;
    public DelegatedGameStates eventGameEnd;
    public static GameManager instance;


    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public Image fadeImage;
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;
    [SerializeField] private float timer = 0;

    // -------------------------
    // Ciclo de vida
    // -------------------------
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        GamePrepate();
    }

    private void GamePrepate()
    {
        if (fadeCanvasGroup != null)
        {
            fadeImage.color = fadeColor;
            fadeCanvasGroup.alpha = 1f;
            StartFadeIn();
        }

        Invoke(nameof(GameStart), 0.2f);
    }

    public void GameStart()
    {
        eventGameStart?.Invoke();
    }

    public void GameEnd()
    {
        eventGameEnd?.Invoke();
    }

    // -------------------------
    // API - Control del juego
    // -------------------------

    public void GamePause()
    {
        if (!UIManager.instance.pausePanel.activeSelf)
        {
            UIManager.instance.ShowPausePanel(true);
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 1f;
            GameEnd();
        }
        else
        {
            UIManager.instance.ShowPausePanel(false);
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
            GameStart();
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    // -------------------------
    // API - Escenas y transiciones
    // -------------------------

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(FadeAndLoad(sceneIndex));
        Time.timeScale = 1;
        UIManager.instance.ShowCursor(true);
    }

    public void StartFadeOut(Action onComplete = null) =>
        StartCoroutine(FadeOutCoroutine(onComplete));

    public void StartFadeIn(Action onComplete = null) =>
        StartCoroutine(FadeInCoroutine(onComplete));


    // -------------------------
    // Helpers privados - Corutinas
    // -------------------------
    private IEnumerator FadeOutCoroutine(Action onComplete)
    {
        if (fadeImage != null)
        {
            fadeImage.color = fadeColor;
        }

        timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1;
        onComplete?.Invoke();
    }

    private IEnumerator FadeInCoroutine(Action onComplete)
    {
        timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0;
        onComplete?.Invoke();
    }

    public IEnumerator FadeAndLoad(int sceneIndex)
    {
        yield return StartCoroutine(FadeOutCoroutine(null));
        SceneManager.LoadScene(sceneIndex);
    }
}
