using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    void Update()
    {
        if (UIManager.instance == null || UIManager.instance.IsPanelActive())
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ScreenPrintingManager.instance.ToggleMaterialInHand();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ScreenPrintingManager.instance.ToggleInkInHand();
        }
    }
}