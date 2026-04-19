using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
{
    [SerializeField] private RectTransform panelToMove;
    [SerializeField] private Canvas parentCanvas;

    private Vector3 pointerOffset;

    private void Awake()
    {
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (panelToMove != null)
            panelToMove.SetAsLastSibling();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (panelToMove == null || parentCanvas == null)
            return;

        RectTransform canvasRect = parentCanvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect,
            eventData.position,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera,
            out Vector3 pointerWorldPos))
        {
            pointerOffset = panelToMove.position - pointerWorldPos;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (panelToMove == null || parentCanvas == null)
            return;

        RectTransform canvasRect = parentCanvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect,
            eventData.position,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera,
            out Vector3 pointerWorldPos))
        {
            panelToMove.position = pointerWorldPos + pointerOffset;
        }
    }
}