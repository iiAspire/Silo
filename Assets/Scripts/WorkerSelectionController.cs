using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WorkerSelectionController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask selectionMask = ~0;
    [SerializeField] private WorkerInspectorPanel inspectorPanel;

    private readonly List<RaycastResult> uiResults = new();

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (IsPointerOverInspectorUI())
            return;

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
        {
            //Debug.LogWarning("WorkerSelectionController: No target camera assigned.");
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = targetCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, selectionMask, QueryTriggerInteraction.Collide))
        {
            WorkerView worker = hit.collider.GetComponentInParent<WorkerView>();

            if (worker != null)
            {
                if (inspectorPanel != null)
                    inspectorPanel.Show(worker);

                return;
            }
        }

        if (inspectorPanel != null && !inspectorPanel.IsPinned)
            inspectorPanel.Hide();
    }

    private bool IsPointerOverInspectorUI()
    {
        if (inspectorPanel == null || EventSystem.current == null || Mouse.current == null)
            return false;

        GameObject inspectorRoot = inspectorPanel.PanelRoot;
        if (inspectorRoot == null || !inspectorRoot.activeInHierarchy)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        uiResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiResults);

        for (int i = 0; i < uiResults.Count; i++)
        {
            GameObject hitObject = uiResults[i].gameObject;
            if (hitObject == null)
                continue;

            if (hitObject == inspectorRoot || hitObject.transform.IsChildOf(inspectorRoot.transform))
                return true;
        }

        return false;
    }
}