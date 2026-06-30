using Convai.Scripts.Runtime.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

    [Header("UI Elements")]
    public GameObject optionsPanel;
    public GameObject pausePanel;
    public GameObject confirmPanel;
    public GameObject warningPanel;
    public GameObject securityPanel;
    public GameObject interactionPanel;
    public GameObject mousePanel;
    public GameObject dropPanel;
    public ChatBoxUI chatBoxUI;
    public List<Image> Selected;
    public UISettingsPanel settingPanel;
    public TextMeshProUGUI confirmText;
    public TextMeshProUGUI warningText;
    public TextMeshProUGUI scoretext;
    public Image[] inventoryIcons;

    [Header("Inventory Tutorial")]
    public CanvasGroup inventoryHintPanel;
    private bool tutorialShown = false;

    [Header("UI Setting")]
    public int score = 100;
    public bool showCursor = false;
    public PlayerController player;
    public AudioClip WarningSound;
    public void ShowInventoryTutorial()
    {
        if (tutorialShown || inventoryHintPanel == null) return;

        Debug.Log("Mostrando tutorial de inventario");

        return;

        tutorialShown = true; // Asegura que solo pase una vez

        // Secuencia de animación
        inventoryHintPanel.gameObject.SetActive(true);
        inventoryHintPanel.alpha = 0; // Empezar invisible

        // Fade In
        inventoryHintPanel.DOFade(1f, 1f).OnComplete(() => {
            // Esperar 4 segundos visible y luego hacer Fade Out
            inventoryHintPanel.DOFade(0f, 1.5f)
                .SetDelay(4f)
                .OnComplete(() => inventoryHintPanel.gameObject.SetActive(false));
        });
    }

    // -------------------------
    // Ciclo de vida
    // -------------------------

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        chatBoxUI = FindObjectOfType<ChatBoxUI>();
    }

    public void LateUpdate()
    {
        securityPanel.SetActive(IsPanelActive());
    }

    // -------------------------
    // API
    // -------------------------

    public void ShowPausePanel(bool state)
    {
        if (state)
        {
            ShowPanel(pausePanel, true);
            GameManager.instance.GameEnd();
        }
        else
        {
            ShowPanel(pausePanel, false);
            GameManager.instance.GameStart();
        }
    }

    public void ShowOptionsPanel(bool state)
    {
        if (state)
        {
            ShowPanel(optionsPanel, true);
            ShowPanel(pausePanel, false);
            GameManager.instance.GameEnd();
        }
        else
        {
            ShowPanel(optionsPanel, false);
            ShowPanel(pausePanel, true);
            GameManager.instance.GameEnd();
        }
    }

    public void ShowConfirmPanel(bool state, string message = "")
    {
        confirmText.text = message;
        ShowPanel(confirmPanel, state);
    }

    public void ShowWarningPanel(bool state, string message = "")
    {
        warningText.text = message;
        AudioManager.Instance.PlaySound(WarningSound);
        ShowPanel(warningPanel, state);
    }

    public void ShowInteractPanel(bool state)
    {
        interactionPanel.SetActive(state);
    }

    public void ShowDropPanel(bool state)
    {
        dropPanel.SetActive(state);
    }

    public void ShowMousePanel(bool state)
    {
        mousePanel.SetActive(state);
    }

    public void ShowCursor(bool show)
    {
        if (show)
        {
            Debug.Log("Mostrando cursor");
            GameManager.instance.GameEnd();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ShowSlected(false);
        }
        else
        {
            Debug.Log("Ocultando cursor");
            GameManager.instance.GameStart();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ShowSlected(true);
        }
    }

    public void ShowSlected(bool state)
    {
        foreach (var item in Selected)
            item.enabled = state;

    }
    public void UpdateScore(int currentScore)
    {
        score = score - currentScore;
        scoretext.text = score.ToString();
    }

    // -------------------------
    // Helpers
    // -------------------------

    public bool IsPanelActive()
    {
        bool isActive =
          (pausePanel != null && pausePanel.activeSelf) ||
          (optionsPanel != null && optionsPanel.activeSelf) ||
          (confirmPanel != null && confirmPanel.activeSelf) ||
          (warningPanel != null && warningPanel.activeSelf) ||
          (settingPanel != null && settingPanel.PanelCanvasGroup.gameObject.activeSelf);

        if (chatBoxUI != null)
        {
            if (isActive) ToggleChat(false);
            else ToggleChat(true);
        }

        return isActive;
    }

    public void ToggleChat(bool value)
    {
        var cg = chatBoxUI.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            if (value)
                cg.alpha = 1f;
            else
                cg.alpha = 0f;
        }
    }

    private IEnumerator ShowPanelAfterDelay(GameObject panel)
    {
        yield return new WaitForSeconds(0.2f);
        ShowPanel(panel, true);
    }

    public void ShowPanel(GameObject panel, bool state)
    {
        if (panel == null) return;
        Transform rt = panel.transform;

        if (DOTween.IsTweening(rt))
        {
            DOTween.Kill(rt);
            StartCoroutine(WaitAndAnimate(panel, rt, state));
        }
        else
        {
            StartCoroutine(Animate(panel, rt, state));
        }
    }

    private IEnumerator WaitAndAnimate(GameObject panel, Transform rt, bool state)
    {
        yield return new WaitForSeconds(0.1f);
        yield return Animate(panel, rt, state);
    }

    private IEnumerator Animate(GameObject panel, Transform rt, bool state)
    {
        if (!originalScales.ContainsKey(panel))
            originalScales[panel] = Vector3.one;

        if (state)
        {
            panel.SetActive(true);
            rt.localScale = Vector3.zero;

            yield return null;
            if (IsPanelActive())
                ShowCursor(true);

            rt.DOScale(originalScales[panel], 0.2f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(() => {

                    if (IsPanelActive() && Cursor.lockState != CursorLockMode.None && !Cursor.visible)
                        ShowCursor(true);
                });
        }
        else
        {
            rt.DOScale(Vector3.zero, 0.2f)
              .SetEase(Ease.InBack)
              .SetUpdate(true)
              .OnComplete(() =>
              {
                  panel.SetActive(false);
                  rt.localScale = Vector3.zero;

                  if (!IsPanelActive())
                      ShowCursor(false);
              });
        }

        yield return null;
    }

    // -------------------------
    // Wrappers para Inspector
    // -------------------------

    public void ShowConfirmPanelOn(string message) => ShowConfirmPanel(true, message);
    public void ShowConfirmPanelOff() => ShowConfirmPanel(false);
    public void ShowWarningPanelOn(string message) => ShowWarningPanel(true, message);
    public void ShowWarningPanelOff() => ShowWarningPanel(false);

    public void ShowPanelOn(GameObject panel) => ShowPanel(panel, true);
    public void ShowPanelOff(GameObject panel) => ShowPanel(panel, false);
    public void ShowAfterDelay(GameObject panel) => ShowPanelAfterDelay(panel);
}
