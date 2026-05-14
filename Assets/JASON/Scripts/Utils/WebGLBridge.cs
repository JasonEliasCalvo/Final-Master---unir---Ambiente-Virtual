using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLBridge
{
    [DllImport("__Internal")]
    private static extern void OpenBookModal(string url);

    [DllImport("__Internal")]
    private static extern void CloseBookModal();

    public static void ShowUrlInModal(string url)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenBookModal(url);
#else
        Debug.Log("Modal solo funciona en WebGL");
#endif
    }

    public static void HideModal()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CloseBookModal();
#endif
    }
}

