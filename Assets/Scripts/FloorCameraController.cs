using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FloorCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FloorVisibilityController floorVisibilityController;
    [SerializeField] private Transform floorsParent;
    [SerializeField] private Transform towerCenter;
    [SerializeField] private Camera targetCamera;

    [Header("Close View")]
    [SerializeField] private Vector3 closeOffset = new Vector3(6f, 3f, 5f);

    [Header("Far View")]
    [SerializeField] private Vector3 farOffset = new Vector3(100f, 70f, -40f);

    [Header("Look Offset")]
    [SerializeField] private Vector3 closeLookOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private Vector3 farLookOffset = new Vector3(0f, 4f, 0f);

    [Header("Smoothing")]
    [SerializeField] private float positionLerpSpeed = 5f;
    [SerializeField] private float rotationLerpSpeed = 6f;

    [Header("Clipping")]
    [SerializeField] private float closeFarClip = 100f;
    [SerializeField] private float farFarClip = 500f;

    private Transform[] floors;
    private Vector3 currentOffset;
    private Vector3 currentLookOffset;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private enum ViewMode { Close, Far }
    [SerializeField] private ViewMode currentViewMode = ViewMode.Close;

    private void Start()
    {
        if (floorVisibilityController == null)
            floorVisibilityController = FindFirstObjectByType<FloorVisibilityController>();

        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        Invoke(nameof(DelayedInit), 0.2f);
    }

    private void DelayedInit()
    {
        RebuildFloorList();
        SetCloseView();
        SnapToCurrentFloor();
    }

    private void LateUpdate()
    {
        if (floorVisibilityController == null || floors == null || floors.Length == 0)
            return;

        int floorIndex = Mathf.Clamp(floorVisibilityController.currentFloor, 0, floors.Length - 1);
        Transform floor = floors[floorIndex];
        if (floor == null)
            return;

        Vector3 focusPoint = floor.position;
        if (towerCenter != null)
        {
            focusPoint.x = towerCenter.position.x;
            focusPoint.z = towerCenter.position.z;
        }

        Vector3 desiredPosition = focusPoint + currentOffset;
        Vector3 lookTarget = focusPoint + currentLookOffset;

        targetPosition = desiredPosition;
        targetRotation = Quaternion.LookRotation(lookTarget - targetPosition);

        transform.position = Vector3.Lerp(transform.position, targetPosition, positionLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
    }

    public void RebuildFloorList()
    {
        if (floorsParent == null)
        {
            floors = new Transform[0];
            Debug.LogWarning("FloorCameraController has no floorsParent assigned.");
            return;
        }

        var allChildren = floorsParent.GetComponentsInChildren<Transform>(true);
        var list = new List<Transform>();

        foreach (var child in allChildren)
        {
            if (child != floorsParent && child.name.StartsWith("Floor_"))
                list.Add(child);
        }

        floors = list.ToArray();
        System.Array.Sort(floors, (a, b) => b.position.y.CompareTo(a.position.y));

    }

    public void SnapToCurrentFloor()
    {
        if (floorVisibilityController == null || floors == null || floors.Length == 0)
            return;

        int floorIndex = Mathf.Clamp(floorVisibilityController.currentFloor, 0, floors.Length - 1);
        Transform floor = floors[floorIndex];
        if (floor == null)
            return;

        Vector3 focusPoint = floor.position;
        if (towerCenter != null)
        {
            focusPoint.x = towerCenter.position.x;
            focusPoint.z = towerCenter.position.z;
        }

        Vector3 desiredPosition = focusPoint + currentOffset;
        Vector3 lookTarget = focusPoint + currentLookOffset;

        transform.position = desiredPosition;
        transform.rotation = Quaternion.LookRotation(lookTarget - desiredPosition);
    }

    public void RefreshAfterFloorChange()
    {
        RebuildFloorList();
        SnapToCurrentFloor();
    }

    public void NextFloor()
    {
        if (floorVisibilityController == null)
            return;

        if (floors == null || floors.Length == 0)
            RebuildFloorList();

        floorVisibilityController.currentFloor = Mathf.Clamp(
            floorVisibilityController.currentFloor + 1,
            0,
            floors.Length - 1
        );

        floorVisibilityController.UpdateVisibility();
        RefreshAfterFloorChange();
    }

    public void PreviousFloor()
    {
        if (floorVisibilityController == null)
            return;

        if (floors == null || floors.Length == 0)
            RebuildFloorList();

        floorVisibilityController.currentFloor = Mathf.Clamp(
            floorVisibilityController.currentFloor - 1,
            0,
            floors.Length - 1
        );

        floorVisibilityController.UpdateVisibility();
        RefreshAfterFloorChange();
    }

    public void SetCloseView()
    {
        currentViewMode = ViewMode.Close;
        currentOffset = closeOffset;
        currentLookOffset = closeLookOffset;

        if (targetCamera != null)
            targetCamera.farClipPlane = closeFarClip;

        floorVisibilityController?.SetCloseView();
        SnapToCurrentFloor();
    }

    public void SetFarView()
    {
        currentViewMode = ViewMode.Far;
        currentOffset = farOffset;
        currentLookOffset = farLookOffset;

        if (targetCamera != null)
            targetCamera.farClipPlane = farFarClip;

        floorVisibilityController?.SetFarView();
        SnapToCurrentFloor();
    }

    public int GetFloorCount()
    {
        return floors != null ? floors.Length : 0;
    }
}