using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    void Update()
    {
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