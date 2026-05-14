using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AssemblyChecker : MonoBehaviour
{
    [SerializeField] private List<CustomizedSocketType> sockets = new();
    [SerializeField] private UnityEvent onAssembled;
    [SerializeField] private bool isAssemblyCompleted = false;

    private float checkInterval = 0.5f;
    private float nextCheckTime = 0f;

    void Start()
    {
        FindAllSockets();
    }

    private void Update()
    {
        if (isAssemblyCompleted) return;

        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        // Actualizar lista si se agregaron nuevas piezas con sockets
        FindAllSockets();

        // Verificar si todos los sockets están correctamente ensamblados
        if (AreAllSocketsCompleted())
        {
            isAssemblyCompleted = true;
            Debug.Log("¡Ensamblaje completo y correcto!");
            onAssembled?.Invoke();
        }
    }

    private void FindAllSockets()
    {
        var allSockets = GetComponentsInChildren<CustomizedSocketType>(true);

        sockets.Clear();

        foreach (var socket in allSockets)
        {
            if (!sockets.Contains(socket))
                sockets.Add(socket);
        }
    }

    private bool AreAllSocketsCompleted()
    {
        foreach (var socket in sockets)
        {
            if (socket.behavior == PostSocketBehavior.RemoveComponents)
            {
                if (socket.transform.childCount == 0)
                {
                    Debug.Log($"Socket {socket.name} está vacío.");
                    return false; // Si no hay hijos, falla el ensamblaje
                }

                Debug.Log($"Validando tipo de pieza por hijo en socket {socket.name}");

                for (int i = 0; i < socket.transform.childCount; i++)
                {
                    var piece = socket.transform.GetChild(i).GetComponent<TypePiece>();

                    if (piece == null)
                    {
                        Debug.Log($"No se encontró componente TypePiece en hijo {i} de socket {socket.name}");
                        return false;
                    }

                    if (piece.type != socket.validType)
                    {
                        Debug.Log($"Tipo de pieza incorrecto en socket {socket.name}, hijo {i}. Esperado: {socket.validType}, Encontrado: {piece.type}");
                        return false;
                    }

                    Debug.Log($"Tipo de pieza correcto en socket {socket.name}, hijo {i}. Esperado: {socket.validType}, Encontrado: {piece.type}");
                }
            }
            else
            {
                if (!socket.hasSelection)
                {
                    Debug.Log($"Socket {socket.name} no tiene selección.");
                    return false;
                }

                var piece = socket.firstInteractableSelected?.transform.GetComponent<TypePiece>();
                if (piece == null)
                {
                    Debug.Log($"No se encontró componente TypePiece en la selección de socket {socket.name}");
                    return false;
                }

                if (piece.type != socket.validType)
                {
                    Debug.Log($"Tipo de pieza incorrecto en socket {socket.name}. Esperado: {socket.validType}, Encontrado: {piece.type}");
                    return false;
                }

                Debug.Log($"Tipo de pieza correcto en socket {socket.name}. Esperado: {socket.validType}, Encontrado: {piece.type}");
            }
        }

        return true; 
    }

    public bool IsAssembled()
    {
        return isAssemblyCompleted;
    }
}
