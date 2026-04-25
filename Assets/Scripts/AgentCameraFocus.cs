using UnityEngine;

public class AgentCameraFocus : MonoBehaviour
{
    [SerializeField] private FloorCameraController floorCameraController;

    private void Awake()
    {
        if (floorCameraController == null)
            floorCameraController = FindFirstObjectByType<FloorCameraController>();

        //Debug.Log(
        //    $"AgentCameraFocus Awake | " +
        //    $"floorCameraController={(floorCameraController != null ? floorCameraController.name : "null")}"
        //);
    }

    public void FocusWorker(WorkerView worker)
    {
        if (floorCameraController == null)
        {
            //Debug.LogWarning("FocusWorker failed: floorCameraController is null.");
            return;
        }

        floorCameraController.FocusWorker(worker, true);
    }
}