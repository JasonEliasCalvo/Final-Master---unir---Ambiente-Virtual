using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    private PlayerInputActions _action;
    public static event Action OnInteractPressed;
    public static event Action OnInteractStarted;
    public static event Action OnInteractCanceled;

    private void Awake()
    {
        _action = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _action.UI.Enable();
        _action.UI.Pause.performed += OnPauseInput;
        _action.UI.Interact.performed += OnInteractInput;
        _action.UI.Interact.started += ctx => OnInteractStarted?.Invoke();
        _action.UI.Interact.canceled += ctx => OnInteractCanceled?.Invoke();
        _action.UI.ShowCursor.started += ctx => ShowCursor(true);
        _action.UI.ShowCursor.canceled += ctx => ShowCursor(false);
    }

    private void OnDisable()
    {
        _action.UI.Pause.performed -= OnPauseInput;
        _action.UI.Interact.performed -= OnInteractInput;
        _action.UI.Disable();
    }

    private void OnPauseInput(InputAction.CallbackContext ctx)
    {
        if (UIManager.instance == null)
        {
            GameManager.instance?.GamePause();
            return;
        }

        // Comprobamos cada panel solo si existe
        bool confirmActive = UIManager.instance.confirmPanel != null && UIManager.instance.confirmPanel.activeSelf;
        bool warningActive = UIManager.instance.warningPanel != null && UIManager.instance.warningPanel.activeSelf;
        bool settingsActive = UIManager.instance.settingPanel != null &&
                              UIManager.instance.settingPanel.PanelCanvasGroup != null &&
                              UIManager.instance.settingPanel.PanelCanvasGroup.gameObject.activeSelf;

        // Si alguno de los paneles está activo, los cerramos
        if (confirmActive || warningActive || settingsActive)
        {
            if (UIManager.instance.confirmPanel != null)
                UIManager.instance.ShowConfirmPanel(false);

            if (UIManager.instance.warningPanel != null)
                UIManager.instance.ShowWarningPanel(false);

            if (UIManager.instance.settingPanel != null)
            {
                UIManager.instance.settingPanel.PanelCanvasGroup.alpha = 0f;
                UIManager.instance.settingPanel.ToggleSettingsPanel(false);
            }

            return;
        }

        if (UIManager.instance.optionsPanel != null && UIManager.instance.optionsPanel.activeSelf)
        {
            UIManager.instance.ShowOptionsPanel(false);
            UIManager.instance.ShowPausePanel(true);
            return;
        }

        GameManager.instance?.GamePause();
    }

    public void ShowCursor(bool show)
    {
        if (show)
        {
            UIManager.instance.ShowSlected(false);
            GameManager.instance.GameEnd();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            UIManager.instance.ShowSlected(true);
            GameManager.instance.GameStart();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnInteractInput(InputAction.CallbackContext ctx)
    {
        OnInteractPressed?.Invoke();
    }
}
