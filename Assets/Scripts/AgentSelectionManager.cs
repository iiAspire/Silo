using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class AgentSelectionManager : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask selectableLayers = ~0;
    [SerializeField] private AgentCameraFocus cameraFocus;
    [SerializeField] private WorkerInspectorPanel inspectorPanel;

    private WorkerView selectedWorker;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (targetCamera == null || Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            TrySelectFromMouse();
        }
    }

    private void TrySelectFromMouse()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = targetCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, selectableLayers))
        {
            WorkerView worker = hit.collider.GetComponentInParent<WorkerView>();

            if (worker != null)
            {
                SetSelected(worker, false);
                return;
            }
        }

        ClearSelection();
    }

    public void SelectByAgentId(int agentId, bool focusCamera = true)
    {
        if (SimulationManager.Instance == null)
            return;

        WorkerView worker = SimulationManager.Instance.GetWorkerViewByAgentId(agentId);
        //if (worker == null)
        //{
        //    Debug.LogWarning($"No WorkerView found for AgentId={agentId}");
        //    return;
        //}

        SetSelected(worker, focusCamera);
    }

    public void SetSelected(WorkerView worker, bool focusCamera)
    {
        if (selectedWorker == worker)
        {
            if (focusCamera && cameraFocus != null && selectedWorker != null)
                cameraFocus.FocusWorker(selectedWorker);

            if (inspectorPanel != null && selectedWorker != null)
                inspectorPanel.Show(selectedWorker);

            return;
        }

        if (selectedWorker != null)
            selectedWorker.SetSelected(false);

        selectedWorker = worker;

        if (selectedWorker != null)
        {
            selectedWorker.SetSelected(true);

            if (inspectorPanel != null)
                inspectorPanel.Show(selectedWorker);

            if (focusCamera && cameraFocus != null)
                cameraFocus.FocusWorker(selectedWorker);

            AgentRecord agent = selectedWorker.Agent;
            //if (agent != null)
            //{
            //    Debug.Log(
            //        $"Selected Agent {agent.AgentId} | ViewInstance={selectedWorker.gameObject.GetInstanceID()} | " +
            //        $"Job={agent.Job} | BaseJob={agent.BaseJob} | Shift={agent.AssignedShiftLabel} | " +
            //        $"WorkNode={(agent.AssignedWorkNode != null ? agent.AssignedWorkNode.name : "None")}"
            //    );
            //}
        }
    }

    public void ClearSelection()
    {
        if (selectedWorker != null)
            selectedWorker.SetSelected(false);

        selectedWorker = null;

        if (inspectorPanel != null && !inspectorPanel.IsPinned)
            inspectorPanel.Hide();
    }

    public WorkerView GetSelectedWorker()
    {
        return selectedWorker;
    }
}