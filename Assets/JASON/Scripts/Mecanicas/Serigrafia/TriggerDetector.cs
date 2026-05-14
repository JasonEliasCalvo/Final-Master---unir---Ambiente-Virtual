using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    [SerializeField] private ScreenFrame currentScreen;
    [SerializeField] private PrintSurfaceInstance currentSuperfice;
    [SerializeField] private MonoBehaviour currentTarget;

    private void OnTriggerEnter(Collider other)
    {
        // Priorizar tipos relevantes
        var frame = other.GetComponent<ScreenFrame>();
        if (frame != null) { currentScreen = frame; currentTarget = frame;}

        var surface = other.GetComponent<PrintSurfaceInstance>();
        if (surface != null) { currentSuperfice = surface; currentTarget = surface; }

        var target = other.GetComponent<MonoBehaviour>();
        if (target != null) { currentTarget = target; return; }
    }

    private void OnTriggerStay(Collider other)
    {
        var frame = other.GetComponent<ScreenFrame>();
        if (frame != null) { currentScreen = frame; }

        var surface = other.GetComponent<PrintSurfaceInstance>();
        if (surface != null) { currentSuperfice = surface; }

        var target = other.GetComponent<MonoBehaviour>();
        if (target != null) { currentTarget = target; }
    }


    private void OnTriggerExit(Collider other)
    {
        var frame = other.GetComponent<ScreenFrame>();
        if (frame != null) { currentScreen = null; }

        var surface = other.GetComponent<PrintSurfaceInstance>();
        if (surface != null) { currentSuperfice = null; }

        var target = other.GetComponent<MonoBehaviour>();
        if (target != null) { currentTarget = null; }
    }

    public T GetDetected<T>() where T : MonoBehaviour
    {
        return currentTarget as T;
    }

    public ScreenFrame GetCurrentScreenFrame() => currentScreen;
    public PrintSurfaceInstance GetCurrentSurface() => currentSuperfice;
}
