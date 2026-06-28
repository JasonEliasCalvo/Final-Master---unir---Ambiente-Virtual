using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class GenericTransforController : MonoBehaviour
{
    public bool UseLocalPosition = true;

    public void SetPosition(Transform tagetPosition)
    {
        transform.SetParent(transform.parent);
        transform.position = tagetPosition.position;
    }
}
