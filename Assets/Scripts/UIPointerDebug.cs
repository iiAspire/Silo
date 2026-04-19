using UnityEngine;
using UnityEngine.EventSystems;

public class UIPointerDebug : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("UI: pointer enter " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("UI: pointer exit " + gameObject.name);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("UI: pointer down " + gameObject.name);
    }
}