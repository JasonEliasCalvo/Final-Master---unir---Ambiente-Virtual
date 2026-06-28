using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonGeneric : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
    public UnityEvent onHover;
    public UnityEvent outHover;
    public UnityEvent onClick;

    public void OnDeselect(BaseEventData eventData)
    {
        outHover?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHover?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outHover?.Invoke();
    }

    public void OnSelect(BaseEventData eventData)
    {
        onHover?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}
